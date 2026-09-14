using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using KosmosERP.Database.Models;

namespace KosmosERP.Database.Configurations;

public class CreditMemoLineConfiguration : BaseConfiguration<CreditMemoLine>
{
    public override void Configure(EntityTypeBuilder<CreditMemoLine> builder)
    {
        base.Configure(builder);

        builder.ToTable("credit_memo_lines");
        builder.HasIndex(m => m.credit_memo_header_id);
        builder.HasIndex(m => m.product_id);
        builder.HasIndex(m => m.order_line_id);
        builder.HasIndex(m => m.ar_invoice_line_id);
        builder.HasIndex(m => m.guid);
    }
}