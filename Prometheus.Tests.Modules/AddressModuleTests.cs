using System.Data;
using Microsoft.EntityFrameworkCore;
using Prometheus.Models;
using Prometheus.Models.Permissions;
using Prometheus.Tests.Modules.Shared;
using Prometheus.BusinessLayer.Helpers;
using Prometheus.BusinessLayer.Modules;
using Prometheus.Database.Models;
using Prometheus.BusinessLayer.Models.Module.Address.Command.Create;
using Prometheus.BusinessLayer.Models.Module.Address.Command.Edit;
using Prometheus.BusinessLayer.Models.Module.Address.Command.Delete;
using Prometheus.BusinessLayer.Models.Module.Address.Command.Find;
using Prometheus.BusinessLayer.Models.Module.Address.Dto;

namespace Prometheus.Tests.Modules;

public class AddressModuleTests : BaseTestModule<AddressModule>, IModuleTest
{
    [SetUp]
    public async Task SetupModule()
    {
        var the_module = new AddressModule(base._Context);

        await base.SetupModule(the_module);
    }

    protected override async Task SetupRoles()
    {
        var admin_role = CommonDataHelper<Role>.FillCommonFields(new Role()
        {
            name = "Module Admin",
        }, 1);

        _Context.Roles.Add(admin_role);
        await _Context.SaveChangesAsync();

        var user_role = CommonDataHelper<UserRole>.FillCommonFields(new UserRole()
        {
            role_id = admin_role.id,
            user_id = _User.id,
        }, 1);

        _Context.UserRoles.Add(user_role);
        await _Context.SaveChangesAsync();
    }

    protected override async Task SetupReadPermissions()
    {
        var read_permission = await _Context.ModulePermissions.FirstAsync(m => m.module_id == _Module.ModuleIdentifier.ToString() && m.internal_permission_name == AddressPermissions.Read);
        var role = await _Context.Roles.Where(m => m.name == "Module Admin").FirstAsync();

        var role_module_permission = CommonDataHelper<RolePermission>.FillCommonFields(new RolePermission()
        {
            module_permission_id = read_permission.id,
            role_id = role.id,
        }, 1);

        _Context.RolePermissions.Add(role_module_permission);
        await _Context.SaveChangesAsync();
    }

    protected override async Task SetupCreatePermissions()
    {
        var create_permission = await _Context.ModulePermissions.FirstAsync(m => m.module_id == _Module.ModuleIdentifier.ToString() && m.internal_permission_name == AddressPermissions.Create);
        var role = await _Context.Roles.Where(m => m.name == "Module Admin").FirstAsync();

        var role_module_permission = CommonDataHelper<RolePermission>.FillCommonFields(new RolePermission()
        {
            module_permission_id = create_permission.id,
            role_id = role.id,
        }, 1);

        _Context.RolePermissions.Add(role_module_permission);
        await _Context.SaveChangesAsync();
    }

    protected override async Task SetupEditPermissions()
    {
        var edit_permission = await _Context.ModulePermissions.FirstAsync(m => m.module_id == _Module.ModuleIdentifier.ToString() && m.internal_permission_name == AddressPermissions.Edit);
        var role = await _Context.Roles.Where(m => m.name == "Module Admin").FirstAsync();

        var role_module_permission = CommonDataHelper<RolePermission>.FillCommonFields(new RolePermission()
        {
            module_permission_id = edit_permission.id,
            role_id = role.id,
        }, 1);

        _Context.RolePermissions.Add(role_module_permission);
        await _Context.SaveChangesAsync();
    }

    protected override async Task SetupDeletePermissions()
    {
        var delete_permission = await _Context.ModulePermissions.FirstAsync(m => m.module_id == _Module.ModuleIdentifier.ToString() && m.internal_permission_name == AddressPermissions.Delete);
        var role = await _Context.Roles.Where(m => m.name == "Module Admin").FirstAsync();

        var role_module_permission = CommonDataHelper<RolePermission>.FillCommonFields(new RolePermission()
        {
            module_permission_id = delete_permission.id,
            role_id = role.id,
        }, 1);

        _Context.RolePermissions.Add(role_module_permission);
        await _Context.SaveChangesAsync();
    }

    protected override async Task SetupData()
    {
        var address = CommonDataHelper<Address>.FillCommonFields(new Address
        {
            street_address1 = "11005 Chicken Nugget Lane",
            street_address2 = "Unit 12",
            city = "Temple",
            state = "TX",
            postal_code = "76251",
            country = "USA",
            is_deleted = false,
        }, 1);

        _Context.Addresses.Add(address);
        await _Context.SaveChangesAsync();
    }

    [Test]
    public async Task Get()
    {
        var new_result = await _Module.Create(new AddressCreateCommand()
        {
            calling_user_id = 1,
            token = _SessionId,
            street_address1 = "11005 Chicken Nugget Lane",
            street_address2 = "Unit 12",
            city = "Temple",
            state = "TX",
            postal_code = "76251",
            country = "USA",
        });

        Assert.IsTrue(new_result.Success);
        Assert.IsNotNull(new_result.Data);

        var result = await _Module.GetDto(new_result.Data.id);

        ValidateMostDtoFields(result);
    }

    [Test]
    public async Task Create()
    {
        var result = await _Module.Create(new AddressCreateCommand()
        {
            calling_user_id = 1,
            token = _SessionId,
            street_address1 = "11005 Chicken Nugget Lane",
            street_address2 = "Unit 12",
            city = "Temple",
            state = "TX",
            postal_code = "76251",
            country = "USA",
        });

        ValidateMostDtoFields(result);
    }

    [Test]
    public async Task Edit()
    {
        var result = await _Module.Edit(new AddressEditCommand()
        {
            calling_user_id = 1,
            token = _SessionId,
            id = 1,
            street_address1 = "sdfsdfsdf",
            street_address2 = "123123",
            city = "dfdfd",
            state = "44",
            postal_code = "ffefdf",
            country = "US",
        });

        ValidateMostDtoFields(result);
    }

    [Test]
    public async Task Delete()
    {
        var new_result = await _Module.Create(new AddressCreateCommand()
        {
            calling_user_id = 1,
            token = _SessionId,
            street_address1 = "11005 Chicken Nugget Lane",
            street_address2 = "Unit 12",
            city = "Temple",
            state = "TX",
            postal_code = "76251",
            country = "USA",
        });

        Assert.IsTrue(new_result.Success);
        Assert.IsNotNull(new_result.Data);

        var delete_result = await _Module.Delete(new AddressDeleteCommand()
        {
            calling_user_id = 1,
            token = _SessionId,
            id = new_result.Data.id
        });

        Assert.IsTrue(new_result.Success);
        Assert.IsNotNull(new_result.Data);
    }

    [Test]
    public async Task Find()
    {
        var new_result = await _Module.Create(new AddressCreateCommand()
        {
            calling_user_id = 1,
            token = _SessionId,
            street_address1 = "123 Some St",
            street_address2 = "",
            city = "Temple",
            state = "TX",
            postal_code = "76251",
            country = "USA",
        });

        Assert.IsTrue(new_result.Success);
        Assert.IsNotNull(new_result.Data);

        var results = await _Module.Find(
                        new PagingSortingParameters() { ResultCount = 20, Start = 0 },
                        new AddressFindCommand() { calling_user_id = 1, token = _SessionId, wildcard = "TX" });
        
        Assert.IsTrue(results.Success);
        Assert.IsNotNull(results.Data);
        Assert.NotZero(results.Data.Count());

        var first_result = results.Data.First();

        ValidateMostListFields(first_result);
    }

    private void ValidateMostDtoFields(Response<AddressDto> result)
    {
        Assert.IsTrue(result.Success);
        Assert.IsNotNull(result.Data);
        Assert.NotZero(result.Data.id);
        Assert.IsNotEmpty(result.Data.guid);
        Assert.IsNotEmpty(result.Data.street_address1);
        Assert.IsNotEmpty(result.Data.street_address2);
        Assert.IsNotEmpty(result.Data.city);
        Assert.IsNotEmpty(result.Data.state);
        Assert.IsNotEmpty(result.Data.postal_code);
        Assert.IsNotEmpty(result.Data.country);
        Assert.NotZero(result.Data.created_by);
        Assert.NotNull(result.Data.created_on);
        Assert.NotNull(result.Data.created_on_string);
        Assert.NotNull(result.Data.created_on_timezone);
        Assert.NotNull(result.Data.updated_by);
        Assert.NotNull(result.Data.updated_on);
        Assert.NotNull(result.Data.updated_on_string);
        Assert.NotNull(result.Data.updated_on_timezone);
    }

    private void ValidateMostListFields(AddressListDto result)
    {
        Assert.NotZero(result.id);
        Assert.IsNotEmpty(result.guid);
        Assert.IsNotEmpty(result.street_address1);
        Assert.IsNotEmpty(result.street_address2);
        Assert.IsNotEmpty(result.city);
        Assert.IsNotEmpty(result.state);
        Assert.IsNotEmpty(result.postal_code);
        Assert.IsNotEmpty(result.country);
        Assert.NotZero(result.created_by);
        Assert.NotNull(result.created_on);
        Assert.NotNull(result.created_on_string);
        Assert.NotNull(result.created_on_timezone);
        Assert.NotNull(result.updated_by);
        Assert.NotNull(result.updated_on);
        Assert.NotNull(result.updated_on_string);
        Assert.NotNull(result.updated_on_timezone);
    }
}