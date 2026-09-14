using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using KosmosERP.Database.Models;

namespace KosmosERP.Database.Configurations;

public class PurchaseReceiveUploadConfiguration : BaseConfiguration<PurchaseOrderReceiveUpload>
{
    public override void Configure(EntityTypeBuilder<PurchaseOrderReceiveUpload> builder)
    {
        base.Configure(builder);

        builder.ToTable("purchase_order_receive_uploads");

        builder.HasIndex(x => x.purchase_order_receive_header_id);
        builder.HasIndex(x => x.document_upload_id);

        builder.HasOne<DocumentUpload>(x => x.document_upload).WithMany().HasForeignKey(x => x.document_upload_id).HasPrincipalKey(c => c.id);
    }
}
