using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using KosmosERP.Database.Models;

namespace KosmosERP.Database.Configurations;

public class SettingsConfiguration : BaseConfiguration<Settings>
{
    public override void Configure(EntityTypeBuilder<Settings> builder)
    {
        base.Configure(builder);

        builder.ToTable("settings");
        builder.HasIndex(m => m.guid);
    }
}