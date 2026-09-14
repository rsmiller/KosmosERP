using System.Data;
using Microsoft.EntityFrameworkCore;
using KosmosERP.Models;
using KosmosERP.Models.Permissions;
using KosmosERP.Tests.Modules.Shared;
using KosmosERP.BusinessLayer.Helpers;
using KosmosERP.BusinessLayer.Modules;
using KosmosERP.Database.Models;
using KosmosERP.BusinessLayer.Models.Module.Country.Command.Create;
using KosmosERP.BusinessLayer.Models.Module.Country.Command.Edit;
using KosmosERP.BusinessLayer.Models.Module.Country.Command.Delete;
using KosmosERP.BusinessLayer.Models.Module.Country.Command.Find;
using KosmosERP.BusinessLayer.Models.Module.Country.Dto;
using KosmosERP.BusinessLayer;

namespace KosmosERP.Tests.Modules;

public class CountryModuleTests : BaseTestModule<CountryModule>, IModuleTest
{
    [SetUp]
    public async Task SetupModule()
    {
        var logProviderFactory = new LogProviderFactory(new LogProviderSettings() { log_provider = LogProviderType.MOCK }, _Context, null);
        var the_module = new CountryModule(base._Context, logProviderFactory);

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
        var role = await _Context.Roles.Where(m => m.name == "Module Admin").FirstAsync();

        var role_module_permission = CommonDataHelper<RolePermission>.FillCommonFields(new RolePermission()
        {
            module_id = _Module.ModuleIdentifier.ToString(),
            role_id = role.id,
            write = true
        }, 1);

        _Context.RolePermissions.Add(role_module_permission);
        await _Context.SaveChangesAsync();
    }

    protected override async Task SetupEditPermissions()
    {
        var role = await _Context.Roles.Where(m => m.name == "Module Admin").FirstAsync();

        var role_module_permission = CommonDataHelper<RolePermission>.FillCommonFields(new RolePermission()
        {
            module_id = _Module.ModuleIdentifier.ToString(),
            role_id = role.id,
            edit = true
        }, 1);

        _Context.RolePermissions.Add(role_module_permission);
        await _Context.SaveChangesAsync();
    }

    protected override async Task SetupDeletePermissions()
    {
        var role = await _Context.Roles.Where(m => m.name == "Module Admin").FirstAsync();

        var role_module_permission = CommonDataHelper<RolePermission>.FillCommonFields(new RolePermission()
        {
            module_id = _Module.ModuleIdentifier.ToString(),
            role_id = role.id,
            delete = true
        }, 1);

        _Context.RolePermissions.Add(role_module_permission);
        await _Context.SaveChangesAsync();
    }

    [Test]
    public async Task Get()
    {
        var new_result = await _Module.Create(new CountryCreateCommand()
        {
            calling_user_id = _User.external_id,
            iso3 = "US",
            country_name = "United States",
            currency = "Dollar",
            currency_symbol = "$",
            phonecode = "+1",
            region = "North America",
        });

        Assert.That(new_result.Success, Is.True);
        Assert.That(new_result.Data, Is.Not.Null);

        var result = await _Module.GetDto(new_result.Data.id);

        ValidateMostDtoFields(result);
    }

    [Test]
    public async Task Create()
    {
        var result = await _Module.Create(new CountryCreateCommand()
        {
            calling_user_id = _User.external_id,
            iso3 = "US",
            country_name = "United States",
            currency = "Dollar",
            currency_symbol = "$",
            phonecode = "+1",
            region = "North America",
        });

        ValidateMostDtoFields(result);
    }

    [Test]
    public async Task Edit()
    {
        var new_result = await _Module.Create(new CountryCreateCommand()
        {
            calling_user_id = _User.external_id,
            iso3 = "US",
            country_name = "United States",
            currency = "Dollar",
            currency_symbol = "$",
            phonecode = "+1",
            region = "North America",
        });

        Assert.That(new_result.Success, Is.True);
        Assert.That(new_result.Data, Is.Not.Null);

        var edit_command = new CountryEditCommand()
        {
            calling_user_id = _User.external_id,
            id = new_result.Data.id,
            iso3 = "EU",
            country_name = "Europe",
            currency = "Dollar",
            currency_symbol = "�",
            phonecode = "+55",
            region = "Europe",
        };

        var edit_result = await _Module.Edit(edit_command);

        ValidateMostDtoFields(edit_result);

        Assert.That(edit_result.Data.iso3 == edit_command.iso3);
        Assert.That(edit_result.Data.country_name == edit_command.country_name);
        Assert.That(edit_result.Data.currency_symbol == edit_command.currency_symbol);
        Assert.That(edit_result.Data.currency == edit_command.currency);
        Assert.That(edit_result.Data.region == edit_command.region);
        Assert.That(edit_result.Data.phonecode == edit_command.phonecode);
    }

    [Test]
    public async Task Delete()
    {
        var new_result = await _Module.Create(new CountryCreateCommand()
        {
            calling_user_id = _User.external_id,
            iso3 = "US",
            country_name = "United States",
            currency = "Dollar",
            currency_symbol = "$",
            phonecode = "+1",
            region = "North America",
        });

        Assert.That(new_result.Success, Is.True);
        Assert.That(new_result.Data, Is.Not.Null);

        var delete_result = await _Module.Delete(new CountryDeleteCommand()
        {
            calling_user_id = _User.external_id,
            id = new_result.Data.id
        });

        Assert.That(delete_result.Success, Is.True);
        Assert.That(delete_result.Data, Is.Not.Null);
        Assert.That(delete_result.Data.deleted_by, Is.Not.Null);
        Assert.That(delete_result.Data.deleted_on, Is.Not.Null);
        Assert.That(delete_result.Data.deleted_on_string, Is.Not.Null);
        Assert.That(delete_result.Data.deleted_on_timezone, Is.Not.Null);
    }

    [Test]
    public async Task Find()
    {
        var new_result = await _Module.Create(new CountryCreateCommand()
        {
            calling_user_id = _User.external_id,
            iso3 = "US",
            country_name = "United States",
            currency = "Dollar",
            currency_symbol = "$",
            phonecode = "+1",
            region = "North America",
        });

        Assert.That(new_result.Success, Is.True);
        Assert.That(new_result.Data, Is.Not.Null);

        var results = await _Module.Find(
                        new PagingSortingParameters() { ResultCount = 20, Start = 0 },
                        new CountryFindCommand() { calling_user_id = _User.external_id, wildcard = "US" });
        
        Assert.That(results.Success, Is.True);
        Assert.That(results.Data, Is.Not.Null);
        Assert.That(results.Data.Count(), Is.Not.Zero);

        var first_result = results.Data.First();

        ValidateMostListFields(first_result);
    }

    private void ValidateMostDtoFields(Response<CountryDto> result)
    {
        Assert.That(result.Success, Is.True);
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data.id, Is.Not.Zero);
        Assert.That(result.Data.guid, Is.Not.Empty);
        Assert.That(result.Data.iso3, Is.Not.Empty);
        Assert.That(result.Data.country_name, Is.Not.Empty);
        Assert.That(result.Data.currency, Is.Not.Empty);
        Assert.That(result.Data.currency_symbol, Is.Not.Empty);
        Assert.That(result.Data.phonecode, Is.Not.Empty);
        Assert.That(result.Data.region, Is.Not.Empty);
        Assert.That(result.Data.created_by, Is.Not.Zero);
        Assert.That(result.Data.created_on, Is.GreaterThan(DateTime.MinValue));
        Assert.That(result.Data.created_on_string, Is.Not.Null);
        Assert.That(result.Data.created_on_timezone, Is.Not.Null);
        Assert.That(result.Data.updated_by, Is.Not.Null);
        Assert.That(result.Data.updated_on, Is.Not.Null);
        Assert.That(result.Data.updated_on_string, Is.Not.Null);
        Assert.That(result.Data.updated_on_timezone, Is.Not.Null);
    }

    private void ValidateMostListFields(CountryListDto result)
    {
        Assert.That(result.id, Is.Not.Zero);
        Assert.That(result.guid, Is.Not.Empty);
        Assert.That(result.iso3, Is.Not.Empty);
        Assert.That(result.country_name, Is.Not.Empty);
        Assert.That(result.currency, Is.Not.Empty);
        Assert.That(result.currency_symbol, Is.Not.Empty);
        Assert.That(result.phonecode, Is.Not.Empty);
        Assert.That(result.region, Is.Not.Empty);
        Assert.That(result.created_by, Is.Not.Zero);
        Assert.That(result.created_on, Is.GreaterThan(DateTime.MinValue));
        Assert.That(result.created_on_string, Is.Not.Null);
        Assert.That(result.created_on_timezone, Is.Not.Null);
        Assert.That(result.updated_by, Is.Not.Null);
        Assert.That(result.updated_on, Is.Not.Null);
        Assert.That(result.updated_on_string, Is.Not.Null);
        Assert.That(result.updated_on_timezone, Is.Not.Null);
    }
}