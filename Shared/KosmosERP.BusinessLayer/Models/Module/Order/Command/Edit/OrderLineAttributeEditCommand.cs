using KosmosERP.Models;

namespace KosmosERP.BusinessLayer.Models.Module.Order.Command.Edit;

public class OrderLineAttributeEditCommand : DataCommand
{
    public int? id { get; set; }
    public int? order_line_id { get; set; }
    public string? attribute_name { get; set; }
    public string? attribute_value { get; set; }
    public string? attribute_value2 { get; set; }
    public string? attribute_value3 { get; set; }
}
