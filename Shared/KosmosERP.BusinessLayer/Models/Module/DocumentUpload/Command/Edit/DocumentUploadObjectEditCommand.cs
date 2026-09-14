using System.ComponentModel.DataAnnotations;
using KosmosERP.Models;

namespace KosmosERP.BusinessLayer.Models.Module.DocumentUpload.Command.Edit;

public class DocumentUploadObjectEditCommand : DataCommand
{
    [Required]
    public int id { get; set; }
    public string? internal_name { get; set; }

    public string? friendly_name { get; set; }

    public bool? is_deleted { get; set; } 
}
