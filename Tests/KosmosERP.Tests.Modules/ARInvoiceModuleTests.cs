using System.Data;
using Microsoft.EntityFrameworkCore;
using KosmosERP.Models;
using KosmosERP.Models.Permissions;
using KosmosERP.Tests.Modules.Shared;
using KosmosERP.BusinessLayer.Helpers;
using KosmosERP.BusinessLayer.Modules;
using KosmosERP.Database.Models;
using KosmosERP.BusinessLayer.Models.Module.ARInvoice.Command.Create;
using KosmosERP.BusinessLayer.Models.Module.ARInvoice.Command.Edit;
using KosmosERP.BusinessLayer.Models.Module.ARInvoice.Command.Delete;
using KosmosERP.BusinessLayer.Models.Module.ARInvoice.Command.Find;
using KosmosERP.BusinessLayer.Models.Module.ARInvoice.Dto;
using KosmosERP.BusinessLayer;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.VisualBasic;

namespace KosmosERP.Tests.Modules;

public class ARInvoiceModuleTests : BaseTestModule<ARInvoiceModule>, IModuleTest
{
    private Address _Address;
    private Vendor _Vendor;
    private Customer _Customer;
    private PurchaseOrderHeader _PurchaseOrderHeader;
    private PurchaseOrderLine _PurchaseOrderLine;
    private Product _Product;
    private PurchaseOrderReceiveHeader _PurchaseOrderReceiveHeader;
    private PurchaseOrderReceiveLine _PurchaseOrderReceiveLine;
    private OrderHeader _SalesOrderHeader;
    private OrderLine _SalesOrderLine;
    private ARInvoiceHeader _ARInvoiceHeader;
    private ARInvoiceLine _ARInvoiceLine;

    [SetUp]
    public async Task SetupModule()
    {
        var logProviderFactory = new LogProviderFactory(new LogProviderSettings() { log_provider = LogProviderType.MOCK }, _Context, null);
        var kcMemCache = new MemoryCacheService<KeyValueStore>(new MemoryCache(new MemoryCacheOptions()), _Context);
        var custMemCache = new MemoryCacheService<Customer>(new MemoryCache(new MemoryCacheOptions()), _Context);
        var orderMemCache = new MemoryCacheService<OrderHeader>(new MemoryCache(new MemoryCacheOptions()), _Context);

        var paymentProvider = new PaymentProviderFactory(new PaymentProviderSettings() { payment_provider = PaymentProviderType.MOCK }, _Context);
        var financialTransactionModule = new FinancialTransactionModule(base._Context, logProviderFactory);
        var chartOfAccountModule = new ChartOfAccountModule(base._Context, logProviderFactory);

        var the_module = new ARInvoiceModule(base._Context, kcMemCache, custMemCache, orderMemCache, paymentProvider, financialTransactionModule, chartOfAccountModule, logProviderFactory);

        await base.SetupModule(the_module);
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
        }, _User.external_id);

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
        }, _User.external_id);

        _Context.Addresses.Add(address);
        _Context.SaveChanges();

        _Address = address;


        var vendor = CommonDataHelper<Vendor>.FillCommonFields(new Vendor()
        {
            address_id = address.id,
            category = "CAT1",
            fax = "123-123-1234",
            phone = "234-456-2312",
            general_email = "vendor@vendor.com",
            is_critial_vendor = true,
            is_deleted = false,
            vendor_name = "Super Vendor",
            vendor_description = "I am a vendor and junk",
            vendor_number = 121212
        }, _User.external_id);

        _Context.Vendors.Add(vendor);
        _Context.SaveChanges();

        _Vendor = vendor;



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
        }, _User.external_id);

        _Context.Customers.Add(customer);
        _Context.SaveChanges();

        _Customer = customer;


        var customer_address = CommonDataHelper<CustomerAddress>.FillCommonFields(new CustomerAddress
        {
            customer_id = _Customer.id,
            address_type_id = CustomerAddressType.Physical,
            address_id = _Address.id,
        }, _User.external_id);

        _Context.CustomerAddresses.Add(customer_address);
        _Context.SaveChanges();

        var customer_shipto_address = CommonDataHelper<CustomerAddress>.FillCommonFields(new CustomerAddress
        {
            customer_id = _Customer.id,
            address_type_id = CustomerAddressType.ShipTo,
            address_id = _Address.id,
        }, _User.external_id);

        _Context.CustomerAddresses.Add(customer_shipto_address);
        _Context.SaveChanges();



        var purchase_order_header = CommonDataHelper<PurchaseOrderHeader>.FillCommonFields(new PurchaseOrderHeader()
        {
            po_number = 123123,
            po_type = "INTERNAL",
            vendor_id = vendor.id,
            revision_number = 1,
        }, _User.external_id);

        _Context.PurchaseOrderHeaders.Add(purchase_order_header);
        _Context.SaveChanges();

        _PurchaseOrderHeader = purchase_order_header;


        var purchase_order_line = CommonDataHelper<PurchaseOrderLine>.FillCommonFields(new PurchaseOrderLine()
        {
            purchase_order_header_id = _PurchaseOrderHeader.id,
            description = "This is a product",
            product_id = _Product.id,
            unit_price = 100,
            quantity = 1,
            line_number = 1,
            revision_number = 1,
            tax = 1,
            is_taxable = true
        }, _User.external_id);


        _Context.PurchaseOrderLines.Add(purchase_order_line);
        _Context.SaveChanges();

        _PurchaseOrderLine = purchase_order_line;


        var purchase_order_receive_header = CommonDataHelper<PurchaseOrderReceiveHeader>.FillCommonFields(new PurchaseOrderReceiveHeader()
        {
            purchase_order_id = _PurchaseOrderHeader.id,
            units_ordered = 1,
            units_received = 1,
            is_complete = true,
        }, _User.external_id);

        _Context.PurchaseOrderReceiveHeaders.Add(purchase_order_receive_header);
        _Context.SaveChanges();


        _PurchaseOrderReceiveHeader = purchase_order_receive_header;


        var purchase_order_receive_line = CommonDataHelper<PurchaseOrderReceiveLine>.FillCommonFields(new PurchaseOrderReceiveLine()
        {
            purchase_order_receive_header_id = _PurchaseOrderReceiveHeader.id,
            units_ordered = 1,
            units_received = 1,
            is_complete = true,
            purchase_order_line_id = _PurchaseOrderLine.id,
        }, _User.external_id);

        _Context.PurchaseOrderReceiveLines.Add(purchase_order_receive_line);
        _Context.SaveChanges();


        _PurchaseOrderReceiveHeader = purchase_order_receive_header;

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
            tax = 123,
            guid = Guid.NewGuid().ToString(),
            is_complete = false,
            is_canceled = false,
        }, _User.external_id);

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
            quantity = 1
        }, _User.external_id);

        _Context.OrderLines.Add(sales_order_receive_line);
        _Context.SaveChanges();


        _SalesOrderLine = sales_order_receive_line;


        var ar_invoice_header = CommonDataHelper<ARInvoiceHeader>.FillCommonFields(new ARInvoiceHeader()
        {
            invoice_number = 10001,
            invoice_date = DateOnly.Parse(DateTime.Now.ToString("MM/dd/yyyy")),
            invoice_due_date = DateOnly.Parse(DateTime.Now.ToString("MM/dd/yyyy")),
            invoice_total = 100,
            order_header_id = _SalesOrderHeader.id,
            payment_terms = "payment_terms_net_15",
            tax_percentage = 6,
        }, _User.external_id);

        _Context.ARInvoiceHeaders.Add(ar_invoice_header);
        _Context.SaveChanges();

        _ARInvoiceHeader = ar_invoice_header;


        var ar_invoice_line = CommonDataHelper<ARInvoiceLine>.FillCommonFields(new ARInvoiceLine()
        {
            ar_invoice_header_id = _ARInvoiceHeader.id,
            product_id = _Product.id,
            line_description = "A product with stuff",
            line_number = 1,
            invoice_qty = 1,
            line_tax = 1,
            order_line_id = _SalesOrderLine.id,
            order_qty = 1,
            line_total = 100,
        }, _User.external_id);

        _Context.ARInvoiceLines.Add(ar_invoice_line);
        _Context.SaveChanges();


        _ARInvoiceLine = ar_invoice_line;
    }

    [Test]
    public async Task Get()
    {
        var new_result = await _Module.Create(new ARInvoiceHeaderCreateCommand()
        {
            calling_user_id = _User.external_id,
            invoice_date = DateOnly.Parse(DateTime.Now.ToString("MM/dd/yyyy")),
            invoice_due_date = DateOnly.Parse(DateTime.Now.ToString("MM/dd/yyyy")),
            customer_id = _Customer.id,
            order_header_id = _SalesOrderHeader.id,
            payment_terms = "payment_terms_net_15",
            is_taxable = true,
            tax_percentage = 6,
            ar_invoice_lines = new List<ARInvoiceLineCreateCommand>() {
                new ARInvoiceLineCreateCommand()
                {
                    order_line_id = _SalesOrderLine.id,
                    line_number = 1,
                    invoice_qty = 1,
                    line_description = _SalesOrderLine.line_description,
                    is_taxable = true,
                    product_id = _SalesOrderLine.product_id,
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
        var result = await _Module.Create(new ARInvoiceHeaderCreateCommand()
        {
            calling_user_id = _User.external_id,
            invoice_date = DateOnly.Parse(DateTime.Now.ToString("MM/dd/yyyy")),
            invoice_due_date = DateOnly.Parse(DateTime.Now.ToString("MM/dd/yyyy")),
            customer_id = _Customer.id,
            order_header_id = _SalesOrderHeader.id,
            payment_terms = "payment_terms_net_15",
            is_taxable = true,
            tax_percentage = 6,
            ar_invoice_lines = new List<ARInvoiceLineCreateCommand>() {
                new ARInvoiceLineCreateCommand()
                {
                    order_line_id = _SalesOrderLine.id,
                    line_number = 1,
                    invoice_qty = 1,
                    line_description = _SalesOrderLine.line_description,
                    is_taxable = true,
                    product_id = _SalesOrderLine.product_id,
                    calling_user_id = _User.external_id
                }
            }
        });

        ValidateMostDtoFields(result);
    }

    [Test]
    public async Task Edit()
    {
        var old_result = await _Module.Create(new ARInvoiceHeaderCreateCommand()
        {
            calling_user_id = _User.external_id,
            invoice_date = DateOnly.Parse(DateTime.Now.ToString("MM/dd/yyyy")),
            invoice_due_date = DateOnly.Parse(DateTime.Now.ToString("MM/dd/yyyy")),
            customer_id = _Customer.id,
            order_header_id = _SalesOrderHeader.id,
            payment_terms = "payment_terms_net_15",
            is_taxable = true,
            tax_percentage = 6,
            ar_invoice_lines = new List<ARInvoiceLineCreateCommand>() {
                new ARInvoiceLineCreateCommand()
                {
                    order_line_id = _SalesOrderLine.id,
                    line_number = 1,
                    invoice_qty = 1,
                    line_description = _SalesOrderLine.line_description,
                    is_taxable = true,
                    product_id = _SalesOrderLine.product_id,
                    calling_user_id = _User.external_id
                }
            }
        });

        var edit_command = new ARInvoiceHeaderEditCommand()
        {
            calling_user_id = _User.external_id,
            id = old_result.Data.id,
            payment_terms = "payment_terms_net_30",
            tax_percentage = 3,
            is_taxable = false,
            invoice_date = DateOnly.Parse(DateTime.Now.AddDays(-10).ToString("MM/dd/yyyy")),
            invoice_due_date = DateOnly.Parse(DateTime.Now.AddDays(40).ToString("MM/dd/yyyy")),
        };

        var result = await _Module.Edit(edit_command);

        ValidateMostDtoFields(result);

        Assert.That(result.Data.invoice_date == edit_command.invoice_date);
        Assert.That(result.Data.invoice_due_date == edit_command.invoice_due_date);
        Assert.That(result.Data.payment_terms == edit_command.payment_terms);
        Assert.That(result.Data.tax_percentage == edit_command.tax_percentage);
        Assert.That(result.Data.is_taxable == edit_command.is_taxable);
    }

    [Test]
    public async Task Delete()
    {
        var new_result = await _Module.Create(new ARInvoiceHeaderCreateCommand()
        {
            calling_user_id = _User.external_id,
            invoice_date = DateOnly.Parse(DateTime.Now.ToString("MM/dd/yyyy")),
            invoice_due_date = DateOnly.Parse(DateTime.Now.ToString("MM/dd/yyyy")),
            customer_id = _Customer.id,
            order_header_id = _SalesOrderHeader.id,
            payment_terms = "payment_terms_net_15",
            is_taxable = true,
            tax_percentage = 6,
            ar_invoice_lines = new List<ARInvoiceLineCreateCommand>() {
                new ARInvoiceLineCreateCommand()
                {
                    order_line_id = _SalesOrderLine.id,
                    line_number = 1,
                    invoice_qty = 1,
                    line_description = _SalesOrderLine.line_description,
                    is_taxable = true,
                    product_id = _SalesOrderLine.product_id,
                    calling_user_id = _User.external_id
                }
            }
        });

        Assert.That(new_result.Success, Is.True);
        Assert.That(new_result.Data, Is.Not.Null);

        var delete_result = await _Module.Delete(new ARInvoiceHeaderDeleteCommand()
        {
            calling_user_id = _User.external_id,
            id = new_result.Data.id
        });

        Assert.That(delete_result.Success, Is.True);
        Assert.That(delete_result.Data, Is.Not.Null);
        Assert.That(delete_result.Data.ar_invoice_lines.Count(), Is.Zero);
        Assert.That(delete_result.Data.deleted_by, Is.Not.Null);
        Assert.That(delete_result.Data.deleted_on, Is.Not.Null);
        Assert.That(delete_result.Data.deleted_on_string, Is.Not.Null);
        Assert.That(delete_result.Data.deleted_on_timezone, Is.Not.Null);
    }

    [Test]
    public async Task Find()
    {
        var new_result = await _Module.Create(new ARInvoiceHeaderCreateCommand()
        {
            calling_user_id = _User.external_id,
            invoice_date = DateOnly.Parse(DateTime.Now.ToString("MM/dd/yyyy")),
            invoice_due_date = DateOnly.Parse(DateTime.Now.ToString("MM/dd/yyyy")),
            customer_id = _Customer.id,
            order_header_id = _SalesOrderHeader.id,
            payment_terms = "payment_terms_net_15",
            is_taxable = true,
            tax_percentage = 6,
            ar_invoice_lines = new List<ARInvoiceLineCreateCommand>() {
                new ARInvoiceLineCreateCommand()
                {
                    order_line_id = _SalesOrderLine.id,
                    line_number = 1,
                    invoice_qty = 1,
                    line_description = _SalesOrderLine.line_description,
                    is_taxable = true,
                    product_id = _SalesOrderLine.product_id,
                    calling_user_id = _User.external_id
                }
            }
        });

        Assert.That(new_result.Success, Is.True);
        Assert.That(new_result.Data, Is.Not.Null);

        ValidateMostDtoFields(new_result);

        var results = await _Module.Find(
                        new PagingSortingParameters() { ResultCount = 20, Start = 0 },
                        new ARInvoiceHeaderFindCommand() { calling_user_id = _User.external_id, wildcard = new_result.Data.invoice_number.ToString() });

        Assert.That(results.Success, Is.True);
        Assert.That(results.Data, Is.Not.Null);
        Assert.That(results.Data.Count(), Is.Not.Zero);

        var first_result = results.Data.First();

        ValidateMostListFields(first_result);
    }

    
    [Test]
    public async Task CreateLine()
    {
        var create_result = await _Module.Create(new ARInvoiceHeaderCreateCommand()
        {
            calling_user_id = _User.external_id,
            invoice_date = DateOnly.Parse(DateTime.Now.ToString("MM/dd/yyyy")),
            invoice_due_date = DateOnly.Parse(DateTime.Now.ToString("MM/dd/yyyy")),
            customer_id = _Customer.id,
            order_header_id = _SalesOrderHeader.id,
            payment_terms = "payment_terms_net_15",
            is_taxable = true,
            tax_percentage = 6,
            ar_invoice_lines = new List<ARInvoiceLineCreateCommand>() {
                new ARInvoiceLineCreateCommand()
                {
                    order_line_id = _SalesOrderLine.id,
                    line_number = 1,
                    invoice_qty = 1,
                    line_description = _SalesOrderLine.line_description,
                    is_taxable = true,
                    product_id = _SalesOrderLine.product_id,
                    calling_user_id = _User.external_id
                }
            }
        });

        Assert.That(create_result.Success, Is.True);
        Assert.That(create_result.Data, Is.Not.Null);
        Assert.That(create_result.Data.ar_invoice_lines.Count(), Is.Not.Zero);


        var create_command = new ARInvoiceLineCreateCommand()
        {
            calling_user_id = _User.external_id,
            ar_invoice_header_id = create_result.Data.id,
            order_line_id = _SalesOrderLine.id,
            line_number = 2,
            invoice_qty = 111,
            line_description = "asdasdasd",
            is_taxable = false,
            product_id = _SalesOrderLine.product_id,
        };

        var create_line_response = await _Module.CreateLine(create_command);


        Assert.That(create_line_response.Success, Is.True);
        Assert.That(create_line_response.Data, Is.Not.Null);
        Assert.That(create_line_response.Data.line_number == create_command.line_number);
        Assert.That(create_line_response.Data.invoice_qty == create_command.invoice_qty);
        Assert.That(create_line_response.Data.line_description == create_command.line_description);
        Assert.That(create_line_response.Data.is_taxable == create_command.is_taxable);
    }

    [Test]
    public async Task EditLine()
    {
        var create_result = await _Module.Create(new ARInvoiceHeaderCreateCommand()
        {
            calling_user_id = _User.external_id,
            invoice_date = DateOnly.Parse(DateTime.Now.ToString("MM/dd/yyyy")),
            invoice_due_date = DateOnly.Parse(DateTime.Now.ToString("MM/dd/yyyy")),
            customer_id = _Customer.id,
            order_header_id = _SalesOrderHeader.id,
            payment_terms = "payment_terms_net_15",
            is_taxable = true,
            tax_percentage = 6,
            ar_invoice_lines = new List<ARInvoiceLineCreateCommand>() {
                new ARInvoiceLineCreateCommand()
                {
                    order_line_id = _SalesOrderLine.id,
                    line_number = 1,
                    invoice_qty = 1,
                    line_description = _SalesOrderLine.line_description,
                    is_taxable = true,
                    product_id = _SalesOrderLine.product_id,
                    calling_user_id = _User.external_id
                }
            }
        });

        Assert.That(create_result.Success, Is.True);
        Assert.That(create_result.Data, Is.Not.Null);
        Assert.That(create_result.Data.ar_invoice_lines.Count(), Is.Not.Zero);


        var edit_command = new ARInvoiceLineEditCommand()
        {
            calling_user_id = _User.external_id,
            id = create_result.Data.ar_invoice_lines[0].id,
            line_number = 4,
            is_taxable = false,
            invoice_qty = 23,
            line_description = "ASDASD",
            order_line_id = _SalesOrderLine.id,
            ar_invoice_header_id = _SalesOrderHeader.id,
        };

        var edit_line_response = await _Module.EditLine(edit_command);


        Assert.That(edit_line_response.Success, Is.True);
        Assert.That(edit_line_response.Data, Is.Not.Null);
        Assert.That(edit_line_response.Data.line_number == edit_command.line_number);
        Assert.That(edit_line_response.Data.invoice_qty == edit_command.invoice_qty);
        Assert.That(edit_line_response.Data.is_taxable == edit_command.is_taxable);
        Assert.That(edit_line_response.Data.ar_invoice_header_id == edit_command.ar_invoice_header_id);
        Assert.That(edit_line_response.Data.order_line_id == edit_command.order_line_id);
    }


    [Test]
    public async Task DeleteLine()
    {
        var create_result = await _Module.Create(new ARInvoiceHeaderCreateCommand()
        {
            calling_user_id = _User.external_id,
            invoice_date = DateOnly.Parse(DateTime.Now.ToString("MM/dd/yyyy")),
            invoice_due_date = DateOnly.Parse(DateTime.Now.ToString("MM/dd/yyyy")),
            customer_id = _Customer.id,
            order_header_id = _SalesOrderHeader.id,
            payment_terms = "payment_terms_net_15",
            is_taxable = true,
            tax_percentage = 6,
            ar_invoice_lines = new List<ARInvoiceLineCreateCommand>() {
                new ARInvoiceLineCreateCommand()
                {
                    order_line_id = _SalesOrderLine.id,
                    line_number = 1,
                    invoice_qty = 1,
                    line_description = _SalesOrderLine.line_description,
                    is_taxable = true,
                    product_id = _SalesOrderLine.product_id,
                    calling_user_id = _User.external_id
                }
            }
        });

        Assert.That(create_result.Success, Is.True);
        Assert.That(create_result.Data, Is.Not.Null);
        Assert.That(create_result.Data.ar_invoice_lines.Count(), Is.Not.Zero);

        var response = await _Module.DeleteLine(new ARInvoiceLineDeleteCommand()
        {
            calling_user_id = _User.external_id,
            id = create_result.Data.ar_invoice_lines[0].id,
        });


        Assert.That(response.Success, Is.True);
        Assert.That(response.Data, Is.Not.Null);
        Assert.That(response.Data.deleted_by, Is.Not.Null);
        Assert.That(response.Data.deleted_on, Is.Not.Null);
        Assert.That(response.Data.deleted_on_string, Is.Not.Null);
        Assert.That(response.Data.deleted_on_timezone, Is.Not.Null);
    }


    private void ValidateMostDtoFields(Response<ARInvoiceHeaderDto> result)
    {
        Assert.That(result.Success, Is.True);
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data.id, Is.Not.Zero);
        Assert.That(result.Data.guid, Is.Not.Empty);
        Assert.That(result.Data.invoice_number, Is.Not.Zero);
        Assert.That(result.Data.order_number, Is.Not.Null);
        Assert.That(result.Data.customer_id, Is.Not.Zero);
        Assert.That(result.Data.customer_name, Is.Not.Empty);
        Assert.That(result.Data.invoice_total, Is.Not.Zero);
        Assert.That(result.Data.created_by, Is.Not.Zero);
        Assert.That(result.Data.created_on, Is.GreaterThan(DateTime.MinValue));
        Assert.That(result.Data.created_on_string, Is.Not.Null);
        Assert.That(result.Data.created_on_timezone, Is.Not.Null);
        Assert.That(result.Data.updated_by, Is.Not.Null);
        Assert.That(result.Data.updated_on, Is.Not.Null);
        Assert.That(result.Data.updated_on_string, Is.Not.Null);
        Assert.That(result.Data.updated_on_timezone, Is.Not.Null);
    }

    private void ValidateMostListFields(ARInvoiceHeaderListDto result)
    {
        Assert.That(result.id, Is.Not.Zero);
        Assert.That(result.guid, Is.Not.Empty);
        Assert.That(result.invoice_number, Is.Not.Zero);
        Assert.That(result.order_number, Is.Not.Null);
        Assert.That(result.customer_id, Is.Not.Zero);
        Assert.That(result.invoice_total, Is.Not.Zero);
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