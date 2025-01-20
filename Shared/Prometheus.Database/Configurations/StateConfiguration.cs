using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Prometheus.Database.Models;

namespace Prometheus.Database.Configurations;

public class StateConfiguration : BaseConfiguration<State>
{
    public override void Configure(EntityTypeBuilder<State> builder)
    {
        base.Configure(builder);

        builder.ToTable("states");

        builder.HasIndex(x => x.country_id);
    }
}
