using KosmosERP.Models;
using System.ComponentModel.DataAnnotations;

namespace KosmosERP.BusinessLayer.Models.Module.DocumentUpload.Command.Create;

public class DocumentUploadCategoryCreateCommand : DataCommand
{
    public int? parent_category_id { get; set; }

    [Required]
    public string category_name { get; set; }

    [Required]
    public string internal_category_name { get; set; }
} 