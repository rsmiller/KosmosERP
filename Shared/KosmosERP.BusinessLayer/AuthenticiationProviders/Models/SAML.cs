
namespace KosmosERP.BusinessLayer.AuthenticiationProviders.Models;

public class SAML
{
    public int Id { get; set; }
    public string SAMLKey { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

}
