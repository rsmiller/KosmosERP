using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Prometheus.Database.Models;

namespace Prometheus.Database.Configurations;

public class NotificationConfiguration : BaseConfiguration<Notification>
{

    public override void Configure(EntityTypeBuilder<Notification> builder)
    {
        base.Configure(builder);

        builder.ToTable("notifications");
        builder.HasKey(x => x.id);

        builder.HasIndex(x => x.object_id);
        builder.HasIndex(x => x.user_id);
    }
}
