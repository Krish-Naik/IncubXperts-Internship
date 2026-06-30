using LOS.Application.Auth;
using LOS.Application.Users.DTOs;
using LOS.Domain.Entities;
using LOS.Domain.Enums;
using LOS.Infrastructure.Identity;
using LOS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LOS.Application.Users;

public class UserService(
    LOSDbContext db,
    PasswordHasher passwordHasher,
    AuthService authService,
    RefreshTokenService refreshTokenService
)
{
    public async Task<IReadOnlyList<UserListItemDto>> GetUsersAsync(
        CancellationToken ct = default
    ) =>
        await db
            .InternalUsers.Include(x => x.Role)
            .Include(x => x.Branch)
            .OrderBy(x => x.FullName)
            .Select(x => new UserListItemDto(
                x.Id,
                x.EmployeeId,
                x.FullName,
                x.Email,
                x.Phone,
                x.Role.Name,
                x.Branch != null ? x.Branch.Name : null,
                x.Status.ToString(),
                x.CreatedAtUtc
            ))
            .ToListAsync(ct);

    public async Task<UserDetailDto> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var user =
            await db
                .InternalUsers.Include(x => x.Role)
                .Include(x => x.Branch)
                .Include(x => x.Auth)
                .FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException("User not found.");

        return MapDetail(user);
    }

    public async Task<UserDetailDto> CreateAsync(
        CreateUserDto request,
        CancellationToken ct = default
    )
    {
        var email = request.Email.Trim().ToLowerInvariant();
        if (
            await db.InternalUsers.AnyAsync(
                x => x.Email == email || x.EmployeeId == request.EmployeeId,
                ct
            )
        )
        {
            throw new InvalidOperationException("Email or employee ID already exists.");
        }

        await EnsureRoleExistsAsync(request.RoleId, ct);
        if (request.BranchId.HasValue)
        {
            await EnsureBranchExistsAsync(request.BranchId.Value, ct);
        }

        var user = new InternalUser
        {
            Id = Guid.NewGuid(),
            EmployeeId = request.EmployeeId.Trim(),
            FullName = request.FullName.Trim(),
            Email = email,
            Phone = request.Phone.Trim(),
            RoleId = request.RoleId,
            BranchId = request.BranchId,
            Status = UserStatus.Invited,
            CreatedAtUtc = DateTime.UtcNow,
        };

        user.Auth = new AuthUser
        {
            Id = Guid.NewGuid(),
            InternalUserId = user.Id,
            PasswordHash = passwordHasher.Hash(Guid.NewGuid().ToString()),
            MustChangePassword = true,
        };

        db.InternalUsers.Add(user);
        await db.SaveChangesAsync(ct);

        var inviteLink = await authService.CreateResetTokenAsync(user.Id, isInvite: true, ct);
        _ = inviteLink;

        return await GetByIdAsync(user.Id, ct);
    }

    public async Task<UserDetailDto> UpdateAsync(
        Guid id,
        UpdateUserDto request,
        CancellationToken ct = default
    )
    {
        var user =
            await db.InternalUsers.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException("User not found.");

        await EnsureRoleExistsAsync(request.RoleId, ct);
        if (request.BranchId.HasValue)
        {
            await EnsureBranchExistsAsync(request.BranchId.Value, ct);
        }

        user.FullName = request.FullName.Trim();
        user.Phone = request.Phone.Trim();
        user.RoleId = request.RoleId;
        user.BranchId = request.BranchId;
        user.UpdatedAtUtc = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);
        return await GetByIdAsync(id, ct);
    }

    public async Task<UserDetailDto> ActivateAsync(Guid id, CancellationToken ct = default)
    {
        var user =
            await db.InternalUsers.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException("User not found.");

        user.Status = UserStatus.Active;
        user.UpdatedAtUtc = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return await GetByIdAsync(id, ct);
    }

    public async Task<UserDetailDto> DeactivateAsync(Guid id, CancellationToken ct = default)
    {
        var user =
            await db.InternalUsers.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException("User not found.");

        user.Status = UserStatus.Inactive;
        user.UpdatedAtUtc = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        await refreshTokenService.RevokeAllForUserAsync(id, ct);
        return await GetByIdAsync(id, ct);
    }

    public async Task<UserDetailDto> AssignRoleAsync(
        Guid id,
        Guid roleId,
        CancellationToken ct = default
    )
    {
        var user =
            await db.InternalUsers.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException("User not found.");

        await EnsureRoleExistsAsync(roleId, ct);
        user.RoleId = roleId;
        user.UpdatedAtUtc = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return await GetByIdAsync(id, ct);
    }

    public async Task<UserDetailDto> AssignBranchAsync(
        Guid id,
        Guid? branchId,
        CancellationToken ct = default
    )
    {
        var user =
            await db.InternalUsers.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException("User not found.");

        if (branchId.HasValue)
        {
            await EnsureBranchExistsAsync(branchId.Value, ct);
        }

        user.BranchId = branchId;
        user.UpdatedAtUtc = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return await GetByIdAsync(id, ct);
    }

    public async Task<InviteStatusDto> GetInviteStatusAsync(Guid id, CancellationToken ct = default)
    {
        var token = await db
            .PasswordResetTokens.Where(x =>
                x.InternalUserId == id && x.IsInvite && x.UsedAtUtc == null
            )
            .OrderByDescending(x => x.CreatedAtUtc)
            .FirstOrDefaultAsync(ct);

        return token is null
            ? new InviteStatusDto("No active invitation", null)
            : new InviteStatusDto(token.IsValid ? "Pending" : "Expired", token.ExpiresAtUtc);
    }

    public async Task ResendInviteAsync(Guid id, CancellationToken ct = default)
    {
        var user =
            await db.InternalUsers.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException("User not found.");

        if (user.Status != UserStatus.Invited)
        {
            throw new InvalidOperationException("Only invited users can receive a new invitation.");
        }

        await authService.CreateResetTokenAsync(id, isInvite: true, ct);
    }

    private async Task EnsureRoleExistsAsync(Guid roleId, CancellationToken ct)
    {
        if (!await db.Roles.AnyAsync(x => x.Id == roleId, ct))
        {
            throw new InvalidOperationException("Role not found.");
        }
    }

    private async Task EnsureBranchExistsAsync(Guid branchId, CancellationToken ct)
    {
        if (!await db.Branches.AnyAsync(x => x.Id == branchId && x.IsActive, ct))
        {
            throw new InvalidOperationException("Branch not found.");
        }
    }

    private static UserDetailDto MapDetail(InternalUser user) =>
        new(
            user.Id,
            user.EmployeeId,
            user.FullName,
            user.Email,
            user.Phone,
            user.RoleId,
            user.Role.Name,
            user.BranchId,
            user.Branch?.Name,
            user.Status.ToString(),
            user.Auth?.MustChangePassword ?? false,
            user.CreatedAtUtc,
            user.UpdatedAtUtc
        );
}
