using KosmosERP.Models;

namespace KosmosERP.BusinessLayer.Models.Module.JournalEntry.Dto;

public class JournalEntryLineDto : BaseDto
{
    public int journal_entry_header_id { get; set; }

    public int line_number { get; set; }

    public int chart_of_account_id { get; set; }

    public decimal debit_amount { get; set; } = 0;

    public decimal credit_amount { get; set; } = 0;

    public string? description { get; set; }

    public string? account_number { get; set; }

    public string? account_name { get; set; }
}
