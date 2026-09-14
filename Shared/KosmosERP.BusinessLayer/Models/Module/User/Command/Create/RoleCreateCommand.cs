using KosmosERP.Models;
using System.ComponentModel.DataAnnotations;

namespace KosmosERP.BusinessLayer.Models.Module.User.Command.Create;

public class RoleCreateCommand : DataCommand
{
    [Required]
    [MaxLength(200)]
    public string role_name { get; set; }
}
