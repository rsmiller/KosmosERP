using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using KosmosERP.Database.Models;

namespace KosmosERP.Database.Configurations;

public class SubscriptionConfiguration : BaseConfiguration<Subscription>
{
    public override void Configure(EntityTypeBuilder<Subscription> builder)
    {
        base.Configure(builder);

        builder.ToTable("subscriptions");
        builder.Property(m => m.subscription_number).HasDefaultValue(10000).ValueGeneratedOnAdd();

        builder.HasIndex(m => m.subscription_number);
        builder.HasIndex(m => m.guid);

        builder.HasOne<Customer>(x => x.customer).WithMany().HasForeignKey(x => x.customer_id).HasPrincipalKey(c => c.id);
        builder.HasOne<OrderHeader>(x => x.order).WithMany().HasForeignKey(x => x.order_header_id).HasPrincipalKey(c => c.id);
    }
}