using LOS.Application.Auth.DTOs;
using LOS.Application.Options;
using LOS.Domain.Constants;
using LOS.Domain.Entities;
using LOS.Domain.Enums;
using LOS.Infrastructure.Identity;
using LOS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace LOS.Application.Auth;

public class AuthService(
    LOSDbContext db,
    PasswordHasher passwordHasher,
    JwtTokenGenerator jwtTokenGenerator,
    RefreshTokenService refreshTokenService,
    IOptions<SecurityOptions> securityOptions,
    IOptions<FrontendOptions> frontendOptions,
    ILogger<AuthService> logger
)
{
    private readonly SecurityOptions _security = securityOptions.Value;
    private readonly FrontendOptions _frontend = frontendOptions.Value;

    public async Task<LoginResponseDto> LoginAsync(
        LoginRequestDto request,
        CancellationToken ct = default
    )
    {
        var user = await db
            .InternalUsers.Include(x => x.Role)
            .Include(x => x.Branch)
            .Include(x => x.Auth)
            .FirstOrDefaultAsync(x => x.Email == request.Email.Trim().ToLowerInvariant(), ct);

        if (user?.Auth is null)
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        if (user.Status == UserStatus.Inactive)
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        if (user.Status == UserStatus.Invited)
        {
            throw new UnauthorizedAccessException(
                "Account is not activated yet. Please complete your invitation."
            );
        }

        if (user.Auth.LockoutEndUtc > DateTime.UtcNow)
        {
            throw new UnauthorizedAccessException(
                "Account is temporarily locked. Please try again later."
            );
        }

        if (user.Auth.MustChangePassword)
        {
            throw new UnauthorizedAccessException(
                "Please use your invitation or password reset link before signing in."
            );
        }

        if (!passwordHasher.Verify(request.Password, user.Auth.PasswordHash))
        {
            user.Auth.FailedLoginAttempts++;
            if (user.Auth.FailedLoginAttempts >= _security.MaxFailedLoginAttempts)
            {
                user.Auth.LockoutEndUtc = DateTime.UtcNow.AddMinutes(_security.LockoutMinutes);
                user.Status = UserStatus.Locked;
            }

            await db.SaveChangesAsync(ct);
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        user.Auth.FailedLoginAttempts = 0;
        user.Auth.LockoutEndUtc = null;
        if (user.Status == UserStatus.Locked)
        {
            user.Status = UserStatus.Active;
        }

        user.Auth.LastLoginAtUtc = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);

        var (accessToken, expiresAt) = jwtTokenGenerator.CreateAccessToken(user);
        var refresh = await refreshTokenService.CreateAsync(user.Id, ct);

        return new LoginResponseDto(
            accessToken,
            expiresAt,
            refresh.Token,
            MapProfile(user),
            GetLandingRoute(user.Role.Name)
        );
    }

    public async Task<LoginResponseDto> RefreshAsync(
        string refreshToken,
        CancellationToken ct = default
    )
    {
        var token =
            await refreshTokenService.GetActiveAsync(refreshToken, ct)
            ?? throw new UnauthorizedAccessException("Session expired. Please log in again.");

        var user = token.InternalUser;
        if (user.Status != UserStatus.Active)
        {
            throw new UnauthorizedAccessException("Session expired. Please log in again.");
        }

        await refreshTokenService.RevokeAsync(refreshToken, ct);
        var (accessToken, expiresAt) = jwtTokenGenerator.CreateAccessToken(user);
        var newRefresh = await refreshTokenService.CreateAsync(user.Id, ct);

        return new LoginResponseDto(
            accessToken,
            expiresAt,
            newRefresh.Token,
            MapProfile(user),
            GetLandingRoute(user.Role.Name)
        );
    }

    public async Task LogoutAsync(string refreshToken, CancellationToken ct = default)
    {
        await refreshTokenService.RevokeAsync(refreshToken, ct);
    }

    public async Task<MessageResponseDto> ForgotPasswordAsync(
        string email,
        CancellationToken ct = default
    )
    {
        var user = await db.InternalUsers.FirstOrDefaultAsync(
            x => x.Email == email.Trim().ToLowerInvariant() && x.Status == UserStatus.Active,
            ct
        );

        if (user is not null)
        {
            await CreateResetTokenAsync(user.Id, isInvite: false, ct);
        }

        return new MessageResponseDto(
            "If an account exists for that email, password reset instructions have been sent."
        );
    }

    public async Task<MessageResponseDto> ResetPasswordAsync(
        ResetPasswordRequestDto request,
        CancellationToken ct = default
    )
    {
        ValidatePasswordPolicy(request.NewPassword);

        var resetToken =
            await db
                .PasswordResetTokens.Include(x => x.InternalUser)
                    .ThenInclude(x => x.Auth)
                .FirstOrDefaultAsync(x => x.Token == request.Token, ct)
            ?? throw new InvalidOperationException("Invalid or expired reset token.");

        if (!resetToken.IsValid)
        {
            throw new InvalidOperationException("Invalid or expired reset token.");
        }

        var auth =
            resetToken.InternalUser.Auth
            ?? throw new InvalidOperationException("User credentials not found.");

        await EnsurePasswordNotReusedAsync(auth, request.NewPassword, ct);

        auth.PasswordHash = passwordHasher.Hash(request.NewPassword);
        auth.MustChangePassword = false;
        resetToken.UsedAtUtc = DateTime.UtcNow;
        resetToken.InternalUser.Status = UserStatus.Active;

        await db.SaveChangesAsync(ct);
        await refreshTokenService.RevokeAllForUserAsync(resetToken.InternalUserId, ct);

        return new MessageResponseDto("Password updated successfully. You can now sign in.");
    }

    public async Task<UserProfileDto> GetProfileAsync(Guid userId, CancellationToken ct = default)
    {
        var user =
            await db
                .InternalUsers.Include(x => x.Role)
                .Include(x => x.Branch)
                .Include(x => x.Auth)
                .FirstOrDefaultAsync(x => x.Id == userId, ct)
            ?? throw new KeyNotFoundException("User not found.");

        return MapProfile(user);
    }

    public static string GetLandingRoute(string role) =>
        role switch
        {
            AppRoles.SystemAdministrator => "/admin/users",
            AppRoles.VerificationOfficer => "/verification",
            AppRoles.BranchManager => "/approvals",
            AppRoles.Customer => "/my-application",
            AppRoles.BrokerAgent => "/broker/leads",
            _ => "/dashboard",
        };

    internal async Task<string> CreateResetTokenAsync(
        Guid internalUserId,
        bool isInvite,
        CancellationToken ct
    )
    {
        var tokenValue = TokenGenerator.CreateSecureToken();
        var hours = isInvite ? _security.InviteTokenHours : _security.PasswordResetTokenHours;

        db.PasswordResetTokens.Add(
            new PasswordResetToken
            {
                Id = Guid.NewGuid(),
                InternalUserId = internalUserId,
                Token = tokenValue,
                CreatedAtUtc = DateTime.UtcNow,
                ExpiresAtUtc = DateTime.UtcNow.AddHours(hours),
                IsInvite = isInvite,
            }
        );

        await db.SaveChangesAsync(ct);

        var baseUrl = (_frontend.BaseUrl ?? "http://localhost:4200").Trim().TrimEnd('/');
        var link = isInvite
            ? $"{baseUrl}/auth/reset-password?token={tokenValue}&invite=true"
            : $"{baseUrl}/auth/reset-password?token={tokenValue}";

        logger.LogInformation(
            "Password reset link for user {UserId}: {Link}",
            internalUserId,
            link
        );
        return link;
    }

    private async Task EnsurePasswordNotReusedAsync(
        AuthUser auth,
        string newPassword,
        CancellationToken ct
    )
    {
        var history = await db
            .PasswordHistory.Where(x => x.AuthUserId == auth.Id)
            .OrderByDescending(x => x.CreatedAtUtc)
            .Take(_security.PasswordHistoryCount)
            .ToListAsync(ct);

        if (
            history.Any(x => passwordHasher.Verify(newPassword, x.PasswordHash))
            || passwordHasher.Verify(newPassword, auth.PasswordHash)
        )
        {
            throw new InvalidOperationException("You cannot reuse a recent password.");
        }

        db.PasswordHistory.Add(
            new PasswordHistory
            {
                Id = Guid.NewGuid(),
                AuthUserId = auth.Id,
                PasswordHash = auth.PasswordHash,
                CreatedAtUtc = DateTime.UtcNow,
            }
        );
    }

    private static void ValidatePasswordPolicy(string password)
    {
        if (
            password.Length < 8
            || !password.Any(char.IsUpper)
            || !password.Any(char.IsLower)
            || !password.Any(char.IsDigit)
            || !password.Any(ch => !char.IsLetterOrDigit(ch))
        )
        {
            throw new InvalidOperationException(
                "Password must be at least 8 characters and include upper, lower, number, and special character."
            );
        }
    }

    private static UserProfileDto MapProfile(InternalUser user) =>
        new(
            user.Id,
            user.EmployeeId,
            user.FullName,
            user.Email,
            user.Phone,
            user.Role.Name,
            user.BranchId,
            user.Branch?.Name,
            user.Status.ToString(),
            user.Auth?.MustChangePassword ?? false
        );
}
