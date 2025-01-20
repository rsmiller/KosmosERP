using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Prometheus.Database;

public class BaseConfiguration<T> : IEntityTypeConfiguration<T>
    where T : BaseDatabaseModel
{
    public virtual void Configure(EntityTypeBuilder<T> builder)
    {
        builder.HasKey(x => x.id);

        builder.Property(x => x.id).ValueGeneratedOnAdd();
        builder.Property(x => x.created_on).IsRequired(required: true);
        builder.Property(x => x.created_on).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
        builder.Property(x => x.updated_by).IsRequired(required: true);
        builder.Property(x => x.updated_on).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
    }
}
