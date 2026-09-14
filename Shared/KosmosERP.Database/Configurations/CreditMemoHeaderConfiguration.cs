using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using KosmosERP.Database.Models;

namespace KosmosERP.Database.Configurations;

public class CreditMemoHeaderConfiguration : BaseConfiguration<CreditMemoHeader>
{
    public override void Configure(EntityTypeBuilder<CreditMemoHeader> builder)
    {
        base.Configure(builder);

        builder.ToTable("credit_memo_headers");
        builder.HasIndex(m => m.customer_id);
        builder.HasIndex(m => m.ar_invoice_header_id);
        builder.HasIndex(m => m.order_header_id);
        builder.HasIndex(m => m.guid);

        builder.HasOne<Customer>(x => x.customer).WithMany().HasForeignKey(x => x.customer_id).HasPrincipalKey(c => c.id);
        builder.HasMany<CreditMemoLine>(x => x.credit_memo_lines).WithOne().HasForeignKey(x => x.credit_memo_header_id).HasPrincipalKey(c => c.id);
    }
}