using System.Security;
using KosmosERP.Models.Interfaces;

namespace KosmosERP.BusinessLayer.AuthenticiationProviders.Models;

public class SAMLMetadata
{
    private readonly IAuthenticationSettings _AuthenticationSettings;

    public SAMLMetadata(IAuthenticationSettings authenticationSettings)
    {
        _AuthenticationSettings = authenticationSettings;
    }

    public string Build()
    {
        var entityId = SecurityElement.Escape(SAMLServiceProvider.EntityId(_AuthenticationSettings));
        var acsUrl = SecurityElement.Escape(SAMLServiceProvider.AssertionConsumerServiceUrl(_AuthenticationSettings));
        var sloUrl = SecurityElement.Escape(SAMLServiceProvider.SingleLogoutServiceUrl(_AuthenticationSettings));

        return
            "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" +
            "<md:EntityDescriptor " +
                "xmlns:md=\"urn:oasis:names:tc:SAML:2.0:metadata\" " +
                $"entityID=\"{entityId}\">" +
                "<md:SPSSODescriptor " +
                    "AuthnRequestsSigned=\"false\" " +
                    "WantAssertionsSigned=\"true\" " +
                    "protocolSupportEnumeration=\"urn:oasis:names:tc:SAML:2.0:protocol\">" +
                    "<md:SingleLogoutService " +
                        "Binding=\"urn:oasis:names:tc:SAML:2.0:bindings:HTTP-POST\" " +
                        $"Location=\"{sloUrl}\" />" +
                    "<md:NameIDFormat>urn:oasis:names:tc:SAML:1.1:nameid-format:emailAddress</md:NameIDFormat>" +
                    "<md:AssertionConsumerService " +
                        "Binding=\"urn:oasis:names:tc:SAML:2.0:bindings:HTTP-POST\" " +
                        $"Location=\"{acsUrl}\" " +
                        "index=\"0\" " +
                        "isDefault=\"true\" />" +
                "</md:SPSSODescriptor>" +
            "</md:EntityDescriptor>";
    }
}
