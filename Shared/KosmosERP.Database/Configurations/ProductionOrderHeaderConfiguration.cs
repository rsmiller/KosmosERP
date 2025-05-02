using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using KosmosERP.Database.Models;

namespace KosmosERP.Database.Configurations;

public class ProductionOrderHeaderConfiguration : BaseConfiguration<ProductionOrderHeader>
{
    public override void Configure(EntityTypeBuilder<ProductionOrderHeader> builder)
    {
        base.Configure(builder);

        builder.ToTable("production_order_headers");

        builder.HasIndex(x => x.order_header_id);
        builder.HasIndex(x => x.status_id);

        builder.HasOne<OrderHeader>(x => x.order_header).WithMany().HasForeignKey(x => x.order_header_id).HasPrincipalKey(c => c.id);
        builder.HasMany<ProductionOrderLine>(x => x.production_order_lines).WithOne().HasForeignKey(x => x.production_order_header_id).HasPrincipalKey(c => c.id);
    }
}
