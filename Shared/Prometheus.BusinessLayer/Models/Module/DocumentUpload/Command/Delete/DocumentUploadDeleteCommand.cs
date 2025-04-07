using Prometheus.Models;
using System.ComponentModel.DataAnnotations;

namespace Prometheus.BusinessLayer.Models.Module.DocumentUpload.Command.Delete;

public class DocumentUploadDeleteCommand : DataCommand
{
    [Required]
    public int id { get; set; }
}
