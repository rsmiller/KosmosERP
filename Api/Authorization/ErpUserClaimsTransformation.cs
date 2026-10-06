using KosmosERP.Database;
using KosmosERP.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace KosmosERP.Api.Authorization;

/// <summary>
/// Adds the signed-in user's <c>User.guid</c> as the <see cref="ERPClaimTypes.UserGuid"/>
/// claim, which <c>ERPApiController.CurrentUserId</c> reads for <c>calling_user_id</c>.
///
/// The user is found by username (<c>Identity.Name</c>), the same lookup
/// <see cref="ErpCustomAuthorizationHandler"/> uses, so this works the same for database,
/// Keycloak and SAML tokens without putting anything extra in the token. Unknown, deleted
/// and disabled users (including the system service account) get no claim.
/// </summary>
public class ErpUserClaimsTransformation : IClaimsTransformation
{
    private readonly IBaseERPContext _Context;

    public ErpUserClaimsTransformation(IBaseERPContext context)
    {
        _Context = context;
    }

    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        // TransformAsync can run more than once per request.
        if (principal.Identity is not { IsAuthenticated: true } identity
            || string.IsNullOrEmpty(identity.Name)
            || principal.HasClaim(c => c.Type == ERPClaimTypes.UserGuid))
        {
            return principal;
        }

        var userGuid = await _Context.Users
            .Where(u => u.username == identity.Name && u.is_deleted == false && u.is_disabled == false)
            .Select(u => u.guid)
            .FirstOrDefaultAsync();

        if (string.IsNullOrEmpty(userGuid))
            return principal;

        var transformed = principal.Clone();
        ((ClaimsIdentity)transformed.Identity!).AddClaim(new Claim(ERPClaimTypes.UserGuid, userGuid));

        return transformed;
    }
}
