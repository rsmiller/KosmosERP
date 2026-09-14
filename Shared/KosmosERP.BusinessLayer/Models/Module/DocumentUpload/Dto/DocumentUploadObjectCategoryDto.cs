using KosmosERP.Models;

namespace KosmosERP.BusinessLayer.Models.Module.DocumentUpload.Dto;

public class DocumentUploadCategoryDto : BaseDto
{
    public int? parent_category_id { get; set; }
    public string category_name { get; set; }
    public string internal_category_name { get; set; }

    public List<DocumentUploadObjectDto> document_objects { get; set; } = new List<DocumentUploadObjectDto>();
}