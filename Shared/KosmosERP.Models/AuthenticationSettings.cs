using KosmosERP.Models.Interfaces;

namespace KosmosERP.Models;

public class AuthenticationSettings : IAuthenticationSettings
{
    public string APIPrivateKey { get; set; }
}
