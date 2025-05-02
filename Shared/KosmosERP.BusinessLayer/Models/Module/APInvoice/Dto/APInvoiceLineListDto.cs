using KosmosERP.Models;

namespace KosmosERP.BusinessLayer.Models.Module.APInvoice.Dto;

public class APInvoiceLineListDto : BaseDto
{
    public int ap_invoice_header_id { get; set; }

    public int line_number { get; set; }

    public decimal line_total { get; set; }

    public int qty_invoiced { get; set; }

    public int gl_account_id { get; set; }

    public string description { get; set; }

    public int? association_object_id { get; set; }

    public int? association_object_line_id { get; set; }
}
