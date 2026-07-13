using LOS.Application.Email;
using LOS.Domain.Entities;
using LOS.Domain.Enums;
using LOS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LOS.Application.Disbursement;

public class DisbursementService(LOSDbContext db, IEmailService emailService)
{
    public record DisbursementQueueItemDto(
        Guid Id,
        string ReferenceNumber,
        string CustomerName,
        decimal RequestedAmount,
        decimal? ApprovedInterestRate,
        int? ApprovedTenureMonths,
        decimal? MonthlyEmi,
        ApplicationStatus Status
    );

    public Task<List<DisbursementQueueItemDto>> GetQueueAsync(CancellationToken ct) =>
        db
            .LoanApplications.Where(a => a.Status == ApplicationStatus.OfferAccepted)
            .OrderBy(a => a.UpdatedAtUtc)
            .Select(a => new DisbursementQueueItemDto(
                a.Id,
                a.ReferenceNumber,
                a.Customer.FullName,
                a.RequestedAmount,
                a.ApprovedInterestRate,
                a.ApprovedTenureMonths,
                a.MonthlyEmi,
                a.Status
            ))
            .ToListAsync(ct);

    public async Task<LoanApplication> AcceptOfferAsync(
        Guid applicationId,
        Guid customerId,
        CancellationToken ct
    )
    {
        var app =
            await db.LoanApplications.FirstOrDefaultAsync(
                a => a.Id == applicationId && a.CustomerId == customerId,
                ct
            ) ?? throw new InvalidOperationException("Application not found.");

        if (app.Status != ApplicationStatus.Approved)
            throw new InvalidOperationException(
                "Only Approved applications can have their offer accepted."
            );

        app.Status = ApplicationStatus.OfferAccepted;
        app.UpdatedAtUtc = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return app;
    }

    public async Task<LoanApplication> DisburseAsync(
        Guid applicationId,
        Guid managerId,
        string bankReference,
        CancellationToken ct
    )
    {
        var app =
            await db
                .LoanApplications.Include(a => a.Customer)
                .FirstOrDefaultAsync(a => a.Id == applicationId, ct)
            ?? throw new InvalidOperationException("Application not found.");

        if (app.Status != ApplicationStatus.OfferAccepted)
            throw new InvalidOperationException("Offer must be accepted before disbursement.");

        app.Status = ApplicationStatus.Disbursed;
        app.DisbursedAtUtc = DateTime.UtcNow;
        app.UpdatedAtUtc = DateTime.UtcNow;

        db.ApplicationAuditLogs.Add(
            new ApplicationAuditLog
            {
                Id = Guid.NewGuid(),
                LoanApplicationId = app.Id,
                ActorUserId = managerId,
                Action = "Disbursed",
                OldValue = "OfferAccepted",
                NewValue = "Disbursed",
            }
        );

        await db.SaveChangesAsync(ct);

        await emailService.SendAsync(
            app.Customer.Email,
            $"Loan Disbursed - {app.ReferenceNumber}",
            $"<p>Your loan of ₹{app.RequestedAmount} has been disbursed. Bank reference: {bankReference}.</p>",
            ct
        );

        return app;
    }

    public record EmiScheduleRow(
        int Month,
        decimal Emi,
        decimal Principal,
        decimal Interest,
        decimal Balance
    );

    public List<EmiScheduleRow> GetRepaymentSchedule(LoanApplication app)
    {
        if (
            app.ApprovedInterestRate is null
            || app.ApprovedTenureMonths is null
            || app.MonthlyEmi is null
        )
            throw new InvalidOperationException("Loan terms not finalized.");

        var schedule = new List<EmiScheduleRow>();
        var balance = app.RequestedAmount;
        var monthlyRate = app.ApprovedInterestRate.Value / 12 / 100;

        for (var month = 1; month <= app.ApprovedTenureMonths.Value; month++)
        {
            var interest = Math.Round(balance * monthlyRate, 2);
            var principal = Math.Round(app.MonthlyEmi.Value - interest, 2);
            balance = Math.Max(0, balance - principal);
            var emi = Math.Round(principal + interest, 2);
            schedule.Add(new EmiScheduleRow(month, emi, principal, interest, balance));
        }

        return schedule;
    }
}
