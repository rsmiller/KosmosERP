using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Prometheus.Database.Models;

namespace Prometheus.Database.Configurations;

public class DocumentUploadObjectConfiguration : IEntityTypeConfiguration<DocumentUploadObject>
{
    public void Configure(EntityTypeBuilder<DocumentUploadObject> builder)
    {
        builder.ToTable("document_uploads_object");
        builder.Property(x => x.id).ValueGeneratedOnAdd();

        builder.HasIndex(x => new { x.internal_name });
        builder.HasIndex(x => new { x.guid });

        builder.HasMany<DocumentUploadObjectTagTemplate>(x => x.object_tags).WithOne().HasForeignKey(x => x.document_object_id).HasPrincipalKey(c => c.id);
    }
}
