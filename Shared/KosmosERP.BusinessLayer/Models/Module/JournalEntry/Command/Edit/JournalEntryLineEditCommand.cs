using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using KosmosERP.Models;

namespace KosmosERP.BusinessLayer.Models.Module.JournalEntry.Command.Edit;

public class JournalEntryLineEditCommand : DataCommand
{
    [Required]
    public int id { get; set; }

    public int? line_number { get; set; }

    public int? chart_of_account_id { get; set; }

    [Precision(14, 3)]
    public decimal? debit_amount { get; set; }

    [Precision(14, 3)]
    public decimal? credit_amount { get; set; }

    [MaxLength(500)]
    public string? description { get; set; }
}
