using KosmosERP.Models;
using System.ComponentModel.DataAnnotations;

namespace KosmosERP.BusinessLayer.Models.Module.DocumentUpload.Command.Delete;

public class DocumentUploadDeleteCommand : DataCommand
{
    [Required]
    public int id { get; set; }
}
