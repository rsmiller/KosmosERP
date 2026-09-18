using KosmosERP.Models.Interfaces;

namespace KosmosERP.Models;

public class AuthenticationSettings : IAuthenticationSettings
{
    public string APIPrivateKey { get; set; }
    public string AuthenticationProvider { get; set; }
    public string Authority { get; set; }
    public string Audience { get; set; }
    public string TokenURL { get; set; }
    public string ClientSecret { get; set; }
    public string BaseURL { get; set; }
    public string Realm { get; set; }
    public string IdPCertificate { get; set; }
    public string SingleLogoutURL { get; set; }
}
