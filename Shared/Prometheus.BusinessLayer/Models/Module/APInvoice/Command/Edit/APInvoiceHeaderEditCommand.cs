using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using Prometheus.BusinessLayer.Models.Module.APInvoiceLine.Command.Edit;
using Prometheus.Models;

namespace Prometheus.BusinessLayer.Models.Module.APInvoiceHeader.Command.Edit;

public class APInvoiceHeaderEditCommand : DataCommand
{
    [Required]
    public int id { get; set; }

    public string? invoice_number { get; set; }

    public int? vendor_id { get; set; }

    public DateTime? inv_date { get; set; }

    public DateTime? inv_due_date { get; set; }

    public DateTime? inv_received_date { get; set; }

    [Precision(14, 3)]
    public decimal? invoice_total { get; set; }

    [MaxLength(1000)]
    public string? memo { get; set; }

    public int? purchase_order_receive_id { get; set; }

    public int? association_object_id { get; set; }

    public bool? association_is_purchase_order { get; set; }

    public bool? association_is_sales_order { get; set; }

    public bool? association_is_ar_invoice { get; set; }

    public bool? packing_list_is_required { get; set; }

    public bool? is_paid { get; set; } = false;


    public List<APInvoiceLineEditCommand> ap_invoice_lines { get; set; } = new List<APInvoiceLineEditCommand>();
}
