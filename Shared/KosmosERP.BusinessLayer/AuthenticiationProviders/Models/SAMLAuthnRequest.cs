using System.Globalization;
using System.IO.Compression;
using System.Security;
using System.Text;
using KosmosERP.Models.Interfaces;

namespace KosmosERP.BusinessLayer.AuthenticiationProviders.Models;

/// <summary>
/// Builds a SAML 2.0 SP-initiated AuthnRequest and encodes it for the
/// HTTP-Redirect binding.
/// </summary>
public class SAMLAuthnRequest
{
    private readonly IAuthenticationSettings _AuthenticationSettings;

    public SAMLAuthnRequest(IAuthenticationSettings authenticationSettings)
    {
        _AuthenticationSettings = authenticationSettings;
    }

    public SAMLRequest Build(string? relayState)
    {
        var idpUrl = _AuthenticationSettings.Authority;
        var entityId = SAMLServiceProvider.EntityId(_AuthenticationSettings);
        var acsUrl = SAMLServiceProvider.AssertionConsumerServiceUrl(_AuthenticationSettings);

        var id = "_" + Guid.NewGuid().ToString("N");
        var issueInstant = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture);

        var xml =
            "<samlp:AuthnRequest " +
                "xmlns:samlp=\"urn:oasis:names:tc:SAML:2.0:protocol\" " +
                "xmlns:saml=\"urn:oasis:names:tc:SAML:2.0:assertion\" " +
                $"ID=\"{id}\" " +
                "Version=\"2.0\" " +
                $"IssueInstant=\"{issueInstant}\" " +
                $"Destination=\"{SecurityElement.Escape(idpUrl)}\" " +
                "ProtocolBinding=\"urn:oasis:names:tc:SAML:2.0:bindings:HTTP-POST\" " +
                $"AssertionConsumerServiceURL=\"{SecurityElement.Escape(acsUrl)}\">" +
                $"<saml:Issuer>{SecurityElement.Escape(entityId)}</saml:Issuer>" +
                "<samlp:NameIDPolicy " +
                    "Format=\"urn:oasis:names:tc:SAML:1.1:nameid-format:emailAddress\" " +
                    "AllowCreate=\"true\" />" +
            "</samlp:AuthnRequest>";

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
