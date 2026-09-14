using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using KosmosERP.Database.Models;

namespace KosmosERP.Database.Configurations;

public class DocumentUploadTagConfiguration : BaseConfiguration<DocumentUploadRevisionTag>
{
    public override void Configure(EntityTypeBuilder<DocumentUploadRevisionTag> builder)
    {
        base.Configure(builder);

        builder.ToTable("document_uploads_revisions_tag");

        builder.HasIndex(x => x.guid);
        builder.HasIndex(x => x.document_upload_revision_id);
        builder.HasIndex(x => x.document_upload_object_tag_id);
        builder.HasIndex(x => new { x.tag_name, x.tag_value });
        
    }
}
