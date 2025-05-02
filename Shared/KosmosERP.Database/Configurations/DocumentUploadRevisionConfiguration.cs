using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using KosmosERP.Database.Models;

namespace KosmosERP.Database.Configurations;

public class DocumentUploadRevisionConfiguration : BaseConfiguration<DocumentUploadRevision>
{
    public override void Configure(EntityTypeBuilder<DocumentUploadRevision> builder)
    {
        base.Configure(builder);

        builder.ToTable("document_uploads_revisions");

        builder.HasIndex(x => new { x.document_upload_id });
        builder.HasIndex(x => new { x.document_name });
        builder.HasIndex(x => new { x.guid });

        builder.HasMany<DocumentUploadRevisionTag>(x => x.revision_tags).WithOne().HasForeignKey(x => x.document_upload_revision_id).HasPrincipalKey(c => c.id);
    }
}
