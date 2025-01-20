using Microsoft.IdentityModel.Tokens;
using Prometheus.Models.Interfaces;
using Prometheus.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text;

namespace Prometheus.Api.Modules;

public interface ITokenModule
{
    Response<JwtToken> Request(string username, string password);
}

public class TokenModule : ITokenModule
{
    public static string Issuer { get { return "Prometheus"; } }
    public static int Expires { get { return 10000; } }

    private IAuthenticationSettings _AuthenticationSettings;

    public TokenModule(IAuthenticationSettings settings)
    {
        _AuthenticationSettings = settings;
    }

    public Response<JwtToken> Request(string username, string password)
    {
        Response<JwtToken> response = new Response<JwtToken>();

        try
        {
            if (username == _AuthenticationSettings.APIUsername && password == _AuthenticationSettings.APIPassword)
                response.Data = CreateSecurityToken(_AuthenticationSettings.APIPrivateKey);
            else
                response.Success = false;
        }
        catch (Exception e)
        {
            response.SetException(e);
        }

        return response;
    }

    public static SymmetricSecurityKey CreateSecurityKey(string privateKey)
    {
        return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(privateKey));
    }

    public static JwtToken CreateSecurityToken(string privateKey)
    {
        return new JwtToken(new JwtSecurityToken(
            issuer: TokenModule.Issuer,
            expires: DateTime.Now.AddMinutes(TokenModule.Expires),
            signingCredentials: new SigningCredentials(TokenModule.CreateSecurityKey(privateKey), SecurityAlgorithms.HmacSha256Signature)));
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
