using Microsoft.IdentityModel.Tokens;
using Prometheus.Models.Interfaces;
using Prometheus.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text;
using System.Security.Claims;

namespace Prometheus.BusinessLayer.Modules;

public interface ITokenModule
{
}

public class TokenModule : ITokenModule
{
    public static string Issuer { get { return "Prometheus"; } }
    public static int Expires { get { return 60000; } }

    private IAuthenticationSettings _AuthenticationSettings;

    public TokenModule(IAuthenticationSettings settings)
    {
        _AuthenticationSettings = settings;
    }

    public static SymmetricSecurityKey CreateSecurityKey(string privateKey)
    {
        return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(privateKey));
    }

    public static JwtToken CreateSecurityToken(string username, string privateKey)
    {
        var tokenHandler = new JwtSecurityTokenHandler();

        var claim = new SecurityTokenDescriptor()
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, username)
            }),
            Issuer = TokenModule.Issuer,
            Expires = DateTime.UtcNow.AddMinutes(TokenModule.Expires),
            SigningCredentials = new SigningCredentials(TokenModule.CreateSecurityKey(privateKey), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateJwtSecurityToken(claim);

        return new JwtToken(token);


    }

    public static string CreateAPIKey(string username, string password)
    {
        return HashString(username + ":" + password);
    }

    public static string HashPassword(string password)
    {
        return HashString(password);
    }

    private static string HashString(string input)
    {
        using (MD5 md5 = MD5.Create())
        {
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
            byte[] hashBytes = md5.ComputeHash(inputBytes);

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hashBytes.Length; i++)
            {
                sb.Append(hashBytes[i].ToString("X2"));
            }

            return sb.ToString();
        }
    }
}
