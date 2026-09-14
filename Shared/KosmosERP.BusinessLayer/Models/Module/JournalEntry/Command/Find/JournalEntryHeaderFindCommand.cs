using KosmosERP.Models;

namespace KosmosERP.BusinessLayer.Models.Module.JournalEntry.Command.Find;

public class JournalEntryHeaderFindCommand : DataCommand
{
    public string? wildcard { get; set; }
    public bool? is_posted { get; set; }
    public bool? is_reversed { get; set; }
    public int? reference_type { get; set; }
    public DateTime? from_date { get; set; }
    public DateTime? to_date { get; set; }
    public string? fiscal_period { get; set; }
}
