using KosmosERP.Models;

namespace KosmosERP.BusinessLayer.Models.Module.CreditMemo.Dto;

public class CreditMemoLineDto : BaseDto
{
    public int credit_memo_header_id { get; set; }

    public int line_number { get; set; }

    public decimal line_total { get; set; } = 0;

    public int qty_credited { get; set; } = 0;

    public string gl_account_id { get; set; }

    public string description { get; set; }

    public int? product_id { get; set; }

    public int? ar_invoice_line_id { get; set; }

    public int? order_line_id { get; set; }

    public string? product_name { get; set; }

    public string? product_sku { get; set; }
} 