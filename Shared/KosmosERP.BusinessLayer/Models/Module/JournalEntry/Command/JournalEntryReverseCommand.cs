using KosmosERP.Models;
using System.ComponentModel.DataAnnotations;

namespace KosmosERP.BusinessLayer.Models.Module.JournalEntry.Command;

public class JournalEntryReverseCommand : DataCommand
{
    [Required]
    public int id { get; set; }

    [Required]
    public DateTime reversal_date { get; set; }

    [MaxLength(1000)]
    public string? description { get; set; }
}
