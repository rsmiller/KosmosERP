using KosmosERP.Models;
using System.ComponentModel.DataAnnotations;

namespace KosmosERP.BusinessLayer.Models.Module.Contact.Command.Delete;

public class ContactDeleteCommand : DataCommand
{
    [Required]
    public int id { get; set; }
}
