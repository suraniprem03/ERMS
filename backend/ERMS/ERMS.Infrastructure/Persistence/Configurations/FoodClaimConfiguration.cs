using ERMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERMS.Infrastructure.Persistence.Configurations;

public class FoodClaimConfiguration : IEntityTypeConfiguration<FoodClaim>
{
    public void Configure(EntityTypeBuilder<FoodClaim> builder)
    {
        BaseEntityConfiguration.ConfigureBaseEntity(builder);

        builder.Property(foodClaim => foodClaim.Amount)
            .HasPrecision(18, 2);

        builder.Property(foodClaim => foodClaim.FoodDate)
            .IsRequired(false);

        builder.HasIndex(foodClaim => foodClaim.ClaimId)
            .IsUnique();
    }
}
