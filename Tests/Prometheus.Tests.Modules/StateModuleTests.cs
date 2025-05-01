using System.Data;
using Microsoft.EntityFrameworkCore;
using Prometheus.Models;
using Prometheus.Models.Permissions;
using Prometheus.Tests.Modules.Shared;
using Prometheus.BusinessLayer.Helpers;
using Prometheus.BusinessLayer.Modules;
using Prometheus.Database.Models;
using Prometheus.BusinessLayer.Models.Module.State.Command.Create;
using Prometheus.BusinessLayer.Models.Module.State.Command.Edit;
using Prometheus.BusinessLayer.Models.Module.State.Command.Delete;
using Prometheus.BusinessLayer.Models.Module.State.Command.Find;
using Prometheus.BusinessLayer.Models.Module.State.Dto;

namespace Prometheus.Tests.Modules;

public class StateModuleTests : BaseTestModule<StateModule>, IModuleTest
{
    private Country _Country;


    [SetUp]
    public async Task SetupModule()
    {
        var the_module = new StateModule(base._Context);

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

    protected override async Task SetupCreatePermissions()
    {
        var create_permission = await _Context.ModulePermissions.FirstAsync(m => m.module_id == _Module.ModuleIdentifier.ToString() && m.internal_permission_name == StatePermissions.Create);
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
        var edit_permission = await _Context.ModulePermissions.FirstAsync(m => m.module_id == _Module.ModuleIdentifier.ToString() && m.internal_permission_name == StatePermissions.Edit);
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
        var delete_permission = await _Context.ModulePermissions.FirstAsync(m => m.module_id == _Module.ModuleIdentifier.ToString() && m.internal_permission_name == StatePermissions.Delete);
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
        var country = CommonDataHelper<Country>.FillCommonFields(new Country()
        {
            iso3 = "US",
            country_name = "United States",
            currency = "Dollar",
            currency_symbol = "$",
            phonecode = "+1",
            region = "North America",
        }, 1);

        _Context.Countries.Add(country);
        await _Context.SaveChangesAsync();

        _Country = country;
    }

    [Test]
    public async Task Get()
    {
        var new_result = await _Module.Create(new StateCreateCommand()
        {
            calling_user_id = 1,
            token = _SessionId,
            iso2 = "TX",
            state_name = "Texas",
            country_id = _Country.id,
        });

        Assert.IsTrue(new_result.Success);
        Assert.IsNotNull(new_result.Data);

        var result = await _Module.GetDto(new_result.Data.id);

        ValidateMostDtoFields(result);
    }

    [Test]
    public async Task Create()
    {
        var result = await _Module.Create(new StateCreateCommand()
        {
            calling_user_id = 1,
            token = _SessionId,
            iso2 = "TX",
            state_name = "Texas",
            country_id = _Country.id,
        });

        ValidateMostDtoFields(result);
    }

    [Test]
    public async Task Edit()
    {
        var new_result = await _Module.Create(new StateCreateCommand()
        {
            calling_user_id = 1,
            token = _SessionId,
            iso2 = "TX",
            state_name = "Texas",
            country_id = _Country.id,
        });

        Assert.IsTrue(new_result.Success);
        Assert.IsNotNull(new_result.Data);

        var edit_command = new StateEditCommand()
        {
            calling_user_id = 1,
            token = _SessionId,
            id = new_result.Data.id,
            iso2 = "OK",
            state_name = "Oklahoma",
            country_id = _Country.id,
        };

        var edit_result = await _Module.Edit(edit_command);

        ValidateMostDtoFields(edit_result);

        Assert.That(edit_result.Data.iso2 == edit_command.iso2);
        Assert.That(edit_result.Data.state_name == edit_command.state_name);
        Assert.That(edit_result.Data.country_id == edit_command.country_id);
    }

    [Test]
    public async Task Delete()
    {
        var new_result = await _Module.Create(new StateCreateCommand()
        {
            calling_user_id = 1,
            token = _SessionId,
            iso2 = "TX",
            state_name = "Texas",
            country_id = _Country.id,
        });

        Assert.IsTrue(new_result.Success);
        Assert.IsNotNull(new_result.Data);

        var delete_result = await _Module.Delete(new StateDeleteCommand()
        {
            calling_user_id = 1,
            token = _SessionId,
            id = new_result.Data.id
        });

        Assert.IsTrue(delete_result.Success);
        Assert.IsNotNull(delete_result.Data);
        Assert.NotNull(delete_result.Data.deleted_by);
        Assert.NotNull(delete_result.Data.deleted_on);
        Assert.NotNull(delete_result.Data.deleted_on_string);
        Assert.NotNull(delete_result.Data.deleted_on_timezone);
    }

    [Test]
    public async Task Find()
    {
        var new_result = await _Module.Create(new StateCreateCommand()
        {
            calling_user_id = 1,
            token = _SessionId,
            iso2 = "TX",
            state_name = "Texas",
            country_id = _Country.id,
        });

        Assert.IsTrue(new_result.Success);
        Assert.IsNotNull(new_result.Data);

        var results = await _Module.Find(
                        new PagingSortingParameters() { ResultCount = 20, Start = 0 },
                        new StateFindCommand() { calling_user_id = 1, token = _SessionId, wildcard = "TX" });
        
        Assert.IsTrue(results.Success);
        Assert.IsNotNull(results.Data);
        Assert.NotZero(results.Data.Count());

        var first_result = results.Data.First();

        ValidateMostListFields(first_result);
    }

    private void ValidateMostDtoFields(Response<StateDto> result)
    {
        Assert.IsTrue(result.Success);
        Assert.IsNotNull(result.Data);
        Assert.NotZero(result.Data.id);
        Assert.IsNotEmpty(result.Data.guid);
        Assert.IsNotEmpty(result.Data.state_name);
        Assert.IsNotEmpty(result.Data.iso2);
        Assert.NotZero(result.Data.country_id);
        Assert.NotZero(result.Data.created_by);
        Assert.NotNull(result.Data.created_on);
        Assert.NotNull(result.Data.created_on_string);
        Assert.NotNull(result.Data.created_on_timezone);
        Assert.NotNull(result.Data.updated_by);
        Assert.NotNull(result.Data.updated_on);
        Assert.NotNull(result.Data.updated_on_string);
        Assert.NotNull(result.Data.updated_on_timezone);
    }

    private void ValidateMostListFields(StateListDto result)
    {
        Assert.NotZero(result.id);
        Assert.IsNotEmpty(result.guid);
        Assert.IsNotEmpty(result.iso2);
        Assert.NotZero(result.country_id);
        Assert.IsNotEmpty(result.state_name);
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