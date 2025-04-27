using Prometheus.BusinessLayer.Models.Module.PurchaseOrderReceive.Dto;
using Prometheus.Models;

namespace Prometheus.BusinessLayer.Models.Module.APInvoice.Dto;

public class APInvoiceLineDto : BaseDto
{
    public int ap_invoice_header_id { get; set; }

    public int line_number { get; set; }

    public decimal line_total { get; set; }

    public int qty_invoiced { get; set; }

    public int gl_account_id { get; set; }

    public string description { get; set; }

    public int? association_object_id { get; set; }

    public int? association_object_line_id { get; set; }

    public bool association_is_purchase_order { get; set; }

    public bool association_is_sales_order { get; set; }

    public bool association_is_ar_invoice { get; set; }

    public List<PurchaseOrderReceiveLineDto> receive_lines { get; set; } = new List<PurchaseOrderReceiveLineDto>();
}
