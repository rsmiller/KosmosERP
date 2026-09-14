using KosmosERP.Models;
using System.ComponentModel.DataAnnotations;

namespace KosmosERP.BusinessLayer.Models.Module.JournalEntry.Command.Delete;

public class JournalEntryHeaderDeleteCommand : DataCommand
{
    [Required]
    public int id { get; set; }
}
