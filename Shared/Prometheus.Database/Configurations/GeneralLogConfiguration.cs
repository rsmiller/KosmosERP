using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Prometheus.Database.Models;

namespace Prometheus.Database.Configurations;

public class GeneralLogConfiguration : IEntityTypeConfiguration<GeneralLog>
{
    public void Configure(EntityTypeBuilder<GeneralLog> builder)
    {
        builder.ToTable("logs_general");
        builder.HasKey(x => x.id);

        builder.Property(x => x.id).ValueGeneratedOnAdd();
        builder.Property(x => x.category).IsRequired(required: true);
        builder.Property(x => x.message).IsRequired(required: true);
        builder.Property(x => x.created_on).IsRequired(required: true);

        builder.Property(x => x.created_on).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
    }
}
