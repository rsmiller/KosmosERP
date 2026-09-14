using Microsoft.AspNetCore.Mvc;

using System.Security.Claims;

namespace KosmosERP.Module;

public class ERPApiController : ControllerBase 
{
    private IBaseERPModule _Module;

    public ERPApiController(IBaseERPModule module)
    {
        _Module = module;
    }

    /// <summary>
    /// Gets the current user's ID from the Keycloak token claims.
    /// Returns the 'sub' (subject) claim which contains the user's unique identifier.
    /// </summary>
    protected string? CurrentUserId
    {
        get
        {
            return User?.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                ?? User?.FindFirst("sub")?.Value;
        }
    }

    /// <summary>
    /// Gets the current user's username from the Keycloak token claims.
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

