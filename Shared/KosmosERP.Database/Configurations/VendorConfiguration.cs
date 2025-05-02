using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using KosmosERP.Database.Models;

namespace KosmosERP.Database.Configurations;

public class VendorConfiguration : BaseConfiguration<Vendor>
{
    public override void Configure(EntityTypeBuilder<Vendor> builder)
    {
        base.Configure(builder);

        builder.ToTable("vendors");
        builder.HasIndex(m => m.vendor_number);
        builder.HasIndex(m => m.guid);
        builder.Property(m => m.vendor_number).HasDefaultValue(10000).ValueGeneratedOnAdd();

        builder.HasMany<Product>(x => x.products).WithOne().HasForeignKey(x => x.vendor_id).HasPrincipalKey(c => c.id);
    }
}