using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using KosmosERP.Database.Models;

namespace KosmosERP.Database.Configurations;

public class PurchaseReceiveLineConfiguration : BaseConfiguration<PurchaseOrderReceiveLine>
{
    public override void Configure(EntityTypeBuilder<PurchaseOrderReceiveLine> builder)
    {
        base.Configure(builder);

        builder.ToTable("purchase_order_receive_lines");

        builder.HasIndex(x => x.purchase_order_receive_header_id);
        builder.HasIndex(x => x.purchase_order_line_id);
        builder.HasIndex(m => m.guid);

        builder.HasIndex(x => new { x.purchase_order_receive_header_id, x.is_complete });
        builder.HasIndex(x => new { x.purchase_order_receive_header_id, x.is_canceled });

        builder.HasOne<PurchaseOrderLine>(x => x.purchase_order_line).WithMany().HasForeignKey(x => x.purchase_order_line_id).HasPrincipalKey(c => c.id);
    }
}
