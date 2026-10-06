using KosmosERP.Models;
using KosmosERP.BusinessLayer.Models;
using System.ComponentModel.DataAnnotations;

namespace KosmosERP.BusinessLayer.Models.Module.PurchaseOrder.Command.Create;

public class PurchaseOrderHeaderCreateCommand : DataCommand
{
    [Required]
    public required int vendor_id { get; set; }

    [Required]
    [RegularExpression(HeaderTypes.Pattern, ErrorMessage = HeaderTypes.ErrorMessage)]
    public required string po_type { get; set; }

    public List<PurchaseOrderLineCreateCommand> purchase_order_lines { get; set; } = new List<PurchaseOrderLineCreateCommand>();
}
