using System.Data;
using Microsoft.EntityFrameworkCore;
using KosmosERP.Models;
using KosmosERP.Models.Permissions;
using KosmosERP.Tests.Modules.Shared;
using KosmosERP.BusinessLayer.Helpers;
using KosmosERP.BusinessLayer.Modules;
using KosmosERP.Database.Models;
using KosmosERP.BusinessLayer.Models.Module.Transaction.Command.Create;
using KosmosERP.BusinessLayer.Models.Module.Transaction.Command.Edit;
using KosmosERP.BusinessLayer.Models.Module.Transaction.Command.Delete;
using KosmosERP.BusinessLayer.Models.Module.Transaction.Command.Find;
using KosmosERP.BusinessLayer.Models.Module.Transaction.Dto;

namespace KosmosERP.Tests.Modules;

public class TransactionModuleTests : BaseTestModule<TransactionModule>, IModuleTest
{
    private Customer _Customer;
    private OrderHeader _SalesOrderHeader;
    private OrderLine _SalesOrderLine;
    private Product _Product;
    private Address _Address;

    [SetUp]
    public async Task SetupModule()
    {
        var the_module = new TransactionModule(base._Context);

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
        var read_permission = await _Context.ModulePermissions.FirstAsync(m => m.module_id == _Module.ModuleIdentifier.ToString() && m.internal_permission_name == TransactionPermissions.Read);
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
        var create_permission = await _Context.ModulePermissions.FirstAsync(m => m.module_id == _Module.ModuleIdentifier.ToString() && m.internal_permission_name == TransactionPermissions.Create);
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
        var edit_permission = await _Context.ModulePermissions.FirstAsync(m => m.module_id == _Module.ModuleIdentifier.ToString() && m.internal_permission_name == TransactionPermissions.Edit);
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
        var delete_permission = await _Context.ModulePermissions.FirstAsync(m => m.module_id == _Module.ModuleIdentifier.ToString() && m.internal_permission_name == TransactionPermissions.Delete);
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
        var product = CommonDataHelper<Product>.FillCommonFields(new Product()
        {
            category = "Slings",
            sales_price = 100,
            list_price = 150,
            product_class = "Rope",
            product_name = "100ft Wirerope Sling",
            internal_description = "This is a cool sling",
            external_description = "This is a cool sling",
            identifier1 = "SL-100-C",
            our_cost = 40,
            unit_cost = 60,
            is_taxable = true,
            is_shippable = true,
            is_sales_item = true,
        }, 1);

        _Context.Products.Add(product);
        await _Context.SaveChangesAsync();

        _Product = product;

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

        _Address = address;

        var sales_order_header = CommonDataHelper<OrderHeader>.FillCommonFields(new OrderHeader()
        {
            order_number = 100002,
            order_type = "R",
            order_date = DateOnly.Parse(DateTime.Now.ToString("MM/dd/yyyy")),
            required_date = DateOnly.Parse(DateTime.Now.AddDays(2).ToString("MM/dd/yyyy")),
            customer_id = _Customer.id,
            ship_to_address_id = _Address.id,
            shipping_cost = 100,
            revision_number = 1,
            shipping_method_id = 1,
            pay_method_id = 1,
            po_number = "ASDSD",
            price = 1002,
            tax = 123
        }, 1);

        _Context.OrderHeaders.Add(sales_order_header);
        await _Context.SaveChangesAsync();

        _SalesOrderHeader = sales_order_header;

        var sales_order_line = CommonDataHelper<OrderLine>.FillCommonFields(new OrderLine()
        {
            order_header_id = _SalesOrderHeader.id,
            product_id = _Product.id,
            line_description = "A product with stuff",
            line_number = 1,
            unit_price = 100,
            quantity = 1
        }, 1);

        _Context.OrderLines.Add(sales_order_line);
        await _Context.SaveChangesAsync();

        _SalesOrderLine = sales_order_line;
    }

    [Test]
    public async Task Get()
    {
        var new_result = await _Module.Create(new TransactionCreateCommand()
        {
            calling_user_id = 1,
            token = _SessionId,
            object_reference_id = _SalesOrderHeader.id,
            object_sub_reference_id = _SalesOrderLine.id,
            product_id = _Product.id,
            transaction_date = DateTime.Now,
            transaction_type = 1,
            units_purchased = 1,
            units_received = 2,
            units_shipped = 3,
            units_sold = 4,
            sold_unit_price = 300,
            purchased_unit_cost = 100
        });

        Assert.IsTrue(new_result.Success);
        Assert.IsNotNull(new_result.Data);

        var result = await _Module.GetDto(new_result.Data.id);

        ValidateMostDtoFields(result);
    }

    [Test]
    public async Task Create()
    {
        var result = await _Module.Create(new TransactionCreateCommand()
        {
            calling_user_id = 1,
            token = _SessionId,
            object_reference_id = _SalesOrderHeader.id,
            object_sub_reference_id = _SalesOrderLine.id,
            product_id = _Product.id,
            transaction_date = DateTime.Now,
            transaction_type = 1,
            units_purchased = 1,
            units_received = 2,
            units_shipped = 3,
            units_sold = 4,
            sold_unit_price = 300,
            purchased_unit_cost = 100
        });

        ValidateMostDtoFields(result);
    }

    [Test]
    public async Task Edit()
    {
        var new_result = await _Module.Create(new TransactionCreateCommand()
        {
            calling_user_id = 1,
            token = _SessionId,
            object_reference_id = _SalesOrderHeader.id,
            object_sub_reference_id = _SalesOrderLine.id,
            product_id = _Product.id,
            transaction_date = DateTime.Now,
            transaction_type = 1,
            units_purchased = 1,
            units_received = 2,
            units_shipped = 3,
            units_sold = 4,
            sold_unit_price = 300,
            purchased_unit_cost = 100
        });

        Assert.IsTrue(new_result.Success);
        Assert.IsNotNull(new_result.Data);

        var edit_command = new TransactionEditCommand()
        {
            calling_user_id = 1,
            token = _SessionId,
            id = new_result.Data.id,
            object_reference_id = _SalesOrderHeader.id,
            object_sub_reference_id = _SalesOrderLine.id,
            product_id = _Product.id,
            transaction_date = DateTime.Now.AddDays(1),
            transaction_type = 2,
            units_purchased = 10,
            units_received = 20,
            units_shipped = 30,
            units_sold = 40,
            sold_unit_price = 500,
            purchased_unit_cost = 400
        };

        var edit_result = await _Module.Edit(edit_command);

        ValidateMostDtoFields(edit_result);

        Assert.That(edit_result.Data.transaction_date == edit_command.transaction_date);
        Assert.That(edit_result.Data.transaction_type == edit_command.transaction_type);
        Assert.That(edit_result.Data.units_purchased == edit_command.units_purchased);
        Assert.That(edit_result.Data.units_received == edit_command.units_received);
        Assert.That(edit_result.Data.units_shipped == edit_command.units_shipped);
        Assert.That(edit_result.Data.units_sold == edit_command.units_sold);
        Assert.That(edit_result.Data.sold_unit_price == edit_command.sold_unit_price);
        Assert.That(edit_result.Data.purchased_unit_cost == edit_command.purchased_unit_cost);
    }

    [Test]
    public async Task Delete()
    {
        var new_result = await _Module.Create(new TransactionCreateCommand()
        {
            calling_user_id = 1,
            token = _SessionId,
            object_reference_id = _SalesOrderHeader.id,
            object_sub_reference_id = _SalesOrderLine.id,
            product_id = _Product.id,
            transaction_date = DateTime.Now,
            transaction_type = 1,
            units_purchased = 1,
            units_received = 2,
            units_shipped = 3,
            units_sold = 4,
            sold_unit_price = 300,
            purchased_unit_cost = 100
        });

        Assert.IsTrue(new_result.Success);
        Assert.IsNotNull(new_result.Data);

        var delete_result = await _Module.Delete(new TransactionDeleteCommand()
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
        var new_result = await _Module.Create(new TransactionCreateCommand()
        {
            calling_user_id = 1,
            token = _SessionId,
            object_reference_id = _SalesOrderHeader.id,
            object_sub_reference_id = _SalesOrderLine.id,
            product_id = _Product.id,
            transaction_date = DateTime.Now,
            transaction_type = 1,
            units_purchased = 1,
            units_received = 2,
            units_shipped = 3,
            units_sold = 4,
            sold_unit_price = 300,
            purchased_unit_cost = 100
        });

        Assert.IsTrue(new_result.Success);
        Assert.IsNotNull(new_result.Data);

        var results = await _Module.Find(
                        new PagingSortingParameters() { ResultCount = 20, Start = 0 },
                        new TransactionFindCommand() { calling_user_id = 1, token = _SessionId, object_reference_id = _SalesOrderHeader.id });
        
        Assert.IsTrue(results.Success);
        Assert.IsNotNull(results.Data);
        Assert.NotZero(results.Data.Count());

        var first_result = results.Data.First();

        ValidateMostListFields(first_result);
    }

    private void ValidateMostDtoFields(Response<TransactionDto> result)
    {
        Assert.IsTrue(result.Success);
        Assert.IsNotNull(result.Data);
        Assert.NotZero(result.Data.id);
        Assert.IsNotEmpty(result.Data.guid);
        Assert.NotZero(result.Data.object_reference_id);
        Assert.NotZero(result.Data.product_id);
        Assert.NotZero(result.Data.transaction_type);
        Assert.NotZero(result.Data.created_by);
        Assert.NotNull(result.Data.created_on);
        Assert.NotNull(result.Data.created_on_string);
        Assert.NotNull(result.Data.created_on_timezone);
        Assert.NotNull(result.Data.updated_by);
        Assert.NotNull(result.Data.updated_on);
        Assert.NotNull(result.Data.updated_on_string);
        Assert.NotNull(result.Data.updated_on_timezone);
    }

    private void ValidateMostListFields(TransactionListDto result)
    {
        Assert.NotZero(result.id);
        Assert.IsNotEmpty(result.guid);
        Assert.NotZero(result.object_reference_id);
        Assert.NotZero(result.product_id);
        Assert.NotZero(result.transaction_type);
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