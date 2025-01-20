using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Prometheus.Database.Models;

namespace Prometheus.Database.Configurations;

public class APInvoiceLineConfiguration : BaseConfiguration<APInvoiceLine>
{
    public override void Configure(EntityTypeBuilder<APInvoiceLine> builder)
    {
        base.Configure(builder);

        builder.ToTable("ap_invoice_lines");

        builder.HasIndex(x => x.ap_invoice_header_id);
        builder.HasIndex(x => x.gl_account_id);
        builder.HasIndex(x => new { x.association_object_id, x.association_object_line_id });
        
    }
}
