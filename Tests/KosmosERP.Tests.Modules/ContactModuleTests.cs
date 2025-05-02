using System.Data;
using Microsoft.EntityFrameworkCore;
using KosmosERP.Models;
using KosmosERP.Models.Permissions;
using KosmosERP.Tests.Modules.Shared;
using KosmosERP.BusinessLayer.Helpers;
using KosmosERP.BusinessLayer.Modules;
using KosmosERP.Database.Models;
using KosmosERP.BusinessLayer.Models.Module.Contact.Command.Create;
using KosmosERP.BusinessLayer.Models.Module.Contact.Command.Edit;
using KosmosERP.BusinessLayer.Models.Module.Contact.Command.Delete;
using KosmosERP.BusinessLayer.Models.Module.Contact.Command.Find;
using KosmosERP.BusinessLayer.Models.Module.Contact.Dto;

namespace KosmosERP.Tests.Modules;

public class ContactModuleTests : BaseTestModule<ContactModule>, IModuleTest
{
    private Customer _Customer;

    [SetUp]
    public async Task SetupModule()
    {
        var the_module = new ContactModule(base._Context);

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
        var read_permission = await _Context.ModulePermissions.FirstAsync(m => m.module_id == _Module.ModuleIdentifier.ToString() && m.internal_permission_name == ContactPermissions.Read);
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
        var create_permission = await _Context.ModulePermissions.FirstAsync(m => m.module_id == _Module.ModuleIdentifier.ToString() && m.internal_permission_name == ContactPermissions.Create);
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
        var edit_permission = await _Context.ModulePermissions.FirstAsync(m => m.module_id == _Module.ModuleIdentifier.ToString() && m.internal_permission_name == ContactPermissions.Edit);
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
        var delete_permission = await _Context.ModulePermissions.FirstAsync(m => m.module_id == _Module.ModuleIdentifier.ToString() && m.internal_permission_name == ContactPermissions.Delete);
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
    }

    [Test]
    public async Task Get()
    {
        var new_result = await _Module.Create(new ContactCreateCommand()
        {
            calling_user_id = 1,
            token = _SessionId,
            customer_id = _Customer.id,
            first_name = "Bob",
            last_name = "Builder",
            title = "Guy",
            email = "bob@builder.com",
            phone = "444-555-6666",
            cell_phone = "111-222-3333",
        });

        Assert.IsTrue(new_result.Success);
        Assert.IsNotNull(new_result.Data);

        var result = await _Module.GetDto(new_result.Data.id);

        ValidateMostDtoFields(result);
    }

    [Test]
    public async Task Create()
    {
        var result = await _Module.Create(new ContactCreateCommand()
        {
            calling_user_id = 1,
            token = _SessionId,
            customer_id = _Customer.id,
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
        var new_result = await _Module.Create(new ContactCreateCommand()
        {
            calling_user_id = 1,
            token = _SessionId,
            customer_id = _Customer.id,
            first_name = "Bob",
            last_name = "Builder",
            title = "Guy",
            email = "bob@builder.com",
            phone = "444-555-6666",
            cell_phone = "111-222-3333",
        });

        Assert.IsTrue(new_result.Success);
        Assert.IsNotNull(new_result.Data);

        var edit_command = new ContactEditCommand()
        {
            calling_user_id = 1,
            token = _SessionId,
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
        var new_result = await _Module.Create(new ContactCreateCommand()
        {
            calling_user_id = 1,
            token = _SessionId,
            first_name = "Bob123",
            last_name = "Builder333",
            title = "Guy111",
            email = "bob33@builder.com",
            phone = "999-888-6666",
            cell_phone = "444-555-3333",
        });

        Assert.IsTrue(new_result.Success);
        Assert.IsNotNull(new_result.Data);

        var delete_result = await _Module.Delete(new ContactDeleteCommand()
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
        var new_result = await _Module.Create(new ContactCreateCommand()
        {
            calling_user_id = 1,
            token = _SessionId,
            first_name = "Bob123",
            last_name = "Builder333",
            title = "Guy111",
            email = "bob33@builder.com",
            phone = "999-888-6666",
            cell_phone = "444-555-3333",
        });

        Assert.IsTrue(new_result.Success);
        Assert.IsNotNull(new_result.Data);

        var results = await _Module.Find(
                        new PagingSortingParameters() { ResultCount = 20, Start = 0 },
                        new ContactFindCommand() { calling_user_id = 1, token = _SessionId, wildcard = "Bob" });
        
        Assert.IsTrue(results.Success);
        Assert.IsNotNull(results.Data);
        Assert.NotZero(results.Data.Count());

        var first_result = results.Data.First();

        ValidateMostListFields(first_result);
    }

    private void ValidateMostDtoFields(Response<ContactDto> result)
    {
        Assert.IsTrue(result.Success);
        Assert.IsNotNull(result.Data);
        Assert.NotZero(result.Data.id);
        Assert.IsNotEmpty(result.Data.guid);
        Assert.IsNotEmpty(result.Data.first_name);
        Assert.IsNotEmpty(result.Data.last_name);
        Assert.IsNotEmpty(result.Data.email);
        Assert.IsNotEmpty(result.Data.phone);
        Assert.IsNotEmpty(result.Data.cell_phone);
        Assert.NotZero(result.Data.created_by);
        Assert.NotNull(result.Data.created_on);
        Assert.NotNull(result.Data.created_on_string);
        Assert.NotNull(result.Data.created_on_timezone);
        Assert.NotNull(result.Data.updated_by);
        Assert.NotNull(result.Data.updated_on);
        Assert.NotNull(result.Data.updated_on_string);
        Assert.NotNull(result.Data.updated_on_timezone);
    }

    private void ValidateMostListFields(ContactListDto result)
    {
        Assert.NotZero(result.id);
        Assert.IsNotEmpty(result.guid);
        Assert.IsNotEmpty(result.first_name);
        Assert.IsNotEmpty(result.last_name);
        Assert.IsNotEmpty(result.email);
        Assert.IsNotEmpty(result.phone);
        Assert.IsNotEmpty(result.cell_phone);
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