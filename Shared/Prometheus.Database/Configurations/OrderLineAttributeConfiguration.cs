using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Prometheus.Database.Models;

namespace Prometheus.Database.Configurations;

public class OrderLineAttributeConfiguration : BaseConfiguration<OrderLineAttribute>
{
    public override void Configure(EntityTypeBuilder<OrderLineAttribute> builder)
    {
        base.Configure(builder);

        builder.ToTable("order_line_attributes");
        builder.HasIndex(m => m.order_line_id);
    }
}