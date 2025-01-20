using System.IdentityModel.Tokens.Jwt;

namespace Prometheus.Models;

public class JwtToken
{
    private JwtSecurityToken token;

    public long ValidTo { get { return DateTimeOffset.Parse(token.ValidTo.ToString()).ToUnixTimeMilliseconds(); } }
    public string Value { get { return new JwtSecurityTokenHandler().WriteToken(token); } }

    public JwtToken(JwtSecurityToken securityToken)
    {
        token = securityToken;
    }
}
