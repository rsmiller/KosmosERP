using KosmosERP.Models;
using System.ComponentModel.DataAnnotations;

namespace KosmosERP.BusinessLayer.Models.Module.KeyValue.Command.Edit;

public class KeyValueEditCommand : DataCommand
{
    [Required]
    public int id { get; set; }
    
    public string? key { get; set; }
    
    public string? value { get; set; }
    
    public int? int_value { get; set; }
    
    public string? module_id { get; set; }
} 