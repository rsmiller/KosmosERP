using System.IdentityModel.Tokens.Jwt;
using KosmosERP.Models.Interfaces;

namespace KosmosERP.Models;

public class JwtToken : IAuthenticationToken
{
    private JwtSecurityToken token;

    public long expires_in { get { return DateTimeOffset.Parse(token.ValidTo.ToString()).ToUnixTimeMilliseconds(); }  set{}}
    public string access_token { get { return new JwtSecurityTokenHandler().WriteToken(token); }  set {}}

    public JwtToken(JwtSecurityToken securityToken)
    {
        token = securityToken;
    }
}
