using KosmosERP.Models;
using System.ComponentModel.DataAnnotations;

namespace KosmosERP.BusinessLayer.Models.Module.Transaction.Command.Edit;

public class TransactionEditCommand : DataCommand
{
    public int? id { get; set; }

    public int? object_reference_id { get; set; }
    public int? object_sub_reference_id { get; set; }

    public int? product_id { get; set; }
    public int? transaction_type { get; set; }
    public DateTime? transaction_date { get; set; }
    public int? units_sold { get; set; }
    public int? units_shipped { get; set; }
    public int? units_purchased { get; set; }
    public int? units_received { get; set; }
    public decimal? purchased_unit_cost { get; set; }
    public decimal? sold_unit_price { get; set; }
}
