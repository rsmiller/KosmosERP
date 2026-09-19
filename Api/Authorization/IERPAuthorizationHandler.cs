using KosmosERP.Api.Models;
using KosmosERP.Database;
using Microsoft.AspNetCore.Http;
using System.Security.Principal;

namespace KosmosERP.Api.Authorization;

/// <summary>
/// Custom authorization seam used by <see cref="ERPAuthorizeAttribute"/>.
///
/// Implement this to authorize a request against your own source of truth — for
/// example, the calling user's database permissions resolved via
/// UserRole -> Role -> RolePermission (read/write/edit/delete per module).
///
/// Return value semantics:
///   <c>true</c>  -> allow the request,
///   <c>false</c> -> deny the request (403),
///   <c>null</c>  -> defer to the default claim/role check.
/// </summary>
public interface IERPAuthorizationHandler
{
    Task<bool?> AuthorizeAsync(HttpContext context, IIdentity user, Guid? moduleId, IBaseERPContext dbContext, IReadOnlyList<ERPPermission> permissions);
}
