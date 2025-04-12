using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Prometheus.Database.Models;

namespace Prometheus.Database.Configurations;

public class ProductionOrderLineConfiguration : BaseConfiguration<ProductionOrderLine>
{
    public override void Configure(EntityTypeBuilder<ProductionOrderLine> builder)
    {
        base.Configure(builder);

        builder.ToTable("production_order_liness");

        builder.HasIndex(x => x.order_line_id);
        builder.HasIndex(x => x.status_id);

        builder.HasOne<OrderLine>(x => x.order_line).WithMany().HasForeignKey(x => x.order_line_id).HasPrincipalKey(c => c.id);
      
    }
}
