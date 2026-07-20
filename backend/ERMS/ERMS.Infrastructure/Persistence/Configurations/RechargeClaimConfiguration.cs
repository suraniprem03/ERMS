using ERMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERMS.Infrastructure.Persistence.Configurations;

public class RechargeClaimConfiguration : IEntityTypeConfiguration<RechargeClaim>
{
    public void Configure(EntityTypeBuilder<RechargeClaim> builder)
    {
        BaseEntityConfiguration.ConfigureBaseEntity(builder);

        builder.Property(rechargeClaim => rechargeClaim.Amount)
            .HasPrecision(18, 2);

        builder.HasIndex(rechargeClaim => rechargeClaim.ClaimId)
            .IsUnique();
    }
}
