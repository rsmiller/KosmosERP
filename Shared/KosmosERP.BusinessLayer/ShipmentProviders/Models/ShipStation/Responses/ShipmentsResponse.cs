namespace KosmosERP.BusinessLayer.ShipmentProviders.Models.ShipStation.Responses
{
    public class ShipmentsResponse
    {
        public bool has_errors { get; set; }

        public List<ShipStationShipment> shipments { get; set; } = new();
    }
}
