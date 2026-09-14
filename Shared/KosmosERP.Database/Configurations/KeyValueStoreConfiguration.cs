using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using KosmosERP.Database.Models;

namespace KosmosERP.Database.Configurations;

public class KeyValueStoreConfiguration : BaseConfiguration<KeyValueStore>
{
    public override void Configure(EntityTypeBuilder<KeyValueStore> builder)
    {
        base.Configure(builder);

        builder.ToTable("key_value_stores");
        builder.HasIndex(x => x.key);
    }
}
