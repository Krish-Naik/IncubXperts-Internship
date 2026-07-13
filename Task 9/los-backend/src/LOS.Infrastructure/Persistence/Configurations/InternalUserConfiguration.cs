using LOS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LOS.Infrastructure.Persistence.Configurations;

public class InternalUserConfiguration : IEntityTypeConfiguration<InternalUser>
{
    public void Configure(EntityTypeBuilder<InternalUser> builder)
    {
        builder.ToTable("InternalUsers");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.EmployeeId).HasMaxLength(50).IsRequired();
        builder.Property(x => x.FullName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Email).HasMaxLength(256).IsRequired();
        builder.Property(x => x.Phone).HasMaxLength(30);
        builder.HasIndex(x => x.Email).IsUnique();
        builder.HasIndex(x => x.EmployeeId).IsUnique();
        builder.HasOne(x => x.Role).WithMany(x => x.Users).HasForeignKey(x => x.RoleId);
        builder.HasOne(x => x.Branch).WithMany(x => x.Users).HasForeignKey(x => x.BranchId);
        builder
            .HasOne(x => x.ReferredByBroker)
            .WithMany()
            .HasForeignKey(x => x.ReferredByBrokerId)
            .OnDelete(DeleteBehavior.SetNull);
        builder
            .HasOne(x => x.Auth)
            .WithOne(x => x.InternalUser)
            .HasForeignKey<AuthUser>(x => x.InternalUserId);
    }
}
