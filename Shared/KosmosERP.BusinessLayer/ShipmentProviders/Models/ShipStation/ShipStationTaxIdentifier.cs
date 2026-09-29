namespace KosmosERP.BusinessLayer.ShipmentProviders.Models.ShipStation
{
    public class ShipStationTaxIdentifier
    {
        public string? taxable_entity_type { get; set; }

        public string? identifier_type { get; set; }

        public string? issuing_authority { get; set; }

        public string? value { get; set; }
    }
}
