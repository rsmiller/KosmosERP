using System.Data;
using Microsoft.EntityFrameworkCore;
using KosmosERP.Models;
using KosmosERP.Models.Permissions;
using KosmosERP.Tests.Modules.Shared;
using KosmosERP.BusinessLayer.Helpers;
using KosmosERP.BusinessLayer.Modules;
using KosmosERP.Database.Models;
using KosmosERP.BusinessLayer.Models.Module.ProductionOrder.Command.Create;
using KosmosERP.BusinessLayer.Models.Module.ProductionOrder.Command.Edit;
using KosmosERP.BusinessLayer.Models.Module.ProductionOrder.Command.Delete;
using KosmosERP.BusinessLayer.Models.Module.ProductionOrder.Command.Find;
using KosmosERP.BusinessLayer.Models.Module.ProductionOrder.Dto;
using KosmosERP.BusinessLayer;
using Microsoft.Extensions.Caching.Memory;

namespace KosmosERP.Tests.Modules;

public class ProductionOrderModuleTests : BaseTestModule<ProductionOrderModule>, IModuleTest
{
    private Address _Address;
    private Customer _Customer;
    private Product _Product;
    private OrderHeader _SalesOrderHeader;
    private OrderLine _SalesOrderLine;

    [SetUp]
    public async Task SetupModule()
    {
        var messagePublisherSettings = new MessagePublisherSettings() { transaction_movement_topic = "transaction_movement_topic", account_provider = MessagePublisherType.MOCK };
        var logProviderFactory = new LogProviderFactory(new LogProviderSettings() { log_provider = LogProviderType.MOCK }, _Context, null);
        var kcMemCache = new MemoryCacheService<KeyValueStore>(new MemoryCache(new MemoryCacheOptions()), _Context);
        var custMemCache = new MemoryCacheService<Customer>(new MemoryCache(new MemoryCacheOptions()), _Context);
        var paymentProvider = new PaymentProviderFactory(new PaymentProviderSettings() { payment_provider = PaymentProviderType.MOCK }, _Context);
        
        var the_module = new ProductionOrderModule(base._Context, kcMemCache, messagePublisherSettings, custMemCache, paymentProvider, logProviderFactory);

        await base.SetupModule(the_module);
    }

    protected override async Task SetupData()
    {
        //if (_Product != null)
        //    return;


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
        _Context.SaveChanges();

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
        _Context.SaveChanges();

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
            tax_rate = 10,
            is_taxable = true,
            payment_terms = "payment_terms_net_15"
        }, 1);

        _Context.Customers.Add(customer);
        _Context.SaveChanges();

        _Customer = customer;


        var customer_address = CommonDataHelper<CustomerAddress>.FillCommonFields(new CustomerAddress
        {
            customer_id = _Customer.id,
            address_type_id = CustomerAddressType.Physical,
            address_id = _Address.id,
        }, 1);

        _Context.CustomerAddresses.Add(customer_address);
        _Context.SaveChanges();

        var customer_shipto_address = CommonDataHelper<CustomerAddress>.FillCommonFields(new CustomerAddress
        {
            customer_id = _Customer.id,
            address_type_id = CustomerAddressType.ShipTo,
            address_id = _Address.id,
        }, 1);

        _Context.CustomerAddresses.Add(customer_shipto_address);
        _Context.SaveChanges();

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
        _Context.SaveChanges();

        _SalesOrderHeader = sales_order_header;


        var sales_order_receive_line = CommonDataHelper<OrderLine>.FillCommonFields(new OrderLine()
        {
            order_header_id = _SalesOrderHeader.id,
            product_id = _Product.id,
            line_description = "A product with stuff",
            line_number = 1,
            unit_price = 100,
            quantity = 1,
        }, 1);

        _Context.OrderLines.Add(sales_order_receive_line);
        _Context.SaveChanges();


        _SalesOrderLine = sales_order_receive_line;

        var line_attribute_1 = CommonDataHelper<OrderLineAttribute>.FillCommonFields(new OrderLineAttribute()
        {
            order_line_id = _SalesOrderLine.id,
            attribute_name = "Length",
            attribute_value = "10ft",
        }, 1);

        var line_attribute_2 = CommonDataHelper<OrderLineAttribute>.FillCommonFields(new OrderLineAttribute()
        {
            order_line_id = _SalesOrderLine.id,
            attribute_name = "Material",
            attribute_value = "Plastic"
        }, 1);

        _Context.OrderLineAttributes.Add(line_attribute_1);
        _Context.OrderLineAttributes.Add(line_attribute_2);
        _Context.SaveChanges();

    }

    [Test]
    public async Task SeedPermissions_CreatesTheProductionStatuses()
    {
        // SetupModule already ran SeedPermissions. BUG-003: one list, under KeyValueIds.ProductionStatuses.
        var keys = await _Context.KeyValueStores
            .Where(m => m.module_id == KeyValueIds.ProductionStatuses)
            .Select(m => m.key)
            .ToListAsync();

        Assert.That(keys, Is.EquivalentTo(new[]
        {
            ProductionOrderStatus.Submitted, ProductionOrderStatus.PartsPulled, ProductionOrderStatus.WorkInProgress,
            ProductionOrderStatus.QualityCheck, ProductionOrderStatus.Complete, ProductionOrderStatus.ReadyToShip,
            ProductionOrderStatus.Canceled,
        }));
    }

    [Test]
    public async Task SeedPermissions_AddsOnlyMissingStatuses()
    {
        // An admin's rename is kept; a missing status (Canceled, on databases seeded before it existed) is added.
        var submitted = await _Context.KeyValueStores.SingleAsync(m => m.module_id == KeyValueIds.ProductionStatuses && m.key == ProductionOrderStatus.Submitted);
        submitted.value = "Queued";
        _Context.KeyValueStores.Remove(await _Context.KeyValueStores.SingleAsync(m => m.module_id == KeyValueIds.ProductionStatuses && m.key == ProductionOrderStatus.Canceled));
        await _Context.SaveChangesAsync();

        _Module.SeedPermissions();

        Assert.That((await _Context.KeyValueStores.SingleAsync(m => m.key == ProductionOrderStatus.Submitted)).value, Is.EqualTo("Queued"));
        Assert.That(await _Context.KeyValueStores.CountAsync(m => m.module_id == KeyValueIds.ProductionStatuses && m.key == ProductionOrderStatus.Canceled), Is.EqualTo(1));
    }

    [Test]
    public async Task Get()
    {
        var new_result = await _Module.Create(new ProductionOrderHeaderCreateCommand()
        {
            calling_user_id = _User.external_id,
            order_header_id = _SalesOrderHeader.id,
            planned_start_date = DateOnly.FromDateTime(DateTime.Now.AddDays(3)),
            planned_complete_date = DateOnly.FromDateTime(DateTime.Now.AddDays(4)),
            priority_id = 1,
            status = ProductionOrderStatus.Submitted,
            production_order_lines = new List<ProductionOrderLineCreateCommand>()
            {
                new ProductionOrderLineCreateCommand()
                {
                    line_number = _SalesOrderLine.line_number,
                    quantity = _SalesOrderLine.quantity,
                    order_line_id = _SalesOrderLine.id,
                    status = ProductionOrderStatus.Submitted,
                    calling_user_id = _User.external_id
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
        var result = await _Module.Create(new ProductionOrderHeaderCreateCommand()
        {
            calling_user_id = _User.external_id,
            order_header_id = _SalesOrderHeader.id,
            planned_start_date = DateOnly.FromDateTime(DateTime.Now.AddDays(3)),
            planned_complete_date = DateOnly.FromDateTime(DateTime.Now.AddDays(4)),
            priority_id = 1,
            status = ProductionOrderStatus.Submitted,
            production_order_lines = new List<ProductionOrderLineCreateCommand>()
            {
                new ProductionOrderLineCreateCommand()
                {
                    line_number = _SalesOrderLine.line_number,
                    quantity = _SalesOrderLine.quantity,
                    order_line_id = _SalesOrderLine.id,
                    status = ProductionOrderStatus.Submitted,
                    calling_user_id = _User.external_id
                }
            }
        });

        ValidateMostDtoFields(result);
    }

    [Test]
    public async Task Edit()
    {
        var old_result = await _Module.Create(new ProductionOrderHeaderCreateCommand()
        {
            calling_user_id = _User.external_id,
            order_header_id = _SalesOrderHeader.id,
            planned_start_date = DateOnly.FromDateTime(DateTime.Now.AddDays(3)),
            planned_complete_date = DateOnly.FromDateTime(DateTime.Now.AddDays(4)),
            priority_id = 1,
            status = ProductionOrderStatus.Submitted,
            production_order_lines = new List<ProductionOrderLineCreateCommand>()
            {
                new ProductionOrderLineCreateCommand()
                {
                    line_number = _SalesOrderLine.line_number,
                    quantity = _SalesOrderLine.quantity,
                    order_line_id = _SalesOrderLine.id,
                    status = ProductionOrderStatus.Submitted,
                    calling_user_id = _User.external_id
                }
            }
        });

        var edit_command = new ProductionOrderHeaderEditCommand()
        {
            calling_user_id = _User.external_id,
            id = old_result.Data.id,
            planned_start_date = DateOnly.FromDateTime(DateTime.Now.AddDays(9)),
            planned_complete_date = DateOnly.FromDateTime(DateTime.Now.AddDays(14)),
            priority_id = 2,
            status = ProductionOrderStatus.PartsPulled,
            actual_completed_on = DateOnly.FromDateTime(DateTime.Now.AddDays(15)),
            is_complete = true,
        };

        var result = await _Module.Edit(edit_command);

        ValidateMostDtoFields(result);

        Assert.That(result.Data.planned_start_date == edit_command.planned_start_date);
        Assert.That(result.Data.planned_complete_date == edit_command.planned_complete_date);
        Assert.That(result.Data.priority_id == edit_command.priority_id);
        Assert.That(result.Data.status == edit_command.status);
        Assert.That(result.Data.actual_completed_on == edit_command.actual_completed_on);
        Assert.That(result.Data.is_complete == edit_command.is_complete);
    }

    [Test]
    public async Task Delete()
    {
        var new_result = await _Module.Create(new ProductionOrderHeaderCreateCommand()
        {
            calling_user_id = _User.external_id,
            order_header_id = _SalesOrderHeader.id,
            planned_start_date = DateOnly.FromDateTime(DateTime.Now.AddDays(3)),
            planned_complete_date = DateOnly.FromDateTime(DateTime.Now.AddDays(4)),
            priority_id = 1,
            status = ProductionOrderStatus.Submitted,
            production_order_lines = new List<ProductionOrderLineCreateCommand>()
            {
                new ProductionOrderLineCreateCommand()
                {
                    line_number = _SalesOrderLine.line_number,
                    quantity = _SalesOrderLine.quantity,
                    order_line_id = _SalesOrderLine.id,
                    status = ProductionOrderStatus.Submitted,
                    calling_user_id = _User.external_id
                }
            }
        });

        Assert.That(new_result.Success, Is.True);
        Assert.That(new_result.Data, Is.Not.Null);

        var delete_result = await _Module.Delete(new ProductionOrderHeaderDeleteCommand()
        {
            calling_user_id = _User.external_id,
            id = new_result.Data.id
        });

        Assert.That(delete_result.Success, Is.True);
        Assert.That(delete_result.Data, Is.Not.Null);
        Assert.That(delete_result.Data.production_order_lines.Count(), Is.Zero);
        Assert.That(delete_result.Data.deleted_by, Is.Not.Null);
        Assert.That(delete_result.Data.deleted_on, Is.Not.Null);
        Assert.That(delete_result.Data.deleted_on_string, Is.Not.Null);
        Assert.That(delete_result.Data.deleted_on_timezone, Is.Not.Null);
    }

    [Test]
    public async Task Find()
    {
        var new_result = await _Module.Create(new ProductionOrderHeaderCreateCommand()
        {
            calling_user_id = _User.external_id,
            order_header_id = _SalesOrderHeader.id,
            planned_start_date = DateOnly.FromDateTime(DateTime.Now.AddDays(3)),
            planned_complete_date = DateOnly.FromDateTime(DateTime.Now.AddDays(4)),
            priority_id = 1,
            status = ProductionOrderStatus.Submitted,
            production_order_lines = new List<ProductionOrderLineCreateCommand>()
            {
                new ProductionOrderLineCreateCommand()
                {
                    line_number = _SalesOrderLine.line_number,
                    quantity = _SalesOrderLine.quantity,
                    order_line_id = _SalesOrderLine.id,
                    status = ProductionOrderStatus.Submitted,
                    calling_user_id = _User.external_id
                }
            }
        });

        Assert.That(new_result.Success, Is.True);
        Assert.That(new_result.Data, Is.Not.Null);

        ValidateMostDtoFields(new_result);

        var results = await _Module.Find(
                        new PagingSortingParameters() { ResultCount = 20, Start = 0 },
                        new ProductionOrderHeaderFindCommand() { calling_user_id = _User.external_id, wildcard = _SalesOrderHeader.order_number.ToString() });

        Assert.That(results.Success, Is.True);
        Assert.That(results.Data, Is.Not.Null);
        Assert.That(results.Data.Count(), Is.Not.Zero);

        var first_result = results.Data.First();

        ValidateMostListFields(first_result);
    }


    [Test]
    public async Task CreateLine()
    {
        var create_result = await _Module.Create(new ProductionOrderHeaderCreateCommand()
        {
            calling_user_id = _User.external_id,
            order_header_id = _SalesOrderHeader.id,
            planned_start_date = DateOnly.FromDateTime(DateTime.Now.AddDays(3)),
            planned_complete_date = DateOnly.FromDateTime(DateTime.Now.AddDays(4)),
            priority_id = 1,
            status = ProductionOrderStatus.Submitted,
            production_order_lines = new List<ProductionOrderLineCreateCommand>()
            {
                new ProductionOrderLineCreateCommand()
                {
                    line_number = _SalesOrderLine.line_number,
                    quantity = _SalesOrderLine.quantity,
                    order_line_id = _SalesOrderLine.id,
                    status = ProductionOrderStatus.Submitted,
                    calling_user_id = _User.external_id
                }
            }
        });

        Assert.That(create_result.Success, Is.True);
        Assert.That(create_result.Data, Is.Not.Null);
        Assert.That(create_result.Data.production_order_lines.Count(), Is.Not.Zero);
        Assert.That(create_result.Data.production_order_lines[0].order_line, Is.Not.Null);
        Assert.That(create_result.Data.production_order_lines[0].order_line.attributes.Count(), Is.Not.Zero);

        var create_command = new ProductionOrderLineCreateCommand()
        {
            calling_user_id = _User.external_id,
            production_order_header_id = create_result.Data.id,
            quantity = 12323,
            line_number = 2,
            order_line_id = _SalesOrderLine.id,
            status = ProductionOrderStatus.PartsPulled,
            started_on = DateTime.Now,
        };

        var create_line_response = await _Module.CreateLine(create_command);


        Assert.That(create_line_response.Success, Is.True);
        Assert.That(create_line_response.Data, Is.Not.Null);
        Assert.That(create_line_response.Data.order_line_id == _SalesOrderLine.id);
        Assert.That(create_line_response.Data.order_line, Is.Not.Null);
        Assert.That(create_line_response.Data.quantity == create_command.quantity);
        Assert.That(create_line_response.Data.status == create_command.status);
        Assert.That(create_line_response.Data.started_on == create_command.started_on);
    }

    [Test]
    public async Task EditLine()
    {
        var create_result = await _Module.Create(new ProductionOrderHeaderCreateCommand()
        {
            calling_user_id = _User.external_id,
            order_header_id = _SalesOrderHeader.id,
            planned_start_date = DateOnly.FromDateTime(DateTime.Now.AddDays(3)),
            planned_complete_date = DateOnly.FromDateTime(DateTime.Now.AddDays(4)),
            priority_id = 1,
            status = ProductionOrderStatus.Submitted,
            production_order_lines = new List<ProductionOrderLineCreateCommand>()
            {
                new ProductionOrderLineCreateCommand()
                {
                    line_number = _SalesOrderLine.line_number,
                    quantity = _SalesOrderLine.quantity,
                    order_line_id = _SalesOrderLine.id,
                    status = ProductionOrderStatus.Submitted,
                    calling_user_id = _User.external_id
                }
            }
        });

        Assert.That(create_result.Success, Is.True);
        Assert.That(create_result.Data, Is.Not.Null);
        Assert.That(create_result.Data.production_order_lines.Count(), Is.Not.Zero);
        Assert.That(create_result.Data.production_order_lines[0].order_line, Is.Not.Null);
        Assert.That(create_result.Data.production_order_lines[0].order_line.attributes.Count(), Is.Not.Zero);

        var edit_command = new ProductionOrderLineEditCommand()
        {
            calling_user_id = _User.external_id,
            id = create_result.Data.production_order_lines[0].id,
            quantity = 111,
            line_number = 2,
            status = ProductionOrderStatus.PartsPulled,
            started_on = DateTime.Now,
        };

        var edit_line_response = await _Module.EditLine(edit_command);


        Assert.That(edit_line_response.Success, Is.True);
        Assert.That(edit_line_response.Data, Is.Not.Null);
        Assert.That(edit_line_response.Data.order_line_id == _SalesOrderLine.id);
        Assert.That(edit_line_response.Data.order_line, Is.Not.Null);
        Assert.That(edit_line_response.Data.quantity == edit_command.quantity);
        Assert.That(edit_line_response.Data.status == edit_command.status);
        Assert.That(edit_line_response.Data.started_on == edit_command.started_on);
    }

    [Test]
    public async Task EditLine_StatusOnly_ChangesJustTheStatus()
    {
        // What the production order edit page sends (BUG-024).
        var line = (await CreateOrderWithLineStatus(ProductionOrderStatus.Submitted)).production_order_lines[0];

        var result = await _Module.EditLine(new ProductionOrderLineEditCommand()
        {
            calling_user_id = _User.external_id,
            id = line.id,
            status = ProductionOrderStatus.PartsPulled,
        });

        Assert.That(result.Success, Is.True);
        Assert.That(result.Data.status, Is.EqualTo(ProductionOrderStatus.PartsPulled));
        Assert.That(result.Data.quantity, Is.EqualTo(line.quantity));
    }

    [Test]
    public async Task EditLine_WithoutStatus_KeepsStatus()
    {
        // Regression: an edit that didn't send a status wiped it.
        var line = (await CreateOrderWithLineStatus(ProductionOrderStatus.Submitted)).production_order_lines[0];

        var result = await _Module.EditLine(new ProductionOrderLineEditCommand()
        {
            calling_user_id = _User.external_id,
            id = line.id,
            quantity = 5,
        });

        Assert.That(result.Success, Is.True);
        Assert.That(result.Data.quantity, Is.EqualTo(5));
        Assert.That(result.Data.status, Is.EqualTo(ProductionOrderStatus.Submitted));
    }

    [Test]
    public async Task Edit_WithoutStatus_KeepsStatus()
    {
        var order = await CreateOrderWithLineStatus(ProductionOrderStatus.Submitted);

        var result = await _Module.Edit(new ProductionOrderHeaderEditCommand()
        {
            calling_user_id = _User.external_id,
            id = order.id,
            priority_id = 2,
        });

        Assert.That(result.Success, Is.True);
        Assert.That(result.Data.priority_id, Is.EqualTo(2));
        Assert.That(result.Data.status, Is.EqualTo(ProductionOrderStatus.Submitted));
    }

    private async Task<ProductionOrderHeaderDto> CreateOrderWithLineStatus(string lineStatus)
    {
        var result = await _Module.Create(new ProductionOrderHeaderCreateCommand()
        {
            calling_user_id = _User.external_id,
            order_header_id = _SalesOrderHeader.id,
            planned_start_date = DateOnly.FromDateTime(DateTime.Now.AddDays(3)),
            planned_complete_date = DateOnly.FromDateTime(DateTime.Now.AddDays(4)),
            priority_id = 1,
            status = ProductionOrderStatus.Submitted,
            production_order_lines = new List<ProductionOrderLineCreateCommand>()
            {
                new ProductionOrderLineCreateCommand()
                {
                    line_number = _SalesOrderLine.line_number,
                    quantity = _SalesOrderLine.quantity,
                    order_line_id = _SalesOrderLine.id,
                    status = lineStatus,
                    calling_user_id = _User.external_id
                }
            }
        });

        Assert.That(result.Success, Is.True);
        return result.Data;
    }


    [Test]
    public async Task DeleteLine()
    {
        var create_result = await _Module.Create(new ProductionOrderHeaderCreateCommand()
        {
            calling_user_id = _User.external_id,
            planned_start_date = DateOnly.FromDateTime(DateTime.Now.AddDays(3)),
            planned_complete_date = DateOnly.FromDateTime(DateTime.Now.AddDays(4)),
            priority_id = 1,
            status = ProductionOrderStatus.Submitted,
            order_header_id = _SalesOrderHeader.id,
            production_order_lines = new List<ProductionOrderLineCreateCommand>()
            {
                new ProductionOrderLineCreateCommand()
                {
                    line_number = _SalesOrderLine.line_number,
                    quantity = _SalesOrderLine.quantity,
                    order_line_id = _SalesOrderLine.id,
                    status = ProductionOrderStatus.Submitted,
                    calling_user_id = _User.external_id
                }
            }
        });

        Assert.That(create_result.Success, Is.True);
        Assert.That(create_result.Data, Is.Not.Null);
        Assert.That(create_result.Data.production_order_lines.Count(), Is.Not.Zero);

        var response = await _Module.DeleteLine(new ProductionOrderLineDeleteCommand()
        {
            calling_user_id = _User.external_id,
            id = create_result.Data.production_order_lines[0].id,
        });


        Assert.That(response.Success, Is.True);
        Assert.That(response.Data, Is.Not.Null);
        Assert.That(response.Data.deleted_by, Is.Not.Null);
        Assert.That(response.Data.deleted_on, Is.Not.Null);
        Assert.That(response.Data.deleted_on_string, Is.Not.Null);
        Assert.That(response.Data.deleted_on_timezone, Is.Not.Null);
    }


    private void ValidateMostDtoFields(Response<ProductionOrderHeaderDto> result)
    {
        Assert.That(result.Success, Is.True);
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data.id, Is.Not.Zero);
        Assert.That(result.Data.guid, Is.Not.Empty);
        Assert.That(result.Data.priority_id, Is.Not.Zero);
        Assert.That(result.Data.status, Is.Not.Null);
        Assert.That(result.Data.created_by, Is.Not.Zero);
        Assert.That(result.Data.created_on, Is.GreaterThan(DateTime.MinValue));
        Assert.That(result.Data.created_on_string, Is.Not.Null);
        Assert.That(result.Data.created_on_timezone, Is.Not.Null);
        Assert.That(result.Data.updated_by, Is.Not.Null);
        Assert.That(result.Data.updated_on, Is.Not.Null);
        Assert.That(result.Data.updated_on_string, Is.Not.Null);
        Assert.That(result.Data.updated_on_timezone, Is.Not.Null);
    }

    private void ValidateMostListFields(ProductionOrderHeaderListDto result)
    {
        Assert.That(result.id, Is.Not.Zero);
        Assert.That(result.guid, Is.Not.Empty);
        Assert.That(result.priority_id, Is.Not.Zero);
        Assert.That(result.status, Is.Not.Null);
        Assert.That(result.created_on, Is.GreaterThan(DateTime.MinValue));
        Assert.That(result.created_on_string, Is.Not.Null);
        Assert.That(result.created_on_timezone, Is.Not.Null);
        Assert.That(result.updated_by, Is.Not.Null);
        Assert.That(result.updated_on, Is.Not.Null);
        Assert.That(result.updated_on_string, Is.Not.Null);
        Assert.That(result.updated_on_timezone, Is.Not.Null);
    }
}