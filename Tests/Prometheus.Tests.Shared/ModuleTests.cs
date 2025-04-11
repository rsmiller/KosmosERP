using Microsoft.EntityFrameworkCore;
using Prometheus.BusinessLayer.Modules;
using Prometheus.Database;
using Prometheus.Database.Models;
using Prometheus.Models;
using Prometheus.Models.Permissions;
using Prometheus.Module;

namespace Prometheus.Tests.Shared;

public class ModuleTests
{
    private ERPDbContext _Context;
    private User _User;

    private string _ModuleId = Guid.NewGuid().ToString();
    private string _SessionId = Guid.NewGuid().ToString();

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<DbContext>()
                  .UseInMemoryDatabase(Guid.NewGuid().ToString())
                  .Options;

        _Context = new ERPDbContext(options);

        var baseUser = new User()
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
            created_by = 1,
            created_on = DateTime.Now,
            updated_by = 1,
            updated_on = DateTime.Now,
        };

        _Context.Users.Add(baseUser);
        _Context.SaveChanges();

        _User = _Context.Users.First();

        var userSession = new UserSessionState()
        {
            user_id = _User.id,
            session_id = _SessionId,
            created_on = DateTime.Now,
            session_expires = DateTime.Now.AddMinutes(30),
        };

        _Context.UserSessionStates.Add(userSession);
        _Context.SaveChanges();

        var roleModel1 = new Role()
        {
            name = "ExampleRole",
            created_by = 1,
            created_on = DateTime.Now,
            updated_by = 1,
            updated_on = DateTime.Now
        };

        var roleModel2 = new Role()
        {
            name = "AnotherRole",
            created_by = 1,
            created_on = DateTime.Now,
            updated_by = 1,
            updated_on = DateTime.Now
        };

        _Context.Roles.Add(roleModel1);
        _Context.Roles.Add(roleModel2);
        _Context.SaveChanges();

        var userRole1 = new UserRole()
        {
            role_id = roleModel1.id,
            user_id = _User.id,
            created_by = 1,
            created_on = DateTime.Now,
            updated_by = 1,
            updated_on = DateTime.Now
        };

        var userRole2 = new UserRole()
        {
            role_id = roleModel2.id,
            user_id = _User.id,
            created_by = 1,
            created_on = DateTime.Now,
            updated_by = 1,
            updated_on = DateTime.Now
        };

        _Context.UserRoles.Add(userRole1);
        _Context.UserRoles.Add(userRole2);
        _Context.SaveChanges();


        var moduleReadModel = new ModulePermission()
        {
            module_id = _ModuleId,
            module_name = "A module",
            permission_name = "module_read",
            internal_permission_name = "module_read",
            read = true,
            edit = false,
            delete = false,
            write = false,
            is_active = true
        };

        var moduleEditModel = new ModulePermission()
        {
            module_id = _ModuleId,
            module_name = "A module",
            permission_name = "module_edit",
            internal_permission_name = "module_edit",
            read = false,
            edit = true,
            delete = false,
            write = false,
            is_active = true
        };

        _Context.ModulePermissions.Add(moduleReadModel);
        _Context.ModulePermissions.Add(moduleEditModel);
        _Context.SaveChanges();


        var role = _Context.Roles.Single(m => m.name == roleModel1.name);
        var readModulePermission = _Context.ModulePermissions.Single(m => m.module_id == _ModuleId && m.permission_name == moduleReadModel.permission_name);
        var editModulePermission = _Context.ModulePermissions.Single(m => m.module_id == _ModuleId && m.permission_name == moduleEditModel.permission_name);

        var rolePermissionReadModel = new RolePermission()
        {
            module_permission_id = readModulePermission.id,
            role_id = role.id,
            created_by = 1,
            created_on = DateTime.Now,
            updated_by = 1,
            updated_on = DateTime.Now
        };

        var rolePermissionEditModel = new RolePermission()
        {
            module_permission_id = editModulePermission.id,
            role_id = role.id,
            created_by = 1,
            created_on = DateTime.Now,
            updated_by = 1,
            updated_on = DateTime.Now
        };


        _Context.RolePermissions.Add(rolePermissionReadModel);
        _Context.RolePermissions.Add(rolePermissionEditModel);

        _Context.SaveChanges();
    }

    [TearDown]
    public void Destroy()
    {
        _Context.Dispose();
    }

    [Test]
    public async Task GetUserPermissions_BaseERPModule()
    {
        var base_module = new BaseERPModule(_Context);
        base_module.ModuleIdentifier = Guid.Parse(_ModuleId);

        var user_permissions = await base_module.GetUserPermissions(_User.id);

        Assert.NotNull(user_permissions);
        Assert.That(user_permissions.permissions.Count() == 2);
    }

    [Test]
    public async Task HasPermission_Read_BaseERPModule()
    {
        var base_module = new BaseERPModule(_Context);
        base_module.ModuleIdentifier = Guid.Parse(_ModuleId);

        var has_permission = await base_module.HasPermission(_User.id, _SessionId, "module_read", true);

        Assert.NotNull(has_permission);
        Assert.IsTrue(has_permission);
    }

    [Test]
    public async Task HasPermission_Edit_BaseERPModule()
    {
        var base_module = new BaseERPModule(_Context);
        base_module.ModuleIdentifier = Guid.Parse(_ModuleId);

        var has_permission = await base_module.HasPermission(_User.id, _SessionId, "module_edit", edit: true);

        Assert.NotNull(has_permission);
        Assert.IsTrue(has_permission);
    }

    [Test]
    public async Task HasPermission_Write_BaseERPModule()
    {
        var base_module = new BaseERPModule(_Context);
        base_module.ModuleIdentifier = Guid.Parse(_ModuleId);

        var has_permission = await base_module.HasPermission(_User.id, _SessionId, "write_user", write: true);

        Assert.NotNull(has_permission);
        Assert.IsFalse(has_permission);
    }

    [Test]
    public async Task HasPermission_Delete_BaseERPModule()
    {
        var base_module = new BaseERPModule(_Context);
        base_module.ModuleIdentifier = Guid.Parse(_ModuleId);



        var has_permission = await base_module.HasPermission(_User.id, _SessionId, UserPermissions.Delete, delete: true);

        Assert.NotNull(has_permission);
        Assert.IsFalse(has_permission);
    }

    [Test]
    public async Task ErrorLog_BaseERPModule()
    {
        var base_module = new BaseERPModule(_Context);
        base_module.ModuleIdentifier = Guid.Parse(_ModuleId);

        await base_module.LogError(10, "Here", "There", new Exception("ASDASD"));

        var log_result = await _Context.ErrorLogs.FirstAsync();

        Assert.NotNull(log_result);
        Assert.That(log_result.error_severity == 10);
        Assert.That(log_result.source == "Here");
        Assert.That(log_result.method == "There");
        Assert.That(log_result.error_message == "ASDASD");
    }

    [Test]
    public async Task GeneralLog_BaseERPModule()
    {
        var base_module = new BaseERPModule(_Context);
        base_module.ModuleIdentifier = Guid.Parse(_ModuleId);

        await base_module.LogGeneral("Admin", "Someone did something");

        var log_result = await _Context.GeneralLogs.FirstAsync();

        Assert.NotNull(log_result);
        Assert.That(log_result.category == "Admin");
        Assert.That(log_result.message == "Someone did something");
    }
}