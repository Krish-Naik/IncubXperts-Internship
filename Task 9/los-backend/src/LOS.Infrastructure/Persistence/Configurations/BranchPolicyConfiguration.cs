using LOS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LOS.Infrastructure.Persistence.Configurations;

public class BranchPolicyConfiguration : IEntityTypeConfiguration<BranchPolicy>
{
    public void Configure(EntityTypeBuilder<BranchPolicy> builder)
    {
        builder.HasKey(p => p.Id);
        builder
            .HasOne(p => p.Branch)
            .WithOne(b => b.Policy)
            .HasForeignKey<BranchPolicy>(p => p.BranchId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Property(p => p.MinInterestRate).HasColumnType("decimal(5,2)");
        builder.Property(p => p.MaxInterestRate).HasColumnType("decimal(5,2)");
        builder.Property(p => p.HighValueThreshold).HasColumnType("decimal(18,2)");
    }
}
