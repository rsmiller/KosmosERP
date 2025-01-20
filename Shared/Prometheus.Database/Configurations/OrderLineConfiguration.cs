using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Prometheus.Database.Models;

namespace Prometheus.Database.Configurations;

public class OrderLineConfiguration : BaseConfiguration<OrderLine>
{
    public override void Configure(EntityTypeBuilder<OrderLine> builder)
    {
        base.Configure(builder);

        builder.ToTable("order_lines");
        builder.HasIndex(m => m.order_id);
        builder.HasIndex(m => m.product_id);
        builder.HasIndex(m => m.guid);

        builder.HasMany<OrderLineAttribute>(x => x.attributes).WithOne().HasForeignKey(x => x.order_line_id).HasPrincipalKey(c => c.id);
    }
}