using KosmosERP.Models;

namespace KosmosERP.BusinessLayer.Models.Module.Activity.Dto;

public class ActivityListDto : BaseDto
{
    public string subject { get; set; }
    public string description { get; set; }
    public string activity_type { get; set; }
    public string status { get; set; }
    public int owner_id { get; set; }
    public DateTime start_date { get; set; }
    public DateTime? end_date { get; set; }
    public int priority { get; set; }
    public int? customer_id { get; set; }
    public int? contact_id { get; set; }
    public int? opportunity_id { get; set; }
    public int? lead_id { get; set; }
    public string? related_entity_id { get; set; }
    public string? related_entity_type { get; set; }
    public bool is_all_day { get; set; }
    public string? location { get; set; }
    public string? reminder_type { get; set; }
    public DateTime? reminder_time { get; set; }
    public string guid { get; set; }

    // Navigation properties for list view
    public string? owner_name { get; set; }
    public string? customer_name { get; set; }
    public string? contact_name { get; set; }
    public string? opportunity_name { get; set; }
    public string? lead_name { get; set; }
} 