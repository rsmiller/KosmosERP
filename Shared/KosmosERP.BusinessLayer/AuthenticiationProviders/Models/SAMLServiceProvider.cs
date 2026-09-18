using KosmosERP.Models.Interfaces;

namespace KosmosERP.BusinessLayer.AuthenticiationProviders.Models;

public static class SAMLServiceProvider
{
    public const string AssertionConsumerServicePath = "api/v1/SAML/consume";
    public const string SingleLogoutServicePath = "api/v1/SAML/logout/response";

    public static string EntityId(IAuthenticationSettings settings)
    {
        return !string.IsNullOrEmpty(settings.Audience)
            ? settings.Audience
            : NormalizeBaseUrl(settings.BaseURL);
    }

    public static string AssertionConsumerServiceUrl(IAuthenticationSettings settings)
    {
        return $"{NormalizeBaseUrl(settings.BaseURL)}/{AssertionConsumerServicePath}";
    }

    /// <summary>
    /// The absolute URL the IdP sends the LogoutResponse (and any IdP-initiated
    /// LogoutRequest) back to. Advertised as the SP SingleLogoutService.
    /// </summary>
    public static string SingleLogoutServiceUrl(IAuthenticationSettings settings)
    {
        return $"{NormalizeBaseUrl(settings.BaseURL)}/{SingleLogoutServicePath}";
    }

    /// <summary>
    /// The IdP endpoint the SP sends the LogoutRequest to. Prefers the configured
    /// SingleLogoutURL, falling back to the Authority used for SSO.
    /// </summary>
    public static string IdentityProviderLogoutUrl(IAuthenticationSettings settings)
    {
        return !string.IsNullOrEmpty(settings.SingleLogoutURL)
            ? settings.SingleLogoutURL
            : settings.Authority;
    }

    private static string NormalizeBaseUrl(string? baseUrl)
    {
        return (baseUrl ?? string.Empty).TrimEnd('/');
    }
}
