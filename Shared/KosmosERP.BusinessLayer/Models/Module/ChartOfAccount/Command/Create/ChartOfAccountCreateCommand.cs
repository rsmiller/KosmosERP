using System.ComponentModel.DataAnnotations;
using KosmosERP.Models;

namespace KosmosERP.BusinessLayer.Models.Module.ChartOfAccount.Command.Create;

public class ChartOfAccountCreateCommand : DataCommand
{
    [Required]
    [MaxLength(20)]
    public string account_number { get; set; }

    [Required]
    [MaxLength(200)]
    public string account_name { get; set; }

    [Required]
    public int account_type { get; set; }

    public int? parent_account_id { get; set; }

    [Required]
    public bool is_active { get; set; } = true;

    [Required]
    public int normal_balance { get; set; }

    [MaxLength(1000)]
    public string? description { get; set; }
}
