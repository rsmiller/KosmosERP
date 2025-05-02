using Microsoft.EntityFrameworkCore;
using KosmosERP.BusinessLayer.Helpers;
using KosmosERP.Database;
using KosmosERP.Database.Models;
using KosmosERP.Module;

namespace KosmosERP.Tests.Modules.Shared;

public class BaseTestModule<T> where T : IBaseERPModule
{
    public T _Module;
    public ERPDbContext _Context;
    public User _User;
    public string _SessionId = Guid.NewGuid().ToString();

    [SetUp]
    public async Task Setup()
    {
        var options = new DbContextOptionsBuilder<DbContext>()
                  .UseInMemoryDatabase(Guid.NewGuid().ToString())
                  .Options;

        _Context = new ERPDbContext(options);

        var baseUser = CommonDataHelper<KosmosERP.Database.Models.User>.FillCommonFields(new User()
        {
            first_name = "test",
            last_name = "user",
            email = "test@email.com",
            username = "test",
            password = "password",
            password_salt = "asdasd",
            employee_number = "10001",
            department = 1,
            guid = Guid.NewGuid().ToString(),
            is_admin = false,
        }, 1);

        _Context.Users.Add(baseUser);
        _Context.SaveChanges();

        _User = _Context.Users.First();

        var userSession = new UserSessionState()
        {
            user_id = _User.id,
            session_id = _SessionId,
            created_on = DateTime.UtcNow,
            session_expires = DateTime.UtcNow.AddMinutes(30),
        };

        _Context.UserSessionStates.Add(userSession);
        _Context.SaveChanges();



    }

    protected virtual async Task SetupModule(T module)
    {
        _Module = module;

        _Module.SeedPermissions();

        await SetupRoles();
        await SetupReadPermissions();
        await SetupCreatePermissions();
        await SetupEditPermissions();
        await SetupDeletePermissions();
        await SetupData();
    }

    protected virtual async Task SetupRoles()
    {

    }

    protected virtual async Task SetupReadPermissions()
    {

    }

    protected virtual async Task SetupCreatePermissions()
    {

    }

    protected virtual async Task SetupEditPermissions()
    {

    }

    protected virtual async Task SetupDeletePermissions()
    {

    }

    protected virtual async Task SetupData()
    {

    }

    [TearDown]
    public void Destroy()
    {
        _Context.Dispose();
    }
}
