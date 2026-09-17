
namespace KosmosERP.BusinessLayer.AuthenticiationProviders.Models;

public class SAMLRequest
{
    public string? Id { get; set; }
    public string Request { get; set; }
    public string RedirectUrl { get; set; }
}
