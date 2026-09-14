using KosmosERP.Models;

namespace KosmosERP.BusinessLayer.Models.Module.FinancialTransaction.Dto;

public class AccountBalanceDto
{
    public int chart_of_account_id { get; set; }

    public string account_number { get; set; }

    public string account_name { get; set; }

    public int account_type { get; set; }

    public decimal balance { get; set; } = 0;

    public decimal total_debits { get; set; } = 0;

    public decimal total_credits { get; set; } = 0;

    public DateTime? as_of_date { get; set; }
}
