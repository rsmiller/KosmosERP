using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Prometheus.Database.Models;

namespace Prometheus.Database.Configurations;

public class ARInvoiceLineConfiguration : BaseConfiguration<ARInvoiceLine>
{
    public override void Configure(EntityTypeBuilder<ARInvoiceLine> builder)
    {
        base.Configure(builder);

        builder.ToTable("ar_invoice_lines");

        builder.HasIndex(x => x.ar_invoice_header_id);
        builder.HasIndex(x => x.order_line_id);
        builder.HasIndex(x => x.product_id);

        builder.HasIndex(x => new { x.ar_invoice_header_id, x.is_deleted });

        builder.HasOne<Product>(x => x.product).WithMany().HasForeignKey(x => x.product_id).HasPrincipalKey(c => c.id);
        builder.HasOne<OrderLine>(x => x.order_line).WithMany().HasForeignKey(x => x.order_line_id).HasPrincipalKey(c => c.id);
    }
}
