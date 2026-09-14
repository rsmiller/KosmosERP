using KosmosERP.Models;
using System.ComponentModel.DataAnnotations;

namespace KosmosERP.BusinessLayer.Models.Module.DocumentUpload.Command.Create;

public class DocumentUploadObjectTagCreateCommand : DataCommand
{
    [Required]
    public int document_object_id { get; set; }

    [Required]
    public string name { get; set; }

    [Required]
    public bool is_required { get; set; }
}
