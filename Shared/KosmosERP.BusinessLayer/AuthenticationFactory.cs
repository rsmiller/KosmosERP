using KosmosERP.BusinessLayer.AuthenticationProviders;
using KosmosERP.BusinessLayer.AuthenticiationProviders;
using KosmosERP.BusinessLayer.Interfaces;
using KosmosERP.Database;
using KosmosERP.Models.Interfaces;

namespace KosmosERP.BusinessLayer;

public interface IAuthenticationFactory {
    IAuthenticationProvider GetProvider();
}

public class AuthenticationFactory : IAuthenticationFactory
{
    private IBaseERPContext _Context;
    private IAuthenticationSettings _Settings;
    private IAuthenticationProvider _AuthenticationProvider;

    public AuthenticationFactory(IAuthenticationSettings settings, IBaseERPContext context)
    {
        _Context = context;
        _Settings = settings;
        
        ValidateSettings(settings);

        if (settings.AuthenticationProvider.Equals(KosmosERP.Models.AuthenticiationProviders.Database, StringComparison.OrdinalIgnoreCase))
        {
            _AuthenticationProvider = new DatabaseAuthenticationProvider(settings, context);
        }
        else if (settings.AuthenticationProvider.Equals(KosmosERP.Models.AuthenticiationProviders.Keycloak, StringComparison.OrdinalIgnoreCase))
        {

            _AuthenticationProvider = new KeycloakAuthenticationProvider(settings, context);
        }
        else if (settings.AuthenticationProvider.Equals(KosmosERP.Models.AuthenticiationProviders.SAML, StringComparison.OrdinalIgnoreCase))
        {

            _AuthenticationProvider = new SAMLAuthenticationProvider(settings, context);
        }
        else if (settings.AuthenticationProvider.Equals(KosmosERP.Models.AuthenticiationProviders.MOCK, StringComparison.OrdinalIgnoreCase))
        {

            _AuthenticationProvider = new MockAuthenticationProvider(settings);
        }
        else
        {
            throw new ArgumentNullException("Authentication provider not supported.");
        }
    }

    public IAuthenticationProvider GetProvider()
    {
        return _AuthenticationProvider;
    }

    private void ValidateSettings(IAuthenticationSettings settings)
    {
        if (string.IsNullOrEmpty(settings.AuthenticationProvider))
            throw new ArgumentNullException("Authenication Provider cannot be null or empty.");
    }
}
