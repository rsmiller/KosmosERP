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

        builder.Property(m => m.units_purchased).HasDefaultValue(0);
        builder.Property(m => m.units_received).HasDefaultValue(0);
        builder.Property(m => m.units_shipped).HasDefaultValue(0);
        builder.Property(m => m.units_sold).HasDefaultValue(0);
        builder.Property(m => m.purchased_unit_cost).HasDefaultValue(0);
        builder.Property(m => m.sold_unit_price).HasDefaultValue(0);

        builder.HasIndex(m => new { m.object_reference_id, m.transaction_type });
        builder.HasIndex(m => new { m.object_reference_id, m.transaction_type, m.product_id });
    
    }
}