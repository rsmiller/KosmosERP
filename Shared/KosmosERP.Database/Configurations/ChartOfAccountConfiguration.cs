using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using KosmosERP.Database.Models;

namespace KosmosERP.Database.Configurations;

public class ChartOfAccountConfiguration : BaseConfiguration<ChartOfAccount>
{
    public override void Configure(EntityTypeBuilder<ChartOfAccount> builder)
    {
        base.Configure(builder);

        builder.ToTable("chart_of_accounts");
        builder.HasIndex(m => m.account_number).IsUnique();
        builder.HasIndex(m => m.account_type);
        builder.HasIndex(m => m.parent_account_id);
        builder.HasIndex(m => m.is_active);
        builder.HasIndex(m => m.guid);

        builder.HasOne<ChartOfAccount>(x => x.parent_account).WithMany(x => x.child_accounts).HasForeignKey(x => x.parent_account_id).HasPrincipalKey(c => c.id);
    }
}
