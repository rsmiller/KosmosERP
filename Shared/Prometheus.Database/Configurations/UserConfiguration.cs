using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Prometheus.Database.Models;

namespace Prometheus.Database.Configurations;

public class UserConfiguration : BaseConfiguration<User>
{
    public override void Configure(EntityTypeBuilder<User> builder)
    {
        base.Configure(builder);

        builder.ToTable("users");
        builder.HasIndex(m => m.username);
        builder.HasIndex(m => m.guid);

        builder.HasMany<UserSessionState>(x => x.sessions).WithOne().HasForeignKey(x => x.user_id).HasPrincipalKey(c => c.id);
        builder.HasMany<UserRole>(x => x.roles).WithOne().HasForeignKey(x => x.user_id).HasPrincipalKey(c => c.id);
    }
}