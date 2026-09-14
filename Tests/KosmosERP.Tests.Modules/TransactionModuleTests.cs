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
using KosmosERP.BusinessLayer;

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
        var logProviderFactory = new LogProviderFactory(new LogProviderSettings() { log_provider = LogProviderType.MOCK }, _Context, null);
        var the_module = new TransactionModule(base._Context, logProviderFactory);
        the_module.SeedPermissions();

        await base.SetupModule(the_module);
    }

    protected override async Task SetupRoles()
    {
        // The TransactionModule.SeedPermissions() creates "Transaction Administrators" role
        // We need to find that role and assign it to our test user
        var admin_role = await _Context.Roles.Where(m => m.name == "Transaction Administrators").FirstAsync();

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
        var role = await _Context.Roles.Where(m => m.name == "Transaction Administrators").FirstAsync();

        var role_module_permission = CommonDataHelper<RolePermission>.FillCommonFields(new RolePermission()
        {
            module_id = _Module.ModuleIdentifier.ToString(),
            role_id = role.id,
            read = true,
        }, 1);

        _Context.RolePermissions.Add(role_module_permission);
        await _Context.SaveChangesAsync();
    }

    protected override async Task SetupCreatePermissions()
    {
        var role = await _Context.Roles.Where(m => m.name == "Transaction Administrators").FirstAsync();

        var role_module_permission = CommonDataHelper<RolePermission>.FillCommonFields(new RolePermission()
        {
            module_id = _Module.ModuleIdentifier.ToString(),
            role_id = role.id,
            write = true,
        }, 1);

        _Context.RolePermissions.Add(role_module_permission);
        await _Context.SaveChangesAsync();
    }

    protected override async Task SetupEditPermissions()
    {
        var role = await _Context.Roles.Where(m => m.name == "Transaction Administrators").FirstAsync();

        var role_module_permission = CommonDataHelper<RolePermission>.FillCommonFields(new RolePermission()
        {
            module_id = _Module.ModuleIdentifier.ToString(),
            role_id = role.id,
            edit = true,
        }, 1);

        _Context.RolePermissions.Add(role_module_permission);
        await _Context.SaveChangesAsync();
    }

    protected override async Task SetupDeletePermissions()
    {
        var role = await _Context.Roles.Where(m => m.name == "Transaction Administrators").FirstAsync();

        var role_module_permission = CommonDataHelper<RolePermission>.FillCommonFields(new RolePermission()
        {
            module_id = _Module.ModuleIdentifier.ToString(),
            role_id = role.id,
            delete = true,
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
            payment_terms = "payment_terms_net_15"
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
            shipping_method = "shipping_method_pickup",
            pay_method = "payment_method_cash",
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
            calling_user_id = _User.external_id,
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
            purchased_unit_cost = 100,
        });

        Assert.That(new_result.Success, Is.True);
        Assert.That(new_result.Data, Is.Not.Null);

        var result = await _Module.GetDto(new_result.Data.id);

        ValidateMostDtoFields(result);
    }

    [Test]
    public async Task Create()
    {
        try
        {
            var result = await _Module.Create(new TransactionCreateCommand()
            {
                calling_user_id = _User.external_id,
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

            if (!result.Success)
            {
                Console.WriteLine($"Create failed: {result.Exception}");
                Assert.Fail($"Create operation failed: {result.Exception}");
            }
            Assert.That(result.Success, Is.True);
            Assert.That(result.Data, Is.Not.Null);

            ValidateMostDtoFields(result);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Create test exception: {ex.Message}");
            Assert.Fail($"Create test failed with exception: {ex.Message}");
        }
    }

    [Test]
    public async Task Edit()
    {
        var new_result = await _Module.Create(new TransactionCreateCommand()
        {
            calling_user_id = _User.external_id,
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

        Assert.That(new_result.Success, Is.True);
        Assert.That(new_result.Data, Is.Not.Null);

        var edit_command = new TransactionEditCommand()
        {
            calling_user_id = _User.external_id,
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
            calling_user_id = _User.external_id,
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

        Assert.That(new_result.Success, Is.True);
        Assert.That(new_result.Data, Is.Not.Null);

        var delete_result = await _Module.Delete(new TransactionDeleteCommand()
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
        var new_result = await _Module.Create(new TransactionCreateCommand()
        {
            calling_user_id = _User.external_id,
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

        Assert.That(new_result.Success, Is.True);
        Assert.That(new_result.Data, Is.Not.Null);

        var results = await _Module.Find(
                        new PagingSortingParameters() { ResultCount = 20, Start = 0 },
                        new TransactionFindCommand() { calling_user_id = _User.external_id, object_reference_id = _SalesOrderHeader.id });
        
        Assert.That(results.Success, Is.True);
        Assert.That(results.Data, Is.Not.Null);
        Assert.That(results.Data.Count(), Is.Not.Zero);

        var first_result = results.Data.First();

        ValidateMostListFields(first_result);
    }

    private void ValidateMostDtoFields(Response<TransactionDto> result)
    {
        Assert.That(result.Success, Is.True);
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data.guid, Is.Not.Empty);
        Assert.That(result.Data.product_id, Is.Not.Zero);
        Assert.That(result.Data.transaction_type, Is.Not.Zero);
        Assert.That(result.Data.created_by, Is.Not.Zero);
        Assert.That(result.Data.created_on, Is.GreaterThan(DateTime.MinValue));
        Assert.That(result.Data.created_on_string, Is.Not.Null);
        Assert.That(result.Data.created_on_timezone, Is.Not.Null);
        Assert.That(result.Data.updated_by, Is.Not.Null);
        Assert.That(result.Data.updated_on, Is.Not.Null);
        Assert.That(result.Data.updated_on_string, Is.Not.Null);
        Assert.That(result.Data.updated_on_timezone, Is.Not.Null);
    }

    private void ValidateMostListFields(TransactionListDto result)
    {
        Assert.That(result.guid, Is.Not.Empty);
        Assert.That(result.product_id, Is.Not.Zero);
        Assert.That(result.transaction_type, Is.Not.Zero);
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