using KosmosERP.Models;
using System.ComponentModel.DataAnnotations;

namespace KosmosERP.BusinessLayer.Models.Module.Lead.Command.Delete;

public class LeadDeleteCommand : DataCommand
{
    [Required]
    public int id { get; set; }
}
