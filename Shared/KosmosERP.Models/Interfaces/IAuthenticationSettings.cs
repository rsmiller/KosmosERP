namespace KosmosERP.Models.Interfaces;

public interface IAuthenticationSettings
{
    string APIPrivateKey { get; set; }
    string AuthenticationProvider { get; set; }
    string Authority { get; set; }
    string Audience { get; set; }
    string TokenURL { get; set; }
    string ClientSecret { get; set; }
    string BaseURL { get; set; }
    string Realm { get; set; }
    string IdPCertificate { get; set; }
}
