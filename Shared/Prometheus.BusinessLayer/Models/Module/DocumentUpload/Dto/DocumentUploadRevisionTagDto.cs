using Prometheus.Models;

namespace Prometheus.BusinessLayer.Models.Module.DocumentUpload.Dto;

public class DocumentUploadRevisionTagDto : BaseDto
{
    public int document_upload_revision_id { get; set; }
    public int document_upload_object_tag_id { get; set; }
    public string tag_name { get; set; }
    public string tag_value { get; set; }
    public bool is_required { get; set; } = false;
    public string guid { get; set; }
}
