using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using KosmosERP.Database.Models;

namespace KosmosERP.Database.Configurations;

public class PurchaseOrderHeaderConfiguration : BaseConfiguration<PurchaseOrderHeader>
{
    public override void Configure(EntityTypeBuilder<PurchaseOrderHeader> builder)
    {
        base.Configure(builder);

        builder.ToTable("purchase_order_headers");

        builder.Property(x => x.guid).IsRequired(required: true);
        builder.Property(x => x.revision_number).HasDefaultValue(1);
        builder.Property(x => x.po_number).HasDefaultValue(10000).ValueGeneratedOnAdd();

        builder.HasIndex(x => x.po_number);
        builder.HasIndex(x => x.vendor_id);
        builder.HasIndex(m => m.guid);

        builder.HasIndex(x => new { x.po_number, x.po_type });
        builder.HasIndex(x => new { x.po_number, x.po_type, x.revision_number });
        builder.HasIndex(x => new { x.po_number, x.revision_number });

        builder.HasOne<Vendor>(x => x.vendor).WithMany().HasForeignKey(x => x.vendor_id).HasPrincipalKey(c => c.id);
        builder.HasMany<PurchaseOrderLine>(x => x.purchase_order_lines).WithOne().HasForeignKey(x => x.purchase_order_header_id).HasPrincipalKey(c => c.id);
    }
}
