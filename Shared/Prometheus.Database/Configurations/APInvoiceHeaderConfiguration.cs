using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Prometheus.Database.Models;

namespace Prometheus.Database.Configurations;

public class APInvoiceHeaderConfiguration : BaseConfiguration<APInvoiceHeader>
{
    public override void Configure(EntityTypeBuilder<APInvoiceHeader> builder)
    {
        base.Configure(builder);

        builder.ToTable("ap_invoice_headers");

        builder.HasIndex(x => x.vendor_id);
        builder.HasIndex(x => x.invoice_number);

        builder.HasIndex(x => new { x.vendor_id, x.is_paid });
        builder.HasIndex(x => new { x.vendor_id, x.is_deleted });

        builder.HasMany<APInvoiceLine>(x => x.ap_invoice_lines).WithOne().HasForeignKey(x => x.ap_invoice_header_id).HasPrincipalKey(c => c.id);
    }
}
