using System.Security.Cryptography.Xml;
using System.Text;
using System.Xml;
using KosmosERP.Models.Interfaces;

namespace KosmosERP.BusinessLayer.AuthenticiationProviders.Models;

/// <summary>
/// Parses and validates the LogoutResponse the IdP returns in reply to an
/// SP-initiated LogoutRequest.
/// </summary>
public class SAMLLogoutResponse
{
    public const string StatusSuccess = "urn:oasis:names:tc:SAML:2.0:status:Success";

    public XmlDocument SAMLDocument { get; private set; }
    private readonly IAuthenticationSettings _AuthenticationSettings;

    public SAMLLogoutResponse(IAuthenticationSettings authenticationSettings)
    {
        _AuthenticationSettings = authenticationSettings;
    }

    public void LoadXml(string xml)
    {
        SAMLDocument = new XmlDocument
        {
            PreserveWhitespace = true,
            XmlResolver = null
        };

        SAMLDocument.LoadXml(xml);
    }

    public void LoadXmlFromBase64(string response)
    {
        LoadXml(Encoding.UTF8.GetString(Convert.FromBase64String(response)));
    }

    /// <summary>
    /// Verifies the message signature against the configured IdP certificate.
    /// Returns true when the response carries no signature (some IdPs sign the
    /// binding rather than the message); callers still rely on <see cref="IsSuccess"/>.
    /// </summary>
    public bool IsValid()
    {
        var xmlNamespaceManager = new XmlNamespaceManager(SAMLDocument.NameTable);
        xmlNamespaceManager.AddNamespace("ds", "http://www.w3.org/2000/09/xmldsig#");

        var xmlNodeList = SAMLDocument.SelectNodes("//ds:Signature", xmlNamespaceManager);

        if (xmlNodeList == null || xmlNodeList.Count == 0)
            return true;

        var certificate = new SAMLCertificate();
        certificate.LoadCertificate(_AuthenticationSettings.IdPCertificate);

        var signedXml = new SignedXml(SAMLDocument);
        signedXml.LoadXml((XmlElement)xmlNodeList[0]);

        return signedXml.CheckSignature(certificate.Certificate, true);
    }

    /// <summary>
    /// True when the IdP reported a successful logout (top-level StatusCode).
    /// </summary>
    public bool IsSuccess()
    {
        var xmlNamespaceManager = new XmlNamespaceManager(SAMLDocument.NameTable);
        xmlNamespaceManager.AddNamespace("samlp", "urn:oasis:names:tc:SAML:2.0:protocol");

        var xmlNode = SAMLDocument.SelectSingleNode("/samlp:LogoutResponse/samlp:Status/samlp:StatusCode", xmlNamespaceManager);

        return xmlNode?.Attributes?["Value"]?.Value == StatusSuccess;
    }

    /// <summary>
    /// The ID of the LogoutRequest this response answers.
    /// </summary>
    public string GetInResponseTo()
    {
        var xmlNamespaceManager = new XmlNamespaceManager(SAMLDocument.NameTable);
        xmlNamespaceManager.AddNamespace("samlp", "urn:oasis:names:tc:SAML:2.0:protocol");

        var xmlNode = SAMLDocument.SelectSingleNode("/samlp:LogoutResponse/@InResponseTo", xmlNamespaceManager);

        return xmlNode != null ? xmlNode.Value : "";
    }
}
