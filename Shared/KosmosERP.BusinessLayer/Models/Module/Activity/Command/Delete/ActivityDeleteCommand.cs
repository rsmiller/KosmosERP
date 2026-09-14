using KosmosERP.Models;
using System.ComponentModel.DataAnnotations;

namespace KosmosERP.BusinessLayer.Models.Module.Activity.Command.Delete;

public class ActivityDeleteCommand : DataCommand
{
    [Required]
    public int id { get; set; }
} 