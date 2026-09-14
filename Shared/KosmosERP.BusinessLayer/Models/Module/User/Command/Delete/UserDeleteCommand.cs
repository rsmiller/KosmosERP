using KosmosERP.Models;
using System.ComponentModel.DataAnnotations;

namespace KosmosERP.BusinessLayer.Models.Module.User.Command.Delete;

public class UserDeleteCommand : DataCommand
{
    [Required]
    public int id { get; set; }
}
