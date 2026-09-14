using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KosmosERP.Database.Models;

public class JournalEntryHeader : BaseDatabaseModel
{
    [Required]
    public int entry_number { get; set; }

    [Required]
    public DateTime entry_date { get; set; }

    [MaxLength(1000)]
    public string? description { get; set; }

    public int? reference_type { get; set; }

    public int? reference_id { get; set; }

    [Required]
    public bool is_posted { get; set; } = false;

    public DateTime? posted_on { get; set; }

    [MaxLength(100)]
    public string? posted_by { get; set; }

    [Required]
    public bool is_reversed { get; set; } = false;

    public int? reversed_by_entry_id { get; set; }

    [MaxLength(10)]
    public string? fiscal_period { get; set; }

    [Required]
    public string guid { get; set; } = Guid.NewGuid().ToString();

    [NotMapped]
    public List<JournalEntryLine> journal_entry_lines { get; set; } = new List<JournalEntryLine>();

    [NotMapped]
    public decimal total_debits { get; set; }

    [NotMapped]
    public decimal total_credits { get; set; }
}
