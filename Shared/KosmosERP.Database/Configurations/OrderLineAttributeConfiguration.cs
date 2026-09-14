using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using KosmosERP.Database.Models;

namespace KosmosERP.Database.Configurations;

public class OrderLineAttributeConfiguration : BaseConfiguration<OrderLineAttribute>
{
    public override void Configure(EntityTypeBuilder<OrderLineAttribute> builder)
    {
        base.Configure(builder);

        builder.ToTable("order_line_attributes");
        builder.HasIndex(m => m.order_line_id);
    }
}