using LOS.Application.Options;
using LOS.Domain.Entities;
using LOS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace LOS.Infrastructure.Identity;

public class RefreshTokenService(LOSDbContext db, IOptions<JwtOptions> options)
{
    private readonly JwtOptions _jwtOptions = options.Value;

    public async Task<RefreshToken> CreateAsync(Guid internalUserId, CancellationToken ct = default)
    {
        var token = new RefreshToken
        {
            Id = Guid.NewGuid(),
            InternalUserId = internalUserId,
            Token = TokenGenerator.CreateSecureToken(),
            CreatedAtUtc = DateTime.UtcNow,
            ExpiresAtUtc = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenDays),
        };

        db.RefreshTokens.Add(token);
        await db.SaveChangesAsync(ct);
        return token;
    }

    public async Task<RefreshToken?> GetActiveAsync(string token, CancellationToken ct = default) =>
        await db
            .RefreshTokens.Include(x => x.InternalUser)
                .ThenInclude(x => x.Role)
            .FirstOrDefaultAsync(
                x => x.Token == token && x.RevokedAtUtc == null && x.ExpiresAtUtc > DateTime.UtcNow,
                ct
            );

    public async Task RevokeAsync(string token, CancellationToken ct = default)
    {
        var entity = await db.RefreshTokens.FirstOrDefaultAsync(x => x.Token == token, ct);
        if (entity is not null)
        {
            entity.RevokedAtUtc = DateTime.UtcNow;
            await db.SaveChangesAsync(ct);
        }
    }

    public async Task RevokeAllForUserAsync(Guid internalUserId, CancellationToken ct = default)
    {
        var tokens = await db
            .RefreshTokens.Where(x => x.InternalUserId == internalUserId && x.RevokedAtUtc == null)
            .ToListAsync(ct);

        foreach (var token in tokens)
        {
            token.RevokedAtUtc = DateTime.UtcNow;
        }

        await db.SaveChangesAsync(ct);
    }
}
