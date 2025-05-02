using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using KosmosERP.Database.Models;

namespace KosmosERP.Database.Configurations;

public class ARInvoiceHeaderConfiguration : BaseConfiguration<ARInvoiceHeader>
{
    public override void Configure(EntityTypeBuilder<ARInvoiceHeader> builder)
    {
        base.Configure(builder);

        builder.ToTable("ar_invoice_headers");
        builder.Property(x => x.invoice_number).HasDefaultValue(10000).ValueGeneratedOnAdd();

        builder.HasIndex(x => x.customer_id);
        builder.HasIndex(x => x.order_header_id);

        builder.HasMany<ARInvoiceLine>(x => x.ar_invoice_lines).WithOne().HasForeignKey(x => x.ar_invoice_header_id).HasPrincipalKey(c => c.id);
    }
}
