using Microsoft.EntityFrameworkCore;
using Prometheus.Models;
using System.ComponentModel.DataAnnotations;

namespace Prometheus.BusinessLayer.Models.Module.APInvoice.Command.Edit;

public class APInvoiceLineEditCommand : DataCommand
{
    [Required]
    public int id { get; set; }

    public int? line_number { get; set; }

    [Precision(14, 3)]
    public decimal? line_total { get; set; } = 0;

    public int? qty_invoiced { get; set; } = 0;

    public int? gl_account_id { get; set; }

    [MaxLength(1000)]
    public string description { get; set; }

    public int? purchase_order_receive_line_id { get; set; }

    public int? association_object_id { get; set; }

    public int? association_object_line_id { get; set; }

    public bool? association_is_purchase_order { get; set; }

    public bool? association_is_sales_order { get; set; }

    public bool? association_is_ar_invoice { get; set; }
}
