using KosmosERP.Api.Models;
using KosmosERP.Database;
using Microsoft.EntityFrameworkCore;
using System.Security.Principal;

namespace KosmosERP.Api.Authorization;

/// <summary>
/// Authorizes database users against their own role permissions.
///
///   - Users not found in the database (e.g. Keycloak users) return <c>null</c>,
///     deferring to <see cref="ERPAuthorizeAttribute"/>'s claim/role check.
///   - <c>is_admin</c> users are allowed everything.
///   - An "admin" role on the attribute makes the action admin-only.
///   - No required permissions means any signed-in user is allowed.
///   - Otherwise every required permission must be granted, for the controller's
///     module, by at least one of the calling user's roles.
/// </summary>
public class ErpCustomAuthorizationHandler : IERPAuthorizationHandler
{
    public const string AdminRole = "admin";

    public async Task<bool?> AuthorizeAsync(HttpContext context, IIdentity user, Guid? moduleId, IBaseERPContext dbContext,
                                            IReadOnlyList<ERPPermission> permissions, IReadOnlyList<string> roles)
    {
        var dbUser = await dbContext.Users.FirstOrDefaultAsync(u => u.username == user.Name && u.is_deleted == false && u.is_disabled == false);

        if (dbUser == null)
            return null;

        if (dbUser.is_admin)
            return true;

        if (roles.Any(r => string.Equals(r, AdminRole, StringComparison.OrdinalIgnoreCase)))
            return false;

        if (permissions.Count == 0)
            return true;

        if (moduleId == null)
            return false;

        var rolePermissions = await (from ur in dbContext.UserRoles
                                     join r in dbContext.Roles on ur.role_id equals r.id
                                     join rp in dbContext.RolePermissions on r.id equals rp.role_id
                                     where ur.user_id == dbUser.id
                                        && rp.module_id == moduleId.ToString()
                                        && !ur.is_deleted && !r.is_deleted && !rp.is_deleted
                                     select rp).ToListAsync();

        return permissions.All(permission => permission switch
        {
            ERPPermission.Read => rolePermissions.Any(rp => rp.read),
            ERPPermission.Write => rolePermissions.Any(rp => rp.write),
            ERPPermission.Edit => rolePermissions.Any(rp => rp.edit),
            ERPPermission.Delete => rolePermissions.Any(rp => rp.delete),
            _ => false,
        });
    }
}
