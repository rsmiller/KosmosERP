using KosmosERP.Models;
using System.ComponentModel.DataAnnotations;

namespace KosmosERP.BusinessLayer.Models.Module.Opportunity.Command.Edit;

public class OpportunityLineEditCommand : DataCommand
{
    public int? id { get; set; }

    public int? opportunity_id { get; set; }
    public int? product_id { get; set; }
    public string? description { get; set; }
    public int? line_number { get; set; }
    public int? quantity { get; set; }
    public decimal? unit_price { get; set; }
} 