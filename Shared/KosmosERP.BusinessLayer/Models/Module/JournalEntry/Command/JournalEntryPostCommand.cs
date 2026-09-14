using KosmosERP.Models;
using System.ComponentModel.DataAnnotations;

namespace KosmosERP.BusinessLayer.Models.Module.JournalEntry.Command;

public class JournalEntryPostCommand : DataCommand
{
    [Required]
    public int id { get; set; }
}
