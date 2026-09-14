
namespace KosmosERP.Models.Interfaces;

public interface IPaymentProviderSettings
{
    string? payment_provider { get; set; }
    string? square_token { get; set; }
     string? stripe_api_key { get; set; }
}
