using KosmosERP.Models;
using System.ComponentModel.DataAnnotations;


namespace KosmosERP.BusinessLayer.Models.Module.Notification.Command.Find;

public class NotificationFindCommand : DataCommand
{
    [Required]
    public int user_id { get; set; }
    public bool show_read { get; set; } = false;
}
