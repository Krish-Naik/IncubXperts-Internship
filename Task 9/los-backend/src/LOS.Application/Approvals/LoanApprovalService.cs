using LOS.Application.Email;
using LOS.Domain.Entities;
using LOS.Domain.Enums;
using LOS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LOS.Application.Approvals;

public class LoanApprovalService(LOSDbContext db, IEmailService emailService)
{
    public record ApprovalQueueItemDto(
        Guid Id,
        string ReferenceNumber,
        string CustomerName,
        LoanType LoanType,
        decimal RequestedAmount,
        int RequestedTenureMonths,
        ApplicationStatus Status,
        DateTime? SubmittedAtUtc
    );

    public Task<List<ApprovalQueueItemDto>> GetQueueAsync(CancellationToken ct) =>
        db
            .LoanApplications.Where(a =>
                a.Status == ApplicationStatus.Verified
                || a.Status == ApplicationStatus.PendingSeniorApproval
            )
            .OrderBy(a => a.SubmittedAtUtc)
            .Select(a => new ApprovalQueueItemDto(
                a.Id,
                a.ReferenceNumber,
                a.Customer.FullName,
                a.LoanType,
                a.RequestedAmount,
                a.RequestedTenureMonths,
                a.Status,
                a.SubmittedAtUtc
            ))
            .ToListAsync(ct);

    public async Task<LoanApplication> ApproveAsync(
        Guid applicationId,
        Guid managerId,
        decimal interestRate,
        int tenureMonths,
        CancellationToken ct
    )
    {
        var app =
            await db
                .LoanApplications.Include(a => a.Customer)
                .FirstOrDefaultAsync(a => a.Id == applicationId, ct)
            ?? throw new InvalidOperationException("Application not found.");

        if (app.Status != ApplicationStatus.Verified)
            throw new InvalidOperationException("Only Verified applications can be approved.");

        var branch = await db
            .Branches.Include(b => b.Policy)
            .FirstOrDefaultAsync(b => b.Id == app.Customer.BranchId, ct);
        if (
            branch?.Policy is { } policy
            && (interestRate < policy.MinInterestRate || interestRate > policy.MaxInterestRate)
        )
        {
            throw new InvalidOperationException(
                $"Interest rate must be between {policy.MinInterestRate}% and {policy.MaxInterestRate}% per branch policy."
            );
        }

        var emi = CalculateEmi(app.RequestedAmount, interestRate, tenureMonths);

        app.ApprovedInterestRate = interestRate;
        app.ApprovedTenureMonths = tenureMonths;
        app.MonthlyEmi = emi;
        app.UpdatedAtUtc = DateTime.UtcNow;

        var requiresSeniorApproval =
            branch?.Policy is { } p2 && app.RequestedAmount > p2.HighValueThreshold;
        app.Status = requiresSeniorApproval
            ? ApplicationStatus.PendingSeniorApproval
            : ApplicationStatus.Approved;

        LogAudit(app.Id, managerId, "StatusChanged", "Verified", app.Status.ToString());
        await db.SaveChangesAsync(ct);

        await emailService.SendAsync(
            app.Customer.Email,
            $"Application {app.ReferenceNumber} {app.Status}",
            $"<p>Your loan of ₹{app.RequestedAmount} has been {app.Status} at {interestRate}% for {tenureMonths} months. Monthly EMI: ₹{emi:F2}.</p>",
            ct
        );

        return app;
    }

    public async Task<LoanApplication> RejectAsync(
        Guid applicationId,
        Guid managerId,
        string reason,
        CancellationToken ct
    )
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new InvalidOperationException("Rejection reason is mandatory.");

        var app =
            await db
                .LoanApplications.Include(a => a.Customer)
                .FirstOrDefaultAsync(a => a.Id == applicationId, ct)
            ?? throw new InvalidOperationException("Application not found.");

        app.Status = ApplicationStatus.Rejected;
        app.RejectionReason = reason;
        app.UpdatedAtUtc = DateTime.UtcNow;

        LogAudit(app.Id, managerId, "StatusChanged", "Verified", "Rejected");
        await db.SaveChangesAsync(ct);

        await emailService.SendAsync(
            app.Customer.Email,
            $"Application {app.ReferenceNumber} Rejected",
            $"<p>Unfortunately, your application was rejected. Reason: {reason}</p>",
            ct
        );

        return app;
    }

    public async Task<LoanApplication> RequestMoreInfoAsync(
        Guid applicationId,
        Guid managerId,
        string details,
        CancellationToken ct
    )
    {
        if (string.IsNullOrWhiteSpace(details))
            throw new InvalidOperationException("Describe what information is needed.");

        var app =
            await db
                .LoanApplications.Include(a => a.Customer)
                .FirstOrDefaultAsync(a => a.Id == applicationId, ct)
            ?? throw new InvalidOperationException("Application not found.");

        var previousStatus = app.Status.ToString();

        app.InfoRequestDetails = details;
        app.InfoResponseText = null;
        app.Status = ApplicationStatus.InfoRequested;
        app.UpdatedAtUtc = DateTime.UtcNow;

        LogAudit(app.Id, managerId, "StatusChanged", previousStatus, "InfoRequested");
        await db.SaveChangesAsync(ct);

        await emailService.SendAsync(
            app.Customer.Email,
            $"Additional Information Needed - {app.ReferenceNumber}",
            $"<p>Please provide the following to continue processing: {details}</p>",
            ct
        );

        return app;
    }

    public async Task<LoanApplication> SeniorApproveAsync(
        Guid applicationId,
        Guid seniorApproverId,
        bool approve,
        string? reason,
        CancellationToken ct
    )
    {
        var app =
            await db
                .LoanApplications.Include(a => a.Customer)
                .FirstOrDefaultAsync(a => a.Id == applicationId, ct)
            ?? throw new InvalidOperationException("Application not found.");

        if (app.Status != ApplicationStatus.PendingSeniorApproval)
            throw new InvalidOperationException("Application is not pending senior approval.");

        app.Status = approve ? ApplicationStatus.Approved : ApplicationStatus.Rejected;
        app.RejectionReason = approve ? null : reason;
        app.UpdatedAtUtc = DateTime.UtcNow;

        LogAudit(
            app.Id,
            seniorApproverId,
            "SeniorDecision",
            "PendingSeniorApproval",
            app.Status.ToString()
        );
        await db.SaveChangesAsync(ct);

        await emailService.SendAsync(
            app.Customer.Email,
            $"Application {app.ReferenceNumber} {app.Status}",
            $"<p>Senior review complete. Your application status is now: {app.Status}.</p>",
            ct
        );

        return app;
    }

    private static decimal CalculateEmi(
        decimal principal,
        decimal annualRatePercent,
        int tenureMonths
    )
    {
        var monthlyRate = annualRatePercent / 12 / 100;
        if (monthlyRate == 0)
            return principal / tenureMonths;

        var factor = (double)Math.Pow((double)(1 + monthlyRate), tenureMonths);
        var emi = principal * monthlyRate * (decimal)factor / ((decimal)factor - 1);
        return Math.Round(emi, 2);
    }

    private void LogAudit(Guid appId, Guid actorId, string action, string oldVal, string newVal) =>
        db.ApplicationAuditLogs.Add(
            new ApplicationAuditLog
            {
                Id = Guid.NewGuid(),
                LoanApplicationId = appId,
                ActorUserId = actorId,
                Action = action,
                OldValue = oldVal,
                NewValue = newVal,
            }
        );
}
