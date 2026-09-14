using KosmosERP.Models;

namespace KosmosERP.BusinessLayer.Models.Module.Settings.Command.Find;

public class SettingsFindCommand : DataCommand
{
    public string? wildcard { get; set; }
}