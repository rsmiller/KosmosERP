namespace KosmosERP.BusinessLayer.ShipmentProviders.Models.ShipStation
{
    public class ShipStationTransparencyCode
    {
        public string? order_item_id { get; set; }

        public List<string> codes { get; set; } = new();
    }
}
