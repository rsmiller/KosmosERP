using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Prometheus.Database.Models;

namespace Prometheus.Database.Configurations;

public class ErrorLogConfiguration : IEntityTypeConfiguration<ErrorLog>
{
    public ErrorLogConfiguration()
    {
    }

    public void Configure(EntityTypeBuilder<ErrorLog> builder)
    {
        builder.ToTable("logs_error");
        builder.HasKey(x => x.id);

        builder.Property(x => x.id).ValueGeneratedOnAdd();
        builder.Property(x => x.error_severity).IsRequired(required: true);
        builder.Property(x => x.source).IsRequired(required: true);
        builder.Property(x => x.error_message).IsRequired(required: true);
        builder.Property(x => x.created_on).IsRequired(required: true);
        builder.Property(x => x.created_on).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
    }
}
