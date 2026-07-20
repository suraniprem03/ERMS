using ERMS.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERMS.Infrastructure.Persistence.Configurations;

internal static class BaseEntityConfiguration
{
    public static void ConfigureBaseEntity<TEntity>(EntityTypeBuilder<TEntity> builder)
        where TEntity : BaseEntity
    {
        builder.HasKey(entity => entity.Id);

        builder.Property(entity => entity.Id)
            .ValueGeneratedOnAdd();

        builder.Property(entity => entity.CreatedAt)
            .IsRequired();

        builder.Property(entity => entity.CreatedBy)
            .IsRequired();

        builder.Property(entity => entity.UpdatedAt);

        builder.Property(entity => entity.UpdatedBy);
    }
}
