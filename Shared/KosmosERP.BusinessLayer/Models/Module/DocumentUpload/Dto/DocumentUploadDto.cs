using KosmosERP.Database.Models;
using KosmosERP.Models;

namespace KosmosERP.BusinessLayer.Models.Module.DocumentUpload.Dto;

public class DocumentUploadDto : BaseDto
{
    public int rev_num { get; set; } = 1;
    public int document_object_id { get; set; }

    public List<DocumentUploadRevisionDto> document_revisions { get; set; } = new List<DocumentUploadRevisionDto>();
    // Todo: One day change this to a dto, atm I am trying to finish this module today
    public List<DocumentUploadObjectTagTemplate> tag_templates { get; set; } = new List<DocumentUploadObjectTagTemplate>();
}
