
namespace KosmosERP.BusinessLayer.AuthenticiationProviders.Models;

public class SAMLLogoutCommand
{
    /// <summary>
    /// The application session to terminate. Used to resolve the IdP NameID for
    /// the LogoutRequest and to expire the local <c>UserSessionState</c>.
    /// </summary>
    public string? SessionId { get; set; }

    /// <summary>
    /// Optional opaque state the IdP returns unchanged on the LogoutResponse
    /// (typically the in-app URL to return to after logout completes).
    /// </summary>
    public string? RelayState { get; set; }
}
