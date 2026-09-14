using KosmosERP.Models;
using System.ComponentModel.DataAnnotations;

namespace KosmosERP.BusinessLayer.Models.Module.Settings.Command.Delete;

public class SettingsDeleteCommand : DataCommand
{
    [Required]
    public int id { get; set; }
}