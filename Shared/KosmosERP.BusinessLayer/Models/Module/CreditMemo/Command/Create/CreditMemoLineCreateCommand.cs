using Microsoft.EntityFrameworkCore;
using KosmosERP.Models;
using System.ComponentModel.DataAnnotations;

namespace KosmosERP.BusinessLayer.Models.Module.CreditMemo.Command.Create;

public class CreditMemoLineCreateCommand : DataCommand
{
    public int? credit_memo_header_id { get; set; }

    [Required]
    public int line_number { get; set; }

    [Required]
    [Precision(14, 3)]
    public decimal line_total { get; set; } = 0;

    [Required]
    public int qty_credited { get; set; } = 0;

    [Required]
    public string gl_account_id { get; set; }

    [Required]
    [MaxLength(1000)]
    public string description { get; set; }

    public int? product_id { get; set; }

    public int? ar_invoice_line_id { get; set; }

    public int? order_line_id { get; set; }
} 