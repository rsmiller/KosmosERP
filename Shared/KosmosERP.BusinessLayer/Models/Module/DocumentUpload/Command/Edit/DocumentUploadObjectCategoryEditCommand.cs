using System.ComponentModel.DataAnnotations;
using KosmosERP.Models;

namespace KosmosERP.BusinessLayer.Models.Module.DocumentUpload.Command.Edit;

public class DocumentUploadCategoryEditCommand : DataCommand
{
    [Required]
    public int id { get; set; }
    
    public int? parent_category_id { get; set; }
    public string? category_name { get; set; }
    public string? internal_category_name { get; set; }
} 