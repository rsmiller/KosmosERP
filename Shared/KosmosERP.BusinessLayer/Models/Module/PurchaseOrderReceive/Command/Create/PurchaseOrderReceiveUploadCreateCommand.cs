using KosmosERP.Models;

namespace KosmosERP.BusinessLayer.Models.Module.PurchaseOrderReceive.Command.Create;

public class PurchaseOrderReceiveUploadCreateCommand : DataCommand
{
    public int purchase_order_receive_header_id { get; set; }
    public int document_upload_id { get; set; }
}

