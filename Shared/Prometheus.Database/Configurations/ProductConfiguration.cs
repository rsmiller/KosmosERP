using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Prometheus.Database.Models;

namespace Prometheus.Database.Configurations;

public class ProductConfiguration : BaseConfiguration<Product>
{
    public override void Configure(EntityTypeBuilder<Product> builder)
    {
        base.Configure(builder);

        builder.ToTable("products");

        builder.HasIndex(m => m.vendor_id);
        builder.HasIndex(m => m.identifier1);
        builder.HasIndex(m => m.identifier2);
        builder.HasIndex(m => m.identifier3);
        builder.HasIndex(m => m.guid);

        builder.HasMany<ProductAttribute>(x => x.attributes).WithOne().HasForeignKey(x => x.product_id).HasPrincipalKey(c => c.id);
    }
}