using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using KosmosERP.Database.Models;

namespace KosmosERP.Database.Configurations;

public class CustomerAddressConfiguration : BaseConfiguration<CustomerAddress>
{
    public override void Configure(EntityTypeBuilder<CustomerAddress> builder)
    {
        base.Configure(builder);

        builder.ToTable("customer_addresses");
        builder.HasIndex(m => m.guid);

        //builder.HasMany<Address>(x => x.addresses).WithOne().HasForeignKey(x => x.id).HasPrincipalKey(c => c.id);
    }
}