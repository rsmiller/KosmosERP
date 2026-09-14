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

        builder.HasMany<OrderHeader>(x => x.order_headers).WithOne().HasForeignKey(x => x.id).HasPrincipalKey(c => c.order_header_id);
    }
}