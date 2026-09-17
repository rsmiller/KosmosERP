using System.Data;
using Microsoft.EntityFrameworkCore;
using KosmosERP.Models;
using KosmosERP.Models.Permissions;
using KosmosERP.Tests.Modules.Shared;
using KosmosERP.BusinessLayer.Helpers;
using KosmosERP.BusinessLayer.Modules;
using KosmosERP.Database.Models;
using KosmosERP.BusinessLayer.Models.Module.APInvoice.Command.Create;
using KosmosERP.BusinessLayer.Models.Module.APInvoice.Command.Edit;
using KosmosERP.BusinessLayer.Models.Module.APInvoice.Command.Delete;
using KosmosERP.BusinessLayer.Models.Module.APInvoice.Command.Find;
using KosmosERP.BusinessLayer.Models.Module.APInvoice.Dto;
using KosmosERP.BusinessLayer.Models.Module.APInvoice.Command;
using KosmosERP.BusinessLayer.MessagePublisher;
using KosmosERP.BusinessLayer;
using Microsoft.Extensions.Caching.Memory;

namespace KosmosERP.Tests.Modules;

public class APInvoiceModuleTests : BaseTestModule<APInvoiceModule>, IModuleTest
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
        var poModule = new PurchaseOrderModule(base._Context, logProviderFactory);
        var orderModule = new OrderModule(base._Context, logProviderFactory);
        var arModule = new ARInvoiceModule(base._Context, logProviderFactory);
        var docModule = new DocumentUploadModule(base._Context, logProviderFactory);
        var poRModule = new PurchaseOrderReceiveModule(base._Context, logProviderFactory);
        var financialTransactionModule = new FinancialTransactionModule(base._Context, logProviderFactory);
        var chartOfAccountModule = new ChartOfAccountModule(base._Context, logProviderFactory);

        var the_module = new APInvoiceModule(base._Context, poModule, orderModule, arModule, poRModule, docModule, financialTransactionModule, chartOfAccountModule, logProviderFactory);

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
        }, 1);

        _Context.Vendors.Add(vendor);
        await _Context.SaveChangesAsync();

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



        var purchase_order_header = CommonDataHelper<PurchaseOrderHeader>.FillCommonFields(new PurchaseOrderHeader()
        {
            po_number = 123123,
            po_type = "INTERNAL",
            vendor_id = vendor.id,
            revision_number = 1,
        }, 1);

        _Context.PurchaseOrderHeaders.Add(purchase_order_header);
        await _Context.SaveChangesAsync();

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
        }, 1);


        _Context.PurchaseOrderLines.Add(purchase_order_line);
        await _Context.SaveChangesAsync();

        _PurchaseOrderLine = purchase_order_line;


        var purchase_order_receive_header = CommonDataHelper<PurchaseOrderReceiveHeader>.FillCommonFields(new PurchaseOrderReceiveHeader()
        {
            purchase_order_id = _PurchaseOrderHeader.id,
            units_ordered = 1,
            units_received = 1,
            is_complete = true,
        }, 1);

        _Context.PurchaseOrderReceiveHeaders.Add(purchase_order_receive_header);
        await _Context.SaveChangesAsync();


        _PurchaseOrderReceiveHeader = purchase_order_receive_header;


        var purchase_order_receive_line = CommonDataHelper<PurchaseOrderReceiveLine>.FillCommonFields(new PurchaseOrderReceiveLine()
        {
            purchase_order_receive_header_id = _PurchaseOrderReceiveHeader.id,
            units_ordered = 1,
            units_received = 1,
            is_complete = true,
            purchase_order_line_id = _PurchaseOrderLine.id,
        }, 1);

        _Context.PurchaseOrderReceiveLines.Add(purchase_order_receive_line);
        await _Context.SaveChangesAsync();


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
            tax = 123
        }, 1);

        _Context.OrderHeaders.Add(sales_order_header);
        await _Context.SaveChangesAsync();

        _SalesOrderHeader = sales_order_header;


        var sales_order_receive_line = CommonDataHelper<OrderLine>.FillCommonFields(new OrderLine()
        {
            order_header_id = _SalesOrderHeader.id,
            product_id = _Product.id,
            line_description = "A product with stuff",
            line_number = 1,
            unit_price = 100,
            quantity = 1
        }, 1);

        _Context.OrderLines.Add(sales_order_receive_line);
        await _Context.SaveChangesAsync();


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
        }, 1);

        _Context.ARInvoiceHeaders.Add(ar_invoice_header);
        await _Context.SaveChangesAsync();

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
        }, 1);

        _Context.ARInvoiceLines.Add(ar_invoice_line);
        await _Context.SaveChangesAsync();


        _ARInvoiceLine = ar_invoice_line;
    }

    [Test]
    public async Task Get()
    {
        var new_result = await _Module.Create(new APInvoiceHeaderCreateCommand()
        {
            calling_user_id = _User.external_id,
            vendor_id = _Vendor.id,
            invoice_number = "D1230",
            invoice_date = DateTime.Now,
            invoice_due_date = DateTime.Now.AddDays(20),
            invoice_received_date = DateTime.Now.AddDays(-10),
            invoice_total = 100,
            memo = "Test memo",
            purchase_order_receive_id = _PurchaseOrderReceiveHeader.id,
            packing_list_is_required = true,
            association_object_id = _PurchaseOrderHeader.id,
            association_is_purchase_order = true,
            association_is_sales_order = false,
            association_is_ar_invoice = false,
            is_paid = false,
            ap_invoice_lines = new List<APInvoiceLineCreateCommand>() {
                new APInvoiceLineCreateCommand()
                {
                    gl_account = "gl_account_cash",
                    description = "A line",
                    line_number = 1,
                    line_total = 100,
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
        var result = await _Module.Create(new APInvoiceHeaderCreateCommand()
        {
            calling_user_id = _User.external_id,
            vendor_id = _Vendor.id,
            invoice_number = "D1230",
            invoice_date = DateTime.Now,
            invoice_due_date = DateTime.Now.AddDays(20),
            invoice_received_date = DateTime.Now.AddDays(-10),
            invoice_total = 100,
            memo = "Test memo",
            purchase_order_receive_id = _PurchaseOrderReceiveHeader.id,
            packing_list_is_required = true,
            association_object_id = _PurchaseOrderHeader.id,
            association_is_purchase_order = true,
            association_is_sales_order = false,
            association_is_ar_invoice = false,
            is_paid = false,
            ap_invoice_lines = new List<APInvoiceLineCreateCommand>() {
                new APInvoiceLineCreateCommand()
                {
                    gl_account = "gl_account_cash",
                    description = "A line",
                    line_number = 1,
                    line_total = 100,
                }
            }
        });

        ValidateMostDtoFields(result);
    }

    [Test]
    public async Task Edit()
    {
        var old_result = await _Module.Create(new APInvoiceHeaderCreateCommand()
        {
            calling_user_id = _User.external_id,
            vendor_id = _Vendor.id,
            invoice_number = "D1230",
            invoice_date = DateTime.Now,
            invoice_due_date = DateTime.Now.AddDays(20),
            invoice_received_date = DateTime.Now.AddDays(-10),
            invoice_total = 100,
            memo = "Test memo",
            purchase_order_receive_id = _PurchaseOrderReceiveHeader.id,
            packing_list_is_required = true,
            association_object_id = _PurchaseOrderHeader.id,
            association_is_purchase_order = true,
            association_is_sales_order = false,
            association_is_ar_invoice = false,
            is_paid = false,
            ap_invoice_lines = new List<APInvoiceLineCreateCommand>() {
                new APInvoiceLineCreateCommand()
                {
                    gl_account = "gl_account_cash",
                    description = "A line",
                    line_number = 1,
                    line_total = 100,
                }
            }
        });

        var edit_command = new APInvoiceHeaderEditCommand()
        {
            calling_user_id = _User.external_id,
            id = old_result.Data.id,
            invoice_total = 1000,
            invoice_date = DateTime.Now.AddDays(-10),
            invoice_due_date = DateTime.Now.AddDays(40),
            invoice_received_date = DateTime.Now.AddDays(-140),
            memo = "Test memo234234234",
        };

        var result = await _Module.Edit(edit_command);

        ValidateMostDtoFields(result);

        Assert.That(result.Data.invoice_total == edit_command.invoice_total);
        Assert.That(result.Data.invoice_date == edit_command.invoice_date);
        Assert.That(result.Data.invoice_due_date == edit_command.invoice_due_date);
        Assert.That(result.Data.invoice_received_date == edit_command.invoice_received_date);
        Assert.That(result.Data.memo == edit_command.memo);
    }

    [Test]
    public async Task Delete()
    {
        var new_result = await _Module.Create(new APInvoiceHeaderCreateCommand()
        {
            calling_user_id = _User.external_id,
            vendor_id = _Vendor.id,
            invoice_number = "D1230",
            invoice_date = DateTime.Now,
            invoice_due_date = DateTime.Now.AddDays(20),
            invoice_received_date = DateTime.Now.AddDays(-10),
            invoice_total = 100,
            memo = "Test memo",
            purchase_order_receive_id = _PurchaseOrderReceiveHeader.id,
            packing_list_is_required = true,
            association_object_id = _PurchaseOrderHeader.id,
            association_is_purchase_order = true,
            association_is_sales_order = false,
            association_is_ar_invoice = false,
            is_paid = false,
            ap_invoice_lines = new List<APInvoiceLineCreateCommand>() {
                new APInvoiceLineCreateCommand()
                {
                    gl_account = "gl_account_cash",
                    description = "A line",
                    line_number = 1,
                    line_total = 100,
                }
            }
        });

        Assert.That(new_result.Success, Is.True);
        Assert.That(new_result.Data, Is.Not.Null);

        var delete_result = await _Module.Delete(new APInvoiceHeaderDeleteCommand()
        {
            calling_user_id = _User.external_id,
            id = new_result.Data.id
        });

        Assert.That(delete_result.Success, Is.True);
        Assert.That(delete_result.Data, Is.Not.Null);
        Assert.That(delete_result.Data.ap_invoice_lines.Count(), Is.Zero);
        Assert.That(delete_result.Data.deleted_by, Is.Not.Null);
        Assert.That(delete_result.Data.deleted_on, Is.Not.Null);
        Assert.That(delete_result.Data.deleted_on_string, Is.Not.Null);
        Assert.That(delete_result.Data.deleted_on_timezone, Is.Not.Null);
    }

    [Test]
    public async Task Find()
    {
        var new_result = await _Module.Create(new APInvoiceHeaderCreateCommand()
        {
            calling_user_id = _User.external_id,
            vendor_id = _Vendor.id,
            invoice_number = "D1230",
            invoice_date = DateTime.Now,
            invoice_due_date = DateTime.Now.AddDays(20),
            invoice_received_date = DateTime.Now.AddDays(-10),
            invoice_total = 100,
            memo = "Test memo",
            purchase_order_receive_id = _PurchaseOrderReceiveHeader.id,
            packing_list_is_required = true,
            association_object_id = _PurchaseOrderHeader.id,
            association_is_purchase_order = true,
            association_is_sales_order = false,
            association_is_ar_invoice = false,
            is_paid = false,
            ap_invoice_lines = new List<APInvoiceLineCreateCommand>() {
                new APInvoiceLineCreateCommand()
                {
                    gl_account = "gl_account_cash",
                    description = "A line",
                    line_number = 1,
                    line_total = 100,
                }
            }
        });

        Assert.That(new_result.Success, Is.True);
        Assert.That(new_result.Data, Is.Not.Null);

        ValidateMostDtoFields(new_result);

        var results = await _Module.Find(
                        new PagingSortingParameters() { ResultCount = 20, Start = 0 },
                        new APInvoiceHeaderFindCommand() { calling_user_id = _User.external_id, wildcard = "D1230" });

        Assert.That(results.Success, Is.True);
        Assert.That(results.Data, Is.Not.Null);
        Assert.That(results.Data.Count(), Is.Not.Zero);

        var first_result = results.Data.First();

        ValidateMostListFields(first_result);
    }

    [Test]
    public async Task GetAssociations()
    {
        var purchase_order_create_result = await _Module.Create(new APInvoiceHeaderCreateCommand()
        {
            calling_user_id = _User.external_id,
            vendor_id = _Vendor.id,
            invoice_number = "D6230",
            invoice_date = DateTime.Now,
            invoice_due_date = DateTime.Now.AddDays(20),
            invoice_received_date = DateTime.Now.AddDays(-10),
            invoice_total = 100,
            memo = "Test memo",
            purchase_order_receive_id = _PurchaseOrderReceiveHeader.id,
            packing_list_is_required = true,
            association_object_id = _PurchaseOrderHeader.id,
            association_is_purchase_order = true,
            association_is_sales_order = false,
            association_is_ar_invoice = false,
            is_paid = false,
            ap_invoice_lines = new List<APInvoiceLineCreateCommand>() {
                new APInvoiceLineCreateCommand()
                {
                    gl_account = "gl_account_cash",
                    description = "A line",
                    line_number = 1,
                    line_total = 100,
                    association_is_purchase_order = true,
                    association_object_line_id = _PurchaseOrderLine.id,
                    association_object_id = _PurchaseOrderHeader.id,
                    qty_invoiced = 1
                }
            }
        });

        var ap_invoice_create_result = await _Module.Create(new APInvoiceHeaderCreateCommand()
        {
            calling_user_id = _User.external_id,
            vendor_id = _Vendor.id,
            invoice_number = "D6231",
            invoice_date = DateTime.Now,
            invoice_due_date = DateTime.Now.AddDays(20),
            invoice_received_date = DateTime.Now.AddDays(-10),
            invoice_total = 100,
            memo = "Test memo",
            purchase_order_receive_id = _PurchaseOrderReceiveHeader.id,
            packing_list_is_required = true,
            association_object_id = _SalesOrderHeader.id,
            association_is_purchase_order = false,
            association_is_sales_order = true,
            association_is_ar_invoice = false,
            is_paid = false,
            ap_invoice_lines = new List<APInvoiceLineCreateCommand>() {
                new APInvoiceLineCreateCommand()
                {
                    gl_account = "gl_account_cash",
                    description = "A sales order line",
                    line_number = 1,
                    line_total = 100,
                    association_is_sales_order = true,
                    association_object_line_id = _SalesOrderLine.id,
                    association_object_id = _SalesOrderHeader.id,
                    qty_invoiced = 1
                }
            }
        });

        Assert.That(ap_invoice_create_result.Success, Is.True);
        Assert.That(ap_invoice_create_result.Data, Is.Not.Null);

        var sales_order_results = await _Module.GetAssociations(new APInvoiceAssociationsFindCommand()
        {
            calling_user_id = _User.external_id,
            ap_invoice_object_id = ap_invoice_create_result.Data.id,
            
        });

        Assert.That(sales_order_results, Is.Not.Null);
        Assert.That(sales_order_results.Data, Is.Not.Null);
        Assert.That(sales_order_results.Data.Count(), Is.Not.Zero);
        Assert.That(sales_order_results.Data[0].is_sales_order == true);
        Assert.That(sales_order_results.Data[0].additional_data.Count(), Is.Not.Zero);


        var ar_invoice_create_result = await _Module.Create(new APInvoiceHeaderCreateCommand()
        {
            calling_user_id = _User.external_id,
            vendor_id = _Vendor.id,
            invoice_number = "D6234",
            invoice_date = DateTime.Now,
            invoice_due_date = DateTime.Now.AddDays(20),
            invoice_received_date = DateTime.Now.AddDays(-10),
            invoice_total = 100,
            memo = "Test memo",
            purchase_order_receive_id = _PurchaseOrderReceiveHeader.id,
            packing_list_is_required = true,
            association_object_id = _PurchaseOrderHeader.id,
            association_is_purchase_order = false,
            association_is_sales_order = false,
            association_is_ar_invoice = true,
            is_paid = false,
            ap_invoice_lines = new List<APInvoiceLineCreateCommand>() {
                new APInvoiceLineCreateCommand()
                {
                    gl_account = "gl_account_cash",
                    description = "A ar line",
                    line_number = 1,
                    line_total = 100,
                    association_is_ar_invoice = true,
                    association_object_line_id = _ARInvoiceLine.id,
                    association_object_id = _ARInvoiceHeader.id,
                    qty_invoiced = 1
                }
            }
        });

        Assert.That(ar_invoice_create_result.Success, Is.True);
        Assert.That(ar_invoice_create_result.Data, Is.Not.Null);

        var ar_ionvoice_results = await _Module.GetAssociations(new APInvoiceAssociationsFindCommand()
        {
            calling_user_id = _User.external_id,
            ap_invoice_object_id = ar_invoice_create_result.Data.id,
        });

        Assert.That(ar_ionvoice_results, Is.Not.Null);
        Assert.That(ar_ionvoice_results.Data, Is.Not.Null);
        Assert.That(ar_ionvoice_results.Data.Count(), Is.Not.Zero);
        Assert.That(ar_ionvoice_results.Data[0].is_ar_invoice == true);
        Assert.That(ar_ionvoice_results.Data[0].additional_data.Count(), Is.Not.Zero);
    }


    [Test]
    public async Task AssociateReceivedPO()
    {
        var create_result = await _Module.Create(new APInvoiceHeaderCreateCommand()
        {
            calling_user_id = _User.external_id,
            vendor_id = _Vendor.id,
            invoice_number = "D6239",
            invoice_date = DateTime.Now,
            invoice_due_date = DateTime.Now.AddDays(20),
            invoice_received_date = DateTime.Now.AddDays(-10),
            invoice_total = 100,
            memo = "Test memo",
            ap_invoice_lines = new List<APInvoiceLineCreateCommand>() {
                new APInvoiceLineCreateCommand()
                {
                    gl_account = "gl_account_cash",
                    description = "A line",
                    line_number = 1,
                    line_total = 100,
                    qty_invoiced = 1
                }
            }
        });

        Assert.That(create_result.Success, Is.True);
        Assert.That(create_result.Data, Is.Not.Null);

        var associate_results = await _Module.AssociateReceivedPO(new APInvoiceAssociatePOCommand()
        {
            calling_user_id = _User.external_id,
            ap_invoice_object_id = create_result.Data.id,
            purchase_order_receive_header_id = _PurchaseOrderReceiveHeader.id,
        });

        Assert.That(associate_results.Success, Is.True);
        Assert.That(associate_results.Data, Is.Not.Null);
        Assert.That(associate_results.Data.purchase_order_receive_id, Is.Not.Null);
    }

    [Test]
    public async Task AssociateHeaderObject()
    {
        var create_result = await _Module.Create(new APInvoiceHeaderCreateCommand()
        {
            calling_user_id = _User.external_id,
            vendor_id = _Vendor.id,
            invoice_number = "D6283",
            invoice_date = DateTime.Now,
            invoice_due_date = DateTime.Now.AddDays(20),
            invoice_received_date = DateTime.Now.AddDays(-10),
            invoice_total = 100,
            memo = "Test memo",
            ap_invoice_lines = new List<APInvoiceLineCreateCommand>() {
                new APInvoiceLineCreateCommand()
                {
                    gl_account = "gl_account_cash",
                    description = "A line",
                    line_number = 1,
                    line_total = 100,
                    qty_invoiced = 1
                }
            }
        });

        Assert.That(create_result.Success, Is.True);
        Assert.That(create_result.Data, Is.Not.Null);
        Assert.That(create_result.Data.ap_invoice_lines.Count(), Is.Not.Zero);


        var associate_response = await _Module.AssociateHeaderObject(new APInvoiceAssoicationCommand()
        {
            calling_user_id = _User.external_id,
            ap_invoice_object_id = create_result.Data.ap_invoice_lines[0].id,
            association_object_id = _PurchaseOrderHeader.id,
            association_is_purchase_order = true
        });

        Assert.That(associate_response, Is.Not.Null);
        Assert.That(associate_response.Data, Is.Not.Null);
        Assert.That(associate_response.Data.ap_invoice_lines.Count(), Is.Not.Zero);
        Assert.That(associate_response.Data.association_is_purchase_order == true);
        Assert.That(associate_response.Data.association_object_id == _PurchaseOrderHeader.id);


        var associate_line_response = await _Module.AssociateLineObject(new APInvoiceAssoicationCommand()
        {
            calling_user_id = _User.external_id,
            ap_invoice_object_id = create_result.Data.ap_invoice_lines[0].id,
            association_object_id = _PurchaseOrderLine.id,
            association_is_purchase_order = true,
        });

        Assert.That(associate_response, Is.Not.Null);
        Assert.That(associate_response.Data, Is.Not.Null);
        Assert.That(associate_response.Data.association_is_purchase_order == true);
        Assert.That(associate_response.Data.association_object_id == _PurchaseOrderHeader.id);
    }

    [Test]
    public async Task CreateLine()
    {
        var create_result = await _Module.Create(new APInvoiceHeaderCreateCommand()
        {
            calling_user_id = _User.external_id,
            vendor_id = _Vendor.id,
            invoice_number = "D6283",
            invoice_date = DateTime.Now,
            invoice_due_date = DateTime.Now.AddDays(20),
            invoice_received_date = DateTime.Now.AddDays(-10),
            invoice_total = 100,
            memo = "Test memo",
            ap_invoice_lines = new List<APInvoiceLineCreateCommand>() {
                new APInvoiceLineCreateCommand()
                {
                    gl_account = "gl_account_cash",
                    description = "A line",
                    line_number = 1,
                    line_total = 100,
                    qty_invoiced = 1
                }
            }
        });

        Assert.That(create_result.Success, Is.True);
        Assert.That(create_result.Data, Is.Not.Null);
        Assert.That(create_result.Data.ap_invoice_lines.Count(), Is.Not.Zero);


        var create_command = new APInvoiceLineCreateCommand()
        {
            calling_user_id = _User.external_id,
            ap_invoice_header_id = create_result.Data.id,
            gl_account = "gl_account_cash",
            description = "A line",
            line_number = 2,
            line_total = 100,
            qty_invoiced = 1
        };

        var create_line_response = await _Module.CreateLine(create_command);


        Assert.That(create_line_response.Success, Is.True);
        Assert.That(create_line_response.Data, Is.Not.Null);
        Assert.That(create_line_response.Data.line_number == create_command.line_number);
        Assert.That(create_line_response.Data.gl_account == create_command.gl_account);
        Assert.That(create_line_response.Data.description == create_command.description);
        Assert.That(create_line_response.Data.line_total == create_command.line_total);
        Assert.That(create_line_response.Data.qty_invoiced == create_command.qty_invoiced);
    }

    [Test]
    public async Task EditLine()
    {
        var create_result = await _Module.Create(new APInvoiceHeaderCreateCommand()
        {
            calling_user_id = _User.external_id,
            vendor_id = _Vendor.id,
            invoice_number = "D6283",
            invoice_date = DateTime.Now,
            invoice_due_date = DateTime.Now.AddDays(20),
            invoice_received_date = DateTime.Now.AddDays(-10),
            invoice_total = 100,
            memo = "Test memo",
            ap_invoice_lines = new List<APInvoiceLineCreateCommand>() {
                new APInvoiceLineCreateCommand()
                {
                    gl_account = "gl_account_cash",
                    description = "A line",
                    line_number = 1,
                    line_total = 100,
                    qty_invoiced = 1
                }
            }
        });

        Assert.That(create_result.Success, Is.True);
        Assert.That(create_result.Data, Is.Not.Null);
        Assert.That(create_result.Data.ap_invoice_lines.Count(), Is.Not.Zero);


        var edit_command = new APInvoiceLineEditCommand()
        {
            calling_user_id = _User.external_id,
            id = create_result.Data.ap_invoice_lines[0].id,
            gl_account = "gl_account_check",
            description = "asdasd",
            line_number = 4,
            line_total = 1010,
            qty_invoiced = 2,
        };

        var edit_line_response = await _Module.EditLine(edit_command);


        Assert.That(edit_line_response.Success, Is.True);
        Assert.That(edit_line_response.Data, Is.Not.Null);
        Assert.That(edit_line_response.Data.line_number == edit_command.line_number);
        Assert.That(edit_line_response.Data.gl_account == edit_command.gl_account);
        Assert.That(edit_line_response.Data.description == edit_command.description);
        Assert.That(edit_line_response.Data.line_total == edit_command.line_total);
        Assert.That(edit_line_response.Data.qty_invoiced == edit_command.qty_invoiced);
    }


    [Test]
    public async Task DeleteLine()
    {
        var create_result = await _Module.Create(new APInvoiceHeaderCreateCommand()
        {
            calling_user_id = _User.external_id,
            vendor_id = _Vendor.id,
            invoice_number = "D6283",
            invoice_date = DateTime.Now,
            invoice_due_date = DateTime.Now.AddDays(20),
            invoice_received_date = DateTime.Now.AddDays(-10),
            invoice_total = 100,
            memo = "Test memo",
            ap_invoice_lines = new List<APInvoiceLineCreateCommand>() {
                new APInvoiceLineCreateCommand()
                {
                    gl_account = "gl_account_cash",
                    description = "A line",
                    line_number = 1,
                    line_total = 100,
                    qty_invoiced = 1
                }
            }
        });

        Assert.That(create_result.Success, Is.True);
        Assert.That(create_result.Data, Is.Not.Null);
        Assert.That(create_result.Data.ap_invoice_lines.Count(), Is.Not.Zero);

        var response = await _Module.DeleteLine(new APInvoiceLineDeleteCommand()
        {
            calling_user_id = _User.external_id,
            id = create_result.Data.ap_invoice_lines[0].id,
        });


        Assert.That(response.Success, Is.True);
        Assert.That(response.Data, Is.Not.Null);
        Assert.That(response.Data.deleted_by, Is.Not.Null);
        Assert.That(response.Data.deleted_on, Is.Not.Null);
        Assert.That(response.Data.deleted_on_string, Is.Not.Null);
        Assert.That(response.Data.deleted_on_timezone, Is.Not.Null);
    }


    private void ValidateMostDtoFields(Response<APInvoiceHeaderDto> result)
    {
        Assert.That(result.Success, Is.True);
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data.id, Is.Not.Zero);
        Assert.That(result.Data.guid, Is.Not.Empty);
        Assert.That(result.Data.invoice_number, Is.Not.Empty);
        Assert.That(result.Data.memo, Is.Not.Empty);
        Assert.That(result.Data.vendor_id, Is.Not.Zero);
        Assert.That(result.Data.vendor_name, Is.Not.Empty);
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

    private void ValidateMostListFields(APInvoiceHeaderListDto result)
    {
        Assert.That(result.id, Is.Not.Zero);
        Assert.That(result.guid, Is.Not.Empty);
        Assert.That(result.invoice_number, Is.Not.Empty);
        Assert.That(result.memo, Is.Not.Empty);
        Assert.That(result.vendor_id, Is.Not.Zero);
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