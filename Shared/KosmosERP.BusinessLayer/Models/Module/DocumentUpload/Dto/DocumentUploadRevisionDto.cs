using KosmosERP.Models;

namespace KosmosERP.BusinessLayer.Models.Module.DocumentUpload.Dto;

public class DocumentUploadRevisionDto : BaseDto
{
    public int document_upload_id { get; set; }
    public string document_name { get; set; }
    public string document_path { get; set; }
    public int rev_num { get; set; } = 1;
    public string? rejected_reason { get; set; }
    public DateTime? approved_on { get; set; }
    public int? approved_by { get; set; }
    public DateTime? rejected_on { get; set; }
    public int? rejected_by { get; set; }
    public string guid { get; set; }
    public List<DocumentUploadRevisionTagDto> revision_tags { get; set; } = new List<DocumentUploadRevisionTagDto>();
}
