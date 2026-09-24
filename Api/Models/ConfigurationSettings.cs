using KosmosERP.Models.Interfaces;

namespace KosmosERP.Api.Models
{
    public record class ConfigurationSettings(IAuthenticationSettings authSettings,
                        IFileStorageSettings fileStorageSettings,
                        IMessagePublisherSettings messagePublisherSettings,
                        IPaymentProviderSettings paymentProviderSettings,
                        ILogProviderSettings logProviderSettings,
                        IHangfireSettings hangfireSettings,
                        IShippingSettings shippingSettings,
                        IDatabaseSettings databaseSettings)
    {
    }
}
