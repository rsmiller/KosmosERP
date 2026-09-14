using KosmosERP.BusinessLayer.Interfaces;
using KosmosERP.BusinessLayer.PaymentProviders;
using KosmosERP.Database;
using KosmosERP.Models;
using KosmosERP.Models.Interfaces;

namespace KosmosERP.BusinessLayer;

public interface IPaymentProviderFactory {
    IPaymentProvider GetProvider();
}

public class PaymentProviderFactory : IPaymentProviderFactory
{
    private IBaseERPContext _Context;
    private IPaymentProviderSettings _Settings;
    private IPaymentProvider _PaymentProvider;

    public PaymentProviderFactory(IPaymentProviderSettings settings, IBaseERPContext context)
    {
        _Context = context;
        _Settings = settings;

        ValidateSettings(settings);

        if (settings.payment_provider.Equals(PaymentProviderType.Square, StringComparison.OrdinalIgnoreCase))
        {
            _PaymentProvider = new SquarePaymentProvider(settings);
        }
        else if (settings.payment_provider.Equals(PaymentProviderType.Stripe, StringComparison.OrdinalIgnoreCase))
        {
            _PaymentProvider = new StripePaymentProvider(settings);
        }
        else if (settings.payment_provider.Equals(PaymentProviderType.Paypal, StringComparison.OrdinalIgnoreCase))
        {
            _PaymentProvider = new PaypalPaymentProvider(settings);
        }
        else if (settings.payment_provider.Equals(PaymentProviderType.MOCK, StringComparison.OrdinalIgnoreCase))
        {
            _PaymentProvider = new MockPaymentProvider(settings);
        }
        else
        {
            throw new ArgumentNullException("Payment provider not supported.");
        }
    }

    public IPaymentProvider GetProvider()
    {
        return _PaymentProvider;
    }

    private void ValidateSettings(IPaymentProviderSettings settings)
    {
        if (string.IsNullOrEmpty(settings.payment_provider))
            throw new ArgumentNullException("Payment provider cannot be null or empty.");
    }
}
