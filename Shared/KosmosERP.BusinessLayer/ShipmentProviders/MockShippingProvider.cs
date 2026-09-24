using KosmosERP.BusinessLayer.Interfaces;
using KosmosERP.BusinessLayer.ShipmentProviders.Models;
using KosmosERP.Database;
using KosmosERP.Database.Models;
using KosmosERP.Models;
using KosmosERP.Models.Interfaces;

namespace KosmosERP.BusinessLayer.ShipmentProviders
{
    public class MockShippingProvider : IShippingProvider
    {
        public MockShippingProvider(IBaseERPContext context, IShippingSettings settings)
        {
        }

        public async Task<Response<ShippingCustomer>> CreateCustomer(Customer customer, Address billing_address)
        {
            return new Response<ShippingCustomer>
            {
                Success = true,
                ResultCode = ResultCode.Okay,
                Data = new ShippingCustomer
                {
                    external_id = customer.guid
                }
            };
        }

        public async Task<Response<ShipmentResponse>> CreateShipment(Customer customer, Address billing_address, IShippingPackage package)
        {
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
