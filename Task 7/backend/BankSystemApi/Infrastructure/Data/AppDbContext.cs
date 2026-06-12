using BankApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BankApi.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    public DbSet<BankAccount> BankAccounts => Set<BankAccount>();
    public DbSet<AppUser> AppUsers => Set<AppUser>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<AccountTransaction> AccountTransactions => Set<AccountTransaction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<BankAccount>(entity =>
        {
            entity.ToTable("BankAccounts");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).ValueGeneratedNever();
            entity.Property(x => x.AccountNumber).HasMaxLength(30).IsRequired();
            entity.Property(x => x.HolderName).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Email).HasMaxLength(150).IsRequired();
            entity.Property(x => x.Type).HasMaxLength(20).IsRequired();
            entity.Property(x => x.Balance).HasPrecision(18, 2);
            entity.HasIndex(x => x.AccountNumber).IsUnique();
            entity.HasIndex(x => x.Email);
            entity
                .HasOne(x => x.AppUser)
                .WithMany(x => x.BankAccounts)
                .HasForeignKey(x => x.AppUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<AccountTransaction>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Type).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(500);
            entity.Property(x => x.PerformedByRole).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Amount).HasPrecision(18, 2);
            entity.Property(x => x.BalanceAfter).HasPrecision(18, 2);

            entity
                .HasOne(x => x.BankAccount)
                .WithMany()
                .HasForeignKey(x => x.BankAccountId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(x => new { x.BankAccountId, x.CreatedAtUtc });
        });

        modelBuilder.Entity<AppUser>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Email).HasMaxLength(256);
            entity.Property(x => x.DisplayName).HasMaxLength(200);
            entity.Property(x => x.EntraObjectId).HasMaxLength(100);
            entity.Property(x => x.TenantId).HasMaxLength(100);
            entity.Property(x => x.Role).HasMaxLength(50);
            entity.HasIndex(x => new { x.EntraObjectId, x.TenantId }).IsUnique();
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.TokenHash).HasMaxLength(200);
            entity.HasIndex(x => x.TokenHash).IsUnique();
            entity
                .HasOne(x => x.AppUser)
                .WithMany(x => x.RefreshTokens)
                .HasForeignKey(x => x.AppUserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
