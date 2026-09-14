using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KosmosERP.Database.Models;

public class CreditMemoHeader : BaseDatabaseModel
{
    [Required]
    public int customer_id { get; set; }

    [Required]
    public int credit_memo_number { get; set; }

    [Required]
    public DateTime credit_memo_date { get; set; }

    [Required]
    public DateTime credit_memo_due_date { get; set; }

    [Required]
    [Precision(14, 3)]
    public decimal credit_memo_total { get; set; }

    [MaxLength(1000)]
    public string? memo { get; set; }

    public int? ar_invoice_header_id { get; set; }
    public int? order_header_id { get; set; }

    [Required]
    public string credit_reason { get; set; }

    [Required]
    public bool is_approved { get; set; } = false;

    [Required]
    public bool is_applied { get; set; } = false;

    [Required]
    public string guid { get; set; } = Guid.NewGuid().ToString();

    [NotMapped]
    public Customer customer { get; set; }

    [NotMapped]
    public List<CreditMemoLine> credit_memo_lines { get; set; } = new List<CreditMemoLine>();
} 