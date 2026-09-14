using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using KosmosERP.Models;

namespace KosmosERP.BusinessLayer.Models.Module.JournalEntry.Command.Create;

public class JournalEntryHeaderCreateCommand : DataCommand
{
    [Required]
    public DateTime entry_date { get; set; }

    [MaxLength(1000)]
    public string? description { get; set; }

    public int? reference_type { get; set; }

    public int? reference_id { get; set; }

    [MaxLength(10)]
    public string? fiscal_period { get; set; }

    public List<JournalEntryLineCreateCommand> journal_entry_lines { get; set; } = new List<JournalEntryLineCreateCommand>();
}
