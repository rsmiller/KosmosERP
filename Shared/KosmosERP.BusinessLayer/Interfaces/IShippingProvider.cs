using KosmosERP.BusinessLayer.ShipmentProviders.Models;
using KosmosERP.Database.Models;
using KosmosERP.Models;

namespace KosmosERP.BusinessLayer.Interfaces
{
    public interface IShippingProvider
    {
        Task<Response<ShippingCustomer>> CreateCustomer(Customer customer, Address billing_address);
        Task<Response<ShipmentResponse>> CreateShipment(Customer customer, Address billing_address, IShippingPackage package);
        Task<Response<ShipmentResponse>> CancelShipment(string shipment_external_id);
    }
}
