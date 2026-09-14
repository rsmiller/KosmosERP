using System.Data;
using Microsoft.EntityFrameworkCore;
using KosmosERP.Models;
using KosmosERP.Models.Permissions;
using KosmosERP.Tests.Modules.Shared;
using KosmosERP.BusinessLayer.Helpers;
using KosmosERP.BusinessLayer.Modules;
using KosmosERP.Database.Models;
using KosmosERP.BusinessLayer.Models.Module.Lead.Command.Create;
using KosmosERP.BusinessLayer.Models.Module.Lead.Command.Edit;
using KosmosERP.BusinessLayer.Models.Module.Lead.Command.Delete;
using KosmosERP.BusinessLayer.Models.Module.Lead.Command.Find;
using KosmosERP.BusinessLayer.Models.Module.Lead.Dto;
using KosmosERP.BusinessLayer;

namespace KosmosERP.Tests.Modules;

public class LeadModuleTests : BaseTestModule<LeadModule>, IModuleTest
{
    private Customer _Customer;

    [SetUp]
    public async Task SetupModule()
    {
        var logProviderFactory = new LogProviderFactory(new LogProviderSettings() { log_provider = LogProviderType.MOCK }, _Context, null);
        var the_module = new LeadModule(base._Context, logProviderFactory);

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
        var role = await _Context.Roles.Where(m => m.name == "Module Admin").FirstAsync();

        var role_module_permission = CommonDataHelper<RolePermission>.FillCommonFields(new RolePermission()
        {
            module_id = _Module.ModuleIdentifier.ToString(),
            role_id = role.id,
            read = true
        }, 1);

        _Context.RolePermissions.Add(role_module_permission);
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

    protected override async Task SetupData()
    {
        if (_Customer != null)
            return;

        var customer = CommonDataHelper<Customer>.FillCommonFields(new Customer()
        {
            category = "CAT1",
            fax = "123-123-1234",
            phone = "234-456-2312",
            general_email = "vendor@vendor.com",
            is_deleted = false,
            customer_name = "Some customer",
            website = "google.com",
            payment_terms = "payment_terms_net_15"
        }, 1);

        _Context.Customers.Add(customer);
        await _Context.SaveChangesAsync();

        _Customer = customer;
    }

    [Test]
    public async Task Get()
    {
        var new_result = await _Module.Create(new LeadCreateCommand()
        {
            calling_user_id = _User.external_id,
            company_name = _Customer.customer_name,
            lead_stage = "New",
            first_name = "Bob",
            last_name = "Builder",
            title = "Guy",
            email = "bob@builder.com",
            phone = "444-555-6666",
            cell_phone = "111-222-3333",
        });

        Assert.That(new_result.Success, Is.True);
        Assert.That(new_result.Data, Is.Not.Null);

        var result = await _Module.GetDto(new_result.Data.id);

        ValidateMostDtoFields(result);
    }

    [Test]
    public async Task Create()
    {
        var result = await _Module.Create(new LeadCreateCommand()
        {
            calling_user_id = _User.external_id,
            company_name= _Customer.customer_name,
            lead_stage = "New",
            address_line1 = "1234 St",
            state = "TX",
            city = "Austin",
            country = "USA",
            time_zone = "CST",
            zip = "76726",
            first_name = "Bob",
            last_name = "Builder",
            title = "Guy",
            email = "bob@builder.com",
            phone = "444-555-6666",
            cell_phone = "111-222-3333",
        });

        ValidateMostDtoFields(result);
    }

    [Test]
    public async Task Edit()
    {
        var new_result = await _Module.Create(new LeadCreateCommand()
        {
            calling_user_id = _User.external_id,
            company_name = _Customer.customer_name,
            lead_stage = "New",
            address_line1 = "1234 St",
            state = "TX",
            city = "Austin",
            country = "USA",
            time_zone = "CST",
            zip = "76726",
            first_name = "Bob",
            last_name = "Builder",
            title = "Guy",
            email = "bob@builder.com",
            phone = "444-555-6666",
            cell_phone = "111-222-3333",
        });

        Assert.That(new_result.Success, Is.True);
        Assert.That(new_result.Data, Is.Not.Null);

        var edit_command = new LeadEditCommand()
        {
            calling_user_id = _User.external_id,
            id = new_result.Data.id,
            first_name = "Bob123",
            last_name = "Builder333",
            title = "Guy111",
            email = "bob33@builder.com",
            phone = "999-888-6666",
            cell_phone = "444-555-3333",
        };

        var edit_result = await _Module.Edit(edit_command);

        ValidateMostDtoFields(edit_result);

        Assert.That(edit_result.Data.first_name == edit_command.first_name);
        Assert.That(edit_result.Data.last_name == edit_command.last_name);
        Assert.That(edit_result.Data.title == edit_command.title);
        Assert.That(edit_result.Data.email == edit_command.email);
        Assert.That(edit_result.Data.phone == edit_command.phone);
        Assert.That(edit_result.Data.cell_phone == edit_command.cell_phone);
    }

    [Test]
    public async Task Delete()
    {
        var new_result = await _Module.Create(new LeadCreateCommand()
        {
            calling_user_id = _User.external_id,
            company_name = _Customer.customer_name,
            lead_stage = "New",
            address_line1 = "1234 St",
            state = "TX",
            city = "Austin",
            country = "USA",
            time_zone = "CST",
            zip = "76726",
            first_name = "Bob",
            last_name = "Builder",
            title = "Guy",
            email = "bob@builder.com",
            phone = "444-555-6666",
            cell_phone = "111-222-3333",
        });

        Assert.That(new_result.Success, Is.True);
        Assert.That(new_result.Data, Is.Not.Null);

        var delete_result = await _Module.Delete(new LeadDeleteCommand()
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
        var new_result = await _Module.Create(new LeadCreateCommand()
        {
            calling_user_id = _User.external_id,
            company_name = "Chicken Nuggets",
            lead_stage = "New",
            address_line1 = "1234 St",
            state = "TX",
            city = "Austin",
            country = "USA",
            time_zone = "CST",
            zip = "76726",
            first_name = "Susan",
            last_name = "Builder",
            title = "Guy",
            email = "bob@builder.com",
            phone = "444-555-6666",
            cell_phone = "111-222-3333",
        });

        Assert.That(new_result.Success, Is.True);
        Assert.That(new_result.Data, Is.Not.Null);

        var results = await _Module.Find(
                        new PagingSortingParameters() { ResultCount = 20, Start = 0 },
                        new LeadFindCommand() { calling_user_id = _User.external_id, wildcard = "Susan" });
        
        Assert.That(results.Success, Is.True);
        Assert.That(results.Data, Is.Not.Null);
        Assert.That(results.Data.Count(), Is.Not.Zero);

        var first_result = results.Data.First();

        ValidateMostListFields(first_result);
    }

    private void ValidateMostDtoFields(Response<LeadDto> result)
    {
        Assert.That(result.Success, Is.True);
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data.id, Is.Not.Zero);
        Assert.That(result.Data.guid, Is.Not.Empty);
        Assert.That(result.Data.company_name, Is.Not.Empty);
        Assert.That(result.Data.first_name, Is.Not.Empty);
        Assert.That(result.Data.last_name, Is.Not.Empty);
        Assert.That(result.Data.email, Is.Not.Empty);
        Assert.That(result.Data.phone, Is.Not.Empty);
        Assert.That(result.Data.cell_phone, Is.Not.Empty);
        Assert.That(result.Data.address_line1, Is.Not.Empty);
        Assert.That(result.Data.address_line2, Is.Not.Empty);
        Assert.That(result.Data.city, Is.Not.Empty);
        Assert.That(result.Data.state, Is.Not.Empty);
        Assert.That(result.Data.zip, Is.Not.Empty);
        Assert.That(result.Data.country, Is.Not.Empty);
        Assert.That(result.Data.created_by, Is.Not.Zero);
        Assert.That(result.Data.created_on, Is.GreaterThan(DateTime.MinValue));
        Assert.That(result.Data.created_on_string, Is.Not.Null);
        Assert.That(result.Data.created_on_timezone, Is.Not.Null);
        Assert.That(result.Data.updated_by, Is.Not.Null);
        Assert.That(result.Data.updated_on, Is.Not.Null);
        Assert.That(result.Data.updated_on_string, Is.Not.Null);
        Assert.That(result.Data.updated_on_timezone, Is.Not.Null);
    }

    private void ValidateMostListFields(LeadListDto result)
    {
        Assert.That(result.id, Is.Not.Zero);
        Assert.That(result.guid, Is.Not.Empty);
        Assert.That(result.company_name, Is.Not.Empty);
        Assert.That(result.first_name, Is.Not.Empty);
        Assert.That(result.last_name, Is.Not.Empty);
        Assert.That(result.email, Is.Not.Empty);
        Assert.That(result.phone, Is.Not.Empty);
        Assert.That(result.cell_phone, Is.Not.Empty);
        Assert.That(result.address_line1, Is.Not.Empty);
        Assert.That(result.address_line2, Is.Not.Empty);
        Assert.That(result.city, Is.Not.Empty);
        Assert.That(result.state, Is.Not.Empty);
        Assert.That(result.zip, Is.Not.Empty);
        Assert.That(result.country, Is.Not.Empty);
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