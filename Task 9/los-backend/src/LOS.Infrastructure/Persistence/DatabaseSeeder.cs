using LOS.Application.Options;
using LOS.Domain.Constants;
using LOS.Domain.Entities;
using LOS.Domain.Enums;
using LOS.Infrastructure.Identity;
using LOS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace LOS.Infrastructure.Persistence;

public class DatabaseSeeder(
    LOSDbContext db,
    PasswordHasher passwordHasher,
    IOptions<SeedOptions> seedOptions,
    ILogger<DatabaseSeeder> logger
)
{
    public async Task SeedAsync(CancellationToken ct = default)
    {
        await db.Database.MigrateAsync(ct);

        if (!await db.Roles.AnyAsync(ct))
        {
            db.Roles.AddRange(
                AppRoles.All.Select(name => new Role
                {
                    Id = Guid.NewGuid(),
                    Name = name,
                    Description = $"{name} role",
                })
            );
            await db.SaveChangesAsync(ct);
        }

        if (!await db.Branches.AnyAsync(ct))
        {
            db.Branches.AddRange(
                new Branch
                {
                    Id = Guid.NewGuid(),
                    Code = "HQ",
                    Name = "Head Office",
                    City = "Mumbai",
                },
                new Branch
                {
                    Id = Guid.NewGuid(),
                    Code = "BLR",
                    Name = "Bangalore Branch",
                    City = "Bangalore",
                },
                new Branch
                {
                    Id = Guid.NewGuid(),
                    Code = "DEL",
                    Name = "Delhi Branch",
                    City = "Delhi",
                }
            );
            await db.SaveChangesAsync(ct);
        }

        var adminEmail = seedOptions.Value.AdminEmail.Trim().ToLowerInvariant();
        if (!await db.InternalUsers.AnyAsync(x => x.Email == adminEmail, ct))
        {
            var adminRole = await db.Roles.FirstAsync(
                x => x.Name == AppRoles.SystemAdministrator,
                ct
            );
            var hqBranch = await db.Branches.FirstAsync(x => x.Code == "HQ", ct);

            var admin = new InternalUser
            {
                Id = Guid.NewGuid(),
                EmployeeId = "EMP-001",
                FullName = "System Administrator",
                Email = adminEmail,
                Phone = "+91-9000000001",
                RoleId = adminRole.Id,
                BranchId = hqBranch.Id,
                Status = UserStatus.Active,
                CreatedAtUtc = DateTime.UtcNow,
            };

            admin.Auth = new AuthUser
            {
                Id = Guid.NewGuid(),
                InternalUserId = admin.Id,
                PasswordHash = passwordHasher.Hash(seedOptions.Value.AdminPassword),
                MustChangePassword = false,
            };

            db.InternalUsers.Add(admin);
            await db.SaveChangesAsync(ct);
            logger.LogInformation("Seeded default admin user {Email}", adminEmail);
        }
    }
}
