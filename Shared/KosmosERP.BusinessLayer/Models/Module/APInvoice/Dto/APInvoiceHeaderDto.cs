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

    public bool association_is_purchase_order { get; set; }

    public bool association_is_sales_order { get; set; }

    public bool association_is_ar_invoice { get; set; }

    public bool packing_list_is_required { get; set; } = false;

    public bool is_paid { get; set; } = false;

    public string guid { get; set; }

    public string? vendor_name { get; set; }

    public List<APInvoiceLineDto> ap_invoice_lines { get; set; } = new List<APInvoiceLineDto>();
}
