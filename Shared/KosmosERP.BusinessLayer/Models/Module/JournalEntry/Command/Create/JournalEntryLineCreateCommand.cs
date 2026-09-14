using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using KosmosERP.Models;

namespace KosmosERP.BusinessLayer.Models.Module.JournalEntry.Command.Create;

public class JournalEntryLineCreateCommand : DataCommand
{
    public int? journal_entry_header_id { get; set; }

    [Required]
    public int line_number { get; set; }

    [Required]
    public int chart_of_account_id { get; set; }

    [Required]
    [Precision(14, 3)]
    public decimal debit_amount { get; set; } = 0;

    [Required]
    [Precision(14, 3)]
    public decimal credit_amount { get; set; } = 0;

    [MaxLength(500)]
    public string? description { get; set; }
}
