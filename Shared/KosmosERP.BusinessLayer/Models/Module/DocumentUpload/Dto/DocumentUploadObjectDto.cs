using KosmosERP.Database.Models;
using KosmosERP.Models;

namespace KosmosERP.BusinessLayer.Models.Module.DocumentUpload.Dto;

public class DocumentUploadObjectDto : BaseDto
{
    public string friendly_name { get; set; }
    public string internal_name { get; set; }
    public string guid { get; set; }

     public List<DocumentUploadObjectTagTemplate> tag_templates { get; set; } = new List<DocumentUploadObjectTagTemplate>();
}