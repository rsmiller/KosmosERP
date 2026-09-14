using KosmosERP.Models;
using System.ComponentModel.DataAnnotations;

namespace KosmosERP.BusinessLayer.Models.Module.KeyValue.Command.Create;

public class KeyValueCreateCommand : DataCommand
{
    [Required]
    public string key { get; set; }
    
    [Required]
    public string value { get; set; }
    
    public int? int_value { get; set; }
    
    public string? module_id { get; set; }
} 