using KosmosERP.Models;

namespace KosmosERP.BusinessLayer.Models.Module.DocumentUpload.Dto;

public class DocumentUploadObjectTagDto : BaseDto
{
    public int document_object_id { get; set; }
    public string name { get; set; }
    public bool is_required { get; set; }
}