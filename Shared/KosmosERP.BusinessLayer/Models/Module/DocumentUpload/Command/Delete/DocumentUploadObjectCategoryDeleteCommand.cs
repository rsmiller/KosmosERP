using System.ComponentModel.DataAnnotations;
using KosmosERP.Models;

namespace KosmosERP.BusinessLayer.Models.Module.DocumentUpload.Command.Delete;

public class DocumentUploadCategoryDeleteCommand : DataCommand
{
    [Required]
    public int id { get; set; }
} 