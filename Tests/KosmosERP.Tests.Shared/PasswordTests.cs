using Microsoft.EntityFrameworkCore;
using KosmosERP.BusinessLayer.AuthenticationProviders;
using KosmosERP.BusinessLayer.Helpers;
using KosmosERP.Database;
using KosmosERP.Database.Models;
using KosmosERP.Models;

namespace KosmosERP.Tests.Shared;

/// <summary>
/// Database-login passwords (BUG-009: seeded users had password and salt "seeded",
/// so login threw on the salt and nobody could sign in with the seeded data).
/// </summary>
public class PasswordTests
{
    private ERPDbContext _Context;
    private DatabaseAuthenticationProvider _Provider;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<DbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _Context = new ERPDbContext(options);

        var settings = new AuthenticationSettings() { APIPrivateKey = new string('k', 64) };
        _Provider = new DatabaseAuthenticationProvider(settings, _Context);
    }

    [TearDown]
    public void Destroy()
    {
        _Context.Dispose();
    }

    [Test]
    public void Create_ThenVerify_MatchesOnlyTheSamePassword()
    {
        var (hash, salt) = PasswordHasher.Create("Kosmos-Dev-1");

        Assert.That(PasswordHasher.Verify("Kosmos-Dev-1", hash, salt), Is.True);
        Assert.That(PasswordHasher.Verify("kosmos-dev-1", hash, salt), Is.False);
    }

    [Test]
    public void Create_UsesANewSaltEachTime()
    {
        var first = PasswordHasher.Create("same");
        var second = PasswordHasher.Create("same");

        Assert.That(second.Salt, Is.Not.EqualTo(first.Salt));
        Assert.That(second.Hash, Is.Not.EqualTo(first.Hash));
    }

    [Test]
    public void Verify_NonBase64SaltOrHash_IsAMismatchNotAnException()
    {
        Assert.That(PasswordHasher.Verify("seeded", "seeded", "seeded"), Is.False);
        var (_, salt) = PasswordHasher.Create("x");
        Assert.That(PasswordHasher.Verify("x", "not base64!", salt), Is.False);
    }

    [Test]
    public async Task Authenticate_UserHashedLikeTheSeeder_SignsIn()
    {
        var (hash, salt) = PasswordHasher.Create("Kosmos-Dev-1");
        AddUser("admin", hash, salt);

        var result = await _Provider.Authenticate("admin", "Kosmos-Dev-1");

        Assert.That(result.Success, Is.True, result.Exception?.ToString());
        Assert.That(result.Data!.authenticated, Is.True);
        Assert.That(result.Data.token, Is.Not.Null);
    }

    [Test]
    public async Task Authenticate_UsernameIsCaseInsensitive()
    {
        // Only the stored username was lowercased, so "Admin" failed while "admin" worked.
        var (hash, salt) = PasswordHasher.Create("Kosmos-Dev-1");
        AddUser("admin", hash, salt);

        var result = await _Provider.Authenticate("Admin", "Kosmos-Dev-1");

        Assert.That(result.Success, Is.True, result.Exception?.ToString());
    }

    [Test]
    public async Task Authenticate_OldSeededSalt_IsRejectedCleanly()
    {
        // Regression for BUG-009: this used to throw FormatException (a server error).
        AddUser("ext-jordan", "seeded", "seeded");

        var result = await _Provider.Authenticate("ext-jordan", "seeded");

        Assert.That(result.Success, Is.False);
        Assert.That(result.ResultCode, Is.EqualTo(ResultCode.Invalid));
    }

    private void AddUser(string username, string hash, string salt)
    {
        _Context.Users.Add(CommonDataHelper<User>.FillCommonFields(new User()
        {
            first_name = "Seeded",
            last_name = "User",
            username = username,
            password = hash,
            password_salt = salt,
            employee_number = "E100",
            guid = Guid.NewGuid().ToString(),
        }, 1));
        _Context.SaveChanges();
    }
}
