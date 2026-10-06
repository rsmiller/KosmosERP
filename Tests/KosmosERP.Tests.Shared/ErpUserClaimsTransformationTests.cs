using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using KosmosERP.Api.Authorization;
using KosmosERP.BusinessLayer.AuthenticationProviders;
using KosmosERP.BusinessLayer.Helpers;
using KosmosERP.Database;
using KosmosERP.Database.Models;
using KosmosERP.Models;

namespace KosmosERP.Tests.Shared;

/// <summary>
/// calling_user_id was null for database (username/password) logins: the controllers read
/// the Keycloak 'sub' claim, which database tokens don't have. The API now resolves the
/// signed-in username to the user's User.guid and adds it as a claim for every provider.
/// </summary>
public class ErpUserClaimsTransformationTests
{
    private ERPDbContext _Context = null!;
    private ErpUserClaimsTransformation _Transformation = null!;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<DbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _Context = new ERPDbContext(options);
        _Transformation = new ErpUserClaimsTransformation(_Context);
    }

    [TearDown]
    public void TearDown() => _Context.Dispose();

    [Test]
    public async Task KnownUser_GetsTheirGuid()
    {
        AddUser("other");
        var user = AddUser("admin");

        var result = await _Transformation.TransformAsync(SignedIn("admin"));

        Assert.That(result.FindFirst(ERPClaimTypes.UserGuid)?.Value, Is.EqualTo(user.guid));
    }

    [Test]
    public async Task KeycloakStyleToken_UsesUsernameNotSub()
    {
        var user = AddUser("kc-user");
        var identity = new ClaimsIdentity(new[]
        {
            new Claim("preferred_username", "kc-user"),
            new Claim("sub", Guid.NewGuid().ToString()),
        }, "Bearer", "preferred_username", ClaimTypes.Role);

        var result = await _Transformation.TransformAsync(new ClaimsPrincipal(identity));

        Assert.That(result.FindFirst(ERPClaimTypes.UserGuid)?.Value, Is.EqualTo(user.guid));
    }

    [Test]
    public async Task UnknownDeletedOrDisabledUser_GetsNoClaim()
    {
        AddUser("gone", isDeleted: true);
        AddUser("off", isDisabled: true);

        foreach (var name in new[] { "nobody", "gone", "off" })
        {
            var result = await _Transformation.TransformAsync(SignedIn(name));
            Assert.That(result.HasClaim(c => c.Type == ERPClaimTypes.UserGuid), Is.False, name);
        }
    }

    [Test]
    public async Task RunTwice_AddsTheClaimOnce()
    {
        AddUser("admin");

        var once = await _Transformation.TransformAsync(SignedIn("admin"));
        var twice = await _Transformation.TransformAsync(once);

        Assert.That(twice.FindAll(ERPClaimTypes.UserGuid).Count(), Is.EqualTo(1));
    }

    [Test]
    public async Task Anonymous_IsUntouched()
    {
        AddUser("admin");
        var anonymous = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "admin") }));

        var result = await _Transformation.TransformAsync(anonymous);

        Assert.That(result.HasClaim(c => c.Type == ERPClaimTypes.UserGuid), Is.False);
    }

    [Test]
    public async Task ServiceAccount_GetsNoClaim()
    {
        _Context.Users.Add(ServiceUserFactory.Create());
        _Context.SaveChanges();

        var result = await _Transformation.TransformAsync(SignedIn(SystemUsers.ServiceUsername));

        Assert.That(result.HasClaim(c => c.Type == ERPClaimTypes.UserGuid), Is.False);
    }

    [Test]
    public async Task ServiceAccount_CannotSignIn()
    {
        _Context.Users.Add(ServiceUserFactory.Create());
        _Context.SaveChanges();
        var provider = new DatabaseAuthenticationProvider(new AuthenticationSettings() { APIPrivateKey = new string('k', 64) }, _Context);

        var result = await provider.Authenticate(SystemUsers.ServiceUsername, "anything");

        Assert.That(result.Success, Is.False);
        Assert.That(result.ResultCode, Is.EqualTo(ResultCode.InvalidPermission));
    }

    // A database-login token: only the username, as ClaimTypes.Name.
    private static ClaimsPrincipal SignedIn(string username)
        => new(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, username) }, "Bearer"));

    private User AddUser(string username, bool isDeleted = false, bool isDisabled = false)
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
            is_deleted = isDeleted,
            is_disabled = isDisabled,
        }, SystemUsers.ServiceUserGuid);
        _Context.Users.Add(user);
        _Context.SaveChanges();
        return user;
    }
}
