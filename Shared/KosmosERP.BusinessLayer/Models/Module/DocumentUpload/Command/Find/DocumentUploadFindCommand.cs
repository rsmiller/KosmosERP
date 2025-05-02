using KosmosERP.Models;

namespace KosmosERP.BusinessLayer.Models.Module.DocumentUpload.Command.Find;

public class DocumentUploadFindCommand : DataCommand
{
    public string? wildcard { get; set; }
}
