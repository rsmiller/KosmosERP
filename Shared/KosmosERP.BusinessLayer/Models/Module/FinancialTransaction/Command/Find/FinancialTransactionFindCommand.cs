using KosmosERP.Models;

namespace KosmosERP.BusinessLayer.Models.Module.FinancialTransaction.Command.Find;

public class FinancialTransactionFindCommand : DataCommand
{
    public string? wildcard { get; set; }
    public int? chart_of_account_id { get; set; }
    public int? transaction_type { get; set; }
    public string? source_module { get; set; }
    public DateTime? from_date { get; set; }
    public DateTime? to_date { get; set; }
    public string? fiscal_period { get; set; }
    public bool? is_reversal { get; set; }
}
