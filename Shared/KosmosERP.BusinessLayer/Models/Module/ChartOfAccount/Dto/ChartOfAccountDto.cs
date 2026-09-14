using KosmosERP.Models;

namespace KosmosERP.BusinessLayer.Models.Module.ChartOfAccount.Dto;

public class ChartOfAccountDto : BaseDto
{
    public string account_number { get; set; }

    public string account_name { get; set; }

    public int account_type { get; set; }

    public int? parent_account_id { get; set; }

    public bool is_active { get; set; } = true;

    public int normal_balance { get; set; }

    public string? description { get; set; }

    public string? parent_account_number { get; set; }

    public string? parent_account_name { get; set; }

    public string? account_type_name { get; set; }

    public List<ChartOfAccountDto> child_accounts { get; set; } = new List<ChartOfAccountDto>();
}
