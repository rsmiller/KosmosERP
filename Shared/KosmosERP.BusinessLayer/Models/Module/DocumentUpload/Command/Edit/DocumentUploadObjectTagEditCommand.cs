using System.ComponentModel.DataAnnotations;
using KosmosERP.Models;

namespace KosmosERP.BusinessLayer.Models.Module.DocumentUpload.Command.Edit;

public class DocumentUploadObjectTagEditCommand : DataCommand
{
    [Required]
    public int id { get; set; }
    
    public bool? is_required { get; set; }
    public string? name { get; set; }
} 