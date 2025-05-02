using KosmosERP.Models;
using System.ComponentModel.DataAnnotations;

namespace KosmosERP.BusinessLayer.Models.Module.BOM.Command.Delete;

public class BOMDeleteCommand : DataCommand
{
    [Required]
    public int id { get; set; }
}
