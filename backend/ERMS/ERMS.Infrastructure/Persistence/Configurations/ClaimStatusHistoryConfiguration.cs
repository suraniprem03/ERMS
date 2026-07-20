using ERMS.Domain.Entities;
using ERMS.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERMS.Infrastructure.Persistence.Configurations;

public class ClaimStatusHistoryConfiguration : IEntityTypeConfiguration<ClaimStatusHistory>
{
    public void Configure(EntityTypeBuilder<ClaimStatusHistory> builder)
    {
        BaseEntityConfiguration.ConfigureBaseEntity(builder);

        builder.Property(history => history.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(history => history.Comment)
            .HasMaxLength(2000);
    }
}
