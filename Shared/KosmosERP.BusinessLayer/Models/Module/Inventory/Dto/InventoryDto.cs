
using KosmosERP.Models;

namespace KosmosERP.BusinessLayer.Models.Module.Inventory.Dto;

public class InventoryDto
{
    public int id { get; set; }
    public int product_id { get; set; }
    public string product_name { get; set; }
    public int current_stock { get; set; }
    public int required_stock { get; set; }
    public int reorder_level { get; set; }
    public int on_hand { get; set; }
    public int reserved { get; set; }
    public int on_order { get; set; }
    public int to_order { get; set; }
    public int total_units_sold { get; set; }
    public int total_units_received { get; set; }
    public int total_units_shipped { get; set; }
    public int total_on_purchased { get; set; }
    public string guid { get; set; }
    public DateTime created_on { get; set; }
    public DateTime updated_on { get; set; }
}