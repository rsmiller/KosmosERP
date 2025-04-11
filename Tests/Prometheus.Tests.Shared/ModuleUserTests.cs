using Microsoft.EntityFrameworkCore;
using Prometheus.BusinessLayer.Models.Module.User.Command.Create;
using Prometheus.BusinessLayer.Models.Module.User.Command.Edit;
using Prometheus.BusinessLayer.Models.Module.User.Command.Find;
using Prometheus.BusinessLayer.Modules;
using Prometheus.Database;
using Prometheus.Database.Models;
using Prometheus.Models;
using Prometheus.Models.Permissions;

namespace Prometheus.Tests.Shared;

public class ModuleUserTests
{
    private ERPDbContext _Context;
    private UserModule _Module;

    public int _EditRoleId { get; set; }
    public int _CreateRoleId { get; set; }

    public int _UserId { get; set; }
    private string _SessionId = Guid.NewGuid().ToString();
    private string _PrivateKey = "Key123";

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<DbContext>()
                    .UseInMemoryDatabase(Guid.NewGuid().ToString())
                    .Options;

        _Context = new ERPDbContext(options);

        var auth_settings = new AuthenticationSettings()
        {
            APIPrivateKey = _PrivateKey
        };

        _Module = new UserModule(_Context, auth_settings);

        _Module.SeedPermissions();

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
            is_admin = true,
            created_by = 1,
            created_on = DateTime.Now,
            updated_by = 1,
            updated_on = DateTime.Now,
        };

        _Context.Users.Add(baseUser);
        _Context.SaveChanges();

        var userSession = new UserSessionState()
        {
            user_id = baseUser.id,
            session_id = _SessionId,
            created_on = DateTime.Now,
            session_expires = DateTime.Now.AddMinutes(30),
        };

        _Context.UserSessionStates.Add(userSession);
        _Context.SaveChanges();

        var editRoleModel = new Role()
        {
            name = "EditRole",
            created_by = 1,
            created_on = DateTime.Now,
            updated_by = 1,
            updated_on = DateTime.Now
        };

        var createRoleModel = new Role()
        {
            name = "CreateRole",
            created_by = 1,
            created_on = DateTime.Now,
            updated_by = 1,
            updated_on = DateTime.Now
        };

        _Context.Roles.Add(editRoleModel);
        _Context.Roles.Add(createRoleModel);
        _Context.SaveChanges();

        _EditRoleId = editRoleModel.id;
        _CreateRoleId = createRoleModel.id;

        _UserId = baseUser.id;

    }

    [TearDown]
    public void Destroy()
    {
        _Context.Dispose();
    }

    [Test]
    public async Task Permissions_Seeded()
    {
        var seeded_permissions = await _Context.ModulePermissions.ToListAsync();

        Assert.That(seeded_permissions.Count > 0);
    }

    [Test]
    public async Task AssignUserPermissions()
    {
        var seeded_permissions = await _Context.ModulePermissions.ToListAsync();

        Assert.That(seeded_permissions.Count > 0);

        var edit_permission = seeded_permissions.Single(m => m.internal_permission_name == UserPermissions.Read);
        var create_permission = seeded_permissions.Single(m => m.internal_permission_name == UserPermissions.Read);

        // Have to give the user edit permissions in order to assign them in the module
        var rolePermission = new RolePermission()
        {
            role_id = _EditRoleId,
            module_permission_id = edit_permission.id,
            created_by = 1,
            created_on = DateTime.Now,
            updated_by = 1,
            updated_on = DateTime.Now
        };

        var userRole = new UserRole()
        {
            user_id = _UserId,
            role_id = _EditRoleId,
            created_by = 1,
            created_on = DateTime.Now,
            updated_by = 1,
            updated_on = DateTime.Now
        };

        await _Context.RolePermissions.AddAsync(rolePermission);
        await _Context.UserRoles.AddAsync(userRole);

        await _Context.SaveChangesAsync();

        await _Module.AssignUserRole(new AssignUserRoleCommand()
        {
            calling_user_id = _UserId,
            role_id = rolePermission.role_id,
            user_id = _UserId,
        });


        var permission_result = await _Module.HasPermission(_UserId, _SessionId, edit_permission.internal_permission_name, edit_permission.read, edit_permission.write, edit_permission.edit, edit_permission.delete);
        var permission_result_admin = await _Module.HasPermission(_UserId, _SessionId, "ASDASD", seeded_permissions[0].read, seeded_permissions[0].write, seeded_permissions[0].edit, seeded_permissions[0].delete);

        Assert.IsTrue(permission_result);
        Assert.IsTrue(permission_result_admin);
    }

    [Test]
    public async Task GetRoles()
    {
        var roles_result = await _Module.GetRoles();

        Assert.IsTrue(roles_result.Success);
        Assert.NotNull(roles_result.Data);
        Assert.That(roles_result.Data.Count() > 0);
    }

    [Test]
    public async Task AuthenticateUser()
    {
        await SetupForCreate();

        var create_model = new UserCreateCommand()
        {
            calling_user_id = _UserId,
            token = _SessionId,
            first_name = "User",
            last_name = "last",
            username = "username",
            password = "password",
            employee_number = "14112",
            department = 1,
            email = "emaiL@email.com"
        };

        var create_response = await _Module.Create(create_model);

        Assert.IsTrue(create_response.Success);
        Assert.NotNull(create_response.Data);

        var authenticate_response = await _Module.Authenticate(create_model.username, create_model.password);

        Assert.IsTrue(authenticate_response.Success);
        Assert.NotNull(authenticate_response.Data);
        Assert.IsTrue(authenticate_response.Data.authenticated);
        Assert.NotNull(authenticate_response.Data.session);
        Assert.NotNull(authenticate_response.Data.token);
    }

    [Test]
    public async Task CreateUser()
    {
        await SetupForCreate();

        var create_model = new UserCreateCommand()
        {
            calling_user_id = _UserId,
            token = _SessionId,
            first_name = "User",
            last_name = "last",
            username = "username",
            password = "password",
            employee_number = "14112",
            department = 1,
            email = "emaiL@email.com"
        };

        var create_response = await _Module.Create(create_model);

        Assert.IsTrue(create_response.Success);
        Assert.NotNull(create_response.Data);

        Assert.That(create_response.Data.first_name == create_model.first_name);
        Assert.That(create_response.Data.last_name == create_model.last_name);
        Assert.That(create_response.Data.username == create_model.username);
        Assert.That(create_response.Data.employee_number == create_model.employee_number);
        Assert.That(create_response.Data.department == create_model.department);
        Assert.That(create_response.Data.email == create_model.email);
    }

    [Test]
    public async Task EditUser()
    {
        await SetupForCreate();
        await SetupForEdit();

        var create_model = new UserCreateCommand()
        {
            calling_user_id = _UserId,
            token = _SessionId,
            first_name = "User",
            last_name = "last",
            username = "username",
            password = "password",
            employee_number = "14112",
            department = 1,
            email = "emaiL@email.com"
        };

        var create_response = await _Module.Create(create_model);

        Assert.IsTrue(create_response.Success);
        Assert.NotNull(create_response.Data);

        var edit_model = new UserEditCommand()
        {
            calling_user_id = _UserId,
            token = _SessionId,
            id = create_response.Data.id,
            first_name = "User1",
            last_name = "last2",
            password = "password44",
            employee_number = "1411211",
            department = 2,
            email = "emaiL1@email.com",
            is_admin = true,
            is_external_user = true,
            is_guest = true,
            is_management = true
        };

        var edit_response = await _Module.Edit(edit_model);

        Assert.IsTrue(edit_response.Success);
        Assert.NotNull(edit_response.Data);

        Assert.That(edit_response.Data.first_name == edit_model.first_name);
        Assert.That(edit_response.Data.last_name == edit_model.last_name);
        Assert.That(edit_response.Data.employee_number == edit_model.employee_number);
        Assert.That(edit_response.Data.department == edit_model.department);
        Assert.That(edit_response.Data.email == edit_model.email);
        Assert.That(edit_response.Data.is_admin == edit_model.is_admin);
        Assert.That(edit_response.Data.is_external_user == edit_model.is_external_user);
        Assert.That(edit_response.Data.is_guest == edit_model.is_guest);
        Assert.That(edit_response.Data.is_management == edit_model.is_management);
    }

    [Test]
    public async Task UserFind()
    {
        await SetupForRead();
        await SetupForCreate();

        var create_model = new UserCreateCommand()
        {
            calling_user_id = _UserId,
            token = _SessionId,
            first_name = "User",
            last_name = "last",
            username = "username",
            password = "password",
            employee_number = "14112",
            department = 1,
            email = "emaiL@email.com"
        };

        var create_response = await _Module.Create(create_model);

        Assert.IsTrue(create_response.Success);
        Assert.NotNull(create_response.Data);

        var find_response = await _Module.Find(new PagingSortingParameters()
        {
            Start = 0,
            ResultCount = 200,
            SortDefinitions = new List<SortDefinition>()
            {
                new SortDefinition() { ColumnName = "first_name", SortOrder = SortOrder.Ascending }
            }
        }, new UserFindCommand()
        {
            calling_user_id = _UserId,
            token = _SessionId,
            wildcard = create_model.first_name
        });

        Assert.IsTrue(find_response.Success);
        Assert.NotNull(find_response.Data);
        Assert.That(find_response.Data.Count > 0);

        // Negative test

        var find_response_negative = await _Module.Find(new Models.PagingSortingParameters()
        {
            Start = 0,
            ResultCount = 200,
            SortDefinitions = new List<SortDefinition>()
            {
                new SortDefinition() { ColumnName = "first_name", SortOrder = SortOrder.Ascending }
            }
        }, new UserFindCommand()
        {
            calling_user_id = _UserId,
            token = _SessionId,
            wildcard = "Derp"
        });

        Assert.IsTrue(find_response_negative.Success);
        Assert.NotNull(find_response_negative.Data);
        Assert.That(find_response_negative.Data.Count == 0);
    }

    private async Task SetupForRead()
    {
        var seeded_permissions = await _Context.ModulePermissions.ToListAsync();
        var read_permission = seeded_permissions.Single(m => m.internal_permission_name == UserPermissions.Read);

        var rolePermission = new RolePermission()
        {
            role_id = _CreateRoleId,
            module_permission_id = read_permission.id,
            created_by = 1,
            created_on = DateTime.Now,
            updated_by = 1,
            updated_on = DateTime.Now
        };

        var userRole = new UserRole()
        {
            user_id = _UserId,
            role_id = _CreateRoleId,
            created_by = 1,
            created_on = DateTime.Now,
            updated_by = 1,
            updated_on = DateTime.Now
        };

        await _Context.UserRoles.AddAsync(userRole);
        await _Context.RolePermissions.AddAsync(rolePermission);
        await _Context.SaveChangesAsync();
    }

    private async Task SetupForCreate()
    {
        var seeded_permissions = await _Context.ModulePermissions.ToListAsync();
        var create_permission = seeded_permissions.Single(m => m.internal_permission_name == UserPermissions.Create);

        var rolePermission = new RolePermission()
        {
            role_id = _CreateRoleId,
            module_permission_id = create_permission.id,
            created_by = 1,
            created_on = DateTime.Now,
            updated_by = 1,
            updated_on = DateTime.Now
        };

        var userRole = new UserRole()
        {
            user_id = _UserId,
            role_id = _CreateRoleId,
            created_by = 1,
            created_on = DateTime.Now,
            updated_by = 1,
            updated_on = DateTime.Now
        };

        await _Context.UserRoles.AddAsync(userRole);
        await _Context.RolePermissions.AddAsync(rolePermission);
        await _Context.SaveChangesAsync();
    }

    private async Task SetupForEdit()
    {
        var seeded_permissions = await _Context.ModulePermissions.ToListAsync();
        var edit_permission = seeded_permissions.Single(m => m.internal_permission_name == UserPermissions.Edit);

        var rolePermission = new RolePermission()
        {
            role_id = _EditRoleId,
            module_permission_id = edit_permission.id,
            created_by = 1,
            created_on = DateTime.Now,
            updated_by = 1,
            updated_on = DateTime.Now
        };

        var userRole = new UserRole()
        {
            user_id = _UserId,
            role_id = _EditRoleId,
            created_by = 1,
            created_on = DateTime.Now,
            updated_by = 1,
            updated_on = DateTime.Now
        };

        await _Context.UserRoles.AddAsync(userRole);
        await _Context.RolePermissions.AddAsync(rolePermission);
        await _Context.SaveChangesAsync();
    }
}