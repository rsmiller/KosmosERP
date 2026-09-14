using Microsoft.EntityFrameworkCore;
using KosmosERP.BusinessLayer.Helpers;
using KosmosERP.Database;
using KosmosERP.Database.Models;
using KosmosERP.Models.Permissions;
using KosmosERP.Module;
using KosmosERP.BusinessLayer;
using KosmosERP.Models;

namespace KosmosERP.Tests.Shared;

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

        var baseUser = CommonDataHelper<KosmosERP.Database.Models.User>.FillCommonFields(new User()
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

        var roleModel1 = CommonDataHelper<Role>.FillCommonFields(new Role()
        {
            name = "ExampleRole",
        }, 1);

        var roleModel2 = CommonDataHelper<Role>.FillCommonFields(new Role()
        {
            name = "AnotherRole",

        }, 1);

        _Context.Roles.Add(roleModel1);
        _Context.Roles.Add(roleModel2);
        _Context.SaveChanges();

        var userRole1 = CommonDataHelper<UserRole>.FillCommonFields(new UserRole()
        {
            role_id = roleModel1.id,
            user_id = _User.id,
        }, 1);

        var userRole2 = CommonDataHelper<UserRole>.FillCommonFields(new UserRole()
        {
            role_id = roleModel2.id,
            user_id = _User.id,
        }, 1);

        _Context.UserRoles.Add(userRole1);
        _Context.UserRoles.Add(userRole2);
        _Context.SaveChanges();


        var moduleReadModel = CommonDataHelper<ModulePermission>.FillCommonFields(new ModulePermission()
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
        }, 1);

        var moduleEditModel = CommonDataHelper<ModulePermission>.FillCommonFields(new ModulePermission()
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
        }, 1);

        _Context.ModulePermissions.Add(moduleReadModel);
        _Context.ModulePermissions.Add(moduleEditModel);
        _Context.SaveChanges();


        var role = _Context.Roles.Single(m => m.name == roleModel1.name);
        var readModulePermission = _Context.ModulePermissions.Single(m => m.module_id == _ModuleId && m.permission_name == moduleReadModel.permission_name);
        var editModulePermission = _Context.ModulePermissions.Single(m => m.module_id == _ModuleId && m.permission_name == moduleEditModel.permission_name);

        var rolePermissionReadModel = CommonDataHelper<RolePermission>.FillCommonFields(new RolePermission()
        {
            module_id = _ModuleId,
            role_id = role.id,
            read = true,
        }, 1);

        var rolePermissionEditModel = CommonDataHelper<RolePermission>.FillCommonFields(new RolePermission()
        {
            module_id = _ModuleId,
            role_id = role.id,
            edit = true,
        }, 1);


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
    public async Task ErrorLog_BaseERPModule()
    {
        var logProviderFactory = new LogProviderFactory(new LogProviderSettings() { log_provider = LogProviderType.MOCK }, _Context, null);
        var base_module = new BaseERPModule(logProviderFactory);
        base_module.ModuleIdentifier = Guid.Parse(_ModuleId);

        await base_module.LogError(10, "Here", "There", new Exception("ASDASD"));
/*
        var log_result = await _Context.ErrorLogs.FirstAsync();

        Assert.That(log_result, Is.Not.Null);
        Assert.That(log_result.error_severity == 10);
        Assert.That(log_result.source == "Here");
        Assert.That(log_result.method == "There");
        Assert.That(log_result.error_message == "ASDASD");*/
    }

    [Test]
    public async Task GeneralLog_BaseERPModule()
    {
        var logProviderFactory = new LogProviderFactory(new LogProviderSettings() { log_provider = LogProviderType.MOCK }, _Context, null);
        var base_module = new BaseERPModule(logProviderFactory);
        base_module.ModuleIdentifier = Guid.Parse(_ModuleId);

        await base_module.LogTrace("Admin", "Someone did something");
        
        /*
        var log_result = await _Context.GeneralLogs.FirstAsync();

        
        Assert.That(log_result, Is.Not.Null);
        Assert.That(log_result.category == "Admin");
        Assert.That(log_result.message == "Someone did something");*/
    }
}