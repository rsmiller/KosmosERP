using System.ComponentModel.DataAnnotations;

namespace KosmosERP.Database.Models;

public class State : BaseDatabaseModel
{
    [Required]
    public int country_id { get; set; }

    [Required]
    [MaxLength(50)]
    public string state_name { get; set; }

    [Required]
    [MaxLength(5)]
    public string iso2 { get; set; }
}
