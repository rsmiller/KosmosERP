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
        var the_module = new ARInvoiceModule(base._Context);

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
        var read_permission = await _Context.ModulePermissions.FirstAsync(m => m.module_id == _Module.ModuleIdentifier.ToString() && m.internal_permission_name == ARInvoicePermissions.Read);
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
        var create_permission = await _Context.ModulePermissions.FirstAsync(m => m.module_id == _Module.ModuleIdentifier.ToString() && m.internal_permission_name == ARInvoicePermissions.Create);
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
        var edit_permission = await _Context.ModulePermissions.FirstAsync(m => m.module_id == _Module.ModuleIdentifier.ToString() && m.internal_permission_name == ARInvoicePermissions.Edit);
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
        var delete_permission = await _Context.ModulePermissions.FirstAsync(m => m.module_id == _Module.ModuleIdentifier.ToString() && m.internal_permission_name == ARInvoicePermissions.Delete);
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
            shipping_method_id = 1,
            pay_method_id = 1,
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
            payment_terms = 1,
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
        var new_result = await _Module.Create(new ARInvoiceHeaderCreateCommand()
        {
            calling_user_id = 1,
            token = _SessionId,
            invoice_date = DateOnly.Parse(DateTime.Now.ToString("MM/dd/yyyy")),
            invoice_due_date = DateOnly.Parse(DateTime.Now.ToString("MM/dd/yyyy")),
            customer_id = _Customer.id,
            order_header_id = _SalesOrderHeader.id,
            payment_terms = 1,
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
                    product_id = _SalesOrderLine.product_id
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
        var result = await _Module.Create(new ARInvoiceHeaderCreateCommand()
        {
            calling_user_id = 1,
            token = _SessionId,
            invoice_date = DateOnly.Parse(DateTime.Now.ToString("MM/dd/yyyy")),
            invoice_due_date = DateOnly.Parse(DateTime.Now.ToString("MM/dd/yyyy")),
            customer_id = _Customer.id,
            order_header_id = _SalesOrderHeader.id,
            payment_terms = 1,
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
                    product_id = _SalesOrderLine.product_id
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
            calling_user_id = 1,
            token = _SessionId,
            invoice_date = DateOnly.Parse(DateTime.Now.ToString("MM/dd/yyyy")),
            invoice_due_date = DateOnly.Parse(DateTime.Now.ToString("MM/dd/yyyy")),
            customer_id = _Customer.id,
            order_header_id = _SalesOrderHeader.id,
            payment_terms = 1,
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
                    product_id = _SalesOrderLine.product_id
                }
            }
        });

        var edit_command = new ARInvoiceHeaderEditCommand()
        {
            calling_user_id = 1,
            token = _SessionId,
            id = old_result.Data.id,
            payment_terms = 2,
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
            calling_user_id = 1,
            token = _SessionId,
            invoice_date = DateOnly.Parse(DateTime.Now.ToString("MM/dd/yyyy")),
            invoice_due_date = DateOnly.Parse(DateTime.Now.ToString("MM/dd/yyyy")),
            customer_id = _Customer.id,
            order_header_id = _SalesOrderHeader.id,
            payment_terms = 1,
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
                    product_id = _SalesOrderLine.product_id
                }
            }
        });

        Assert.IsTrue(new_result.Success);
        Assert.IsNotNull(new_result.Data);

        var delete_result = await _Module.Delete(new ARInvoiceHeaderDeleteCommand()
        {
            calling_user_id = 1,
            token = _SessionId,
            id = new_result.Data.id
        });

        Assert.IsTrue(delete_result.Success);
        Assert.IsNotNull(delete_result.Data);
        Assert.Zero(delete_result.Data.ar_invoice_lines.Count());
        Assert.NotNull(delete_result.Data.deleted_by);
        Assert.NotNull(delete_result.Data.deleted_on);
        Assert.NotNull(delete_result.Data.deleted_on_string);
        Assert.NotNull(delete_result.Data.deleted_on_timezone);
    }

    [Test]
    public async Task Find()
    {
        var new_result = await _Module.Create(new ARInvoiceHeaderCreateCommand()
        {
            calling_user_id = 1,
            token = _SessionId,
            invoice_date = DateOnly.Parse(DateTime.Now.ToString("MM/dd/yyyy")),
            invoice_due_date = DateOnly.Parse(DateTime.Now.ToString("MM/dd/yyyy")),
            customer_id = _Customer.id,
            order_header_id = _SalesOrderHeader.id,
            payment_terms = 1,
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
                    product_id = _SalesOrderLine.product_id
                }
            }
        });

        Assert.IsTrue(new_result.Success);
        Assert.IsNotNull(new_result.Data);

        ValidateMostDtoFields(new_result);

        var results = await _Module.Find(
                        new PagingSortingParameters() { ResultCount = 20, Start = 0 },
                        new ARInvoiceHeaderFindCommand() { calling_user_id = 1, token = _SessionId, wildcard = new_result.Data.invoice_number.ToString() });

        Assert.IsTrue(results.Success);
        Assert.IsNotNull(results.Data);
        Assert.NotZero(results.Data.Count());

        var first_result = results.Data.First();

        ValidateMostListFields(first_result);
    }

    
    [Test]
    public async Task CreateLine()
    {
        var create_result = await _Module.Create(new ARInvoiceHeaderCreateCommand()
        {
            calling_user_id = 1,
            token = _SessionId,
            invoice_date = DateOnly.Parse(DateTime.Now.ToString("MM/dd/yyyy")),
            invoice_due_date = DateOnly.Parse(DateTime.Now.ToString("MM/dd/yyyy")),
            customer_id = _Customer.id,
            order_header_id = _SalesOrderHeader.id,
            payment_terms = 1,
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
                    product_id = _SalesOrderLine.product_id
                }
            }
        });

        Assert.IsTrue(create_result.Success);
        Assert.IsNotNull(create_result.Data);
        Assert.NotZero(create_result.Data.ar_invoice_lines.Count());


        var create_command = new ARInvoiceLineCreateCommand()
        {
            calling_user_id = 1,
            token = _SessionId,
            ar_invoice_header_id = create_result.Data.id,
            order_line_id = _SalesOrderLine.id,
            line_number = 2,
            invoice_qty = 111,
            line_description = "asdasdasd",
            is_taxable = false,
            product_id = _SalesOrderLine.product_id,
        };

        var create_line_response = await _Module.CreateLine(create_command);


        Assert.IsTrue(create_line_response.Success);
        Assert.IsNotNull(create_line_response.Data);
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
            calling_user_id = 1,
            token = _SessionId,
            invoice_date = DateOnly.Parse(DateTime.Now.ToString("MM/dd/yyyy")),
            invoice_due_date = DateOnly.Parse(DateTime.Now.ToString("MM/dd/yyyy")),
            customer_id = _Customer.id,
            order_header_id = _SalesOrderHeader.id,
            payment_terms = 1,
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
                    product_id = _SalesOrderLine.product_id
                }
            }
        });

        Assert.IsTrue(create_result.Success);
        Assert.IsNotNull(create_result.Data);
        Assert.NotZero(create_result.Data.ar_invoice_lines.Count());


        var edit_command = new ARInvoiceLineEditCommand()
        {
            calling_user_id = 1,
            token = _SessionId,
            id = create_result.Data.ar_invoice_lines[0].id,
            line_number = 4,
            is_taxable = false,
            invoice_qty = 23,
            line_description = "ASDASD",
            order_line_id = _SalesOrderLine.id,
            ar_invoice_header_id = _SalesOrderHeader.id,
        };

        var edit_line_response = await _Module.EditLine(edit_command);


        Assert.IsTrue(edit_line_response.Success);
        Assert.IsNotNull(edit_line_response.Data);
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
            calling_user_id = 1,
            token = _SessionId,
            invoice_date = DateOnly.Parse(DateTime.Now.ToString("MM/dd/yyyy")),
            invoice_due_date = DateOnly.Parse(DateTime.Now.ToString("MM/dd/yyyy")),
            customer_id = _Customer.id,
            order_header_id = _SalesOrderHeader.id,
            payment_terms = 1,
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
                    product_id = _SalesOrderLine.product_id
                }
            }
        });

        Assert.IsTrue(create_result.Success);
        Assert.IsNotNull(create_result.Data);
        Assert.NotZero(create_result.Data.ar_invoice_lines.Count());

        var response = await _Module.DeleteLine(new ARInvoiceLineDeleteCommand()
        {
            calling_user_id = 1,
            token = _SessionId,
            id = create_result.Data.ar_invoice_lines[0].id,
        });


        Assert.IsTrue(response.Success);
        Assert.IsNotNull(response.Data);
        Assert.NotNull(response.Data.deleted_by);
        Assert.NotNull(response.Data.deleted_on);
        Assert.NotNull(response.Data.deleted_on_string);
        Assert.NotNull(response.Data.deleted_on_timezone);
    }


    private void ValidateMostDtoFields(Response<ARInvoiceHeaderDto> result)
    {
        Assert.IsTrue(result.Success);
        Assert.IsNotNull(result.Data);
        Assert.NotZero(result.Data.id);
        Assert.IsNotEmpty(result.Data.guid);
        Assert.NotZero(result.Data.invoice_number);
        Assert.NotZero(result.Data.order_header_id);
        Assert.NotNull(result.Data.order_number);
        Assert.NotZero(result.Data.customer_id);
        Assert.IsNotEmpty(result.Data.customer_name);
        Assert.NotZero(result.Data.invoice_total);
        Assert.NotZero(result.Data.created_by);
        Assert.NotNull(result.Data.created_on);
        Assert.NotNull(result.Data.created_on_string);
        Assert.NotNull(result.Data.created_on_timezone);
        Assert.NotNull(result.Data.updated_by);
        Assert.NotNull(result.Data.updated_on);
        Assert.NotNull(result.Data.updated_on_string);
        Assert.NotNull(result.Data.updated_on_timezone);
    }

    private void ValidateMostListFields(ARInvoiceHeaderListDto result)
    {
        Assert.NotZero(result.id);
        Assert.IsNotEmpty(result.guid);
        Assert.NotZero(result.invoice_number);
        Assert.NotZero(result.order_header_id);
        Assert.NotNull(result.order_number);
        Assert.NotZero(result.customer_id);
        Assert.NotZero(result.invoice_total);
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