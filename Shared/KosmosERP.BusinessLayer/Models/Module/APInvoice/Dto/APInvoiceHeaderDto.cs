using KosmosERP.BusinessLayer.Models.Module.ARInvoice.Dto;
using KosmosERP.BusinessLayer.Models.Module.DocumentUpload.Dto;
using KosmosERP.BusinessLayer.Models.Module.Order.Dto;
using KosmosERP.BusinessLayer.Models.Module.PurchaseOrder.Dto;
using KosmosERP.BusinessLayer.Models.Module.PurchaseOrderReceive.Dto;
using KosmosERP.Models;

namespace KosmosERP.BusinessLayer.Models.Module.APInvoice.Dto;

public class APInvoiceHeaderDto : BaseDto
{
    public int vendor_id { get; set; }

    public string invoice_number { get; set; }

    public DateTime invoice_date { get; set; }
    public DateTime invoice_due_date { get; set; }

    public DateTime invoice_received_date { get; set; }

    public decimal invoice_total { get; set; }

    public string? memo { get; set; }

    public int? purchase_order_receive_id { get; set; }

    public int? association_object_id { get; set; }

    public int? association_number { get; set; }

    public bool association_is_purchase_order { get; set; }

    public bool association_is_sales_order { get; set; }

    public bool association_is_ar_invoice { get; set; }

    public bool packing_list_is_required { get; set; } = false;

    public bool is_paid { get; set; } = false;

    public string? vendor_name { get; set; }

    public DateTime? first_po_receive_date { get; set; }

    public List<APInvoiceLineDto> ap_invoice_lines { get; set; } = new List<APInvoiceLineDto>();
    public List<PurchaseOrderLineDto> po_lines { get; set; } = new List<PurchaseOrderLineDto>();
    public List<OrderLineDto> order_lines { get; set; } = new List<OrderLineDto>();
    public List<ARInvoiceLineDto> ar_lines { get; set; } = new List<ARInvoiceLineDto>();
    public List<DocumentUploadDto> documents { get; set; } = new List<DocumentUploadDto>();
    public List<PurchaseOrderReceiveLineDto> receive_lines { get; set; } = new List<PurchaseOrderReceiveLineDto>();
}
