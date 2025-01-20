using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Prometheus.Database.Models;

namespace Prometheus.Database.Configurations;

public class CountryConfiguration : BaseConfiguration<Country>
{
    public override void Configure(EntityTypeBuilder<Country> builder)
    {
        base.Configure(builder);

        builder.ToTable("countries");

        builder.HasIndex(x => x.iso3);
    }
}
