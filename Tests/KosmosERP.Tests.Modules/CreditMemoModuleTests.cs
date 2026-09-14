using KosmosERP.Models;
using KosmosERP.Models.Permissions;
using KosmosERP.Tests.Modules.Shared;
using KosmosERP.BusinessLayer.Helpers;
using KosmosERP.BusinessLayer.Modules;
using KosmosERP.Database.Models;
using KosmosERP.BusinessLayer.Models.Module.CreditMemo.Command.Create;
using KosmosERP.BusinessLayer.Models.Module.CreditMemo.Command.Edit;
using KosmosERP.BusinessLayer.Models.Module.CreditMemo.Command.Delete;
using KosmosERP.BusinessLayer.Models.Module.CreditMemo.Command.Find;
using Microsoft.EntityFrameworkCore;
using KosmosERP.BusinessLayer;

namespace KosmosERP.Tests.Modules;

public class CreditMemoModuleTests : BaseTestModule<CreditMemoModule>, IModuleTest
{
    private Customer _Customer;
    private CreditMemoHeader _CreditMemoHeader;
    private CreditMemoLine _CreditMemoLine;

    [SetUp]
    public async Task SetupModule()
    {
        var logProviderFactory = new LogProviderFactory(new LogProviderSettings() { log_provider = LogProviderType.MOCK }, _Context, null);
        var the_module = new CreditMemoModule(base._Context, logProviderFactory);

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

    protected override async Task SetupTestData()
    {
        _Customer = CommonDataHelper<Customer>.FillCommonFields(new Customer()
        {
            customer_name = "Test Customer",
            phone = "555-1234",
            website = "www.testcustomer.com",
            category = "Business",
            payment_terms = "Net 30",
            is_taxable = true,
            tax_rate = 0.08m,
        }, 1);

        _Context.Customers.Add(_Customer);
        await _Context.SaveChangesAsync();

        _CreditMemoHeader = CommonDataHelper<CreditMemoHeader>.FillCommonFields(new CreditMemoHeader()
        {
            customer_id = _Customer.id,
            credit_memo_number = 1,
            credit_memo_date = DateTime.UtcNow,
            credit_memo_due_date = DateTime.UtcNow.AddDays(30),
            credit_memo_total = 100.00m,
            memo = "Test credit memo",
            credit_reason = "Return",
            is_approved = false,
            is_applied = false,
            guid = Guid.NewGuid().ToString(),
        }, 1);

        _Context.CreditMemoHeaders.Add(_CreditMemoHeader);
        await _Context.SaveChangesAsync();

        _CreditMemoLine = CommonDataHelper<CreditMemoLine>.FillCommonFields(new CreditMemoLine()
        {
            credit_memo_header_id = _CreditMemoHeader.id,
            line_number = 1,
            line_total = 100.00m,
            qty_credited = 1,
            gl_account_id = "TEST",
            description = "Test credit memo line",
            guid = Guid.NewGuid().ToString(),
        }, 1);

        _Context.CreditMemoLines.Add(_CreditMemoLine);
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
    
    [Test]
    public async Task Get()
    {
        var result = await _Module.GetDto(_CreditMemoHeader.id);

        Assert.That(result.Success, Is.True);
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data.credit_memo_number, Is.GreaterThan(0));
        Assert.That(result.Data.customer_name, Is.EqualTo("Test Customer"));
    }

    [Test]
    public async Task Create()
    {
        var command = new CreditMemoHeaderCreateCommand()
        {
            calling_user_id = _User.external_id,
            customer_id = _Customer.id,
            credit_memo_date = DateTime.UtcNow,
            credit_memo_due_date = DateTime.UtcNow.AddDays(30),
            credit_memo_total = 200.00m,
            memo = "Test credit memo 2",
            credit_reason = "Discount",
            is_approved = false,
            is_applied = false,
            credit_memo_lines = new List<CreditMemoLineCreateCommand>()
            {
                new CreditMemoLineCreateCommand()
                {
                    calling_user_id = _User.external_id,
                    line_number = 1,
                    line_total = 200.00m,
                    qty_credited = 2,
                    gl_account_id = "TEST",
                    description = "Test credit memo line 2"
                }
            }
        };

        var result = await _Module.Create(command);

        Assert.That(result.Success, Is.True);
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data.credit_memo_number, Is.GreaterThan(0));
        Assert.That(result.Data.credit_memo_lines.Count, Is.EqualTo(1));
    }

    [Test]
    public async Task Edit()
    {
        var command = new CreditMemoHeaderEditCommand()
        {
            calling_user_id = _User.external_id,
            id = _CreditMemoHeader.id,
            memo = "Updated memo",
            is_approved = true
        };

        var result = await _Module.Edit(command);

        Assert.That(result.Success, Is.True);
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data.memo, Is.EqualTo("Updated memo"));
        Assert.That(result.Data.is_approved, Is.True);
    }

    [Test]
    public async Task Delete()
    {
        var command = new CreditMemoHeaderDeleteCommand()
        {
            calling_user_id = _User.external_id,
            id = _CreditMemoHeader.id
        };

        var result = await _Module.Delete(command);

        Assert.That(result.Success, Is.True);
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data.is_deleted, Is.True);
    }

    [Test]
    public async Task Find()
    {
        var createCommand = new CreditMemoHeaderCreateCommand()
        {
            calling_user_id = _User.external_id,
            customer_id = _Customer.id,
            credit_memo_date = DateTime.UtcNow,
            credit_memo_due_date = DateTime.UtcNow.AddDays(30),
            credit_memo_total = 200.00m,
            memo = "Test credit memo 2",
            credit_reason = "Discount",
            is_approved = false,
            is_applied = false,
            credit_memo_lines = new List<CreditMemoLineCreateCommand>()
            {
                new CreditMemoLineCreateCommand()
                {
                    calling_user_id = _User.external_id,
                    line_number = 1,
                    line_total = 200.00m,
                    qty_credited = 2,
                    gl_account_id = "TEST",
                    description = "Test credit memo line 2"
                }
            }
        };

        var createResult = await _Module.Create(createCommand);

        Assert.That(createResult.Success, Is.True);
        Assert.That(createResult.Data, Is.Not.Null);

        var command = new CreditMemoHeaderFindCommand()
        {
            calling_user_id = _User.external_id,
            wildcard = createResult.Data.credit_memo_number.ToString()
        };

        var parameters = new PagingSortingParameters(0, 10, "id");
        var result = await _Module.Find(parameters, command);
    
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data.Count, Is.GreaterThan(0));
        Assert.That(result.Data[0].credit_memo_number, Is.GreaterThan(0));
    }
} 