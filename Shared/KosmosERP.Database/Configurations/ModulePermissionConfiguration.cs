using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using KosmosERP.Database.Models;

namespace KosmosERP.Database.Configurations;

public class ModulePermissionConfiguration : IEntityTypeConfiguration<ModulePermission>
{
    public void Configure(EntityTypeBuilder<ModulePermission> builder)
    {
        builder.ToTable("module_permissions");
        builder.HasKey(x => x.id);

        builder.Property(x => x.id).ValueGeneratedOnAdd();
        builder.Property(x => x.module_id).IsRequired(required: true);
        builder.Property(x => x.permission_name).IsRequired(required: true);
        builder.Property(x => x.internal_permission_name).IsRequired(required: true);
        builder.Property(x => x.is_active).IsRequired(required: true);

        builder.Property(x => x.read).HasDefaultValue(false);
        builder.Property(x => x.write).HasDefaultValue(false);
        builder.Property(x => x.edit).HasDefaultValue(false);
        builder.Property(x => x.delete).HasDefaultValue(false);
        builder.Property(x => x.requires_admin).HasDefaultValue(false);
        builder.Property(x => x.requires_management).HasDefaultValue(false);
        builder.Property(x => x.requires_guest).HasDefaultValue(false);
        builder.Property(x => x.is_active).HasDefaultValue(true);

        builder.HasIndex(x => x.module_id);
        builder.HasIndex(x => x.internal_permission_name);
        builder.HasIndex(x => new { x.module_id, x.internal_permission_name });
    }
}
