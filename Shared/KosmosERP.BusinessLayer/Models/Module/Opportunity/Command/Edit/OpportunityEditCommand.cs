using KosmosERP.Models;
using System.ComponentModel.DataAnnotations;

namespace KosmosERP.BusinessLayer.Models.Module.Opportunity.Command.Edit;

public class OpportunityEditCommand : DataCommand
{
    [Required]
    public int id { get; set; }

    public string? opportunity_name { get; set; }
    public int? customer_id { get; set; }
    public int? contact_id { get; set; }
    public decimal? amount { get; set; }
    public string? stage { get; set; }
    public int? win_chance { get; set; }
    public DateOnly? expected_close { get; set; }
    public string? owner_id { get; set; }

    public List<OpportunityLineEditCommand> opportunity_lines { get; set; } = new List<OpportunityLineEditCommand>();
}
