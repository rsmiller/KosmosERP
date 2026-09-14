using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KosmosERP.Database.Models;

public class DocumentUploadRevision : BaseDatabaseModel
{
    [Required]
    public int document_upload_id { get; set; }

    [Required]
    [MaxLength(150)]
    public string document_name { get; set; }

    [Required]
    [MaxLength(1500)]
    public string document_path { get; set; }

    [Required]
    [MaxLength(200)]
    public string document_type { get; set; }

    [Required]
    public int rev_num { get; set; } = 1;

    [MaxLength(500)]
    public string? rejected_reason { get; set; }

    public DateTime? approved_on { get; set; }
    public int? approved_by { get; set; }
    public DateTime? rejected_on { get; set; }
    public int? rejected_by { get; set; }

    [Required]
    public string guid { get; set; } = Guid.NewGuid().ToString();

    [NotMapped]
    public List<DocumentUploadRevisionTag> revision_tags { get; set; } = new List<DocumentUploadRevisionTag>();
}
