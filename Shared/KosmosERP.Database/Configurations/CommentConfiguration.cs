using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using KosmosERP.Database.Models;

namespace KosmosERP.Database.Configurations;

public class CommentConfiguration : BaseConfiguration<Comment>
{
    public override void Configure(EntityTypeBuilder<Comment> builder)
    {
        base.Configure(builder);

        builder.ToTable("comments");
        builder.HasKey(x => x.id);

        builder.HasIndex(x => x.object_guid);
    }
}
