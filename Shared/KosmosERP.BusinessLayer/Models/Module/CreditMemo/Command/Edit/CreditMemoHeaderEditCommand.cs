using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using KosmosERP.Models;

namespace KosmosERP.BusinessLayer.Models.Module.CreditMemo.Command.Edit;

public class CreditMemoHeaderEditCommand : DataCommand
{
    [Required]
    public int id { get; set; }

    public int? customer_id { get; set; }

    public DateTime? credit_memo_date { get; set; }

    public DateTime? credit_memo_due_date { get; set; }

    [Precision(14, 3)]
    public decimal? credit_memo_total { get; set; }

    [MaxLength(1000)]
    public string? memo { get; set; }

    public int? ar_invoice_header_id { get; set; }

    public int? order_header_id { get; set; }

    public string? credit_reason { get; set; }

    public bool? is_approved { get; set; }

    public bool? is_applied { get; set; }

    public List<CreditMemoLineEditCommand> credit_memo_lines { get; set; } = new List<CreditMemoLineEditCommand>();
} 