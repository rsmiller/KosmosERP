using KosmosERP.Models;
using System.ComponentModel.DataAnnotations;

namespace KosmosERP.BusinessLayer.Models.Module.DocumentUpload.Command.Create;

public class DocumentUploadCreateCommand : DataCommand
{
    [Required]
    public string document_name { get; set; }
    
    [Required]
    public int document_object_id { get; set; }

    public List<DocumentUploadRevisionTagCreateCommand> revision_tags { get; set; } = new List<DocumentUploadRevisionTagCreateCommand>();
}
