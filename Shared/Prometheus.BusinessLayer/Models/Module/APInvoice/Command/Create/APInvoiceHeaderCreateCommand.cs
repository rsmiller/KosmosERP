using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Prometheus.Models;
using Prometheus.BusinessLayer.Models.Module.APInvoiceLine.Command.Create;

namespace Prometheus.BusinessLayer.Models.Module.APInvoiceHeader.Command.Create;

public class APInvoiceHeaderCreateCommand : DataCommand
{
    [Required]
    public int vendor_id { get; set; }

    [Required]
    public string invoice_number { get; set; }

    [Required]
    public DateTime inv_date { get; set; }

    [Required]
    public DateTime inv_due_date { get; set; }

    [Required]
    public DateTime inv_received_date { get; set; }

    [Required]
    [Precision(14, 3)]
    public decimal invoice_total { get; set; }

    [MaxLength(1000)]
    public string? memo { get; set; }

    public int? purchase_order_receive_id { get; set; }

    [Required]
    public bool packing_list_is_required { get; set; } = false;

    public int? association_object_id { get; set; }

    [Required]
    public bool association_is_purchase_order { get; set; } = false;

    [Required]
    public bool association_is_sales_order { get; set; } = false;

    [Required]
    public bool association_is_ar_invoice { get; set; } = false;

    [Required]
    public bool is_paid { get; set; } = false;
    
    public List<APInvoiceLineCreateCommand> ap_invoice_lines { get; set; } = new List<APInvoiceLineCreateCommand>();
}
