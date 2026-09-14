
namespace KosmosERP.BusinessLayer.PaymentProviders.Models;

public class PaymentCardGetDto
{
    public string identifier { get; set; }
    public string clientSecret { get; set; }
    public bool success { get; set; } = false;
}