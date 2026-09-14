using KosmosERP.Models;
using System.ComponentModel.DataAnnotations;

namespace KosmosERP.BusinessLayer.Models.Module.KeyValue.Command.Delete;

public class KeyValueDeleteCommand : DataCommand
{
    [Required]
    public int id { get; set; }
} 