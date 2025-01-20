using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Prometheus.Database.Models;


namespace Prometheus.Database.Configurations;

public class ShipmentHeaderConfiguration : BaseConfiguration<ShipmentHeader>
{
    public override void Configure(EntityTypeBuilder<ShipmentHeader> builder)
    {
        base.Configure(builder);

        builder.ToTable("shipment_headers");
        builder.Property(x => x.shipment_number).HasDefaultValue(10000).ValueGeneratedOnAdd();

        builder.HasIndex(x => x.order_id);
        builder.HasIndex(x => x.shipment_number);
        builder.HasIndex(m => m.guid);
        builder.HasIndex(x => x.address_id);

        builder.HasIndex(x => new { x.order_id, x.is_canceled });
        builder.HasIndex(x => new { x.order_id, x.is_deleted });
        builder.HasIndex(x => new { x.order_id, x.is_complete });

        builder.HasMany<ShipmentLine>(x => x.shipment_lines).WithOne().HasForeignKey(x => x.shipment_header_id).HasPrincipalKey(c => c.id);
        builder.HasOne<Address>(x => x.address).WithMany().HasForeignKey(x => x.address_id).HasPrincipalKey(c => c.id);
    }
}
