using ERMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERMS.Infrastructure.Persistence.Configurations;

public class EmployeeAreaConfiguration : IEntityTypeConfiguration<EmployeeArea>
{
    public void Configure(EntityTypeBuilder<EmployeeArea> builder)
    {
        BaseEntityConfiguration.ConfigureBaseEntity(builder);

        builder.HasIndex(employeeArea => new { employeeArea.EmployeeId, employeeArea.AreaId })
            .IsUnique();
    }
}
