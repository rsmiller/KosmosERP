using System.Reflection;
using System.Security.Claims;
using System.Security.Principal;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using KosmosERP.Api.Authorization;
using KosmosERP.Api.Models;
using KosmosERP.BusinessLayer.Helpers;
using KosmosERP.Database;
using KosmosERP.Database.Models;

namespace KosmosERP.Tests.Shared;

/// <summary>
/// API authorization (BUG-026): the database-permission handler, the
/// [ERPAuthorize] attribute, and a guard that every endpoint declares a rule.
/// </summary>
public class AuthorizationTests
{
    private static readonly Guid ModuleId = Guid.NewGuid();
    private static readonly Guid OtherModuleId = Guid.NewGuid();

    private ERPDbContext _Context = null!;
    private readonly ErpCustomAuthorizationHandler _Handler = new();

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<DbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _Context = new ERPDbContext(options);
    }

    [TearDown]
    public void TearDown() => _Context.Dispose();

    // ---- Handler -------------------------------------------------------------

    [Test]
    public async Task Handler_AdminUser_IsAllowedEverything()
    {
        AddUser("ada", isAdmin: true);

        Assert.That(await Authorize("ada", ERPPermission.Delete), Is.True);
        Assert.That(await Authorize("ada", roles: "admin"), Is.True);
    }

    [Test]
    public async Task Handler_UserWithGrantingRole_IsAllowed()
    {
        var user = AddUser("rita");
        GrantRole(user, ModuleId, read: true);

        Assert.That(await Authorize("rita", ERPPermission.Read), Is.True);
    }

    [Test]
    public async Task Handler_UserWithoutPermission_IsDenied()
    {
        var user = AddUser("rita");
        GrantRole(user, ModuleId, read: true);

        Assert.That(await Authorize("rita", ERPPermission.Write), Is.False);
    }

    [Test]
    public async Task Handler_AnotherUsersRole_DoesNotGrantAccess()
    {
        // Regression: the permission query used to ignore which user was calling.
        AddUser("rita");
        var other = AddUser("olga");
        GrantRole(other, ModuleId, read: true, write: true, edit: true, delete: true);

        Assert.That(await Authorize("rita", ERPPermission.Read), Is.False);
    }

    [Test]
    public async Task Handler_RequiresEveryPermission()
    {
        var user = AddUser("rita");
        GrantRole(user, ModuleId, read: true);

        Assert.That(await Authorize("rita", ERPPermission.Read, ERPPermission.Edit), Is.False);
    }

    [Test]
    public async Task Handler_PermissionsCanComeFromDifferentRoles()
    {
        var user = AddUser("rita");
        GrantRole(user, ModuleId, read: true);
        GrantRole(user, ModuleId, edit: true);

        Assert.That(await Authorize("rita", ERPPermission.Read, ERPPermission.Edit), Is.True);
    }

    [Test]
    public async Task Handler_PermissionOnAnotherModule_DoesNotCount()
    {
        var user = AddUser("rita");
        GrantRole(user, OtherModuleId, read: true);

        Assert.That(await Authorize("rita", ERPPermission.Read), Is.False);
    }

    [Test]
    public async Task Handler_DeletedRoleAssignment_DoesNotCount()
    {
        var user = AddUser("rita");
        var userRole = GrantRole(user, ModuleId, read: true);
        userRole.is_deleted = true;
        _Context.SaveChanges();

        Assert.That(await Authorize("rita", ERPPermission.Read), Is.False);
    }

    [Test]
    public async Task Handler_AdminRole_IsAdminOnly()
    {
        var user = AddUser("rita");
        GrantRole(user, ModuleId, read: true, write: true, edit: true, delete: true);

        Assert.That(await Authorize("rita", roles: "admin"), Is.False);
    }

    [Test]
    public async Task Handler_NoRequiredPermissions_AllowsAnySignedInUser()
    {
        AddUser("rita");

        Assert.That(await Authorize("rita"), Is.True);
    }

    [Test]
    public async Task Handler_UnknownUser_DefersToRoleClaims()
    {
        Assert.That(await Authorize("keycloak-only-user", ERPPermission.Read), Is.Null);
    }

    [Test]
    public async Task Handler_DisabledUser_DefersToRoleClaims()
    {
        var user = AddUser("rita", isAdmin: true);
        user.is_disabled = true;
        _Context.SaveChanges();

        Assert.That(await Authorize("rita", ERPPermission.Read), Is.Null);
    }

    // ---- Attribute -----------------------------------------------------------

    [Test]
    public async Task Attribute_Anonymous_Is401()
    {
        var context = FilterContext(new ClaimsPrincipal(new ClaimsIdentity()));

        await new ERPAuthorizeAttribute(new[] { ERPPermission.Read }).OnAuthorizationAsync(context);

        Assert.That(context.Result, Is.InstanceOf<UnauthorizedResult>());
    }

    [Test]
    public async Task Attribute_SignedInButNotAllowed_Is403()
    {
        AddUser("rita");
        var context = FilterContext(SignedIn("rita"));

        await new ERPAuthorizeAttribute(new ERPPermission[] { }, "admin").OnAuthorizationAsync(context);

        Assert.That(context.Result, Is.InstanceOf<ForbidResult>());
    }

    [Test]
    public async Task Attribute_Allowed_LeavesRequestAlone()
    {
        AddUser("ada", isAdmin: true);
        var context = FilterContext(SignedIn("ada"));

        await new ERPAuthorizeAttribute(new ERPPermission[] { }, "admin").OnAuthorizationAsync(context);

        Assert.That(context.Result, Is.Null);
    }

    [Test]
    public async Task Attribute_UnknownUser_UsesRoleClaims()
    {
        var withRole = FilterContext(SignedIn("kc-user", "admin"));
        var withoutRole = FilterContext(SignedIn("kc-user"));

        var attribute = new ERPAuthorizeAttribute(new ERPPermission[] { }, "admin");
        await attribute.OnAuthorizationAsync(withRole);
        await attribute.OnAuthorizationAsync(withoutRole);

        Assert.That(withRole.Result, Is.Null);
        Assert.That(withoutRole.Result, Is.InstanceOf<ForbidResult>());
    }

    // ---- Every endpoint declares a rule ----------------------------------------

    /// <summary>
    /// Endpoints that are deliberately public. Adding one here should be a
    /// conscious decision; the reason is commented on each action/controller.
    /// </summary>
    private static readonly string[] PublicEndpoints =
    {
        "Diagnostics.Health",
        "Docs.*",
        "SAML.*",
        "Settings.GetBaseSettings",
        "User.Authenticate", // POST /User/AuthenticateUser
    };

    [Test]
    public void EveryEndpoint_HasERPAuthorizeOrAllowAnonymous()
    {
        var unprotected = ApiActions()
            .Where(a => !HasAttribute<ERPAuthorizeAttribute>(a) && !HasAttribute<AllowAnonymousAttribute>(a))
            .Select(Name)
            .ToList();

        Assert.That(unprotected, Is.Empty, "Add [ERPAuthorize(...)] (or a deliberate, commented [AllowAnonymous]).");
    }

    [Test]
    public void PublicEndpoints_AreExactlyTheIntendedOnes()
    {
        var anonymous = ApiActions()
            .Where(HasAttribute<AllowAnonymousAttribute>)
            .Select(a => HasAttribute<AllowAnonymousAttribute>(a.DeclaringType!) ? $"{ControllerName(a)}.*" : Name(a))
            .Distinct()
            .OrderBy(n => n)
            .ToList();

        Assert.That(anonymous, Is.EqualTo(PublicEndpoints.OrderBy(n => n).ToList()));
    }

    // ---- Helpers -------------------------------------------------------------

    private User AddUser(string username, bool isAdmin = false)
    {
        var user = CommonDataHelper<User>.FillCommonFields(new User
        {
            first_name = username,
            last_name = "test",
            username = username,
            password = "x",
            password_salt = "x",
            employee_number = "1",
            guid = Guid.NewGuid().ToString(),
            external_id = Guid.NewGuid().ToString(),
            is_admin = isAdmin,
        }, 1);
        _Context.Users.Add(user);
        _Context.SaveChanges();
        return user;
    }

    private UserRole GrantRole(User user, Guid moduleId, bool read = false, bool write = false, bool edit = false, bool delete = false)
    {
        var role = CommonDataHelper<Role>.FillCommonFields(new Role { name = "role-" + Guid.NewGuid() }, 1);
        _Context.Roles.Add(role);
        _Context.SaveChanges();

        _Context.RolePermissions.Add(CommonDataHelper<RolePermission>.FillCommonFields(new RolePermission
        {
            role_id = role.id,
            module_id = moduleId.ToString(),
            read = read,
            write = write,
            edit = edit,
            delete = delete,
        }, 1));

        var userRole = CommonDataHelper<UserRole>.FillCommonFields(new UserRole { user_id = user.id, role_id = role.id }, 1);
        _Context.UserRoles.Add(userRole);
        _Context.SaveChanges();
        return userRole;
    }

    private Task<bool?> Authorize(string username, params ERPPermission[] permissions) =>
        _Handler.AuthorizeAsync(new DefaultHttpContext(), new GenericIdentity(username), ModuleId, _Context, permissions, Array.Empty<string>());

    private Task<bool?> Authorize(string username, string roles) =>
        _Handler.AuthorizeAsync(new DefaultHttpContext(), new GenericIdentity(username), ModuleId, _Context, Array.Empty<ERPPermission>(), new[] { roles });

    private static ClaimsPrincipal SignedIn(string username, params string[] roles)
    {
        var claims = new List<Claim> { new(ClaimTypes.Name, username) };
        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));
        return new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
    }

    private AuthorizationFilterContext FilterContext(ClaimsPrincipal user)
    {
        var services = new ServiceCollection()
            .AddSingleton<IERPAuthorizationHandler>(_Handler)
            .AddSingleton<IBaseERPContext>(_Context)
            .BuildServiceProvider();

        var httpContext = new DefaultHttpContext { User = user, RequestServices = services };
        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
        return new AuthorizationFilterContext(actionContext, new List<IFilterMetadata>());
    }

    private static IEnumerable<MethodInfo> ApiActions() =>
        typeof(ERPAuthorizeAttribute).Assembly.GetTypes()
            .Where(t => typeof(ControllerBase).IsAssignableFrom(t) && !t.IsAbstract)
            .SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            .Where(m => m.GetCustomAttributes<Microsoft.AspNetCore.Mvc.Routing.HttpMethodAttribute>().Any());

    private static bool HasAttribute<T>(MethodInfo method) where T : Attribute =>
        method.GetCustomAttribute<T>() != null || method.DeclaringType!.GetCustomAttribute<T>() != null;

    private static bool HasAttribute<T>(Type type) where T : Attribute => type.GetCustomAttribute<T>() != null;

    private static string ControllerName(MethodInfo method) => method.DeclaringType!.Name.Replace("Controller", "");

    private static string Name(MethodInfo method) => $"{ControllerName(method)}.{method.Name}";
}
