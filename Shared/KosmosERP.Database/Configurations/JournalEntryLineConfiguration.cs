using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using KosmosERP.Database.Models;

namespace KosmosERP.Database.Configurations;

public class JournalEntryLineConfiguration : BaseConfiguration<JournalEntryLine>
{
    public override void Configure(EntityTypeBuilder<JournalEntryLine> builder)
    {
        base.Configure(builder);

        builder.ToTable("journal_entry_lines");
        builder.HasIndex(m => m.journal_entry_header_id);
        builder.HasIndex(m => m.chart_of_account_id);
        builder.HasIndex(m => m.guid);

        builder.HasOne<ChartOfAccount>(x => x.chart_of_account).WithMany().HasForeignKey(x => x.chart_of_account_id).HasPrincipalKey(c => c.id);
    }
}
