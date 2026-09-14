using KosmosERP.Models;

namespace KosmosERP.BusinessLayer.Models.Module.FinancialTransaction.Dto;

public class FinancialTransactionDto : BaseDto
{
    public DateTime transaction_date { get; set; }

    public int transaction_type { get; set; }

    public string source_module { get; set; }

    public int source_id { get; set; }

    public string source_guid { get; set; }

    public int chart_of_account_id { get; set; }

    public decimal debit_amount { get; set; } = 0;

    public decimal credit_amount { get; set; } = 0;

    public decimal running_balance { get; set; } = 0;

    public string? description { get; set; }

    public string? fiscal_period { get; set; }

    public int? journal_entry_id { get; set; }

    public bool is_reversal { get; set; } = false;

    public string? transaction_type_name { get; set; }

    public string? account_number { get; set; }

    public string? account_name { get; set; }
}
