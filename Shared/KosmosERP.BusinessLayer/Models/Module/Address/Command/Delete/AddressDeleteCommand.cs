using KosmosERP.Models;
using System.ComponentModel.DataAnnotations;

namespace KosmosERP.BusinessLayer.Models.Module.Address.Command.Delete;

public class AddressDeleteCommand : DataCommand
{
    [Required]
    public int id { get; set; }
}
