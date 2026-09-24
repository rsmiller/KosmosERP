using KosmosERP.BusinessLayer.Interfaces;
using KosmosERP.Database;
using KosmosERP.Models;
using KosmosERP.Models.Interfaces;
using KosmosERP.BusinessLayer.ShipmentProviders;

namespace KosmosERP.BusinessLayer
{
    public interface IShippingProviderFactory
    {
        IShippingProvider GetProvider();
    }

    public class ShippingProviderFactory : IShippingProviderFactory
    {
        private IBaseERPContext _Context;
        private IShippingSettings _Settings;
        private IShippingProvider _ShippingProvider;

        public ShippingProviderFactory(IBaseERPContext context, IShippingSettings settings)
        {
            this._Context = context;
            this._Settings = settings;

            ValidateSettings(settings);

            if (settings.shipping_provider.Equals(ShippingProviderType.ShipStation, StringComparison.OrdinalIgnoreCase))
            {
                _ShippingProvider = new ShipStationShippingProvider(context, settings);
            }
            else if (settings.shipping_provider.Equals(ShippingProviderType.MOCK, StringComparison.OrdinalIgnoreCase))
            {
                _ShippingProvider = new MockShippingProvider(context, settings);
            }
            else
            {
                _ShippingProvider = new DatabaseShippingProvider(context, settings);
            }
        }

        public IShippingProvider GetProvider()
        {
            return _ShippingProvider;
        }

        private void ValidateSettings(IShippingSettings settings)
        {
            if (string.IsNullOrEmpty(settings.shipping_provider))
                throw new ArgumentNullException("Shipping provider cannot be null or empty.");
        }
    }
}
