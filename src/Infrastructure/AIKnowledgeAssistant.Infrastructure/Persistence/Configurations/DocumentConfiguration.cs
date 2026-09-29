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

        builder.OwnsOne(
            d => d.Ingestion,
            ingestion =>
            {
                ingestion.Property(i => i.AttemptCount)
                    .HasColumnName("ingestion_attempt_count")
                    .HasDefaultValue(0);

                ingestion.Property(i => i.FailureReason)
                    .HasColumnName("ingestion_failure_reason")
                    .HasMaxLength(2000);

                ingestion.Property(i => i.StartedAtUtc)
                    .HasColumnName("ingestion_started_at_utc");

                ingestion.Property(i => i.CompletedAtUtc)
                    .HasColumnName("ingestion_completed_at_utc");
            });

        builder.HasIndex(d => d.OriginalFileName)
            .IsUnique();

        builder.HasIndex(d => d.CreatedAtUtc);
    }
}
