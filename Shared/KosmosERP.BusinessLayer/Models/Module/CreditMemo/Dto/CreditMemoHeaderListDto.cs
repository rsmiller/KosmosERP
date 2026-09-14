using KosmosERP.Models;

namespace KosmosERP.BusinessLayer.Models.Module.CreditMemo.Dto;

public class CreditMemoHeaderListDto : BaseDto
{
    public int customer_id { get; set; }

    public int credit_memo_number { get; set; }

    public DateTime credit_memo_date { get; set; }

    public DateTime credit_memo_due_date { get; set; }

    public decimal credit_memo_total { get; set; }

    public string? memo { get; set; }

    public string credit_reason { get; set; }

    public bool is_approved { get; set; } = false;

    public bool is_applied { get; set; } = false;

    public string? customer_name { get; set; }

    public int? ar_invoice_header_id { get; set; }
    public int? order_header_id { get; set; }
} 