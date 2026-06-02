using BankApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BankApi.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<BankAccount> BankAccounts => Set<BankAccount>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<BankAccount>();

        entity.ToTable("BankAccounts");
        entity.HasKey(x => x.Id);

        entity.Property(x => x.Id).ValueGeneratedNever();
        entity.Property(x => x.AccountNumber).HasMaxLength(30).IsRequired();
        entity.Property(x => x.HolderName).HasMaxLength(100).IsRequired();
        entity.Property(x => x.Email).HasMaxLength(150).IsRequired();
        entity.Property(x => x.Type).HasMaxLength(20).IsRequired();
        entity.Property(x => x.Balance).HasPrecision(18, 2);

        entity.HasIndex(x => x.AccountNumber).IsUnique();
        entity.HasIndex(x => x.Email).IsUnique();
    }
}