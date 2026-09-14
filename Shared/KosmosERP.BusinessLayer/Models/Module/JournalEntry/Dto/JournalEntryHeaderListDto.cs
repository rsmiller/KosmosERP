using KosmosERP.Models;

namespace KosmosERP.BusinessLayer.Models.Module.JournalEntry.Dto;

public class JournalEntryHeaderListDto : BaseDto
{
    public int entry_number { get; set; }

    public DateTime entry_date { get; set; }

    public string? description { get; set; }

    public int? reference_type { get; set; }

    public bool is_posted { get; set; } = false;

    public DateTime? posted_on { get; set; }

    public bool is_reversed { get; set; } = false;

    public string? fiscal_period { get; set; }

    public string? reference_type_name { get; set; }

    public decimal total_debits { get; set; }

    public decimal total_credits { get; set; }
}
