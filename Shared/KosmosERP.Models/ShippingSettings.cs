using KosmosERP.Models.Interfaces;

namespace KosmosERP.Models
{
    public class ShippingSettings : IShippingSettings
    {
        public string shipping_provider { get; set; }
        public string ship_station_api_key { get; set; }
    }
}
