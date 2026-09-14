using KosmosERP.Models.Interfaces;

namespace KosmosERP.Models;

public class PaymentProviderSettings : IPaymentProviderSettings
{
    public string? payment_provider { get; set; }
    public string? square_token { get; set; }
    public string? stripe_api_key { get; set; }
}
