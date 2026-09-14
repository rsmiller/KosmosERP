using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using KosmosERP.Database.Models;

namespace KosmosERP.Database.Configurations;

public class MessageQueueConfiguration : BaseConfiguration<MessageQueue>
{
    public override void Configure(EntityTypeBuilder<MessageQueue> builder)
    {
        base.Configure(builder);

        builder.ToTable("message_queue");
        builder.HasIndex(m => m.guid);
        
        builder.HasIndex(x => new { x.read });
        builder.HasIndex(x => new { x.queue });
    }
}