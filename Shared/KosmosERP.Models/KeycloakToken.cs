using KosmosERP.Models.Interfaces;

namespace KosmosERP.Models;

public class KeycloakToken : IAuthenticationToken
{
    public string access_token { get; set; }
    public long expires_in { get; set; }
    public string refresh_token { get; set; }
    public int refresh_expires_in { get; set; }
    public string token_type { get; set; }
}