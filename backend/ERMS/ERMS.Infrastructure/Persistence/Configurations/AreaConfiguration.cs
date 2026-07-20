using ERMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERMS.Infrastructure.Persistence.Configurations;

public class AreaConfiguration : IEntityTypeConfiguration<Area>
{
    public void Configure(EntityTypeBuilder<Area> builder)
    {
        BaseEntityConfiguration.ConfigureBaseEntity(builder);

        builder.Property(area => area.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(area => area.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(area => area.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.HasIndex(area => new { area.StateId, area.Code })
            .IsUnique();

        builder.HasMany(area => area.EmployeeAreas)
            .WithOne(employeeArea => employeeArea.Area)
            .HasForeignKey(employeeArea => employeeArea.AreaId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
