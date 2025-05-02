using System.ComponentModel.DataAnnotations;

namespace KosmosERP.Database.Models;

public class DocumentUploadRevisionTag : BaseDatabaseModel
{
    [Required]
    public int document_upload_revision_id { get; set; }

    [Required]
    public int document_upload_object_tag_id { get; set; }

    [Required]
    public string tag_name { get; set; }

    [Required]
    public string tag_value { get; set; }

    [Required]
    public bool is_required { get; set; } = false;

    [Required]
    [MaxLength(50)]
    public string guid { get; set; } = Guid.NewGuid().ToString();
}
