using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Prometheus.Database.Models;

namespace Prometheus.Database.Configurations;

public class OpportunityConfiguration : BaseConfiguration<Opportunity>
{
    public override void Configure(EntityTypeBuilder<Opportunity> builder)
    {
        base.Configure(builder);

        builder.ToTable("opportunity");

        builder.HasIndex(x => x.stage);
        builder.HasIndex(x => x.owner_id);
        builder.HasIndex(x => x.customer_id);
        builder.HasIndex(x => new { x.owner_id, x.is_deleted });
        builder.HasIndex(x => new { x.customer_id, x.stage });

        builder.HasOne<Contact>(x => x.contact).WithMany().HasForeignKey(x => x.contact_id).HasPrincipalKey(c => c.id);
        builder.HasOne<Customer>(x => x.customer).WithMany().HasForeignKey(x => x.customer_id).HasPrincipalKey(c => c.id);
        builder.HasMany<OpportunityLine>(x => x.opportunity_lines).WithOne().HasForeignKey(x => x.opportunity_id).HasPrincipalKey(c => c.id);
    }
}
