using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using KosmosERP.Database.Models;

namespace KosmosERP.Database.Configurations;

public class OpportunityLineConfiguration : BaseConfiguration<OpportunityLine>
{
    public override void Configure(EntityTypeBuilder<OpportunityLine> builder)
    {
        base.Configure(builder);

        builder.ToTable("opportunity_lines");
        builder.HasIndex(m => m.opportunity_id);
        builder.HasIndex(m => m.product_id);
        builder.HasIndex(m => m.guid);
    }
}