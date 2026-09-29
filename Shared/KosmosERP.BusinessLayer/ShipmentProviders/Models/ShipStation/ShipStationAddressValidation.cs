namespace KosmosERP.BusinessLayer.ShipmentProviders.Models.ShipStation
{
    public class ShipStationAddressValidation
    {
        public string? status { get; set; }

        public List<ShipStationAddressValidationMessage> messages { get; set; } = new();
    }
}
