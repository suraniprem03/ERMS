using ERMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERMS.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        BaseEntityConfiguration.ConfigureBaseEntity(builder);

        builder.Property(user => user.PasswordHash)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(user => user.IsPasswordChangeRequired)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(user => user.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.HasIndex(user => user.EmployeeId)
            .IsUnique();
    }
}
