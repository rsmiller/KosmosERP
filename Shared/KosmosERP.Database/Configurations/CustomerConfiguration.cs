using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using KosmosERP.Database.Models;

namespace KosmosERP.Database.Configurations;

public class CustomerConfiguration : BaseConfiguration<Customer>
{
    public override void Configure(EntityTypeBuilder<Customer> builder)
    {
        base.Configure(builder);

        builder.ToTable("customers");
        builder.HasIndex(m => m.customer_number);
        builder.HasIndex(m => m.guid);
        builder.Property(m => m.customer_number).HasDefaultValue(10000).ValueGeneratedOnAdd();

        builder.HasMany<CustomerAddress>(x => x.addresses).WithOne().HasForeignKey(x => x.customer_id).HasPrincipalKey(c => c.id);
    }
}