
namespace KosmosERP.BusinessLayer.PaymentProviders.Models;

public class PaymentTransaction
{
    public string identifier { get; set; }
    public bool success { get; set; } = false;
}