using KosmosERP.BusinessLayer.AuthenticationProviders;
using KosmosERP.BusinessLayer.Interfaces;
using KosmosERP.BusinessLayer.MessagePublisher;
using KosmosERP.Database;
using KosmosERP.Models;
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

        if (settings.AuthenticationProvider.Equals(AuthenticiationProviders.Database, StringComparison.OrdinalIgnoreCase))
        {
            _AuthenticationProvider = new DatabaseAuthenticationProvider(settings, context);
        }
        else if (settings.AuthenticationProvider.Equals(AuthenticiationProviders.Keycloak, StringComparison.OrdinalIgnoreCase))
        {

            _AuthenticationProvider = new KeycloakAuthenticationProvider(settings, context);
        }
        else if (settings.AuthenticationProvider.Equals(AuthenticiationProviders.MOCK, StringComparison.OrdinalIgnoreCase))
        {

            _AuthenticationProvider = new MockAuthenticationProvider(settings);
        }
        else
        {
            throw new ArgumentNullException("Messaging account provider not supported.");
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
