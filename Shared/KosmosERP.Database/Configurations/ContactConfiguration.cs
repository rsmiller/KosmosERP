using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using KosmosERP.Database.Models;

namespace KosmosERP.Database.Configurations;

public class ContactConfiguration : BaseConfiguration<Contact>
{

    public override void Configure(EntityTypeBuilder<Contact> builder)
    {
        base.Configure(builder);

        builder.ToTable("contacts");
        builder.HasKey(x => x.id);

        builder.HasIndex(x => x.customer_id);
    }
}
