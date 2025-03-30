using Microsoft.EntityFrameworkCore;
using Prometheus.Database.Models;
using Prometheus.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
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

    [Required]
    public bool packing_list_is_required { get; set; } = false;

    [Required]
    public bool is_paid { get; set; } = false;

    [Required]
    public string guid { get; set; } = Guid.NewGuid().ToString();


    public List<APInvoiceLineCreateCommand> ap_invoice_lines { get; set; } = new List<APInvoiceLineCreateCommand>();
}
