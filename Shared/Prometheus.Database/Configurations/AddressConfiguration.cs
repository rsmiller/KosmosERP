using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Prometheus.Database.Models;

namespace Prometheus.Database.Configurations;

public class AddressConfiguration : BaseConfiguration<Address>
{
    public override void Configure(EntityTypeBuilder<Address> builder)
    {
        base.Configure(builder);

        builder.ToTable("addresses");
        builder.HasIndex(m => m.guid);
    }
}