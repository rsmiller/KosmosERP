using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using KosmosERP.Database.Models;

namespace KosmosERP.Database.Configurations;

public class AddressConfiguration : BaseConfiguration<Address>
{
    public override void Configure(EntityTypeBuilder<Address> builder)
    {
        base.Configure(builder);

        builder.ToTable("addresses");
        builder.HasIndex(m => m.guid);

        
    }
}