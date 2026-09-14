using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using KosmosERP.Database.Models;

namespace KosmosERP.Database.Configurations;

public class JournalEntryHeaderConfiguration : BaseConfiguration<JournalEntryHeader>
{
    public override void Configure(EntityTypeBuilder<JournalEntryHeader> builder)
    {
        base.Configure(builder);

        builder.ToTable("journal_entry_headers");
        builder.HasIndex(m => m.entry_number);
        builder.HasIndex(m => m.entry_date);
        builder.HasIndex(m => m.reference_type);
        builder.HasIndex(m => m.reference_id);
        builder.HasIndex(m => m.is_posted);
        builder.HasIndex(m => m.fiscal_period);
        builder.HasIndex(m => m.guid);

        builder.HasMany<JournalEntryLine>(x => x.journal_entry_lines).WithOne().HasForeignKey(x => x.journal_entry_header_id).HasPrincipalKey(c => c.id);
    }
}
