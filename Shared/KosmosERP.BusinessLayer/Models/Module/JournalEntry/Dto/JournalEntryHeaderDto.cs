using KosmosERP.Models;

namespace KosmosERP.BusinessLayer.Models.Module.JournalEntry.Dto;

public class JournalEntryHeaderDto : BaseDto
{
    public int entry_number { get; set; }

    public DateTime entry_date { get; set; }

    public string? description { get; set; }

    public int? reference_type { get; set; }

    public int? reference_id { get; set; }

    public bool is_posted { get; set; } = false;

    public DateTime? posted_on { get; set; }

    public string? posted_by { get; set; }

    public bool is_reversed { get; set; } = false;

    public int? reversed_by_entry_id { get; set; }

    public string? fiscal_period { get; set; }

    public string? reference_type_name { get; set; }

    public decimal total_debits { get; set; }

    public decimal total_credits { get; set; }

    public List<JournalEntryLineDto> journal_entry_lines { get; set; } = new List<JournalEntryLineDto>();
}
