using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Prometheus.Database.Models;

namespace Prometheus.Database.Configurations;

public class ShipmentLineConfiguration : BaseConfiguration<ShipmentLine>
{
    public override void Configure(EntityTypeBuilder<ShipmentLine> builder)
    {
        base.Configure(builder);

        builder.ToTable("shipment_lines");

        builder.HasIndex(x => x.shipment_header_id);
        builder.HasIndex(x => x.order_line_id);
        builder.HasIndex(x => x.guid);

        builder.HasIndex(x => new { x.order_line_id, x.is_canceled });
        builder.HasIndex(x => new { x.order_line_id, x.is_deleted });
        builder.HasIndex(x => new { x.order_line_id, x.is_complete });

        builder.HasOne<OrderLine>(x => x.order_line).WithMany().HasForeignKey(x => x.order_line_id).HasPrincipalKey(c => c.id);
    }
}
