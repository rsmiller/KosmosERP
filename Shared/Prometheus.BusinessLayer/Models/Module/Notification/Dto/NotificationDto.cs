using Prometheus.Models;

namespace Prometheus.BusinessLayer.Models.Module.Notification.Dto;

public class NotificationDto : BaseDto
{
    public int? user_id { get; set; }
    public string object_name { get; set; }
    public int object_id { get; set; }
    public string alert_text { get; set; }
    public bool notified { get; set; }
    public bool notification_read { get; set; }
}
