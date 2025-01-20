using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Prometheus.Database.Models;

namespace Prometheus.Database.Configurations;

public class RolePermissionConfiguration : BaseConfiguration<RolePermission>
{
    public override void Configure(EntityTypeBuilder<RolePermission> builder)
    {
        base.Configure(builder);

        builder.ToTable("role_permissions");

        builder.HasOne<ModulePermission>(x => x.module_permission).WithMany().HasForeignKey(x => x.module_permission_id).HasPrincipalKey(c => c.id);
    }
}