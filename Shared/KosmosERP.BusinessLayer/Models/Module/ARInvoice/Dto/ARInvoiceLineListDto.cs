using KosmosERP.Models;

namespace KosmosERP.BusinessLayer.Models.Module.ARInvoice.Dto;

public class ARInvoiceLineListDto : BaseDto
{
    public int ar_invoice_header_id { get; set; }
    public int line_number { get; set; }
    public int order_line_id { get; set; }
    public int product_id { get; set; }
    public int order_qty { get; set; }
    public int invoice_qty { get; set; }
    public decimal line_total { get; set; }
    public decimal line_tax { get; set; }
    public bool is_taxable { get; set; } = false;
    public string line_description { get; set; }
}
