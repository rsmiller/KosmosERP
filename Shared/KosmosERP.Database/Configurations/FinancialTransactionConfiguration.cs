using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using KosmosERP.Database.Models;

namespace KosmosERP.Database.Configurations;

public class FinancialTransactionConfiguration : BaseConfiguration<FinancialTransaction>
{
    public override void Configure(EntityTypeBuilder<FinancialTransaction> builder)
    {
        base.Configure(builder);

        builder.ToTable("financial_transactions");
        builder.HasIndex(m => m.transaction_date);
        builder.HasIndex(m => m.transaction_type);
        builder.HasIndex(m => m.source_module);
        builder.HasIndex(m => m.source_id);
        builder.HasIndex(m => m.chart_of_account_id);
        builder.HasIndex(m => m.fiscal_period);
        builder.HasIndex(m => m.journal_entry_id);
        builder.HasIndex(m => m.guid);

        builder.HasOne<ChartOfAccount>(x => x.chart_of_account).WithMany().HasForeignKey(x => x.chart_of_account_id).HasPrincipalKey(c => c.id);
    }
}
