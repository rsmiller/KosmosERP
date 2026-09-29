namespace KosmosERP.BusinessLayer.ShipmentProviders.Models.ShipStation
{
    public class ShipStationShipmentItem
    {
        public string? name { get; set; }

        public string? sales_order_id { get; set; }

        public string? sales_order_item_id { get; set; }

        public int quantity { get; set; }

        public string? sku { get; set; }

        public string? external_order_id { get; set; }

        public string? external_order_item_id { get; set; }

        public string? asin { get; set; }

        public string? order_source_code { get; set; }
    }
}
