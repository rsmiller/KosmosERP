using Prometheus.Models;
using System.ComponentModel.DataAnnotations;

namespace Prometheus.BusinessLayer.Models.Module.DocumentUpload.Command.Create;

public class DocumentUploadCreateCommand : DataCommand
{
    [Required]
    public string document_name { get; set; }
}
