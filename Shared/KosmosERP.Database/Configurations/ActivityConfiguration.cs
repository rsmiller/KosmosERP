using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using KosmosERP.Database.Models;

namespace KosmosERP.Database.Configurations;

public class ActivityConfiguration : BaseConfiguration<Activity>
{
    public override void Configure(EntityTypeBuilder<Activity> builder)
    {
        base.Configure(builder);

        builder.ToTable("activities");
        builder.HasIndex(m => m.guid);
        builder.Property(x => x.subject).IsRequired().HasMaxLength(200);
        builder.Property(x => x.activity_type).IsRequired().HasMaxLength(50);
        builder.Property(x => x.status).IsRequired().HasMaxLength(50);
        builder.Property(x => x.priority).IsRequired();
        
        builder.HasIndex(x => x.owner_id);
        builder.HasIndex(x => x.start_date);
        builder.HasIndex(x => x.customer_id);
        builder.HasIndex(x => x.opportunity_id);
        builder.HasIndex(x => x.activity_type);
        builder.HasIndex(x => x.status);
        
    }
}