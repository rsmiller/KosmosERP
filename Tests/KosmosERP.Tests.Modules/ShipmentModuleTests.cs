using System.Data;
using Microsoft.EntityFrameworkCore;
using KosmosERP.Models;
using KosmosERP.Models.Permissions;
using KosmosERP.Tests.Modules.Shared;
using KosmosERP.BusinessLayer.Helpers;
using KosmosERP.BusinessLayer.Modules;
using KosmosERP.Database.Models;
using KosmosERP.BusinessLayer.Models.Module.Shipment.Command.Create;
using KosmosERP.BusinessLayer.Models.Module.Shipment.Command.Edit;
using KosmosERP.BusinessLayer.Models.Module.Shipment.Command.Delete;
using KosmosERP.BusinessLayer.Models.Module.Shipment.Command.Find;
using KosmosERP.BusinessLayer.Models.Module.Shipment.Dto;
using KosmosERP.BusinessLayer.MessagePublisher;

namespace KosmosERP.Tests.Modules;

public class ShipmentModuleTests : BaseTestModule<ShipmentModule>, IModuleTest
{
    private Address _Address;
    private Customer _Customer;
    private Product _Product;
    private OrderHeader _SalesOrderHeader;
    private OrderLine _SalesOrderLine;

    [SetUp]
    public async Task SetupModule()
    {
        var the_module = new ShipmentModule(base._Context, new MockMessagePublisher(new MessagePublisherSettings()));

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
        var read_permission = await _Context.ModulePermissions.FirstAsync(m => m.module_id == _Module.ModuleIdentifier.ToString() && m.internal_permission_name == ShipmentPermissions.Read);
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
        var create_permission = await _Context.ModulePermissions.FirstAsync(m => m.module_id == _Module.ModuleIdentifier.ToString() && m.internal_permission_name == ShipmentPermissions.Create);
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
        var edit_permission = await _Context.ModulePermissions.FirstAsync(m => m.module_id == _Module.ModuleIdentifier.ToString() && m.internal_permission_name == ShipmentPermissions.Edit);
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
        var delete_permission = await _Context.ModulePermissions.FirstAsync(m => m.module_id == _Module.ModuleIdentifier.ToString() && m.internal_permission_name == ShipmentPermissions.Delete);
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


        var customer_address = CommonDataHelper<CustomerAddress>.FillCommonFields(new CustomerAddress
        {
            customer_id = _Customer.id,
            address_type_id = CustomerAddressType.Physical,
            address_id = _Address.id,
        }, 1);

        _Context.CustomerAddresses.Add(customer_address);
        await _Context.SaveChangesAsync();

        var customer_shipto_address = CommonDataHelper<CustomerAddress>.FillCommonFields(new CustomerAddress
        {
            customer_id = _Customer.id,
            address_type_id = CustomerAddressType.ShipTo,
            address_id = _Address.id,
        }, 1);

        _Context.CustomerAddresses.Add(customer_shipto_address);
        await _Context.SaveChangesAsync();



        
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
        var new_result = await _Module.Create(new ShipmentHeaderCreateCommand()
        {
            calling_user_id = 1,
            token = _SessionId,
            ship_attn = "Bob",
            freight_carrier = "Yellow Truck",
            order_header_id = _SalesOrderHeader.id,
            freight_charge_amount = 1000,
            address_id = _Address.id,
            ship_via = "Frieght",
            tax = 100,
            shipment_lines = new List<ShipmentLineCreateCommand>()
            {
                new ShipmentLineCreateCommand()
                {
                    calling_user_id = 1,
                    token = _SessionId,
                    order_line_id = _SalesOrderLine.id,
                    units_to_ship = 10,
                    units_shipped = 10
                }
            }
        });

        Assert.IsTrue(new_result.Success);
        Assert.IsNotNull(new_result.Data);

        var result = await _Module.GetDto(new_result.Data.id);

        ValidateMostDtoFields(result);
    }

    [Test]
    public async Task Create()
    {
        var result = await _Module.Create(new ShipmentHeaderCreateCommand()
        {
            calling_user_id = 1,
            token = _SessionId,
            ship_attn = "Bob",
            freight_carrier = "Yellow Truck",
            order_header_id = _SalesOrderHeader.id,
            freight_charge_amount = 1000,
            address_id = _Address.id,
            ship_via = "Frieght",
            tax = 100,
            shipment_lines = new List<ShipmentLineCreateCommand>()
            {
                new ShipmentLineCreateCommand()
                {
                    calling_user_id = 1,
                    token = _SessionId,
                    order_line_id = _SalesOrderLine.id,
                    units_to_ship = 10,
                    units_shipped = 10
                }
            }
        });

        ValidateMostDtoFields(result);
    }

    [Test]
    public async Task Edit()
    {
        var old_result = await _Module.Create(new ShipmentHeaderCreateCommand()
        {
            calling_user_id = 1,
            token = _SessionId,
            ship_attn = "Bob",
            freight_carrier = "Yellow Truck",
            order_header_id = _SalesOrderHeader.id,
            freight_charge_amount = 1000,
            address_id = _Address.id,
            ship_via = "Frieght",
            tax = 100,
            shipment_lines = new List<ShipmentLineCreateCommand>()
            {
                new ShipmentLineCreateCommand()
                {
                    calling_user_id = 1,
                    token = _SessionId,
                    order_line_id = _SalesOrderLine.id,
                    units_to_ship = 10,
                    units_shipped = 10
                }
            }
        });

        var edit_command = new ShipmentHeaderEditCommand()
        {
            calling_user_id = 1,
            token = _SessionId,
            id = old_result.Data.id,
            ship_attn = "Sara",
            freight_carrier = "UPS",
            freight_charge_amount = 100,
            address_id = _Address.id,
            ship_via = "UPS",
            tax = 10,
        };

        var result = await _Module.Edit(edit_command);

        ValidateMostDtoFields(result);

        Assert.That(result.Data.ship_attn == edit_command.ship_attn);
        Assert.That(result.Data.freight_carrier == edit_command.freight_carrier);
        Assert.That(result.Data.freight_charge_amount == edit_command.freight_charge_amount);
        Assert.That(result.Data.ship_via == edit_command.ship_via);
        Assert.That(result.Data.tax == edit_command.tax);
    }

    [Test]
    public async Task Delete()
    {
        var new_result = await _Module.Create(new ShipmentHeaderCreateCommand()
        {
            calling_user_id = 1,
            token = _SessionId,
            ship_attn = "Bob",
            freight_carrier = "Yellow Truck",
            order_header_id = _SalesOrderHeader.id,
            freight_charge_amount = 1000,
            address_id = _Address.id,
            ship_via = "Frieght",
            tax = 100,
            shipment_lines = new List<ShipmentLineCreateCommand>()
            {
                new ShipmentLineCreateCommand()
                {
                    calling_user_id = 1,
                    token = _SessionId,
                    order_line_id = _SalesOrderLine.id,
                    units_to_ship = 10,
                    units_shipped = 10
                }
            }
        });

        Assert.IsTrue(new_result.Success);
        Assert.IsNotNull(new_result.Data);

        var delete_result = await _Module.Delete(new ShipmentHeaderDeleteCommand()
        {
            calling_user_id = 1,
            token = _SessionId,
            id = new_result.Data.id
        });

        Assert.IsTrue(delete_result.Success);
        Assert.IsNotNull(delete_result.Data);
        Assert.Zero(delete_result.Data.shipment_lines.Count());
        Assert.NotNull(delete_result.Data.deleted_by);
        Assert.NotNull(delete_result.Data.deleted_on);
        Assert.NotNull(delete_result.Data.deleted_on_string);
        Assert.NotNull(delete_result.Data.deleted_on_timezone);
    }

    [Test]
    public async Task Find()
    {
        var new_result = await _Module.Create(new ShipmentHeaderCreateCommand()
        {
            calling_user_id = 1,
            token = _SessionId,
            ship_attn = "Bob",
            freight_carrier = "Yellow Truck",
            order_header_id = _SalesOrderHeader.id,
            freight_charge_amount = 1000,
            address_id = _Address.id,
            ship_via = "Frieght",
            tax = 100,
            shipment_lines = new List<ShipmentLineCreateCommand>()
            {
                new ShipmentLineCreateCommand()
                {
                    calling_user_id = 1,
                    token = _SessionId,
                    order_line_id = _SalesOrderLine.id,
                    units_to_ship = 10,
                    units_shipped = 10
                }
            }
        });

        Assert.IsTrue(new_result.Success);
        Assert.IsNotNull(new_result.Data);

        ValidateMostDtoFields(new_result);

        var results = await _Module.Find(
                        new PagingSortingParameters() { ResultCount = 20, Start = 0 },
                        new ShipmentHeaderFindCommand() { calling_user_id = 1, token = _SessionId, wildcard = new_result.Data.shipment_number.ToString() });

        Assert.IsTrue(results.Success);
        Assert.IsNotNull(results.Data);
        Assert.NotZero(results.Data.Count());

        var first_result = results.Data.First();

        ValidateMostListFields(first_result);
    }

    
    [Test]
    public async Task CreateLine()
    {
        var create_result = await _Module.Create(new ShipmentHeaderCreateCommand()
        {
            calling_user_id = 1,
            token = _SessionId,
            ship_attn = "Bob",
            freight_carrier = "Yellow Truck",
            order_header_id = _SalesOrderHeader.id,
            freight_charge_amount = 1000,
            address_id = _Address.id,
            ship_via = "Frieght",
            tax = 100,
            shipment_lines = new List<ShipmentLineCreateCommand>()
            {
                new ShipmentLineCreateCommand()
                {
                    calling_user_id = 1,
                    token = _SessionId,
                    order_line_id = _SalesOrderLine.id,
                    units_to_ship = 10,
                    units_shipped = 10
                }
            }
        });

        Assert.IsTrue(create_result.Success);
        Assert.IsNotNull(create_result.Data);
        Assert.NotZero(create_result.Data.shipment_lines.Count());


        var create_command = new ShipmentLineCreateCommand()
        {
            calling_user_id = 1,
            token = _SessionId,
            shipment_header_id = create_result.Data.id,
            order_line_id = _SalesOrderLine.id,
            units_to_ship = 20,
            units_shipped = 5
        };

        var create_line_response = await _Module.CreateLine(create_command);


        Assert.IsTrue(create_line_response.Success);
        Assert.IsNotNull(create_line_response.Data);
        Assert.That(create_line_response.Data.order_line_id == create_command.order_line_id);
        Assert.That(create_line_response.Data.units_to_ship == create_command.units_to_ship);
        Assert.That(create_line_response.Data.units_shipped == create_command.units_shipped);
    }

    [Test]
    public async Task EditLine()
    {
        var create_result = await _Module.Create(new ShipmentHeaderCreateCommand()
        {
            calling_user_id = 1,
            token = _SessionId,
            ship_attn = "Bob",
            freight_carrier = "Yellow Truck",
            order_header_id = _SalesOrderHeader.id,
            freight_charge_amount = 1000,
            address_id = _Address.id,
            ship_via = "Frieght",
            tax = 100,
            shipment_lines = new List<ShipmentLineCreateCommand>()
            {
                new ShipmentLineCreateCommand()
                {
                    calling_user_id = 1,
                    token = _SessionId,
                    order_line_id = _SalesOrderLine.id,
                    units_to_ship = 10,
                    units_shipped = 10
                }
            }
        });

        Assert.IsTrue(create_result.Success);
        Assert.IsNotNull(create_result.Data);
        Assert.NotZero(create_result.Data.shipment_lines.Count());


        var edit_command = new ShipmentLineEditCommand()
        {
            calling_user_id = 1,
            token = _SessionId,
            id = create_result.Data.shipment_lines[0].id,
            order_line_id = _SalesOrderLine.id,
            units_to_ship = 90,
            units_shipped = 30
        };

        var edit_line_response = await _Module.EditLine(edit_command);


        Assert.IsTrue(edit_line_response.Success);
        Assert.IsNotNull(edit_line_response.Data);
        Assert.That(edit_line_response.Data.units_to_ship == edit_command.units_to_ship);
        Assert.That(edit_line_response.Data.units_shipped == edit_command.units_shipped);
        Assert.That(edit_line_response.Data.order_line_id == edit_command.order_line_id);
    }


    [Test]
    public async Task DeleteLine()
    {
        var create_result = await _Module.Create(new ShipmentHeaderCreateCommand()
        {
            calling_user_id = 1,
            token = _SessionId,
            ship_attn = "Bob",
            freight_carrier = "Yellow Truck",
            order_header_id = _SalesOrderHeader.id,
            freight_charge_amount = 1000,
            address_id = _Address.id,
            ship_via = "Frieght",
            tax = 100,
            shipment_lines = new List<ShipmentLineCreateCommand>()
            {
                new ShipmentLineCreateCommand()
                {
                    calling_user_id = 1,
                    token = _SessionId,
                    order_line_id = _SalesOrderLine.id,
                    units_to_ship = 10,
                    units_shipped = 10
                }
            }
        });

        Assert.IsTrue(create_result.Success);
        Assert.IsNotNull(create_result.Data);
        Assert.NotZero(create_result.Data.shipment_lines.Count());

        var response = await _Module.DeleteLine(new ShipmentLineDeleteCommand()
        {
            calling_user_id = 1,
            token = _SessionId,
            id = create_result.Data.shipment_lines[0].id,
        });


        Assert.IsTrue(response.Success);
        Assert.IsNotNull(response.Data);
        Assert.NotNull(response.Data.deleted_by);
        Assert.NotNull(response.Data.deleted_on);
        Assert.NotNull(response.Data.deleted_on_string);
        Assert.NotNull(response.Data.deleted_on_timezone);
    }


    private void ValidateMostDtoFields(Response<ShipmentHeaderDto> result)
    {
        Assert.IsTrue(result.Success);
        Assert.IsNotNull(result.Data);
        Assert.NotZero(result.Data.id);
        Assert.IsNotEmpty(result.Data.guid);
        Assert.NotZero(result.Data.order_header_id);
        Assert.NotZero(result.Data.freight_charge_amount);
        Assert.NotNull(result.Data.freight_carrier);
        Assert.NotZero(result.Data.address_id);
        Assert.NotZero(result.Data.shipment_number);
        Assert.NotNull(result.Data.ship_attn);
        Assert.NotNull(result.Data.ship_via);
        Assert.NotZero(result.Data.units_to_ship);
        Assert.NotZero(result.Data.created_by);
        Assert.NotNull(result.Data.created_on);
        Assert.NotNull(result.Data.created_on_string);
        Assert.NotNull(result.Data.created_on_timezone);
        Assert.NotNull(result.Data.updated_by);
        Assert.NotNull(result.Data.updated_on);
        Assert.NotNull(result.Data.updated_on_string);
        Assert.NotNull(result.Data.updated_on_timezone);
    }

    private void ValidateMostListFields(ShipmentHeaderListDto result)
    {
        Assert.NotZero(result.id);
        Assert.IsNotEmpty(result.guid);
        Assert.NotZero(result.order_header_id);
        Assert.NotZero(result.freight_charge_amount);
        Assert.NotNull(result.freight_carrier);
        Assert.NotZero(result.address_id);
        Assert.NotZero(result.shipment_number);
        Assert.NotNull(result.ship_attn);
        Assert.NotNull(result.ship_via);
        Assert.NotZero(result.units_to_ship);
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