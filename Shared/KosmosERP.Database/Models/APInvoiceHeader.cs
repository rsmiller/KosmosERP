using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KosmosERP.Database.Models;

public class APInvoiceHeader : BaseDatabaseModel
{
    [Required]
    public int vendor_id { get; set; }

    [Required]
    public string invoice_number { get; set; }

    [Required]
    public DateTime invoice_date { get; set; }

    [Required]
    public DateTime invoice_due_date { get; set; }

    [Required]
    public DateTime invoice_received_date { get; set; }

    [Required]
    [Precision(14, 3)]
    public decimal invoice_total { get; set; }

    [MaxLength(1000)]
    public string? memo { get; set; }

    public int? purchase_order_receive_id { get; set; }

    public int? association_object_id { get; set; }

    public bool association_is_purchase_order { get; set; } = false;

    public bool association_is_sales_order { get; set; } = false;

    public bool association_is_ar_invoice { get; set; } = false;

    [Required]
    public bool packing_list_is_required { get; set; } = false;

    [Required]
    public bool is_paid { get; set; } = false;

    [Required]
    public string guid { get; set; } = Guid.NewGuid().ToString();

    [NotMapped]
    public List<APInvoiceLine> ap_invoice_lines { get; set; } = new List<APInvoiceLine>();
}
