using KosmosERP.Models;
using System.ComponentModel.DataAnnotations;

namespace KosmosERP.BusinessLayer.Models.Module.DocumentUpload.Command.Create;

public class DocumentUploadObjectCreateCommand : DataCommand
{

    [Required]
    public string internal_name { get; set; }

    [Required]
    public string friendly_name { get; set; }
} 