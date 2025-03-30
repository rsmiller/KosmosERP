using Prometheus.Models;

namespace Prometheus.BusinessLayer.Models.Module.Contact.Dto;

public class ContactListDto : BaseDto
{
    public required int customer_id { get; set; }
    public required string first_name { get; set; }
    public required string last_name { get; set; }
    public string? title { get; set; }
    public string? email { get; set; }
    public string? phone { get; set; }
    public string? cell_phone { get; set; }
    public string guid { get; set; }
}
