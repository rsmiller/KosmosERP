using KosmosERP.Models;
using System.ComponentModel.DataAnnotations;

namespace KosmosERP.BusinessLayer.Models.Module.ChartOfAccount.Command.Delete;

public class ChartOfAccountDeleteCommand : DataCommand
{
    [Required]
    public int id { get; set; }
}
