using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Prometheus.Database.Models;

namespace Prometheus.Database.Configurations;

public class UserSessionStateConfiguration : IEntityTypeConfiguration<UserSessionState>
{
    public void Configure(EntityTypeBuilder<UserSessionState> builder)
    {
        builder.ToTable("user_sessions");
        builder.HasKey(x => x.id);

        builder.Property(x => x.id).ValueGeneratedOnAdd();

        builder.Property(x => x.created_on).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
    }
}
