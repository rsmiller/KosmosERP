using System.ComponentModel.DataAnnotations;

namespace KosmosERP.Database.Models;

public class Notification : BaseDatabaseModel
{
    public int? user_id { get; set; }
    [Required]
    public string object_name { get; set; }
    [Required]
    public int object_id { get; set; }
    [Required]
    public string alert_text { get; set; }
    [Required]
    public bool notified { get; set; } = false;
    [Required]
    public bool notification_read { get; set; } = false;
    [Required]
    public string guid { get; set; } = Guid.NewGuid().ToString();
}
