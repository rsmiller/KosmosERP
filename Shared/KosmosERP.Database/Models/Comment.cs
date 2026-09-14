
using System.ComponentModel.DataAnnotations;

namespace KosmosERP.Database.Models;

public class Comment : BaseDatabaseModel
{
    [Required]
    public string object_guid { get; set; }
    [Required]
    public string comment_text { get; set; }
    public string guid { get; set; } = Guid.NewGuid().ToString();
}
