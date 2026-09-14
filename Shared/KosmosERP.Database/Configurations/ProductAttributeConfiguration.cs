using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using KosmosERP.Database.Models;

namespace KosmosERP.Database.Configurations;

public class ProductAttributeConfiguration : BaseConfiguration<ProductAttribute>
{
    public override void Configure(EntityTypeBuilder<ProductAttribute> builder)
    {
        base.Configure(builder);

        builder.ToTable("product_attributes");

        builder.HasIndex(m => m.product_id);
        builder.HasIndex(m => m.attribute_name);

        //builder.HasOne<Product>(x => x.product).WithMany().HasForeignKey(x => x.product_id).HasPrincipalKey(c => c.id);
    }
}