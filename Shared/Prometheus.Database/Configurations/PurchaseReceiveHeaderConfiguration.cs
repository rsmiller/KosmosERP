using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Prometheus.Database.Models;

namespace Prometheus.Database.Configurations;

public class PurchaseReceiveHeaderConfiguration : BaseConfiguration<PurchaseOrderReceiveHeader>
{
    public override void Configure(EntityTypeBuilder<PurchaseOrderReceiveHeader> builder)
    {
        base.Configure(builder);

        builder.ToTable("purchase_order_receive_headers");

        builder.HasIndex(x => x.purchase_order_id);

        builder.HasIndex(m => m.guid);
        builder.HasIndex(m => m.purchase_order_id);

        builder.HasIndex(x => new { x.purchase_order_id, x.is_canceled });
        builder.HasIndex(x => new { x.purchase_order_id, x.is_deleted });
        builder.HasIndex(x => new { x.purchase_order_id, x.is_complete });

        builder.HasOne<PurchaseOrderHeader>(x => x.purchase_order).WithMany().HasForeignKey(x => x.purchase_order_id).HasPrincipalKey(c => c.id);
        builder.HasMany<PurchaseOrderReceiveLine>(x => x.purchase_order_receive_lines).WithOne().HasForeignKey(x => x.purchase_order_receive_header_id).HasPrincipalKey(c => c.id);
    }
}
