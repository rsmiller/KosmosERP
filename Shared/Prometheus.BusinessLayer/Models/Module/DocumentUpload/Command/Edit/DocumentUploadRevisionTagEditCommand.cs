using System.ComponentModel.DataAnnotations;
using Prometheus.Models;

namespace Prometheus.BusinessLayer.Models.Module.DocumentUpload.Command.Edit;

public class DocumentUploadRevisionTagEditCommand : DataCommand
{
    public int? id { get; set; }
    public int? document_upload_object_tag_id { get; set; }

    public string tag_name { get; set; }

    public string? tag_value { get; set; }

    [Required]
    public bool is_deleted { get; set; } 
}
