using ERMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERMS.Infrastructure.Persistence.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        BaseEntityConfiguration.ConfigureBaseEntity(builder);

        builder.Property(role => role.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(role => role.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasIndex(role => role.Code)
            .IsUnique();

        builder.HasMany(role => role.Users)
            .WithOne(user => user.Role)
            .HasForeignKey(user => user.RoleId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
