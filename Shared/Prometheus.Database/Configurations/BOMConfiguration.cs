using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Prometheus.Database.Models;

namespace Prometheus.Database.Configurations;

public class BOMConfiguration : BaseConfiguration<BOM>
{
    public override void Configure(EntityTypeBuilder<BOM> builder)
    {
        base.Configure(builder);

        builder.ToTable("boms");
        builder.HasIndex(m => m.parent_product_id);
        builder.HasIndex(m => m.guid);

        builder.HasOne<Product>(x => x.parent_product).WithMany().HasForeignKey(x => x.parent_product).HasPrincipalKey(c => c.id);
    }
}