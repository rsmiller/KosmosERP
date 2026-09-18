using System.Globalization;
using System.IO.Compression;
using System.Security;
using System.Text;
using KosmosERP.Models.Interfaces;

namespace KosmosERP.BusinessLayer.AuthenticiationProviders.Models;

/// <summary>
/// Builds a SAML 2.0 SP-initiated LogoutRequest and encodes it for the
/// HTTP-Redirect binding.
/// </summary>
public class SAMLLogoutRequest
{
    private readonly IAuthenticationSettings _AuthenticationSettings;

    public SAMLLogoutRequest(IAuthenticationSettings authenticationSettings)
    {
        _AuthenticationSettings = authenticationSettings;
    }

    /// <summary>
    /// Builds a LogoutRequest for the given subject (the IdP NameID, i.e. the
    /// user's external id) and returns a populated <see cref="SAMLRequest"/>
    /// containing the request id, the encoded request and the full IdP redirect URL.
    /// </summary>
    public SAMLRequest Build(string nameId, string? relayState)
    {
        var idpUrl = SAMLServiceProvider.IdentityProviderLogoutUrl(_AuthenticationSettings);
        var entityId = SAMLServiceProvider.EntityId(_AuthenticationSettings);

        var id = "_" + Guid.NewGuid().ToString("N");
        var issueInstant = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture);

        var xml =
            "<samlp:LogoutRequest " +
                "xmlns:samlp=\"urn:oasis:names:tc:SAML:2.0:protocol\" " +
                "xmlns:saml=\"urn:oasis:names:tc:SAML:2.0:assertion\" " +
                $"ID=\"{id}\" " +
                "Version=\"2.0\" " +
                $"IssueInstant=\"{issueInstant}\" " +
                $"Destination=\"{SecurityElement.Escape(idpUrl)}\">" +
                $"<saml:Issuer>{SecurityElement.Escape(entityId)}</saml:Issuer>" +
                "<saml:NameID " +
                    "Format=\"urn:oasis:names:tc:SAML:1.1:nameid-format:emailAddress\">" +
                    $"{SecurityElement.Escape(nameId)}</saml:NameID>" +
            "</samlp:LogoutRequest>";

        var encoded = DeflateAndEncode(xml);
        var redirectUrl = BuildRedirectUrl(idpUrl, encoded, relayState);

        return new SAMLRequest
        {
            Id = id,
            Request = encoded,
            RedirectUrl = redirectUrl,
            RelayState = relayState
        };
    }

    private static string DeflateAndEncode(string xml)
    {
        var bytes = Encoding.UTF8.GetBytes(xml);

        using var output = new MemoryStream();
        using (var deflate = new DeflateStream(output, CompressionMode.Compress, leaveOpen: true))
        {
            deflate.Write(bytes, 0, bytes.Length);
        }

        return Convert.ToBase64String(output.ToArray());
    }

    private static string BuildRedirectUrl(string idpUrl, string encodedRequest, string? relayState)
    {
        var sb = new StringBuilder(idpUrl);
        sb.Append(idpUrl.Contains('?') ? '&' : '?');
        sb.Append("SAMLRequest=").Append(Uri.EscapeDataString(encodedRequest));

        if (!string.IsNullOrEmpty(relayState))
            sb.Append("&RelayState=").Append(Uri.EscapeDataString(relayState));

        return sb.ToString();
    }
}
