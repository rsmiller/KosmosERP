namespace KosmosERP.Database.Views;

public class vw_ReadyToShip
{
    public int order_number { get; set; }
    public string order_guid { get; set; }
    public string customer_name { get; set; }
    public string product_name { get; set; }
    public int sold_quantity { get; set; }
    public int produced_quantity { get; set; }
    public int shipped_quantity { get; set; }
}

