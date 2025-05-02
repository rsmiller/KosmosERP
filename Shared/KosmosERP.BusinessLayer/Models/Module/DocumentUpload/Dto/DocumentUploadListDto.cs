using KosmosERP.Models;

namespace KosmosERP.BusinessLayer.Models.Module.DocumentUpload.Dto;

public class DocumentUploadListDto : BaseDto
{
    public int rev_num { get; set; } = 1;
    public int document_object_id { get; set; }
    public string guid { get; set; }
}
