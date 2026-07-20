using ERMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERMS.Infrastructure.Persistence.Configurations;

public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        BaseEntityConfiguration.ConfigureBaseEntity(builder);

        builder.Property(employee => employee.EmployeeCode)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(employee => employee.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(employee => employee.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(employee => employee.MobileNumber)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(employee => employee.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.HasIndex(employee => employee.EmployeeCode)
            .IsUnique();

        builder.HasIndex(employee => employee.Email)
            .IsUnique();

        builder.HasOne(employee => employee.User)
            .WithOne(user => user.Employee)
            .HasForeignKey<User>(user => user.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(employee => employee.EmployeeAreas)
            .WithOne(employeeArea => employeeArea.Employee)
            .HasForeignKey(employeeArea => employeeArea.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(employee => employee.Claims)
            .WithOne(claim => claim.Employee)
            .HasForeignKey(claim => claim.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
