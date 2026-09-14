using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using KosmosERP.Database.Models;

namespace KosmosERP.Database.Configurations;

public class InventoryConfiguration : IEntityTypeConfiguration<Inventory>
{
    public void Configure(EntityTypeBuilder<Inventory> builder)
    {
        builder.ToTable("inventory_counts");

        builder.HasIndex(x => x.id);
        builder.HasKey(x => x.id);

        builder.Property(x => x.id).ValueGeneratedOnAdd();
        builder.Property(x => x.created_on).IsRequired(required: true);
        builder.Property(x => x.created_on).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
        builder.Property(x => x.updated_on).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
        builder.Property(x => x.current_stock).HasDefaultValue(0);
        builder.Property(x => x.reorder_level).HasDefaultValue(0);
        builder.Property(x => x.on_order).HasDefaultValue(0);
        builder.Property(x => x.reserved).HasDefaultValue(0);
        builder.Property(x => x.on_hand).HasDefaultValue(0);
        builder.Property(x => x.on_order).HasDefaultValue(0);
        builder.Property(x => x.to_order).HasDefaultValue(0);

        builder.Property(x => x.total_units_sold).HasDefaultValue(0);
        builder.Property(x => x.total_units_received).HasDefaultValue(0);
        builder.Property(x => x.total_units_shipped).HasDefaultValue(0);
        builder.Property(x => x.total_on_purchased).HasDefaultValue(0);
    }
}
