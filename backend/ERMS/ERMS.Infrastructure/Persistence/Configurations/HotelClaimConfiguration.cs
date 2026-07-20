using ERMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERMS.Infrastructure.Persistence.Configurations;

public class HotelClaimConfiguration : IEntityTypeConfiguration<HotelClaim>
{
    public void Configure(EntityTypeBuilder<HotelClaim> builder)
    {
        BaseEntityConfiguration.ConfigureBaseEntity(builder);

        builder.Property(hotelClaim => hotelClaim.Amount)
            .HasPrecision(18, 2);

        builder.HasIndex(hotelClaim => hotelClaim.ClaimId)
            .IsUnique();
    }
}
