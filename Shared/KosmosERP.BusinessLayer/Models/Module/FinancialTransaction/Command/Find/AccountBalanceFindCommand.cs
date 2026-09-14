using KosmosERP.Models;
using System.ComponentModel.DataAnnotations;

namespace KosmosERP.BusinessLayer.Models.Module.FinancialTransaction.Command.Find;

public class AccountBalanceFindCommand : DataCommand
{
    [Required]
    public int chart_of_account_id { get; set; }

    public DateTime? as_of_date { get; set; }
}
