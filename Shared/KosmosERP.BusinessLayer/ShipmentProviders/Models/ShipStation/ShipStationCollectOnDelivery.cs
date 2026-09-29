namespace KosmosERP.BusinessLayer.ShipmentProviders.Models.ShipStation
{
    public class ShipStationCollectOnDelivery
    {
        public string? payment_type { get; set; }

        public ShipStationMonetaryValue? payment_amount { get; set; }
    }
}
