using KosmosERP.Api.Models;
using KosmosERP.Database;
using KosmosERP.Module;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;
using System.Security.Claims;

namespace KosmosERP.Api.Authorization;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public sealed class ERPAuthorizeAttribute : Attribute, IAsyncAuthorizationFilter
{
    public IReadOnlyList<string> Roles { get; }
    public IReadOnlyList<ERPPermission> Permissions { get; }

    // Module id is constant per controller type; resolve once, then reuse.
    private static readonly ConcurrentDictionary<Type, Guid?> _moduleIdByController = new();

    public ERPAuthorizeAttribute(ERPPermission[] permissions, params string[] roles)
    {
        Permissions = (permissions ?? Array.Empty<ERPPermission>())
            .Distinct()
            .ToArray();

        Roles = (roles ?? Array.Empty<string>())
            .SelectMany(r => (r ?? string.Empty)
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;

        // Both paths require an authenticated user.
        if (user?.Identity is null || !user.Identity.IsAuthenticated)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        // Path 1 — custom authorization (implement IERPAuthorizationHandler).
        var handler = context.HttpContext.RequestServices.GetService<IERPAuthorizationHandler>();
        var dbContext = context.HttpContext.RequestServices.GetService<IBaseERPContext>();

        if (handler is not null && dbContext is not null)
        {
            var moduleId = ResolveModuleId(context);

            var decision = await handler.AuthorizeAsync(context.HttpContext, user.Identity, moduleId, dbContext, Permissions);
            if (decision.HasValue)
            {
                if (!decision.Value)
                    context.Result = new UnauthorizedResult();

                return;
            }
        }

        // Path 2 — default: the user must hold one of the required roles as a claim.
        if (!HasAnyRole(user, Roles))
            context.Result = new UnauthorizedResult();
    }

    /// <summary>
    /// Resolves the module id for the executing controller by finding its ERP
    /// module dependency and reading <see cref="IBaseERPModule.ModuleIdentifier"/>.
    /// The controller is never instantiated — only the module service is resolved,
    /// and the resulting id is cached per controller type.
    /// </summary>
    private static Guid? ResolveModuleId(AuthorizationFilterContext context)
    {
        if (context.ActionDescriptor is not ControllerActionDescriptor descriptor)
            return null;

        var controllerType = descriptor.ControllerTypeInfo.AsType();

        return _moduleIdByController.GetOrAdd(controllerType, _ =>
        {
            // The controller takes its module (e.g. ICustomerModule) via the constructor.
            var moduleParamType = controllerType.GetConstructors()
                .SelectMany(c => c.GetParameters())
                .Select(p => p.ParameterType)
                .FirstOrDefault(pt => typeof(IBaseERPModule).IsAssignableFrom(pt));

            if (moduleParamType is null)
                return (Guid?)null;

            var module = context.HttpContext.RequestServices.GetService(moduleParamType) as IBaseERPModule;
            return module?.ModuleIdentifier;
        });
    }

    private static bool HasAnyRole(ClaimsPrincipal user, IReadOnlyList<string> roles)
    {
        // No roles specified => being authenticated is sufficient.
        if (roles.Count == 0)
            return true;

        foreach (var role in roles)
        {
            if (user.IsInRole(role)
                || user.HasClaim(ClaimTypes.Role, role)
                || user.HasClaim("role", role)
                || user.HasClaim("roles", role))
            {
                return true;
            }
        }

        return false;
    }
}
