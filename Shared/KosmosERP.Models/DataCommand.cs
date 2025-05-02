using System.ComponentModel.DataAnnotations;

namespace KosmosERP.Models;

public class DataCommand
{
    [Required]
    public int calling_user_id { get; set; }
    [Required]
    public string token { get; set; }
}
