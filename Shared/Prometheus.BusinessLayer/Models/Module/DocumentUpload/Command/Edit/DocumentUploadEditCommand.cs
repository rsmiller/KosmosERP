using Prometheus.BusinessLayer.Models.Module.DocumentUpload.Command.Create;
using Prometheus.Models;
using System.ComponentModel.DataAnnotations;

namespace Prometheus.BusinessLayer.Models.Module.DocumentUpload.Command.Edit;

public class DocumentUploadEditCommand : DataCommand
{
    [Required]
    public int id { get; set; }
    public int? document_object_id { get; set; }
    public string? document_name { get; set; }
    public bool? is_approved { get; set; }
    public bool? is_rejected { get; set; }
    public string? rejected_reason { get; set; }
    public bool? requires_approval { get; set; }
    public int? approve_by_id { get; set; }

    public List<DocumentUploadRevisionTagCreateCommand> revision_tags { get; set; } = new List<DocumentUploadRevisionTagCreateCommand>();
}
