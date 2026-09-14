using KosmosERP.Models;
using System.ComponentModel.DataAnnotations;

namespace KosmosERP.BusinessLayer.Models.Module.State.Command.Delete;

public class StateDeleteCommand : DataCommand
{
    [Required]
    public int id { get; set; }
}
