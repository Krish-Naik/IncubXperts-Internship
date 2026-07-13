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

    public record LeadRow(
        Guid Id,
        string FullName,
        string Email,
        string Phone,
        UserStatus Status,
        DateTime CreatedAtUtc
    );

    public async Task<List<LeadRow>> GetMyLeadsAsync(Guid brokerId, CancellationToken ct)
    {
        return await db
            .InternalUsers.Where(u => u.ReferredByBrokerId == brokerId)
            .OrderByDescending(u => u.CreatedAtUtc)
            .Select(u => new LeadRow(u.Id, u.FullName, u.Email, u.Phone, u.Status, u.CreatedAtUtc))
            .ToListAsync(ct);
    }

    public async Task ResendLeadInviteAsync(Guid brokerId, Guid leadId, CancellationToken ct)
    {
        var lead =
            await db.InternalUsers.FirstOrDefaultAsync(
                u => u.Id == leadId && u.ReferredByBrokerId == brokerId,
                ct
            ) ?? throw new KeyNotFoundException("Lead not found.");

        if (lead.Status != UserStatus.Invited)
            throw new InvalidOperationException("Only invited leads can receive a new invitation.");

        await authService.CreateResetTokenAsync(leadId, isInvite: true, ct);
    }

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
