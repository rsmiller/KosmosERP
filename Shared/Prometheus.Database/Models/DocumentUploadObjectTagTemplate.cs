using System.ComponentModel.DataAnnotations;

namespace Prometheus.Database.Models;
public class DocumentUploadObjectTagTemplate
{
    [Required]
    [Key]
    public int id { get; set; }

    [Required]
    public int document_object_id { get; set; }

    [Required]
    public string name { get; set; }

    [Required]
    public bool is_required { get; set; } = false;

    [Required]
    public bool is_deleted { get; set; } = false;
}
