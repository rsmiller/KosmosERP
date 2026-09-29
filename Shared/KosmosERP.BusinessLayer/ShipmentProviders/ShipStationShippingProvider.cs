using KosmosERP.BusinessLayer.Interfaces;
using KosmosERP.BusinessLayer.ShipmentProviders.Models;
using KosmosERP.Database;
using KosmosERP.Database.Models;
using KosmosERP.Models;
using KosmosERP.Models.Interfaces;
using RestSharp;

namespace KosmosERP.BusinessLayer.ShipmentProviders
{
    public class ShipStationShippingProvider : IShippingProvider
    {
        private readonly RestClient _client;

        public ShipStationShippingProvider(IBaseERPContext context, IShippingSettings settings)
        {
            _client = new RestClient("https://api.shipstation.com/v2/");
            _client.AddDefaultHeader("API-KEY", settings.ship_station_api_key);
            _client.AddDefaultHeader("Content-Type", "application/json");
        }

        public async Task<Response<ShippingCustomer>> CreateCustomer(Customer customer, Address billing_address)
        {
            return new Response<ShippingCustomer>
            {
                Success = true,
                ResultCode = ResultCode.Okay,
                Data = null
            };
        }

        public async Task<Response<ShipmentResponse>> CreateShipment(Customer customer, Address billing_address, IShippingPackage package)
        {
            RestRequest request = new RestRequest("shipments ", Method.Post);
            var response = await _client.PostAsync(request);

            return new Response<ShipmentResponse>
            {
                Success = true,
                ResultCode = ResultCode.Okay,
                Data = new ShipmentResponse
                {
                    external_id = Guid.NewGuid().ToString(),
                }
            };
        }

        public async Task<Response<ShipmentResponse>> CancelShipment(string shipment_external_id)
        {
            return new Response<ShipmentResponse>
            {
                Success = true,
                ResultCode = ResultCode.Okay,
                Data = new ShipmentResponse
                {
                    external_id = shipment_external_id
                }
            };
        }
    }
}
