using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using KosmosERP.Database.Models;

namespace KosmosERP.Database.Configurations;

public class ModuleConfiguration : BaseConfiguration<Module>
{
    public override void Configure(EntityTypeBuilder<Module> builder)
    {
        base.Configure(builder);

        builder.ToTable("modules");

        builder.Property(e => e.module_id)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(e => e.module_name)
            .IsRequired();
    }
}
