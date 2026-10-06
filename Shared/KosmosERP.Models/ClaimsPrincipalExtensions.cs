using System.Security.Claims;

namespace KosmosERP.Models;

public static class ClaimsPrincipalExtensions
{
    /// <summary>
    /// The signed-in user's <c>User.guid</c> (the <see cref="ERPClaimTypes.UserGuid"/> claim the
    /// API adds), for <c>calling_user_id</c>. Null when the caller has no active database user.
    /// Never take the caller's id from the request body instead.
    /// </summary>
    public static string? GetUserGuid(this ClaimsPrincipal? user)
    {
        return user?.FindFirst(ERPClaimTypes.UserGuid)?.Value;
    }
}
