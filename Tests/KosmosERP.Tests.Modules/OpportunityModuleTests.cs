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

namespace KosmosERP.Tests.Modules;

public class OpportunityModuleTests : BaseTestModule<OpportunityModule>, IModuleTest
{
    private Customer _Customer;
    private Contact _Contact;

    [SetUp]
    public async Task SetupModule()
    {
        var the_module = new OpportunityModule(base._Context);

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
        var read_permission = await _Context.ModulePermissions.FirstAsync(m => m.module_id == _Module.ModuleIdentifier.ToString() && m.internal_permission_name == OpportunityPermissions.Read);
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
        var create_permission = await _Context.ModulePermissions.FirstAsync(m => m.module_id == _Module.ModuleIdentifier.ToString() && m.internal_permission_name == OpportunityPermissions.Create);
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
        var edit_permission = await _Context.ModulePermissions.FirstAsync(m => m.module_id == _Module.ModuleIdentifier.ToString() && m.internal_permission_name == OpportunityPermissions.Edit);
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
        var delete_permission = await _Context.ModulePermissions.FirstAsync(m => m.module_id == _Module.ModuleIdentifier.ToString() && m.internal_permission_name == OpportunityPermissions.Delete);
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
            calling_user_id = 1,
            token = _SessionId,
            opportunity_name = "Opportunity 11123",
            customer_id = _Customer.id,
            contact_id = _Contact.id,
            amount = 135,
            stage = "Prospecting",
            win_chance = 21,
            expected_close = DateOnly.FromDateTime(DateTime.Now.AddDays(10)),
        });

        Assert.IsTrue(new_result.Success);
        Assert.IsNotNull(new_result.Data);

        var result = await _Module.GetDto(new_result.Data.id);

        ValidateMostDtoFields(result);
    }

    [Test]
    public async Task Create()
    {
        var result = await _Module.Create(new OpportunityCreateCommand()
        {
            calling_user_id = 1,
            token = _SessionId,
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
            calling_user_id = 1,
            token = _SessionId,
            opportunity_name = "Opportunity 12",
            customer_id = _Customer.id,
            contact_id = _Contact.id,
            amount = 35,
            stage = "Prospecting",
            win_chance = 50,
            expected_close = DateOnly.FromDateTime(DateTime.Now.AddDays(40)),
        });

        Assert.IsTrue(new_result.Success);
        Assert.IsNotNull(new_result.Data);

        var edit_command = new OpportunityEditCommand()
        {
            calling_user_id = 1,
            token = _SessionId,
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
            calling_user_id = 1,
            token = _SessionId,
            opportunity_name = "Opportunity dd",
            customer_id = _Customer.id,
            contact_id = _Contact.id,
            amount = 35,
            stage = "Prospecting",
            win_chance = 1,
            expected_close = DateOnly.FromDateTime(DateTime.Now.AddDays(40)),
        });

        Assert.IsTrue(new_result.Success);
        Assert.IsNotNull(new_result.Data);

        var delete_result = await _Module.Delete(new OpportunityDeleteCommand()
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
        var new_result = await _Module.Create(new OpportunityCreateCommand()
        {
            calling_user_id = 1,
            token = _SessionId,
            opportunity_name = "The Best",
            customer_id = _Customer.id,
            contact_id = _Contact.id,
            amount = 35,
            stage = "Prospecting",
            win_chance = 1,
            expected_close = DateOnly.FromDateTime(DateTime.Now.AddDays(40)),
        });

        Assert.IsTrue(new_result.Success);
        Assert.IsNotNull(new_result.Data);

        var results = await _Module.Find(
                        new PagingSortingParameters() { ResultCount = 20, Start = 0 },
                        new OpportunityFindCommand() { calling_user_id = 1, token = _SessionId, wildcard = "Best" });
        
        Assert.IsTrue(results.Success);
        Assert.IsNotNull(results.Data);
        Assert.NotZero(results.Data.Count());

        var first_result = results.Data.First();

        ValidateMostListFields(first_result);
    }

    private void ValidateMostDtoFields(Response<OpportunityDto> result)
    {
        Assert.IsTrue(result.Success);
        Assert.IsNotNull(result.Data);
        Assert.NotZero(result.Data.id);
        Assert.IsNotEmpty(result.Data.guid);
        Assert.IsNotEmpty(result.Data.stage);
        Assert.IsNotEmpty(result.Data.opportunity_name);
        Assert.NotZero(result.Data.owner_id);
        Assert.NotZero(result.Data.win_chance);
        Assert.NotZero(result.Data.amount);
        Assert.NotZero(result.Data.contact_id);
        Assert.NotZero(result.Data.customer_id);
        Assert.NotZero(result.Data.created_by);
        Assert.NotNull(result.Data.created_on);
        Assert.NotNull(result.Data.created_on_string);
        Assert.NotNull(result.Data.created_on_timezone);
        Assert.NotNull(result.Data.updated_by);
        Assert.NotNull(result.Data.updated_on);
        Assert.NotNull(result.Data.updated_on_string);
        Assert.NotNull(result.Data.updated_on_timezone);
    }

    private void ValidateMostListFields(OpportunityListDto result)
    {
        Assert.NotZero(result.id);
        Assert.IsNotEmpty(result.guid);
        Assert.IsNotEmpty(result.stage);
        Assert.IsNotEmpty(result.opportunity_name);
        Assert.NotZero(result.owner_id);
        Assert.NotZero(result.win_chance);
        Assert.NotZero(result.amount);
        Assert.NotZero(result.contact_id);
        Assert.NotZero(result.customer_id);
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