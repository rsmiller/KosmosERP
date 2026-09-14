using KosmosERP.Models;

namespace KosmosERP.BusinessLayer.Models.Module.ChartOfAccount.Command.Find;

public class ChartOfAccountFindCommand : DataCommand
{
    public string? wildcard { get; set; }
    public int? account_type { get; set; }
    public bool? is_active { get; set; }
    public int? parent_account_id { get; set; }
}
