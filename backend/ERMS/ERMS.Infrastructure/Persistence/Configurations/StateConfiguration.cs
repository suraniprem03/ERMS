using ERMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERMS.Infrastructure.Persistence.Configurations;

public class StateConfiguration : IEntityTypeConfiguration<State>
{
    public void Configure(EntityTypeBuilder<State> builder)
    {
        BaseEntityConfiguration.ConfigureBaseEntity(builder);

        builder.Property(state => state.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(state => state.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(state => state.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.HasIndex(state => state.Code)
            .IsUnique();

        builder.HasMany(state => state.Areas)
            .WithOne(area => area.State)
            .HasForeignKey(area => area.StateId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
