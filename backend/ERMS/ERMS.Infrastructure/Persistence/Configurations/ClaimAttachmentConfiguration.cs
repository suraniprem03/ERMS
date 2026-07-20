using ERMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERMS.Infrastructure.Persistence.Configurations;

public class ClaimAttachmentConfiguration : IEntityTypeConfiguration<ClaimAttachment>
{
    public void Configure(EntityTypeBuilder<ClaimAttachment> builder)
    {
        BaseEntityConfiguration.ConfigureBaseEntity(builder);

        builder.Property(attachment => attachment.FileName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(attachment => attachment.OriginalFileName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(attachment => attachment.FileExtension)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(attachment => attachment.ContentType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(attachment => attachment.FilePath)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(attachment => attachment.FileSize)
            .IsRequired();
    }
}
