using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Prometheus.Database.Models;

namespace Prometheus.Database.Configurations;

public class PurchaseOrderLineConfiguration : BaseConfiguration<PurchaseOrderLine>
{
    public override void Configure(EntityTypeBuilder<PurchaseOrderLine> builder)
    {
        base.Configure(builder);

        builder.ToTable("purchase_order_lines");

        builder.Property(x => x.revision_number).HasDefaultValue(1);

        builder.HasIndex(m => m.guid);
        builder.HasIndex(m => m.product_id);
        builder.HasIndex(m => m.purchase_order_header_id);

        builder.HasIndex(x => new { x.purchase_order_header_id, x.line_number });
        builder.HasIndex(x => new { x.purchase_order_header_id, x.revision_number });
        builder.HasIndex(x => new { x.purchase_order_header_id, x.revision_number, x.line_number });
        builder.HasIndex(x => new { x.purchase_order_header_id, x.is_canceled });
        builder.HasIndex(x => new { x.purchase_order_header_id, x.is_deleted });
        builder.HasIndex(x => new { x.purchase_order_header_id, x.is_complete });

        builder.HasOne<Product>(x => x.product).WithMany().HasForeignKey(x => x.product_id).HasPrincipalKey(c => c.id);
    }
}
