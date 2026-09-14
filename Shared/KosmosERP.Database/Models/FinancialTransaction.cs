using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KosmosERP.Database.Models;

public class FinancialTransaction : BaseDatabaseModel
{
    [Required]
    public DateTime transaction_date { get; set; }

    [Required]
    public int transaction_type { get; set; }

    [Required]
    [MaxLength(50)]
    public string source_module { get; set; }

    [Required]
    public int source_id { get; set; }

    [Required]
    [MaxLength(50)]
    public string source_guid { get; set; }

    [Required]
    public int chart_of_account_id { get; set; }

    [Required]
    [Precision(14, 3)]
    public decimal debit_amount { get; set; } = 0;

    [Required]
    [Precision(14, 3)]
    public decimal credit_amount { get; set; } = 0;

    [Required]
    [Precision(14, 3)]
    public decimal running_balance { get; set; } = 0;

    [MaxLength(1000)]
    public string? description { get; set; }

    [MaxLength(10)]
    public string? fiscal_period { get; set; }

    public int? journal_entry_id { get; set; }

    [Required]
    public bool is_reversal { get; set; } = false;

    [Required]
    public string guid { get; set; } = Guid.NewGuid().ToString();

    [NotMapped]
    public ChartOfAccount? chart_of_account { get; set; }
}
