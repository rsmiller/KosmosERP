using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Prometheus.Database.Models;

public class ARInvoiceHeader : BaseDatabaseModel
{
    [Required]
    public int customer_id { get; set; }

    [Required]
    public int order_header_id { get; set; }

    [Required]
    public int invoice_number { get; set; }

    [Required]
    public int payment_terms { get; set; }

    [Required]
    public DateOnly invoice_date { get; set; }

    [Required]
    public DateOnly invoice_due_date { get; set; }

    [Required]
    [Precision(14, 3)]
    public decimal invoice_total { get; set; } = 0;

    [Required]
    public decimal tax_percentage { get; set; } = 0;

    [Required]
    public bool is_paid { get; set; } = false;

    [Required]
    public bool is_taxable { get; set; } = false;

    [Required]
    public DateOnly? paid_on { get; set; }

    [Required]
    public string guid { get; set; } = Guid.NewGuid().ToString();

    [NotMapped]
    public List<ARInvoiceLine> ar_invoice_lines { get; set; } = new List<ARInvoiceLine>();
}
