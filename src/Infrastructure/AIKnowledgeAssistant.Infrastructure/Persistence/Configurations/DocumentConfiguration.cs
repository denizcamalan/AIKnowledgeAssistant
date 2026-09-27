using AIKnowledgeAssistant.Domain.Documents;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AIKnowledgeAssistant.Infrastructure.Persistence.Configurations;

internal sealed class DocumentConfiguration : IEntityTypeConfiguration<Document>
{
    public void Configure(EntityTypeBuilder<Document> builder)
    {
        builder.ToTable("documents");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.OriginalFileName)
            .HasMaxLength(512)
            .IsRequired();

        builder.Property(d => d.DisplayName)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(d => d.ContentType)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(d => d.StoragePath)
            .HasMaxLength(1024)
            .IsRequired();

        builder.Property(d => d.Status)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.HasIndex(d => d.OriginalFileName)
            .IsUnique();

        builder.HasIndex(d => d.CreatedAtUtc);
    }
}
