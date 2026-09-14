using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KosmosERP.Database.Models;

public class Activity : BaseDatabaseModel
{
    [Required]
    [MaxLength(200)]
    public string subject { get; set; }

    [Required]
    public string description { get; set; }

    [Required]
    public string activity_type { get; set; } // Call, Meeting, Email, Task, Note, etc.

    [Required]
    public string status { get; set; } // Planned, In Progress, Completed, Cancelled, etc.

    [Required]
    public int owner_id { get; set; }

    [Required]
    public DateTime start_date { get; set; }

    public DateTime? end_date { get; set; }

    [Required]
    public int priority { get; set; } // 1=Low, 2=Medium, 3=High, 4=Urgent

    public int? customer_id { get; set; }

    public int? contact_id { get; set; }

    public int? opportunity_id { get; set; }

    public int? lead_id { get; set; }

    public string? related_entity_id { get; set; }

    [MaxLength(50)]
    public string related_entity_type { get; set; } // Customer, Opportunity, Lead, etc.

    public bool is_all_day { get; set; } = false;

    public string? location { get; set; }

    [MaxLength(50)]
    public string? reminder_type { get; set; } // None, Email, SMS, Popup

    public DateTime? reminder_time { get; set; }

    [Required]
    public string guid { get; set; } = Guid.NewGuid().ToString();

    // Navigation properties
    [NotMapped]
    public User owner { get; set; }

    [NotMapped]
    public Customer customer { get; set; }

    [NotMapped]
    public Contact contact { get; set; }

    [NotMapped]
    public Opportunity opportunity { get; set; }

    [NotMapped]
    public Lead lead { get; set; }
}