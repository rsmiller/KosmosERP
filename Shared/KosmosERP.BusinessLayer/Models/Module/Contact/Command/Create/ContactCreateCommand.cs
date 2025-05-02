using KosmosERP.Models;
using System.ComponentModel.DataAnnotations;

namespace KosmosERP.BusinessLayer.Models.Module.Contact.Command.Create;

public class ContactCreateCommand : DataCommand
{
    [Required]
    public int customer_id { get; set; }

    [Required]
    public string first_name { get; set; }

    [Required]
    public string last_name { get; set; }
    public string? title { get; set; }
    public string? email { get; set; }
    public string? phone { get; set; }
    public string? cell_phone { get; set; }
}
