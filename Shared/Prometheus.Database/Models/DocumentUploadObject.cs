using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Prometheus.Database.Models;
public class DocumentUploadObject
{
    [Required]
    [Key]
    public int id { get; set; }

    [Required]
    public string friendly_name { get; set; }

    [Required]
    public string internal_name { get; set; }

    [Required]
    public string guid { get; set; } = Guid.NewGuid().ToString();

    [Required]
    public bool requires_approval { get; set; } = false;

    public int? approve_by_id { get; set; }

    [NotMapped]
    public List<DocumentUploadObjectTagTemplate> object_tags { get; set; } = new List<DocumentUploadObjectTagTemplate>();
}
