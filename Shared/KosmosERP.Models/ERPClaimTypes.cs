namespace KosmosERP.Models;

/// <summary>
/// Claim types added by the API (not issued by any identity provider).
/// </summary>
public static class ERPClaimTypes
{
    /// <summary>
    /// The calling user's <c>User.guid</c>, added per request by the API's claims
    /// transformation after the token is validated.
    /// </summary>
    public const string UserGuid = "kosmos_user_guid";
}
