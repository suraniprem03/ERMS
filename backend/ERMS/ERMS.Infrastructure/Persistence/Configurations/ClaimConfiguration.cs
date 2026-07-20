using ERMS.Domain.Entities;
using ERMS.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERMS.Infrastructure.Persistence.Configurations;

public class ClaimConfiguration : IEntityTypeConfiguration<Claim>
{
    public void Configure(EntityTypeBuilder<Claim> builder)
    {
        BaseEntityConfiguration.ConfigureBaseEntity(builder);

        builder.Property(claim => claim.ClaimNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(claim => claim.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(claim => claim.AdminRemarks)
            .HasMaxLength(2000);

        builder.HasIndex(claim => claim.ClaimNumber)
            .IsUnique();

        builder.HasMany(claim => claim.Attachments)
            .WithOne(attachment => attachment.Claim)
            .HasForeignKey(attachment => attachment.ClaimId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(claim => claim.StatusHistory)
            .WithOne(history => history.Claim)
            .HasForeignKey(history => history.ClaimId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(claim => claim.TravelClaim)
            .WithOne(travelClaim => travelClaim.Claim)
            .HasForeignKey<TravelClaim>(travelClaim => travelClaim.ClaimId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(claim => claim.FoodClaim)
            .WithOne(foodClaim => foodClaim.Claim)
            .HasForeignKey<FoodClaim>(foodClaim => foodClaim.ClaimId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(claim => claim.HotelClaim)
            .WithOne(hotelClaim => hotelClaim.Claim)
            .HasForeignKey<HotelClaim>(hotelClaim => hotelClaim.ClaimId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(claim => claim.RechargeClaim)
            .WithOne(rechargeClaim => rechargeClaim.Claim)
            .HasForeignKey<RechargeClaim>(rechargeClaim => rechargeClaim.ClaimId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
