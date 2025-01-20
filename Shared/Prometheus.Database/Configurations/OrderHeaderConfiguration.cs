using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Prometheus.Database.Models;

namespace Prometheus.Database.Configurations;

public class OrderHeaderConfiguration : BaseConfiguration<OrderHeader>
{
    public override void Configure(EntityTypeBuilder<OrderHeader> builder)
    {
        base.Configure(builder);

        builder.ToTable("order_headers");
        builder.Property(m => m.order_number).HasDefaultValue(10000).ValueGeneratedOnAdd();

        builder.HasIndex(m => m.order_number);
        builder.HasIndex(m => m.guid);
        builder.HasIndex(x => new { x.order_number, x.order_type });
        builder.HasIndex(x => new { x.order_number, x.order_type, x.revision_number });
        builder.HasIndex(x => new { x.order_number, x.revision_number });

        builder.HasMany<OrderLine>(x => x.order_lines).WithOne().HasForeignKey(x => x.order_id).HasPrincipalKey(c => c.id);
        builder.HasOne<Address>(x => x.ship_to_address).WithMany().HasForeignKey(x => x.ship_to_address_id).HasPrincipalKey(c => c.id);
    }
}