using Microsoft.EntityFrameworkCore;
using KosmosERP.BusinessLayer.Helpers;
using KosmosERP.BusinessLayer.Models.Module.User.Command.Create;
using KosmosERP.BusinessLayer.Models.Module.User.Command.Edit;
using KosmosERP.BusinessLayer.Models.Module.User.Command.Find;
using KosmosERP.BusinessLayer.Modules;
using KosmosERP.Database;
using KosmosERP.Database.Models;
using KosmosERP.Models;
using KosmosERP.Models.Permissions;
using KosmosERP.BusinessLayer.Models.Module.User.ListProfiles;
using KosmosERP.BusinessLayer;

namespace KosmosERP.Tests.Shared;

public class ModuleUserTests
{
    private ERPDbContext _Context;
    private UserModule _Module;

    public int _EditRoleId { get; set; }
    public int _CreateRoleId { get; set; }

    public string _UserId { get; set; }
    private string _SessionId = Guid.NewGuid().ToString();
    private string _PrivateKey = "Key1231231243456efghfghfghf!!@!@!@";

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<DbContext>()
                    .UseInMemoryDatabase(Guid.NewGuid().ToString())
                    .Options;

        _Context = new ERPDbContext(options);

        var auth_settings = new AuthenticationSettings()
        {
            APIPrivateKey = _PrivateKey,
            AuthenticationProvider = AuthenticiationProviders.MOCK,
        };

        var logProviderFactory = new LogProviderFactory(new LogProviderSettings() { log_provider = LogProviderType.MOCK }, _Context, null);
        var authFactory = new AuthenticationFactory(auth_settings, _Context);

        _Module = new UserModule(_Context, auth_settings, new KeyValueModule(_Context, logProviderFactory), authFactory, logProviderFactory);

        _Module.SeedPermissions();

        var baseUser = CommonDataHelper<User>.FillCommonFields(new User()
        {
            first_name = "test",
            last_name = "user",
            email = "test@email.com",
            username = "test",
            password = "password",
            password_salt = "asdasd",
            employee_number = "10001",
            department = "1",
            guid = Guid.NewGuid().ToString(),
            is_admin = true,
        }, 1);

        _Context.Users.Add(baseUser);
        _Context.SaveChanges();

        var userSession = new UserSessionState()
        {
            user_id = baseUser.id,
            session_id = _SessionId,
            created_on = DateTime.UtcNow,
            session_expires = DateTime.UtcNow.AddMinutes(30),
        };

        _Context.UserSessionStates.Add(userSession);
        _Context.SaveChanges();

        var editRoleModel = CommonDataHelper<Role>.FillCommonFields(new Role()
        {
            name = "EditRole",
        }, 1);

        var createRoleModel = CommonDataHelper<Role>.FillCommonFields(new Role()
        {
            name = "CreateRole",
        }, 1);

        _Context.Roles.Add(editRoleModel);
        _Context.Roles.Add(createRoleModel);
        _Context.SaveChanges();

        _EditRoleId = editRoleModel.id;
        _CreateRoleId = createRoleModel.id;

        _UserId = baseUser.external_id;

    }

    [TearDown]
    public void Destroy()
    {
        _Module.Dispose();
        _Context.Dispose();
    }

    [Test]
    public async Task Permissions_Seeded()
    {
        var seeded_permissions = await _Context.ModulePermissions.ToListAsync();

        Assert.That(seeded_permissions.Count() > 0);
    }

    [Test]
    public async Task AssignUserPermissions()
    {
        var seeded_permissions = await _Context.ModulePermissions.ToListAsync();

        Assert.That(seeded_permissions.Count() > 0);

        var edit_permission = seeded_permissions.Single(m => m.internal_permission_name == UserPermissions.Read);
        var create_permission = seeded_permissions.Single(m => m.internal_permission_name == UserPermissions.Read);

        // Have to give the user edit permissions in order to assign them in the module
        var rolePermission = CommonDataHelper<RolePermission>.FillCommonFields(new RolePermission()
        {
            role_id = _EditRoleId,
            module_id = edit_permission.module_id,
            edit = true,
        }, 1);

        var userRole = CommonDataHelper<UserRole>.FillCommonFields(new UserRole()
        {
            user_id = 1,
            role_id = _EditRoleId,
        }, 1);

        await _Context.RolePermissions.AddAsync(rolePermission);
        await _Context.UserRoles.AddAsync(userRole);

        await _Context.SaveChangesAsync();

        await _Module.AssignUserRole(new AssignUserRoleCommand()
        {
            calling_user_id = _UserId,
            role_id = rolePermission.role_id,
            user_id = 1,
        });
    }

    [Test]
    public async Task GetRoles()
    {
        var roles_result = await _Module.GetRoles();

        Assert.That(roles_result.Success, Is.True);
        Assert.That(roles_result.Data, Is.Not.Null);
        Assert.That(roles_result.Data.Count() > 0);
    }
}