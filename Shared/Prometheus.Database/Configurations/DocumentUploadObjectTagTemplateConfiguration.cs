using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Prometheus.Database.Models;

namespace Prometheus.Database.Configurations;

public class DocumentUploadObjectTagTemplateConfiguration : IEntityTypeConfiguration<DocumentUploadObjectTagTemplate>
{
    public void Configure(EntityTypeBuilder<DocumentUploadObjectTagTemplate> builder)
    {
        builder.ToTable("document_uploads_object_tag_template");
        builder.Property(x => x.id).ValueGeneratedOnAdd();

        builder.HasIndex(x => new { x.document_object_id });
        builder.HasIndex(x => new { x.name });
    }
}
