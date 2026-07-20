using ERMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERMS.Infrastructure.Persistence.Configurations;

public class TravelClaimConfiguration : IEntityTypeConfiguration<TravelClaim>
{
    public void Configure(EntityTypeBuilder<TravelClaim> builder)
    {
        BaseEntityConfiguration.ConfigureBaseEntity(builder);

        builder.Property(travelClaim => travelClaim.Day)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(travelClaim => travelClaim.FromLocation)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(travelClaim => travelClaim.ToLocation)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(travelClaim => travelClaim.TotalKm)
            .HasPrecision(10, 2);

        builder.Property(travelClaim => travelClaim.RatePerKm)
            .HasPrecision(10, 2);

        builder.Property(travelClaim => travelClaim.CalculatedAmount)
            .HasPrecision(18, 2);

        builder.HasIndex(travelClaim => travelClaim.ClaimId)
            .IsUnique();
    }
}
