using KosmosERP.Models;

namespace KosmosERP.BusinessLayer.Models.Module.CreditMemo.Command.Find;

public class CreditMemoHeaderFindCommand : DataCommand
{
    public string? wildcard { get; set; }
} 