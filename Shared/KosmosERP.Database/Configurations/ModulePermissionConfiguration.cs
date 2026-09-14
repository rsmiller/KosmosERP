using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using KosmosERP.Database.Models;

namespace KosmosERP.Database.Configurations;

public class ModulePermissionConfiguration : BaseConfiguration<ModulePermission>
{
    public override void Configure(EntityTypeBuilder<ModulePermission> builder)
    {
        base.Configure(builder);

        builder.ToTable("module_permissions");

        builder.Property(e => e.module_id)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(e => e.module_name)
            .IsRequired();

        builder.Property(e => e.permission_name)
            .IsRequired();

        builder.Property(e => e.internal_permission_name)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(e => e.read)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(e => e.write)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(e => e.edit)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(e => e.delete)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(e => e.requires_admin)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(e => e.requires_management)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(e => e.requires_guest)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(e => e.is_active)
            .IsRequired()
            .HasDefaultValue(true);

        // Indexes
        builder.HasIndex(e => e.module_id)
            .HasDatabaseName("IX_module_permissions_module_id");

        builder.HasIndex(e => e.internal_permission_name)
            .HasDatabaseName("IX_module_permissions_internal_permission_name");

        builder.HasIndex(e => new { e.module_id, e.internal_permission_name })
            .HasDatabaseName("IX_module_permissions_module_id_internal_permission_name");
    }
}
