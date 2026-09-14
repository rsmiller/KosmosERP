using System.ComponentModel.DataAnnotations;
using KosmosERP.Models;

namespace KosmosERP.BusinessLayer.Models.Module.ChartOfAccount.Command.Edit;

public class ChartOfAccountEditCommand : DataCommand
{
    [Required]
    public int id { get; set; }

    [MaxLength(20)]
    public string? account_number { get; set; }

    [MaxLength(200)]
    public string? account_name { get; set; }

    public int? account_type { get; set; }

    public int? parent_account_id { get; set; }

    public bool? is_active { get; set; }

    public int? normal_balance { get; set; }

    [MaxLength(1000)]
    public string? description { get; set; }
}
