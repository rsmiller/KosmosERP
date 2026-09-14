using System.ComponentModel.DataAnnotations;

namespace KosmosERP.Database.Models;

public class DocumentUploadObjectCategory : BaseDatabaseModel
{
    [Required]
    public int document_upload_category_id { get; set; }
    [Required]
    public int document_upload_object_id { get; set; }
    [Required]
    public string guid { get; set; } = Guid.NewGuid().ToString();
}