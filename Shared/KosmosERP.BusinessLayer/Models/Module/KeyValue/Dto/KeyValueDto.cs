
using KosmosERP.Models;

namespace KosmosERP.BusinessLayer.Models.Module.KeyValue.Dto;

public class KeyValueDto : BaseDto
{
    public string key { get; set; }
    public string value { get; set; }
    public int? int_value { get; set; }
    public string? module_id { get; set; }
}