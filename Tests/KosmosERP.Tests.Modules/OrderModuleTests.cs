using System.Data;
using Microsoft.EntityFrameworkCore;
using KosmosERP.Models;
using KosmosERP.Tests.Modules.Shared;
using KosmosERP.BusinessLayer.Helpers;
using KosmosERP.BusinessLayer.Modules;
using KosmosERP.Database.Models;
using KosmosERP.BusinessLayer.Models.Module.Order.Command.Create;
using KosmosERP.BusinessLayer.Models.Module.Order.Command.Edit;
using KosmosERP.BusinessLayer.Models.Module.Order.Command.Delete;
using KosmosERP.BusinessLayer.Models.Module.Order.Command.Find;
using KosmosERP.BusinessLayer.Models.Module.Order.Dto;
using KosmosERP.BusinessLayer;
using Microsoft.Extensions.Caching.Memory;

namespace KosmosERP.Tests.Modules;

public class OrderModuleTests : BaseTestModule<OrderModule>, IModuleTest
{
    private Address _Address;
    private Customer _Customer;
    private Product _Product;

    [SetUp]
    public async Task SetupModule()
    {
        var messagePublisherSettings = new MessagePublisherSettings() { transaction_movement_topic = "transaction_movement_topic", account_provider = MessagePublisherType.MOCK };
        var logProviderFactory = new LogProviderFactory(new LogProviderSettings() { log_provider = LogProviderType.MOCK }, _Context, null);
        var kcMemCache = new MemoryCacheService<KeyValueStore>(new MemoryCache(new MemoryCacheOptions()), _Context);
        var custMemCache = new MemoryCacheService<Customer>(new MemoryCache(new MemoryCacheOptions()), _Context);
        
        var messageFactory  = new MessageFactory(messagePublisherSettings, _Context);
        var paymentProvider = new PaymentProviderFactory(new PaymentProviderSettings() { payment_provider = PaymentProviderType.MOCK }, _Context);
        
        var productionOrderModule = new ProductionOrderModule(base._Context, kcMemCache, messagePublisherSettings, custMemCache, paymentProvider, logProviderFactory);
        var addressModule = new AddressModule(base._Context, logProviderFactory);

        var the_module = new OrderModule(base._Context, messageFactory, messagePublisherSettings, kcMemCache, custMemCache, productionOrderModule, paymentProvider, logProviderFactory, addressModule);

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
            tax_rate = 10,
            is_taxable = true,
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
    }

    [Test]
    public async Task Get()
    {
        var new_result = await _Module.Create(new OrderHeaderCreateCommand()
        {
            calling_user_id = _User.external_id,
            order_date = DateOnly.FromDateTime(DateTime.Now),
            required_date = DateOnly.FromDateTime(DateTime.Now.AddDays(3)),
            customer_id = _Customer.id,
            po_number = "123456",
            ship_to_address_id = _Address.id,
            order_type = "D",
            shipping_method = "shipping_method_pickup",
            pay_method = "payment_method_cash",
            shipping_cost = 12,
            order_lines = new List<OrderLineCreateCommand>() {
                new OrderLineCreateCommand()
                {
                    quantity = 1,
                    unit_price = 10,
                    line_number = 1,
                    line_description = "Super cool line",
                    product_id = _Product.id,
                    calling_user_id = _User.external_id,
                    attributes = new List<OrderLineAttributeCreateCommand>()
                    {
                        new OrderLineAttributeCreateCommand()
                        {
                            attribute_name = "Length",
                            attribute_value = "10ft",
                            calling_user_id = _User.external_id
                        },
                        new OrderLineAttributeCreateCommand()
                        {
                            attribute_name = "Material",
                            attribute_value = "Plastic",
                            calling_user_id = _User.external_id
                        }
                    }
                }
            }
        });

        Assert.That(new_result.Success, Is.True);
        Assert.That(new_result.Data, Is.Not.Null);

        var result = await _Module.GetDto(new_result.Data.id);

        ValidateMostDtoFields(result);

        decimal expected_tax = (10 * 1) * 10 + (10 * 1);

        Assert.That(result.Data.tax == expected_tax);
    }

    [Test]
    public async Task Create()
    {
        var result = await _Module.Create(new OrderHeaderCreateCommand()
        {
            calling_user_id = _User.external_id,
            order_date = DateOnly.FromDateTime(DateTime.Now),
            required_date = DateOnly.FromDateTime(DateTime.Now.AddDays(3)),
            customer_id = _Customer.id,
            po_number = "556644",
            ship_to_address_id = _Address.id,
            order_type = "D",
            shipping_method = "shipping_method_pickup",
            pay_method = "payment_method_cash",
            shipping_cost = 12,
            order_lines = new List<OrderLineCreateCommand>() {
                new OrderLineCreateCommand()
                {
                    quantity = 1,
                    unit_price = 10,
                    line_number = 1,
                    line_description = "Super cool line",
                    product_id = _Product.id,
                    attributes = new List<OrderLineAttributeCreateCommand>()
                    {
                        new OrderLineAttributeCreateCommand()
                        {
                            attribute_name = "Length",
                            attribute_value = "10ft"
                        },
                        new OrderLineAttributeCreateCommand()
                        {
                            attribute_name = "Material",
                            attribute_value = "Plastic"
                        }
                    }
                }
            }
        });

        ValidateMostDtoFields(result);
    }

    [Test]
    public async Task Edit()
    {
        var old_result = await _Module.Create(new OrderHeaderCreateCommand()
        {
            calling_user_id = _User.external_id,
            order_date = DateOnly.FromDateTime(DateTime.Now),
            required_date = DateOnly.FromDateTime(DateTime.Now.AddDays(3)),
            customer_id = _Customer.id,
            po_number = "1112323",
            ship_to_address_id = _Address.id,
            order_type = "D",
            shipping_cost = 12,
            shipping_method = "shipping_method_pickup",
            pay_method = "payment_method_cash",
            order_lines = new List<OrderLineCreateCommand>() {
                new OrderLineCreateCommand()
                {
                    quantity = 1,
                    unit_price = 10,
                    line_number = 1,
                    line_description = "Super cool line",
                    product_id = _Product.id,
                    calling_user_id = _User.external_id,
                    attributes = new List<OrderLineAttributeCreateCommand>()
                    {
                        new OrderLineAttributeCreateCommand()
                        {
                            attribute_name = "Length",
                            attribute_value = "10ft",
                            calling_user_id = _User.external_id,
                        },
                        new OrderLineAttributeCreateCommand()
                        {
                            attribute_name = "Material",
                            attribute_value = "Plastic",
                            calling_user_id = _User.external_id,
                        }
                    }
                }
            }
        });

        var edit_command = new OrderHeaderEditCommand()
        {
            calling_user_id = _User.external_id,
            id = old_result.Data.id,
            po_number = "23232323",
            ship_to_address_id = _Address.id,
            order_type = "D",
            shipping_cost = 1232,
            shipping_method = "shipping_method_pickup",
            pay_method = "payment_method_cash",
            order_date = DateOnly.FromDateTime(DateTime.Now.AddDays(7)),
            required_date = DateOnly.FromDateTime(DateTime.Now.AddDays(17)),
        };

        var result = await _Module.Edit(edit_command);

        ValidateMostDtoFields(result);

        Assert.That(result.Data.po_number == edit_command.po_number);
        Assert.That(result.Data.order_type == edit_command.order_type);
        Assert.That(result.Data.pay_method == edit_command.pay_method);
        Assert.That(result.Data.shipping_cost == edit_command.shipping_cost);
        Assert.That(result.Data.shipping_method == edit_command.shipping_method);
        Assert.That(result.Data.order_date == edit_command.order_date);
        Assert.That(result.Data.required_date == edit_command.required_date);
    }

    [Test]
    public async Task Delete()
    {
        var new_result = await _Module.Create(new OrderHeaderCreateCommand()
        {
            calling_user_id = _User.external_id,
            order_date = DateOnly.FromDateTime(DateTime.Now),
            required_date = DateOnly.FromDateTime(DateTime.Now.AddDays(3)),
            customer_id = _Customer.id,
            po_number = "9677563",
            ship_to_address_id = _Address.id,
            order_type = "D",
            shipping_cost = 12,
            shipping_method = "shipping_method_pickup",
            pay_method = "payment_method_cash",
            order_lines = new List<OrderLineCreateCommand>() {
                new OrderLineCreateCommand()
                {
                    quantity = 1,
                    unit_price = 10,
                    line_number = 1,
                    line_description = "Super cool line",
                    product_id = _Product.id,
                    calling_user_id = _User.external_id,
                    attributes = new List<OrderLineAttributeCreateCommand>()
                    {
                        new OrderLineAttributeCreateCommand()
                        {
                            attribute_name = "Length",
                            attribute_value = "10ft",
                            calling_user_id = _User.external_id,
                        },
                        new OrderLineAttributeCreateCommand()
                        {
                            attribute_name = "Material",
                            attribute_value = "Plastic",
                            calling_user_id = _User.external_id,
                        }
                    }
                }
            }
        });

        Assert.That(new_result.Success, Is.True);
        Assert.That(new_result.Data, Is.Not.Null);

        var delete_result = await _Module.Delete(new OrderHeaderDeleteCommand()
        {
            calling_user_id = _User.external_id,
            id = new_result.Data.id
        });

        Assert.That(delete_result.Success, Is.True);
        Assert.That(delete_result.Data, Is.Not.Null);
        Assert.That(delete_result.Data.order_lines.Count(), Is.Zero);
        Assert.That(delete_result.Data.deleted_by, Is.Not.Null);
        Assert.That(delete_result.Data.deleted_on, Is.Not.Null);
        Assert.That(delete_result.Data.deleted_on_string, Is.Not.Null);
        Assert.That(delete_result.Data.deleted_on_timezone, Is.Not.Null);
    }

    [Test]
    public async Task Find()
    {
        var new_result = await _Module.Create(new OrderHeaderCreateCommand()
        {
            calling_user_id = _User.external_id,
            order_date = DateOnly.FromDateTime(DateTime.Now),
            required_date = DateOnly.FromDateTime(DateTime.Now.AddDays(3)),
            customer_id = _Customer.id,
            po_number = "234234443",
            ship_to_address_id = _Address.id,
            order_type = "D",
            shipping_cost = 12,
            shipping_method = "shipping_method_pickup",
            pay_method = "payment_method_cash",
            order_lines = new List<OrderLineCreateCommand>() {
                new OrderLineCreateCommand()
                {
                    quantity = 1,
                    unit_price = 10,
                    line_number = 1,
                    line_description = "Super cool line",
                    product_id = _Product.id,
                    calling_user_id = _User.external_id,
                    attributes = new List<OrderLineAttributeCreateCommand>()
                    {
                        new OrderLineAttributeCreateCommand()
                        {
                            attribute_name = "Length",
                            attribute_value = "10ft",
                            calling_user_id = _User.external_id
                        },
                        new OrderLineAttributeCreateCommand()
                        {
                            attribute_name = "Material",
                            attribute_value = "Plastic",
                            calling_user_id = _User.external_id,
                        }
                    }
                }
            }
        });

        Assert.That(new_result.Success, Is.True);
        Assert.That(new_result.Data, Is.Not.Null);

        ValidateMostDtoFields(new_result);

        var results = await _Module.Find(
                        new PagingSortingParameters() { ResultCount = 20, Start = 0 },
                        new OrderHeaderFindCommand() { calling_user_id = _User.external_id, wildcard = "234234443" });

        Assert.That(results.Success, Is.True);
        Assert.That(results.Data, Is.Not.Null);
        Assert.That(results.Data.Count(), Is.Not.Zero);

        var first_result = results.Data.First();

        ValidateMostListFields(first_result);
    }

    
    [Test]
    public async Task CreateLine()
    {
        var create_result = await _Module.Create(new OrderHeaderCreateCommand()
        {
            calling_user_id = _User.external_id,
            order_date = DateOnly.FromDateTime(DateTime.Now),
            required_date = DateOnly.FromDateTime(DateTime.Now.AddDays(3)),
            customer_id = _Customer.id,
            po_number = "1231242141234",
            ship_to_address_id = _Address.id,
            order_type = "D",
            shipping_cost = 12,
            shipping_method = "shipping_method_pickup",
            pay_method = "payment_method_cash",
            order_lines = new List<OrderLineCreateCommand>() {
                new OrderLineCreateCommand()
                {
                    quantity = 1,
                    unit_price = 10,
                    line_number = 1,
                    line_description = "Super cool line",
                    product_id = _Product.id,
                    calling_user_id = _User.external_id,
                    attributes = new List<OrderLineAttributeCreateCommand>()
                    {
                        new OrderLineAttributeCreateCommand()
                        {
                            attribute_name = "Length",
                            attribute_value = "10ft",
                            calling_user_id = _User.external_id,
                        },
                        new OrderLineAttributeCreateCommand()
                        {
                            attribute_name = "Material",
                            attribute_value = "Plastic",
                            calling_user_id = _User.external_id,
                        }
                    }
                }
            }
        });

        Assert.That(create_result.Success, Is.True);
        Assert.That(create_result.Data, Is.Not.Null);
        Assert.That(create_result.Data.order_lines.Count(), Is.Not.Zero);
        Assert.That(create_result.Data.order_lines[0].attributes.Count(), Is.Not.Zero);

        var create_command = new OrderLineCreateCommand()
        {
            calling_user_id = _User.external_id,
            order_header_id = create_result.Data.id,
            quantity = 111,
            unit_price = 111,
            line_number = 2,
            line_description = "AnotherSuper cool line",
            product_id = _Product.id,
            attributes = new List<OrderLineAttributeCreateCommand>()
                {
                    new OrderLineAttributeCreateCommand()
                    {
                        attribute_name = "Length",
                        attribute_value = "10ft",
                        calling_user_id = _User.external_id,
                    },
                    new OrderLineAttributeCreateCommand()
                    {
                        attribute_name = "Material",
                        attribute_value = "Plastic",
                        calling_user_id = _User.external_id,
                    }
                }
        };

        var create_line_response = await _Module.CreateLine(create_command);


        Assert.That(create_line_response.Success, Is.True);
        Assert.That(create_line_response.Data, Is.Not.Null);
        Assert.That(create_line_response.Data.line_number == create_command.line_number);
        Assert.That(create_line_response.Data.quantity == create_command.quantity);
        Assert.That(create_line_response.Data.unit_price == create_command.unit_price);
    }

    [Test]
    public async Task EditLine()
    {
        var create_result = await _Module.Create(new OrderHeaderCreateCommand()
        {
            calling_user_id = _User.external_id,
            order_date = DateOnly.FromDateTime(DateTime.Now),
            required_date = DateOnly.FromDateTime(DateTime.Now.AddDays(3)),
            customer_id = _Customer.id,
            po_number = "3478955588",
            ship_to_address_id = _Address.id,
            order_type = "D",
            shipping_cost = 12,
            shipping_method = "shipping_method_pickup",
            pay_method = "payment_method_cash",
            order_lines = new List<OrderLineCreateCommand>() {
                new OrderLineCreateCommand()
                {
                    quantity = 1,
                    unit_price = 10,
                    line_number = 1,
                    line_description = "Super cool line",
                    product_id = _Product.id,
                    calling_user_id = _User.external_id,
                    attributes = new List<OrderLineAttributeCreateCommand>()
                    {
                        new OrderLineAttributeCreateCommand()
                        {
                            attribute_name = "Length",
                            attribute_value = "10ft",
                            calling_user_id = _User.external_id,
                        },
                        new OrderLineAttributeCreateCommand()
                        {
                            attribute_name = "Material",
                            attribute_value = "Plastic",
                            calling_user_id = _User.external_id,
                        }
                    }
                }
            }
        });

        Assert.That(create_result.Success, Is.True);
        Assert.That(create_result.Data, Is.Not.Null);
        Assert.That(create_result.Data.order_lines.Count(), Is.Not.Zero);
        Assert.That(create_result.Data.order_lines[0].attributes.Count(), Is.Not.Zero);

        var edit_command = new OrderLineEditCommand()
        {
            calling_user_id = _User.external_id,
            id = create_result.Data.order_lines[0].id,
            quantity = 111,
            unit_price = 111,
            line_number = 34,
            line_description = "AnotherSuper cool lineasdasdasd",
            product_id = _Product.id,
            attributes = new List<OrderLineAttributeEditCommand>()
                {
                    new OrderLineAttributeEditCommand()
                    {
                        attribute_name = "Adding",
                        attribute_value = "1231231",
                        calling_user_id = _User.external_id,
                    },
                    new OrderLineAttributeEditCommand()
                    {
                        attribute_name = "Material",
                        attribute_value = "Plastic",
                        calling_user_id = _User.external_id,
                    }
                }
        };

        var edit_line_response = await _Module.EditLine(edit_command);


        Assert.That(edit_line_response.Success, Is.True);
        Assert.That(edit_line_response.Data, Is.Not.Null);
        Assert.That(edit_line_response.Data.line_number == edit_command.line_number);
        Assert.That(edit_line_response.Data.quantity == edit_command.quantity);
        Assert.That(edit_line_response.Data.unit_price == edit_command.unit_price);
        Assert.That(edit_line_response.Data.line_number == edit_command.line_number);
        Assert.That(edit_line_response.Data.line_description == edit_command.line_description);
    }


    [Test]
    public async Task DeleteLine()
    {
        var create_result = await _Module.Create(new OrderHeaderCreateCommand()
        {
            calling_user_id = _User.external_id,
            order_date = DateOnly.FromDateTime(DateTime.Now),
            required_date = DateOnly.FromDateTime(DateTime.Now.AddDays(3)),
            customer_id = _Customer.id,
            po_number = "1231243550",
            ship_to_address_id = _Address.id,
            order_type = "D",
            shipping_cost = 12,
            shipping_method = "shipping_method_pickup",
            pay_method = "payment_method_cash",
            order_lines = new List<OrderLineCreateCommand>() {
                new OrderLineCreateCommand()
                {
                    quantity = 1,
                    unit_price = 10,
                    line_number = 1,
                    line_description = "Super cool line",
                    product_id = _Product.id,
                    calling_user_id = _User.external_id,
                    attributes = new List<OrderLineAttributeCreateCommand>()
                    {
                        new OrderLineAttributeCreateCommand()
                        {
                            attribute_name = "Length",
                            attribute_value = "10ft",
                            calling_user_id = _User.external_id,
                        },
                        new OrderLineAttributeCreateCommand()
                        {
                            attribute_name = "Material",
                            attribute_value = "Plastic",
                            calling_user_id = _User.external_id,
                        }
                    }
                }
            }
        });

        Assert.That(create_result.Success, Is.True);
        Assert.That(create_result.Data, Is.Not.Null);
        Assert.That(create_result.Data.order_lines.Count(), Is.Not.Zero);

        var response = await _Module.DeleteLine(new OrderLineDeleteCommand()
        {
            calling_user_id = _User.external_id,
            id = create_result.Data.order_lines[0].id,
        });


        Assert.That(response.Success, Is.True);
        Assert.That(response.Data, Is.Not.Null);
        Assert.That(response.Data.deleted_by, Is.Not.Null);
        Assert.That(response.Data.deleted_on, Is.Not.Null);
        Assert.That(response.Data.deleted_on_string, Is.Not.Null);
        Assert.That(response.Data.deleted_on_timezone, Is.Not.Null);
    }


    private void ValidateMostDtoFields(Response<OrderHeaderDto> result)
    {
        Assert.That(result.Success, Is.True);
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data.guid, Is.Not.Empty);
        Assert.That(result.Data.order_number, Is.Not.Zero);
        Assert.That(result.Data.customer_id, Is.Not.Zero);
        Assert.That(result.Data.order_type, Is.Not.Null);
        Assert.That(result.Data.pay_method, Is.Not.Null);
        Assert.That(result.Data.shipping_method, Is.Not.Empty);
        Assert.That(result.Data.ship_to_address_id, Is.Not.Zero);
        Assert.That(result.Data.po_number, Is.Not.Null);
        Assert.That(result.Data.price, Is.Not.Zero);
        Assert.That(result.Data.revision_number, Is.Not.Zero);
        Assert.That(result.Data.tax, Is.Not.Zero);
        Assert.That(result.Data.created_by, Is.Not.Zero);
        Assert.That(result.Data.created_on, Is.GreaterThan(DateTime.MinValue));
        Assert.That(result.Data.created_on_string, Is.Not.Null);
        Assert.That(result.Data.created_on_timezone, Is.Not.Null);
        Assert.That(result.Data.updated_by, Is.Not.Null);
        Assert.That(result.Data.updated_on, Is.Not.Null);
        Assert.That(result.Data.updated_on_string, Is.Not.Null);
        Assert.That(result.Data.updated_on_timezone, Is.Not.Null);
    }

    private void ValidateMostListFields(OrderHeaderListDto result)
    {
        Assert.That(result.guid, Is.Not.Empty);
        Assert.That(result.order_number, Is.Not.Zero);
        Assert.That(result.customer_id, Is.Not.Zero);
        Assert.That(result.order_type, Is.Not.Null);
        Assert.That(result.pay_method, Is.Not.Null);
        Assert.That(result.shipping_method, Is.Not.Empty);
        Assert.That(result.ship_to_address_id, Is.Not.Zero);
        Assert.That(result.po_number, Is.Not.Null);
        Assert.That(result.price, Is.Not.Zero);
        Assert.That(result.revision_number, Is.Not.Zero);
        Assert.That(result.tax, Is.Not.Zero);
        Assert.That(result.created_on, Is.GreaterThan(DateTime.MinValue));
        Assert.That(result.created_on_string, Is.Not.Null);
        Assert.That(result.created_on_timezone, Is.Not.Null);
        Assert.That(result.updated_by, Is.Not.Null);
        Assert.That(result.updated_on, Is.Not.Null);
        Assert.That(result.updated_on_string, Is.Not.Null);
        Assert.That(result.updated_on_timezone, Is.Not.Null);
    }
}