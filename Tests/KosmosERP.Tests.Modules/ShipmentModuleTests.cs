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
using KosmosERP.BusinessLayer;
using Microsoft.Extensions.Caching.Memory;

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
        var logProviderFactory = new LogProviderFactory(new LogProviderSettings() { log_provider = LogProviderType.MOCK }, _Context, null);
        var mem_cache = new MemoryCacheService<KeyValueStore>(new MemoryCache(new MemoryCacheOptions()), base._Context);
        var address_module = new AddressModule(base._Context, logProviderFactory);
        var the_module = new ShipmentModule(base._Context,
                                            new MessageFactory(new MessagePublisherSettings() { account_provider = MessagePublisherType.MOCK, transaction_movement_topic = "test" }, base._Context), 
                                            address_module,
                                            new MessagePublisherSettings{ account_provider = MessagePublisherType.Database, transaction_movement_topic = "test" },
                                            logProviderFactory,
                                            mem_cache);

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
            payment_terms = "payment_terms_net_15"
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
        var new_result = await _Module.Create(new ShipmentHeaderCreateCommand()
        {
            calling_user_id = _User.external_id,
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
                    calling_user_id = _User.external_id,
                    order_line_id = _SalesOrderLine.id,
                    units_to_ship = 10,
                    units_shipped = 10
                }
            }
        });

        Assert.That(new_result.Success, Is.True);
        Assert.That(new_result.Data, Is.Not.Null);

        var result = await _Module.GetDto(new_result.Data.id);

        ValidateMostDtoFields(result);
    }

    [Test]
    public async Task Create()
    {
        var result = await _Module.Create(new ShipmentHeaderCreateCommand()
        {
            calling_user_id = _User.external_id,
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
                    calling_user_id = _User.external_id,
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
            calling_user_id = _User.external_id,
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
                    calling_user_id = _User.external_id,
                    order_line_id = _SalesOrderLine.id,
                    units_to_ship = 10,
                    units_shipped = 10
                }
            }
        });

        var edit_command = new ShipmentHeaderEditCommand()
        {
            calling_user_id = _User.external_id,
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
            calling_user_id = _User.external_id,
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
                    calling_user_id = _User.external_id,
                    order_line_id = _SalesOrderLine.id,
                    units_to_ship = 10,
                    units_shipped = 10
                }
            }
        });

        Assert.That(new_result.Success, Is.True);
        Assert.That(new_result.Data, Is.Not.Null);

        var delete_result = await _Module.Delete(new ShipmentHeaderDeleteCommand()
        {
            calling_user_id = _User.external_id,
            id = new_result.Data.id
        });

        Assert.That(delete_result.Success, Is.True);
        Assert.That(delete_result.Data, Is.Not.Null);
        Assert.That(delete_result.Data.shipment_lines.Count(), Is.Zero);
        Assert.That(delete_result.Data.deleted_by, Is.Not.Null);
        Assert.That(delete_result.Data.deleted_on, Is.Not.Null);
        Assert.That(delete_result.Data.deleted_on_string, Is.Not.Null);
        Assert.That(delete_result.Data.deleted_on_timezone, Is.Not.Null);
    }

    [Test]
    public async Task Find()
    {
        var new_result = await _Module.Create(new ShipmentHeaderCreateCommand()
        {
            calling_user_id = _User.external_id,
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
                    calling_user_id = _User.external_id,
                    order_line_id = _SalesOrderLine.id,
                    units_to_ship = 10,
                    units_shipped = 10
                }
            }
        });

        Assert.That(new_result.Success, Is.True);
        Assert.That(new_result.Data, Is.Not.Null);

        ValidateMostDtoFields(new_result);

        var results = await _Module.Find(
                        new PagingSortingParameters() { ResultCount = 20, Start = 0 },
                        new ShipmentHeaderFindCommand() { calling_user_id = _User.external_id, wildcard = new_result.Data.shipment_number.ToString() });

        Assert.That(results.Success, Is.True);
        Assert.That(results.Data, Is.Not.Null);
        Assert.That(results.Data.Count(), Is.Not.Zero);

        var first_result = results.Data.First();

        ValidateMostListFields(first_result);
    }

    
    [Test]
    public async Task CreateLine()
    {
        var create_result = await _Module.Create(new ShipmentHeaderCreateCommand()
        {
            calling_user_id = _User.external_id,
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
                    calling_user_id = _User.external_id,
                    order_line_id = _SalesOrderLine.id,
                    units_to_ship = 10,
                    units_shipped = 10
                }
            }
        });

        Assert.That(create_result.Success, Is.True);
        Assert.That(create_result.Data, Is.Not.Null);
        Assert.That(create_result.Data.shipment_lines.Count(), Is.Not.Zero);


        var create_command = new ShipmentLineCreateCommand()
        {
            calling_user_id = _User.external_id,
            shipment_header_id = create_result.Data.id,
            order_line_id = _SalesOrderLine.id,
            units_to_ship = 20,
            units_shipped = 5
        };

        var create_line_response = await _Module.CreateLine(create_command);


        Assert.That(create_line_response.Success, Is.True);
        Assert.That(create_line_response.Data, Is.Not.Null);
        Assert.That(create_line_response.Data.order_line_id == create_command.order_line_id);
        Assert.That(create_line_response.Data.units_to_ship == create_command.units_to_ship);
        Assert.That(create_line_response.Data.units_shipped == create_command.units_shipped);
    }

    [Test]
    public async Task EditLine()
    {
        var create_result = await _Module.Create(new ShipmentHeaderCreateCommand()
        {
            calling_user_id = _User.external_id,
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
                    calling_user_id = _User.external_id,
                    order_line_id = _SalesOrderLine.id,
                    units_to_ship = 10,
                    units_shipped = 10
                }
            }
        });

        Assert.That(create_result.Success, Is.True);
        Assert.That(create_result.Data, Is.Not.Null);
        Assert.That(create_result.Data.shipment_lines.Count(), Is.Not.Zero);


        var edit_command = new ShipmentLineEditCommand()
        {
            calling_user_id = _User.external_id,
            id = create_result.Data.shipment_lines[0].id,
            order_line_id = _SalesOrderLine.id,
            units_to_ship = 90,
            units_shipped = 30
        };

        var edit_line_response = await _Module.EditLine(edit_command);


        Assert.That(edit_line_response.Success, Is.True);
        Assert.That(edit_line_response.Data, Is.Not.Null);
        Assert.That(edit_line_response.Data.units_to_ship == edit_command.units_to_ship);
        Assert.That(edit_line_response.Data.units_shipped == edit_command.units_shipped);
        Assert.That(edit_line_response.Data.order_line_id == edit_command.order_line_id);
    }


    [Test]
    public async Task DeleteLine()
    {
        var create_result = await _Module.Create(new ShipmentHeaderCreateCommand()
        {
            calling_user_id = _User.external_id,
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
                    calling_user_id = _User.external_id,
                    order_line_id = _SalesOrderLine.id,
                    units_to_ship = 10,
                    units_shipped = 10
                }
            }
        });

        Assert.That(create_result.Success, Is.True);
        Assert.That(create_result.Data, Is.Not.Null);
        Assert.That(create_result.Data.shipment_lines.Count(), Is.Not.Zero);

        var response = await _Module.DeleteLine(new ShipmentLineDeleteCommand()
        {
            calling_user_id = _User.external_id,
            id = create_result.Data.shipment_lines[0].id,
        });


        Assert.That(response.Success, Is.True);
        Assert.That(response.Data, Is.Not.Null);
        Assert.That(response.Data.deleted_by, Is.Not.Null);
        Assert.That(response.Data.deleted_on, Is.Not.Null);
        Assert.That(response.Data.deleted_on_string, Is.Not.Null);
        Assert.That(response.Data.deleted_on_timezone, Is.Not.Null);
    }


    [Test]
    public async Task GetReadyToShip()
    {
        _Context.ProductionOrderLines.AddRange(
            CommonDataHelper<ProductionOrderLine>.FillCommonFields(new ProductionOrderLine() { order_line_id = _SalesOrderLine.id, line_number = 1, quantity = 3, status = "production_order_status_ready_to_ship" }, 1),
            CommonDataHelper<ProductionOrderLine>.FillCommonFields(new ProductionOrderLine() { order_line_id = _SalesOrderLine.id, line_number = 2, quantity = 2, status = "production_order_status_ready_to_ship" }, 1),
            CommonDataHelper<ProductionOrderLine>.FillCommonFields(new ProductionOrderLine() { order_line_id = _SalesOrderLine.id, line_number = 3, quantity = 7, status = "production_order_status_wip" }, 1),
            CommonDataHelper<ProductionOrderLine>.FillCommonFields(new ProductionOrderLine() { order_line_id = _SalesOrderLine.id, line_number = 4, quantity = 9, status = "production_order_status_ready_to_ship", is_deleted = true }, 1));

        // Multiple shipments against the same order line must sum; canceled and deleted lines must not count.
        _Context.ShipmentLines.AddRange(
            CommonDataHelper<ShipmentLine>.FillCommonFields(new ShipmentLine() { order_line_id = _SalesOrderLine.id, units_to_ship = 1, units_shipped = 1 }, 1),
            CommonDataHelper<ShipmentLine>.FillCommonFields(new ShipmentLine() { order_line_id = _SalesOrderLine.id, units_to_ship = 2, units_shipped = 2 }, 1),
            CommonDataHelper<ShipmentLine>.FillCommonFields(new ShipmentLine() { order_line_id = _SalesOrderLine.id, units_to_ship = 5, units_shipped = 5, is_canceled = true }, 1),
            CommonDataHelper<ShipmentLine>.FillCommonFields(new ShipmentLine() { order_line_id = _SalesOrderLine.id, units_to_ship = 6, units_shipped = 6, is_deleted = true }, 1));

        await _Context.SaveChangesAsync();

        var result = await _Module.GetReadyToShip();

        Assert.That(result.Success, Is.True);
        Assert.That(result.Data, Has.Count.EqualTo(1));

        var row = result.Data[0];
        Assert.That(row.order_number, Is.EqualTo(_SalesOrderHeader.order_number));
        Assert.That(row.order_guid, Is.EqualTo(_SalesOrderHeader.guid));
        Assert.That(row.customer_name, Is.EqualTo(_Customer.customer_name));
        Assert.That(row.product_name, Is.EqualTo(_Product.product_name));
        Assert.That(row.sold_quantity, Is.EqualTo(_SalesOrderLine.quantity));
        Assert.That(row.produced_quantity, Is.EqualTo(5));
        Assert.That(row.shipped_quantity, Is.EqualTo(3));
    }

    private void ValidateMostDtoFields(Response<ShipmentHeaderDto> result)
    {
        Assert.That(result.Success, Is.True);
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data.guid, Is.Not.Empty);
        Assert.That(result.Data.freight_charge_amount, Is.Not.Zero);
        Assert.That(result.Data.freight_carrier, Is.Not.Null);
        Assert.That(result.Data.address_id, Is.Not.Zero);
        Assert.That(result.Data.shipment_number, Is.Not.Zero);
        Assert.That(result.Data.ship_attn, Is.Not.Null);
        Assert.That(result.Data.ship_via, Is.Not.Null);
        Assert.That(result.Data.units_to_ship, Is.Not.Zero);
        Assert.That(result.Data.created_by, Is.Not.Zero);
        Assert.That(result.Data.created_on, Is.GreaterThan(DateTime.MinValue));
        Assert.That(result.Data.created_on_string, Is.Not.Null);
        Assert.That(result.Data.created_on_timezone, Is.Not.Null);
        Assert.That(result.Data.updated_by, Is.Not.Null);
        Assert.That(result.Data.updated_on, Is.Not.Null);
        Assert.That(result.Data.updated_on_string, Is.Not.Null);
        Assert.That(result.Data.updated_on_timezone, Is.Not.Null);
    }

    private void ValidateMostListFields(ShipmentHeaderListDto result)
    {
        Assert.That(result.guid, Is.Not.Empty);
        Assert.That(result.freight_charge_amount, Is.Not.Zero);
        Assert.That(result.freight_carrier, Is.Not.Null);
        Assert.That(result.address_id, Is.Not.Zero);
        Assert.That(result.shipment_number, Is.Not.Zero);
        Assert.That(result.ship_attn, Is.Not.Null);
        Assert.That(result.ship_via, Is.Not.Null);
        Assert.That(result.units_to_ship, Is.Not.Zero);
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