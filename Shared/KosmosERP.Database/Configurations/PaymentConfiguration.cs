using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using KosmosERP.Database.Models;

namespace KosmosERP.Database.Configurations;

public class PaymentConfiguration : BaseConfiguration<Payment>
{
    public override void Configure(EntityTypeBuilder<Payment> builder)
    {
        base.Configure(builder);

        builder.ToTable("payments");
        builder.Property(m => m.payment_number).HasDefaultValue(210000).ValueGeneratedOnAdd();

        builder.HasIndex(m => m.order_header_id);
        builder.HasIndex(m => m.guid);

        // A payment belongs to one order; an order can have many payments.
        // Restrict: payments are financial records and must not disappear with an order.
        builder.HasOne<OrderHeader>().WithMany().HasForeignKey(x => x.order_header_id).HasPrincipalKey(o => o.id)
            .OnDelete(DeleteBehavior.Restrict);
    }
}