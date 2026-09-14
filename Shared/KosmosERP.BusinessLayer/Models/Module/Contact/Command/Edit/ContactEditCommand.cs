using KosmosERP.Models;
using System.ComponentModel.DataAnnotations;

namespace KosmosERP.BusinessLayer.Models.Module.Contact.Command.Edit;

public class ContactEditCommand : DataCommand
{
    [Required]
    public int id { get; set; }
    public int? customer_id { get; set; }
    public string? first_name { get; set; }
    public string? last_name { get; set; }
    public string? title { get; set; }
    public string? email { get; set; }
    public string? phone { get; set; }
    public string? cell_phone { get; set; }
}
