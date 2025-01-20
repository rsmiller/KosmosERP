using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Prometheus.Database.Models;

public class ARInvoiceLine : BaseDatabaseModel
{
    [Required]
    public int ar_invoice_header_id { get; set; }

    [Required]
    public int order_line_id { get; set; }

    [Required]
    public int product_id { get; set; }

    [Required]
    public int order_qty { get; set; } = 0;

    [Required]
    public int invoice_qty { get; set; } = 0;

    [Required]
    [Precision(14, 3)]
    public decimal line_total { get; set; } = 0;

    [Required]
    [Precision(14, 3)]
    public decimal line_tax { get; set; } = 0;

    [Required]
    public bool is_taxable { get; set; } = false;

    [Required]
    public string guid { get; set; } = Guid.NewGuid().ToString();

    [NotMapped]
    public Product product { get; set; }

    [NotMapped]
    public OrderLine order_line { get; set; }
}
