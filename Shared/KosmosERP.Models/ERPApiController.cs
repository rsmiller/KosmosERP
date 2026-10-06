using KosmosERP.Models;
using Microsoft.AspNetCore.Mvc;

using System.Security.Claims;

namespace KosmosERP.Module;

public class ERPApiController : ControllerBase 
{
    private IBaseERPModule _Module;
    public static Guid? ModuleIdentifier { get; private set; }
    public ERPApiController(IBaseERPModule module)
    {
        _Module = module;
        ModuleIdentifier = module.ModuleIdentifier;
    }

    /// <summary>
    /// Gets the current user's <c>User.guid</c>, the value stamped into <c>calling_user_id</c>
    /// and audit fields. The API resolves it from the signed-in username for every
    /// authentication provider (database, Keycloak, SAML) and adds it as the
    /// <see cref="ERPClaimTypes.UserGuid"/> claim. Null when the signed-in user has no
    /// active database user.
    /// </summary>
    protected string? CurrentUserId
    {
        get
        {
            return User.GetUserGuid();
        }
    }

    /// <summary>
    /// Gets the current user's username from the token claims.
    /// Returns the 'preferred_username' claim.
    /// </summary>
    protected string? CurrentUsername
    {
        get
        {
            return User?.FindFirst("preferred_username")?.Value 
                ?? User?.FindFirst(ClaimTypes.Name)?.Value;
        }
    }

    /// <summary>
    /// Gets the current user's email from the Keycloak token claims.
    /// </summary>
    protected string? CurrentUserEmail
    {
        get
        {
            return User?.FindFirst(ClaimTypes.Email)?.Value 
                ?? User?.FindFirst("email")?.Value;
        }
    }

    /// <summary>
    /// Gets all roles assigned to the current user from the Keycloak token.
    /// </summary>
    protected IEnumerable<string> CurrentUserRoles
    {
        get
        {
            return User?.FindAll(ClaimTypes.Role)?.Select(c => c.Value) 
                ?? Enumerable.Empty<string>();
        }
    }
}

