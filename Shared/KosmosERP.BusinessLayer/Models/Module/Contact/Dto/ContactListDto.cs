using KosmosERP.Models;

namespace KosmosERP.BusinessLayer.Models.Module.Contact.Dto;

public class ContactListDto : BaseDto
{
    public int customer_id { get; set; }
    public string first_name { get; set; }
    public string last_name { get; set; }
    public string? title { get; set; }
    public string? email { get; set; }
    public string? phone { get; set; }
    public string? cell_phone { get; set; }

    public string? customer_name { get; set; }
}
