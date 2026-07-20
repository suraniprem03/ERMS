using ERMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERMS.Infrastructure.Persistence.Configurations;

public class ExpenseCategoryConfiguration : IEntityTypeConfiguration<ExpenseCategory>
{
    public void Configure(EntityTypeBuilder<ExpenseCategory> builder)
    {
        BaseEntityConfiguration.ConfigureBaseEntity(builder);

        builder.Property(category => category.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(category => category.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(category => category.DisplayOrder)
            .IsRequired();

        builder.Property(category => category.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.HasIndex(category => category.Code)
            .IsUnique();

        builder.HasMany(category => category.Claims)
            .WithOne(claim => claim.ExpenseCategory)
            .HasForeignKey(claim => claim.ExpenseCategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
