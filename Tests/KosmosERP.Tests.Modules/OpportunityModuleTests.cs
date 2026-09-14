using System.Data;
using Microsoft.EntityFrameworkCore;
using KosmosERP.Models;
using KosmosERP.Models.Permissions;
using KosmosERP.Tests.Modules.Shared;
using KosmosERP.BusinessLayer.Helpers;
using KosmosERP.BusinessLayer.Modules;
using KosmosERP.Database.Models;
using KosmosERP.BusinessLayer.Models.Module.Opportunity.Command.Create;
using KosmosERP.BusinessLayer.Models.Module.Opportunity.Command.Edit;
using KosmosERP.BusinessLayer.Models.Module.Opportunity.Command.Delete;
using KosmosERP.BusinessLayer.Models.Module.Opportunity.Command.Find;
using KosmosERP.BusinessLayer.Models.Module.Opportunity.Dto;
using KosmosERP.BusinessLayer;
using Microsoft.Extensions.Caching.Memory;

namespace KosmosERP.Tests.Modules;

public class OpportunityModuleTests : BaseTestModule<OpportunityModule>, IModuleTest
{
    private Customer _Customer;
    private Contact _Contact;

    [SetUp]
    public async Task SetupModule()
    {
        var logProviderFactory = new LogProviderFactory(new LogProviderSettings() { log_provider = LogProviderType.MOCK }, _Context, null);
        var mem_cache = new MemoryCacheService<KeyValueStore>(new MemoryCache(new MemoryCacheOptions()), base._Context);

        var the_module = new OpportunityModule(base._Context, mem_cache, logProviderFactory);

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


        var contact = CommonDataHelper<Contact>.FillCommonFields(new Contact()
        {
            customer_id = _Customer.id,
            cell_phone = "234-456-2312",
            first_name = "Chicken",
            last_name = "Man",
            email = "email@bob.com",
            phone = "123-123-1234",
            title = "FAFO",
        }, 1);

        _Context.Contacts.Add(contact);
        await _Context.SaveChangesAsync();

        _Contact = contact;
    }

    [Test]
    public async Task Get()
    {
        var new_result = await _Module.Create(new OpportunityCreateCommand()
        {
            calling_user_id = _User.external_id,
            opportunity_name = "Opportunity 11123",
            customer_id = _Customer.id,
            contact_id = _Contact.id,
            amount = 135,
            stage = "Prospecting",
            win_chance = 21,
            expected_close = DateOnly.FromDateTime(DateTime.Now.AddDays(10)),
        });

        Assert.That(new_result.Success, Is.True);
        Assert.That(new_result.Data, Is.Not.Null);

        var result = await _Module.GetDto(new_result.Data.id);

        ValidateMostDtoFields(result);
    }

    [Test]
    public async Task Create()
    {
        var result = await _Module.Create(new OpportunityCreateCommand()
        {
            calling_user_id = _User.external_id,
            opportunity_name = "Opportunity 1",
            customer_id = _Customer.id,
            contact_id = _Contact.id,
            amount = 35,
            stage = "Prospecting",
            win_chance = 1,
            expected_close = DateOnly.FromDateTime(DateTime.Now.AddDays(40)),
        });

        ValidateMostDtoFields(result);
    }

    [Test]
    public async Task Edit()
    {
        var new_result = await _Module.Create(new OpportunityCreateCommand()
        {
            calling_user_id = _User.external_id,
            opportunity_name = "Opportunity 12",
            customer_id = _Customer.id,
            contact_id = _Contact.id,
            amount = 35,
            stage = "Prospecting",
            win_chance = 50,
            expected_close = DateOnly.FromDateTime(DateTime.Now.AddDays(40)),
        });

        Assert.That(new_result.Success, Is.True);
        Assert.That(new_result.Data, Is.Not.Null);

        var edit_command = new OpportunityEditCommand()
        {
            calling_user_id = _User.external_id,
            id = new_result.Data.id,
            opportunity_name = "Opportunity 22",
            customer_id = _Customer.id,
            contact_id = _Contact.id,
            amount = 11,
            stage = "Prospecting",
            win_chance = 40,
            expected_close = DateOnly.FromDateTime(DateTime.Now.AddDays(20)),
        };

        var edit_result = await _Module.Edit(edit_command);

        ValidateMostDtoFields(edit_result);

        Assert.That(edit_result.Data.opportunity_name == edit_command.opportunity_name);
        Assert.That(edit_result.Data.amount == edit_command.amount);
        Assert.That(edit_result.Data.stage == edit_command.stage);
        Assert.That(edit_result.Data.win_chance == edit_command.win_chance);
        Assert.That(edit_result.Data.expected_close == edit_command.expected_close);
    }

    [Test]
    public async Task Delete()
    {
        var new_result = await _Module.Create(new OpportunityCreateCommand()
        {
            calling_user_id = _User.external_id,
            opportunity_name = "Opportunity dd",
            customer_id = _Customer.id,
            contact_id = _Contact.id,
            amount = 35,
            stage = "Prospecting",
            win_chance = 1,
            expected_close = DateOnly.FromDateTime(DateTime.Now.AddDays(40)),
        });

        Assert.That(new_result.Success, Is.True);
        Assert.That(new_result.Data, Is.Not.Null);

        var delete_result = await _Module.Delete(new OpportunityDeleteCommand()
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
        var new_result = await _Module.Create(new OpportunityCreateCommand()
        {
            calling_user_id = _User.external_id,
            opportunity_name = "The Best",
            customer_id = _Customer.id,
            contact_id = _Contact.id,
            amount = 35,
            stage = "Prospecting",
            win_chance = 1,
            expected_close = DateOnly.FromDateTime(DateTime.Now.AddDays(40)),
        });

        Assert.That(new_result.Success, Is.True);
        Assert.That(new_result.Data, Is.Not.Null);

        var results = await _Module.Find(
                        new PagingSortingParameters() { ResultCount = 20, Start = 0 },
                        new OpportunityFindCommand() { calling_user_id = _User.external_id, wildcard = "Best" });
        
        Assert.That(results.Success, Is.True);
        Assert.That(results.Data, Is.Not.Null);
        Assert.That(results.Data.Count(), Is.Not.Zero);

        var first_result = results.Data.First();

        ValidateMostListFields(first_result);
    }

    private void ValidateMostDtoFields(Response<OpportunityDto> result)
    {
        Assert.That(result.Success, Is.True);
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data.id, Is.Not.Zero);
        Assert.That(result.Data.guid, Is.Not.Empty);
        Assert.That(result.Data.stage, Is.Not.Empty);
        Assert.That(result.Data.opportunity_name, Is.Not.Empty);
        Assert.That(result.Data.owner_id, Is.Not.Zero);
        Assert.That(result.Data.win_chance, Is.Not.Zero);
        Assert.That(result.Data.amount, Is.Not.Zero);
        Assert.That(result.Data.contact_id, Is.Not.Zero);
        Assert.That(result.Data.customer_id, Is.Not.Zero);
        Assert.That(result.Data.created_by, Is.Not.Zero);
        Assert.That(result.Data.created_on, Is.GreaterThan(DateTime.MinValue));
        Assert.That(result.Data.created_on_string, Is.Not.Null);
        Assert.That(result.Data.created_on_timezone, Is.Not.Null);
        Assert.That(result.Data.updated_by, Is.Not.Null);
        Assert.That(result.Data.updated_on, Is.Not.Null);
        Assert.That(result.Data.updated_on_string, Is.Not.Null);
        Assert.That(result.Data.updated_on_timezone, Is.Not.Null);
    }

    private void ValidateMostListFields(OpportunityListDto result)
    {
        Assert.That(result.id, Is.Not.Zero);
        Assert.That(result.guid, Is.Not.Empty);
        Assert.That(result.stage, Is.Not.Empty);
        Assert.That(result.opportunity_name, Is.Not.Empty);
        Assert.That(result.owner_id, Is.Not.Zero);
        Assert.That(result.win_chance, Is.Not.Zero);
        Assert.That(result.amount, Is.Not.Zero);
        Assert.That(result.contact_id, Is.Not.Zero);
        Assert.That(result.customer_id, Is.Not.Zero);
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