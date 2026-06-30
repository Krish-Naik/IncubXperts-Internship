using LOS.Domain.Entities;
using LOS.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace LOS.Infrastructure.Persistence;

public class LOSDbContext(DbContextOptions<LOSDbContext> options) : DbContext(options)
{
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Branch> Branches => Set<Branch>();
    public DbSet<InternalUser> InternalUsers => Set<InternalUser>();
    public DbSet<AuthUser> AuthUsers => Set<AuthUser>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();
    public DbSet<PasswordHistory> PasswordHistory => Set<PasswordHistory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(LOSDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
