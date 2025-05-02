using KosmosERP.Models;
using System.ComponentModel.DataAnnotations;

namespace KosmosERP.BusinessLayer.Models.Module.Opportunity.Command.Delete;

public class OpportunityDeleteCommand : DataCommand
{
    [Required]
    public int id { get; set; }
}
