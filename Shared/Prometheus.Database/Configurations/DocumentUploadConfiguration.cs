using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Prometheus.Database.Models;

namespace Prometheus.Database.Configurations;

public class DocumentUploadConfiguration : BaseConfiguration<DocumentUpload>
{
    public override void Configure(EntityTypeBuilder<DocumentUpload> builder)
    {
        base.Configure(builder);

        builder.ToTable("document_uploads");
        builder.Property(x => x.id).ValueGeneratedOnAdd();

        builder.HasIndex(x => new { x.document_object_id });

        builder.HasMany<DocumentUploadRevision>(x => x.document_revisions).WithOne().HasForeignKey(x => x.document_upload_id).HasPrincipalKey(c => c.id); 
    }
}
