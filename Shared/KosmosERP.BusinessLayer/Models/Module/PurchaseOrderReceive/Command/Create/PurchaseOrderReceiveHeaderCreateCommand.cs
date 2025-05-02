using KosmosERP.Models;
using System.ComponentModel.DataAnnotations;

namespace KosmosERP.BusinessLayer.Models.Module.PurchaseOrderReceive.Command.Create;

public class PurchaseOrderReceiveHeaderCreateCommand : DataCommand
{
    [Required]
    public int purchase_order_id { get; set; }

    [Required]
    public int document_upload_id { get; set; }

    [Required]
    public List<PurchaseOrderReceiveLineCreateCommand> received_lines { get; set; } = new List<PurchaseOrderReceiveLineCreateCommand>();
}
