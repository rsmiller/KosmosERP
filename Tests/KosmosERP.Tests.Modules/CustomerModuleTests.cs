using System.Data;
using Microsoft.EntityFrameworkCore;
using KosmosERP.Models;
using KosmosERP.Models.Permissions;
using KosmosERP.Tests.Modules.Shared;
using KosmosERP.BusinessLayer.Helpers;
using KosmosERP.BusinessLayer.Modules;
using KosmosERP.Database.Models;
using KosmosERP.BusinessLayer.Models.Module.Customer.Command.Create;
using KosmosERP.BusinessLayer.Models.Module.Customer.Command.Edit;
using KosmosERP.BusinessLayer.Models.Module.Customer.Command.Delete;
using KosmosERP.BusinessLayer.Models.Module.Customer.Command.Find;
using KosmosERP.BusinessLayer.Models.Module.Customer.Dto;

namespace KosmosERP.Tests.Modules;

public class CustomerModuleTests : BaseTestModule<CustomerModule>, IModuleTest
{
    [SetUp]
    public async Task SetupModule()
    {
        var the_module = new CustomerModule(base._Context);

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
        var read_permission = await _Context.ModulePermissions.FirstAsync(m => m.module_id == _Module.ModuleIdentifier.ToString() && m.internal_permission_name == CustomerPermissions.Read);
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
        var create_permission = await _Context.ModulePermissions.FirstAsync(m => m.module_id == _Module.ModuleIdentifier.ToString() && m.internal_permission_name == CustomerPermissions.Create);
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
        var edit_permission = await _Context.ModulePermissions.FirstAsync(m => m.module_id == _Module.ModuleIdentifier.ToString() && m.internal_permission_name == CustomerPermissions.Edit);
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
        var delete_permission = await _Context.ModulePermissions.FirstAsync(m => m.module_id == _Module.ModuleIdentifier.ToString() && m.internal_permission_name == CustomerPermissions.Delete);
        var role = await _Context.Roles.Where(m => m.name == "Module Admin").FirstAsync();

        var role_module_permission = CommonDataHelper<RolePermission>.FillCommonFields(new RolePermission()
        {
            module_permission_id = delete_permission.id,
            role_id = role.id,
        }, 1);

        _Context.RolePermissions.Add(role_module_permission);
        await _Context.SaveChangesAsync();
    }

    [Test]
    public async Task Get()
    {
        var new_result = await _Module.Create(new CustomerCreateCommand()
        {
            calling_user_id = 1,
            token = _SessionId,
            customer_name = "Cool customer",
            customer_description = "Super cool customer guy",
            phone = "567-554-1234",
            fax = "777-555-2233",
            general_email = "TheEmail@email.com",
            website = "https://chickennuggets.com",
            category = "Category1",
            is_taxable = false,
            tax_rate = 98,
            payment_terms_id = 1,
        });

        Assert.IsTrue(new_result.Success);
        Assert.IsNotNull(new_result.Data);

        var result = await _Module.GetDto(new_result.Data.id);

        ValidateMostDtoFields(result);
    }

    [Test]
    public async Task Create()
    {
        var create_command = new CustomerCreateCommand()
        {
            calling_user_id = 1,
            token = _SessionId,
            customer_name = "Cool customer123123",
            customer_description = "Super cool customer guy123123",
            phone = "999-554-1234",
            fax = "111-555-2233",
            general_email = "TheEmail@email.com",
            website = "https://chickennuggets22.com",
            category = "Category231",
            is_taxable = false,
            tax_rate = 91,
            payment_terms_id = 1,
        };

        var result = await _Module.Create(create_command);

        ValidateMostDtoFields(result);
        Assert.NotZero(result.Data.customer_number);
        Assert.That(result.Data.customer_name == create_command.customer_name);
        Assert.That(result.Data.customer_description == create_command.customer_description);
        Assert.That(result.Data.phone == create_command.phone);
        Assert.That(result.Data.fax == create_command.fax);
        Assert.That(result.Data.general_email == create_command.general_email);
        Assert.That(result.Data.website == create_command.website);
        Assert.That(result.Data.category == create_command.category);
        Assert.That(result.Data.is_taxable == create_command.is_taxable);
        Assert.That(result.Data.tax_rate == create_command.tax_rate);
    }

    [Test]
    public async Task Edit()
    {
        var new_result = await _Module.Create(new CustomerCreateCommand()
        {
            calling_user_id = 1,
            token = _SessionId,
            customer_name = "Cool customer1777",
            customer_description = "Super cool customer guy1545",
            phone = "999-666-5655",
            fax = "111-234-333",
            general_email = "TheEmai1l@email.com",
            website = "https://chickennuggets29898.com",
            category = "Category9",
            is_taxable = false,
            tax_rate = 42,
            payment_terms_id = 1,
        });

        Assert.IsTrue(new_result.Success);
        Assert.IsNotNull(new_result.Data);

        var edit_command = new CustomerEditCommand()
        {
            calling_user_id = 1,
            token = _SessionId,
            id = new_result.Data.id,
            customer_name = "Cool customer000",
            customer_description = "Super cool customer guy000",
            phone = "999-666-0000",
            fax = "111-000-333",
            general_email = "TheEmail99@email.com",
            website = "https://chickennuggets20909.com",
            category = "Category12",
            is_taxable = true,
            tax_rate = 11,
            payment_terms_id = 2,
        };

        var edit_result = await _Module.Edit(edit_command);

        ValidateMostDtoFields(edit_result);

        Assert.That(edit_result.Data.customer_name == edit_command.customer_name);
        Assert.That(edit_result.Data.customer_description == edit_command.customer_description);
        Assert.That(edit_result.Data.phone == edit_command.phone);
        Assert.That(edit_result.Data.fax == edit_command.fax);
        Assert.That(edit_result.Data.general_email == edit_command.general_email);
        Assert.That(edit_result.Data.website == edit_command.website);
        Assert.That(edit_result.Data.category == edit_command.category);
        Assert.That(edit_result.Data.is_taxable == edit_command.is_taxable);
        Assert.That(edit_result.Data.tax_rate == edit_command.tax_rate);
        Assert.That(edit_result.Data.payment_terms_id == edit_command.payment_terms_id);
    }

    [Test]
    public async Task Delete()
    {
        var new_result = await _Module.Create(new CustomerCreateCommand()
        {
            calling_user_id = 1,
            token = _SessionId,
            customer_name = "Cool customer000",
            customer_description = "Super cool customer guy000",
            phone = "999-666-0000",
            fax = "111-000-333",
            general_email = "TheEmail99@email.com",
            website = "https://chickennuggets20909.com",
            category = "Category12",
            is_taxable = true,
            tax_rate = 11,
            payment_terms_id = 1,
        });

        Assert.IsTrue(new_result.Success);
        Assert.IsNotNull(new_result.Data);

        var delete_result = await _Module.Delete(new CustomerDeleteCommand()
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
        var new_result = await _Module.Create(new CustomerCreateCommand()
        {
            calling_user_id = 1,
            token = _SessionId,
            customer_name = "Cool customer4322",
            customer_description = "Super cool customer guy3231",
            phone = "544-889-9778",
            fax = "234-555-333",
            general_email = "TheEmail99@email.com",
            website = "https://chickennuggets289.com",
            category = "Category2",
            is_taxable = true,
            tax_rate = 23,
            payment_terms_id = 1,
        });

        Assert.IsTrue(new_result.Success);
        Assert.IsNotNull(new_result.Data);

        var results = await _Module.Find(
                        new PagingSortingParameters() { ResultCount = 20, Start = 0 },
                        new CustomerFindCommand() { calling_user_id = 1, token = _SessionId, wildcard = "customer4322" });
        
        Assert.IsTrue(results.Success);
        Assert.IsNotNull(results.Data);
        Assert.NotZero(results.Data.Count());

        var first_result = results.Data.First();

        ValidateMostListFields(first_result);
    }

    private void ValidateMostDtoFields(Response<CustomerDto> result)
    {
        Assert.IsTrue(result.Success);
        Assert.IsNotNull(result.Data);
        Assert.NotZero(result.Data.id);
        Assert.IsNotEmpty(result.Data.guid);
        Assert.NotZero(result.Data.customer_number);
        Assert.IsNotEmpty(result.Data.customer_name);
        Assert.IsNotEmpty(result.Data.customer_description);
        Assert.IsNotEmpty(result.Data.phone);
        Assert.IsNotEmpty(result.Data.fax);
        Assert.IsNotEmpty(result.Data.general_email);
        Assert.IsNotEmpty(result.Data.website);
        Assert.IsNotEmpty(result.Data.category);
        Assert.NotNull(result.Data.is_taxable);
        Assert.NotZero(result.Data.payment_terms_id);
        Assert.NotNull(result.Data.tax_rate);
        Assert.NotZero(result.Data.created_by);
        Assert.NotNull(result.Data.created_on);
        Assert.NotNull(result.Data.created_on_string);
        Assert.NotNull(result.Data.created_on_timezone);
        Assert.NotNull(result.Data.updated_by);
        Assert.NotNull(result.Data.updated_on);
        Assert.NotNull(result.Data.updated_on_string);
        Assert.NotNull(result.Data.updated_on_timezone);
    }

    private void ValidateMostListFields(CustomerListDto result)
    {
        Assert.NotZero(result.id);
        Assert.IsNotEmpty(result.guid);
        Assert.NotZero(result.customer_number);
        Assert.IsNotEmpty(result.customer_name);
        Assert.IsNotEmpty(result.customer_description);
        Assert.IsNotEmpty(result.phone);
        Assert.IsNotEmpty(result.fax);
        Assert.IsNotEmpty(result.general_email);
        Assert.NotZero(result.payment_terms_id);
        Assert.IsNotEmpty(result.website);
        Assert.IsNotEmpty(result.category);
        Assert.NotNull(result.is_taxable);
        Assert.NotNull(result.tax_rate);
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