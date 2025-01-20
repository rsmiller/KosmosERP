using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Prometheus.Database.Models;

public class DocumentUpload : BaseDatabaseModel
{
    [Required]
    public int rev_num { get; set; } = 1;

    [Required]
    public int document_object_id { get; set; } = 1;

    [Required]
    [MaxLength(50)]
    public string guid { get; set; } = Guid.NewGuid().ToString();

    [NotMapped]
    public List<DocumentUploadRevision> document_revisions { get; set; } = new List<DocumentUploadRevision>();
}
