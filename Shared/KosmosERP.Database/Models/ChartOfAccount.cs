using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KosmosERP.Database.Models;

public class ChartOfAccount : BaseDatabaseModel
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

    [Required]
    public string guid { get; set; } = Guid.NewGuid().ToString();

    [NotMapped]
    public ChartOfAccount? parent_account { get; set; }

    [NotMapped]
    public List<ChartOfAccount> child_accounts { get; set; } = new List<ChartOfAccount>();
}
