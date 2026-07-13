using LOS.Application.Auth;
using LOS.Domain.Entities;
using LOS.Domain.Enums;
using LOS.Infrastructure.Identity;
using LOS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LOS.Application.Brokers;

public class BrokerService(LOSDbContext db, AuthService authService, PasswordHasher passwordHasher)
{
    public async Task<InternalUser> RegisterLeadAsync(
        Guid brokerId,
        string fullName,
        string email,
        string phone,
        CancellationToken ct
    )
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        if (await db.InternalUsers.AnyAsync(u => u.Email == normalizedEmail, ct))
            throw new InvalidOperationException("A customer with this email already exists.");

        var customerRole = await db.Roles.FirstAsync(r => r.Name == "Customer", ct);
        var lead = new InternalUser
        {
            Id = Guid.NewGuid(),
            EmployeeId = $"LEAD-{Guid.NewGuid().ToString()[..8].ToUpperInvariant()}",
            FullName = fullName,
            Email = normalizedEmail,
            Phone = phone,
            RoleId = customerRole.Id,
            Status = UserStatus.Invited,
            ReferredByBrokerId = brokerId,
            CreatedAtUtc = DateTime.UtcNow,
        };

        // This is the fix: without an AuthUser row, ResetPasswordAsync has nothing
        // to attach the new password hash to and throws "User credentials not found."
        lead.Auth = new AuthUser
        {
            Id = Guid.NewGuid(),
            InternalUserId = lead.Id,
            PasswordHash = passwordHasher.Hash(Guid.NewGuid().ToString()),
            MustChangePassword = true,
        };

        db.InternalUsers.Add(lead);
        await db.SaveChangesAsync(ct);

        await authService.CreateResetTokenAsync(lead.Id, isInvite: true, ct);

        return lead;
    }

    /// <summary>
    /// Verifies the given application was referred by this broker before letting them
    /// act on it (e.g. upload a document on the customer's behalf).
    /// </summary>
    public async Task EnsureBrokerOwnsApplicationAsync(
        Guid brokerId,
        Guid applicationId,
        CancellationToken ct
    )
    {
        var owns = await db.LoanApplications.AnyAsync(
            a => a.Id == applicationId && a.BrokerId == brokerId,
            ct
        );
        if (!owns)
            throw new InvalidOperationException(
                "You can only upload documents for applications referred by you."
            );
    }

    // Id added so the frontend can let brokers pick from a dropdown instead of
    // asking the customer to copy a raw GUID out of the URL (see broker flow fix below).
    public record ReferredApplicationRow(
        Guid Id,
        string ReferenceNumber,
        ApplicationStatus Status,
        decimal? Commission
    );

    public async Task<List<ReferredApplicationRow>> GetPipelineAsync(
        Guid brokerId,
        DateTime? from,
        DateTime? to,
        CancellationToken ct
    )
    {
        var query = db.LoanApplications.Where(a => a.BrokerId == brokerId);
        if (from.HasValue)
            query = query.Where(a => a.CreatedAtUtc >= from.Value);
        if (to.HasValue)
            query = query.Where(a => a.CreatedAtUtc <= to.Value);

        return await query
            .OrderByDescending(a => a.CreatedAtUtc)
            .Select(a => new ReferredApplicationRow(
                a.Id,
                a.ReferenceNumber,
                a.Status,
                a.Status == ApplicationStatus.Disbursed ? a.RequestedAmount * 0.01m : null
            ))
            .ToListAsync(ct);
    }
}
