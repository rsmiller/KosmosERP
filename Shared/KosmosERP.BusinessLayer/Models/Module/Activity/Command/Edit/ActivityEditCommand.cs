using KosmosERP.Models;
using System.ComponentModel.DataAnnotations;

namespace KosmosERP.BusinessLayer.Models.Module.Activity.Command.Edit;

public class ActivityEditCommand : DataCommand
{
    [Required]
    public int id { get; set; }

    [Required]
    [MaxLength(200)]
    public string subject { get; set; }

    [Required]
    public string description { get; set; }

    [Required]
    public string activity_type { get; set; }

    [Required]
    public string status { get; set; }

    [Required]
    public int owner_id { get; set; }

    [Required]
    public DateTime start_date { get; set; }

    public DateTime? end_date { get; set; }

    [Required]
    public int priority { get; set; }

    public int? customer_id { get; set; }

    public int? contact_id { get; set; }

    public int? opportunity_id { get; set; }

    public int? lead_id { get; set; }

    public string? related_entity_id { get; set; }

    [MaxLength(50)]
    public string? related_entity_type { get; set; }

    public bool is_all_day { get; set; } = false;

    public string? location { get; set; }

    [MaxLength(50)]
    public string? reminder_type { get; set; }

    public DateTime? reminder_time { get; set; }
} 