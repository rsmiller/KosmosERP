using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using KosmosERP.Database.Models;

namespace KosmosERP.Database.Configurations;

public class TransactionConfiguration : BaseConfiguration<Transaction>
{
    public override void Configure(EntityTypeBuilder<Transaction> builder)
    {
        base.Configure(builder);

        builder.ToTable("transactions");
        builder.HasIndex(m => m.product_id);
        builder.HasIndex(m => m.guid);
        builder.HasIndex(m => m.object_reference_id);

        builder.HasIndex(m => new { m.object_reference_id, m.transaction_type });
        builder.HasIndex(m => new { m.object_reference_id, m.transaction_type, m.product_id });
    }
}