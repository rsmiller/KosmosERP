
using Microsoft.EntityFrameworkCore;
using KosmosERP.Models;
using System.ComponentModel.DataAnnotations;

namespace KosmosERP.BusinessLayer.Models.Module.APInvoice.Command.Create;

public class APInvoiceLineCreateCommand : DataCommand
{
    public int? ap_invoice_header_id { get; set; }

    [Required]
    public int line_number { get; set; }

    [Required]
    [Precision(14, 3)]
    public decimal line_total { get; set; } = 0;

    [Required]
    public int qty_invoiced { get; set; } = 0;

    [Required]
    public int gl_account_id { get; set; }

    [Required]
    [MaxLength(1000)]
    public string description { get; set; }

    public int? association_object_id { get; set; }

    public int? association_object_line_id { get; set; }

    [Required]
    public bool association_is_purchase_order { get; set; } = false;

    [Required]
    public bool association_is_sales_order { get; set; } = false;

    [Required]
    public bool association_is_ar_invoice { get; set; } = false;
}
