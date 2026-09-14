using System.ComponentModel.DataAnnotations;

namespace KosmosERP.Database.Models;

public class MessageQueue : BaseDatabaseModel
{
    [Required]
    [MaxLength(500)]
    public string queue { get; set;}
    [Required]
    public string body { get; set;}
    [Required]
    public bool read { get; set;} = false;
    [Required]
    public DateTime read_on { get; set;} = DateTime.Now;
    [Required]
    public string guid { get; set; } = Guid.NewGuid().ToString();
}