using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using KosmosERP.Database.Models;

namespace KosmosERP.Database.Configurations;

public class SubscriptionEntryConfiguration : BaseConfiguration<SubscriptionEntry>
{
    public override void Configure(EntityTypeBuilder<SubscriptionEntry> builder)
    {
        base.Configure(builder);

        builder.ToTable("subscription_entries");
        builder.HasIndex(m => m.guid);

        builder.HasOne<OrderHeader>(x => x.order_header).WithMany().HasForeignKey(x => x.order_header_id).HasPrincipalKey(c => c.id);
        builder.HasOne<ARInvoiceHeader>(x => x.ar_invoice_header).WithMany().HasForeignKey(x => x.ar_invoice_header_id).HasPrincipalKey(c => c.id);
        builder.HasOne<Payment>(x => x.payment).WithMany().HasForeignKey(x => x.payment_id).HasPrincipalKey(c => c.id);
    }
}