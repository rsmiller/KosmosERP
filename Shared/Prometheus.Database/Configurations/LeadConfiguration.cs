using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Prometheus.Database.Models;

namespace Prometheus.Database.Configurations;

public class LeadConfiguration : BaseConfiguration<Lead>
{
    public override void Configure(EntityTypeBuilder<Lead> builder)
    {
        base.Configure(builder);

        builder.ToTable("leads");

        builder.HasIndex(x => x.lead_stage);
        builder.HasIndex(x => x.owner_id);
        builder.HasIndex(x => new { x.owner_id, x.lead_stage, x.is_deleted });
    }
}
