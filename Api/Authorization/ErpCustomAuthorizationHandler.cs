using KosmosERP.Api.Models;
using KosmosERP.Database;
using Microsoft.EntityFrameworkCore;
using System.Security.Principal;

namespace KosmosERP.Api.Authorization;

/// <summary>
/// Placeholder for the custom authorization path. It currently returns
/// <c>null</c>, which defers every request to <see cref="ERPAuthorizeAttribute"/>'s
/// default claim/role check — so behavior is unchanged until you implement it.
///
/// To turn on custom authorization, resolve the calling user (e.g. from the
/// token's NameIdentifier/session), load their permissions via
/// UserRole -> Role -> RolePermission, map those to the <paramref name="requiredRoles"/>
/// scheme (e.g. "customers_read"), and return true/false. Return null for any
/// case you want handled by the default check (e.g. Keycloak realm roles).
/// </summary>
public class ErpCustomAuthorizationHandler : IERPAuthorizationHandler
{
    public async Task<bool?> AuthorizeAsync(HttpContext context, IIdentity user, Guid? moduleId, IBaseERPContext dbContext, IReadOnlyList<ERPPermission> permissions)
    {
        var dbUser = await dbContext.Users.FirstOrDefaultAsync(u => u.username == user.Name && u.is_deleted == false && u.is_disabled == false);

        if(dbUser == null || moduleId == null)
            return false;

        if (dbUser.is_admin)
            return true;

        var rolePermissions = await (from u in dbContext.Users
                            join ur in dbContext.UserRoles on u.id equals ur.user_id
                            join rp in dbContext.RolePermissions on ur.role_id equals rp.role_id
                            where rp.module_id == moduleId.ToString()
                            select rp ).ToListAsync();

        if (rolePermissions.Any())
        {
            bool found = false;
            foreach (var permission in permissions)
            {
                switch (permission)
                {
                    case ERPPermission.Read:
                        if (!rolePermissions.Any(rp => rp.read))
                            found = true;
                        break;
                    case ERPPermission.Write:
                        if (!rolePermissions.Any(rp => rp.write))
                            found = true;
                        break;
                    case ERPPermission.Edit:
                        if (!rolePermissions.Any(rp => rp.edit))
                            found = true;
                        break;
                    case ERPPermission.Delete:
                        if (!rolePermissions.Any(rp => rp.delete))
                            found = true;
                        break;
                }
            }

            return found;
        }
        else
        {
            return false;
        }
    }
}
