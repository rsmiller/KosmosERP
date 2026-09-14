using KosmosERP.Models;
using System.ComponentModel.DataAnnotations;

namespace KosmosERP.BusinessLayer.Models.Module.Opportunity.Command.Create;

public class OpportunityLineCreateCommand : DataCommand
{
    [Required]
    public int opportunity_id { get; set; }

    public int? product_id { get; set; }

    [Required]
    [MaxLength(250)]
    public string description { get; set; }

    [Required]
    public int line_number { get; set; }

    [Required]
    public int quantity { get; set; }

    [Required]
    public decimal unit_price { get; set; }
} 