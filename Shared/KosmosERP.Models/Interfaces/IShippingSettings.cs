
namespace KosmosERP.Models.Interfaces
{
    public interface IShippingSettings
    {
        string shipping_provider { get; set; }
        string ship_station_api_key { get; set; }
    }
}
