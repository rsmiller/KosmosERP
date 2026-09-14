using KosmosERP.Models;

namespace KosmosERP.BusinessLayer.Models.Module.Activity.Command.Find;

public class ActivityFindCommand : DataCommand
{
    public int? owner_id { get; set; }
    public int? customer_id { get; set; }
    public int? contact_id { get; set; }
    public int? opportunity_id { get; set; }
    public int? lead_id { get; set; }
    public string? activity_type { get; set; }
    public string? status { get; set; }
    public DateTime? start_date_from { get; set; }
    public DateTime? start_date_to { get; set; }
    public string? wildcard { get; set; }
} 