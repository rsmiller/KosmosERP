using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Prometheus.Database.Models;

namespace Prometheus.Database.Configurations;

public class RoleConfiguration : BaseConfiguration<Role>
{
    public override void Configure(EntityTypeBuilder<Role> builder)
    {
        base.Configure(builder);

        builder.ToTable("roles");

        builder.HasMany<RolePermission>(x => x.role_permissions).WithOne().HasForeignKey(x => x.role_id).HasPrincipalKey(c => c.id);
    }
}