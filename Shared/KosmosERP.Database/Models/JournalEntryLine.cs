using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KosmosERP.Database.Models;

public class JournalEntryLine : BaseDatabaseModel
{
    [Required]
    public int journal_entry_header_id { get; set; }

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

    [Required]
    public string guid { get; set; } = Guid.NewGuid().ToString();

    [NotMapped]
    public ChartOfAccount? chart_of_account { get; set; }
}
