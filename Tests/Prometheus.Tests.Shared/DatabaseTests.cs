using Microsoft.EntityFrameworkCore;
using Mysqlx.Expr;
using Prometheus.Database;
using Prometheus.Database.Models;

namespace Prometheus.Tests.Shared;

public class DatabaseTests
{
    private ERPDbContext _Context;
    private User _User;

    private string _ModuleId = Guid.NewGuid().ToString();

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<DbContext>()
                  .UseInMemoryDatabase(Guid.NewGuid().ToString())
                  .Options;

        _Context = new ERPDbContext(options);

        var baseUser = new User()
        {
            first_name = "test",
            last_name = "user",
            email = "test@email.com",
            username = "test",
            password = "password",
            password_salt = "asdasd",
            employee_number = "10001",
            department = 1,
            guid = Guid.NewGuid().ToString(),
            is_admin = true,
            created_by = 1,
            created_on = DateTime.Now,
            updated_by = 1,
            updated_on = DateTime.Now,
        };

        _Context.Users.Add(baseUser);
        _Context.SaveChanges();

        _User = _Context.Users.First();
    }

    [TearDown]
    public void Destroy()
    {
        _Context.Dispose();
    }

    [Test]
    public async Task UserRoles()
    {
        var roleModel1 = new Role()
        {
            name = "ExampleRole",
            created_by = 1,
            created_on = DateTime.Now,
            updated_by = 1,
            updated_on = DateTime.Now
        };

        var roleModel2 = new Role()
        {
            name = "AnotherRole",
            created_by = 1,
            created_on = DateTime.Now,
            updated_by = 1,
            updated_on = DateTime.Now
        };

        await _Context.Roles.AddAsync(roleModel1);
        await _Context.Roles.AddAsync(roleModel2);
        await _Context.SaveChangesAsync();

        var userRole1 = new UserRole()
        {
            role_id = roleModel1.id,
            user_id = _User.id,
            created_by = 1,
            created_on = DateTime.Now,
            updated_by = 1,
            updated_on = DateTime.Now
        };

        var userRole2 = new UserRole()
        {
            role_id = roleModel2.id,
            user_id = _User.id,
            created_by = 1,
            created_on = DateTime.Now,
            updated_by = 1,
            updated_on = DateTime.Now
        };

        await _Context.UserRoles.AddAsync(userRole1);
        await _Context.UserRoles.AddAsync(userRole2);
        await _Context.SaveChangesAsync();

        _User = await _Context.Users.FirstAsync();

        var userRoles = await _Context.UserRoles.Where(m => m.user_id == _User.id).ToListAsync();
        
        Assert.That(userRoles.Count() == 2);
        Assert.That(_User.roles.Count() == 2);
    }

    [Test]
    public async Task RolesAndPermissions_ReadEdit()
    {
        var roleModel = new Role()
        {
            name = "ReadEditModuleRole",
            created_by = 1,
            created_on = DateTime.Now,
            updated_by = 1,
            updated_on = DateTime.Now
        };

        var moduleReadModel = new ModulePermission()
        {
            module_id = _ModuleId,
            permission_name = "module_read",
            internal_permission_name = "module_read",
            module_name = "module",
            read = true,
            edit = false,
            delete = false,
            write = false,
            is_active = true
        };

        var moduleEditModel = new ModulePermission()
        {
            module_id = _ModuleId,
            permission_name = "module_edit",
            internal_permission_name = "module_edit",
            module_name = "module",
            read = false,
            edit = true,
            delete = false,
            write = false,
            is_active = true
        };

        await _Context.Roles.AddAsync(roleModel);
        await _Context.ModulePermissions.AddAsync(moduleReadModel);
        await _Context.ModulePermissions.AddAsync(moduleEditModel);

        await _Context.SaveChangesAsync();

        var role = await _Context.Roles.SingleAsync(m => m.name == roleModel.name);
        var readModulePermission = await _Context.ModulePermissions.SingleAsync(m => m.module_id == _ModuleId && m.permission_name == moduleReadModel.permission_name);
        var editModulePermission = await _Context.ModulePermissions.SingleAsync(m => m.module_id == _ModuleId && m.permission_name == moduleEditModel.permission_name);

        var rolePermissionReadModel = new RolePermission()
        {
            module_permission_id = readModulePermission.id,
            role_id = role.id,
            created_by = 1,
            created_on = DateTime.Now,
            updated_by = 1,
            updated_on = DateTime.Now
        };

        var rolePermissionEditModel = new RolePermission()
        {
            module_permission_id = editModulePermission.id,
            role_id = role.id,
            created_by = 1,
            created_on = DateTime.Now,
            updated_by = 1,
            updated_on = DateTime.Now
        };


        await _Context.RolePermissions.AddAsync(rolePermissionReadModel);
        await _Context.RolePermissions.AddAsync(rolePermissionEditModel);

        await _Context.SaveChangesAsync();

        // Reget the role to populate the permissions
        role = await _Context.Roles.SingleAsync(m => m.name == roleModel.name);

        // Going to test read and edit
        var can_read = role.role_permissions.Select(m => m.module_permission).Where(m => m.is_active == true && m.read == true).Any();
        var can_edit = role.role_permissions.Select(m => m.module_permission).Where(m => m.is_active == true && m.edit == true).Any();
        var can_delete = role.role_permissions.Select(m => m.module_permission).Where(m => m.is_active == true && m.delete == true).Any();
        var can_write = role.role_permissions.Select(m => m.module_permission).Where(m => m.is_active == true && m.write == true).Any();


        Assert.True(can_read);
        Assert.True(can_edit);
        Assert.False(can_delete);
        Assert.False(can_write);
    }

    [Test]
    public async Task RolesAndPermissions_WriteDelete()
    {
        var roleModel = new Role()
        {
            name = "WriteDeleteModuleRole",
            created_by = 1,
            created_on = DateTime.Now,
            updated_by = 1,
            updated_on = DateTime.Now
        };

        var moduleWriteModel = new ModulePermission()
        {
            module_id = _ModuleId,
            permission_name = "module_write",
            internal_permission_name = "module_write",
            module_name = "module",
            read = false,
            edit = false,
            delete = false,
            write = true,
            is_active = true
        };

        var moduleDeleteModel = new ModulePermission()
        {
            module_id = _ModuleId,
            permission_name = "module_delete",
            internal_permission_name = "module_delete",
            module_name = "module",
            read = false,
            edit = false,
            delete = true,
            write = false,
            is_active = true
        };

        await _Context.Roles.AddAsync(roleModel);
        await _Context.ModulePermissions.AddAsync(moduleWriteModel);
        await _Context.ModulePermissions.AddAsync(moduleDeleteModel);

        await _Context.SaveChangesAsync();

        var role = await _Context.Roles.SingleAsync(m => m.name == roleModel.name);
        var writeModulePermission = await _Context.ModulePermissions.SingleAsync(m => m.module_id == _ModuleId && m.permission_name == moduleWriteModel.permission_name);
        var deleteModulePermission = await _Context.ModulePermissions.SingleAsync(m => m.module_id == _ModuleId && m.permission_name == moduleDeleteModel.permission_name);

        var rolePermissionWriteModel = new RolePermission()
        {
            module_permission_id = writeModulePermission.id,
            role_id = role.id,
            created_by = 1,
            created_on = DateTime.Now,
            updated_by = 1,
            updated_on = DateTime.Now
        };

        var rolePermissionDeleteModel = new RolePermission()
        {
            module_permission_id = deleteModulePermission.id,
            role_id = role.id,
            created_by = 1,
            created_on = DateTime.Now,
            updated_by = 1,
            updated_on = DateTime.Now
        };


        await _Context.RolePermissions.AddAsync(rolePermissionWriteModel);
        await _Context.RolePermissions.AddAsync(rolePermissionDeleteModel);

        await _Context.SaveChangesAsync();

        // Reget the role to populate the permissions
        role = await _Context.Roles.SingleAsync(m => m.name == roleModel.name);

        // Going to test read and edit
        var can_read = role.role_permissions.Select(m => m.module_permission).Where(m => m.is_active == true && m.read == true).Any();
        var can_edit = role.role_permissions.Select(m => m.module_permission).Where(m => m.is_active == true && m.edit == true).Any();
        var can_delete = role.role_permissions.Select(m => m.module_permission).Where(m => m.is_active == true && m.delete == true).Any();
        var can_write = role.role_permissions.Select(m => m.module_permission).Where(m => m.is_active == true && m.write == true).Any();


        Assert.False(can_read);
        Assert.False(can_edit);
        Assert.True(can_delete);
        Assert.True(can_write);
    }

    [Test]
    public async Task VendorProducts()
    {
        var address_model = new Address()
        {
            street_address1 = "123 St",
            city = "City",
            state = "TX",
            postal_code = "77777",
            country = "USA",
            created_by = 1,
            created_on = DateTime.Now,
            updated_by = 1,
            updated_on = DateTime.Now
        };

        await _Context.Addresses.AddAsync(address_model);
        await _Context.SaveChangesAsync();

        var vendor_model = new Vendor()
        {
            vendor_name = "Vendor",
            
            phone = "123-456-7890",
            category = "Cat1",
            address_id = address_model.id,
            created_by = 1,
            created_on = DateTime.Now,
            updated_by = 1,
            updated_on = DateTime.Now
        };

        await _Context.Vendors.AddAsync(vendor_model);
        await _Context.SaveChangesAsync();

        var product_model = new Product()
        {
            vendor_id = vendor_model.id,
            product_class = "Class",
            category = "Cat1",
            identifier1 = "SKU-1",
            internal_description = "A product description",
            product_name = "A product",
            created_by = 1,
            created_on = DateTime.Now,
            updated_by = 1,
            updated_on = DateTime.Now
        };

        await _Context.Products.AddAsync(product_model);
        await _Context.SaveChangesAsync();

        var product_attribute1 = new ProductAttribute()
        {
            product_id = product_model.id,
            attribute_name = "Weight",
            attribute_value = "10",
            created_by = 1,
            created_on = DateTime.Now,
            updated_by = 1,
            updated_on = DateTime.Now
        };

        var product_attribute2 = new ProductAttribute()
        {
            product_id = product_model.id,
            attribute_name = "Length",
            attribute_value = "12",
            created_by = 1,
            created_on = DateTime.Now,
            updated_by = 1,
            updated_on = DateTime.Now
        };

        await _Context.ProductAttributes.AddAsync(product_attribute1);
        await _Context.ProductAttributes.AddAsync(product_attribute2);
        await _Context.SaveChangesAsync();

        vendor_model = await _Context.Vendors.SingleAsync(m => m.id == vendor_model.id);        

        Assert.That(vendor_model.products.Count() == 1);
        Assert.That(vendor_model.products.First().attributes.Count() == 2);
    }

    [Test]
    public async Task CustomerAddresses()
    {
        var customer_model = new Customer()
        { 
            customer_name = "Customer",
            phone = "123-456-7890",
            category = "Cat1",
            created_by = 1,
            created_on = DateTime.Now,
            updated_by = 1,
            updated_on = DateTime.Now
        };

        await _Context.Customers.AddAsync(customer_model);
        await _Context.SaveChangesAsync();


        var address_model1 = new Address()
        {
            street_address1 = "123 St",
            city = "City",
            state = "TX",
            postal_code = "77777",
            country = "USA",
            created_by = 1,
            created_on = DateTime.Now,
            updated_by = 1,
            updated_on = DateTime.Now
        };

        var address_model2 = new Address()
        {
            street_address1 = "9999 St",
            city = "Place",
            state = "OK",
            postal_code = "99999",
            country = "USA",
            created_by = 1,
            created_on = DateTime.Now,
            updated_by = 1,
            updated_on = DateTime.Now
        };

        await _Context.Addresses.AddAsync(address_model1);
        await _Context.Addresses.AddAsync(address_model2);
        await _Context.SaveChangesAsync();

        var customer_address_model1 = new CustomerAddress()
        {
            customer_id = customer_model.id,
            address_id = address_model1.id,
            created_by = 1,
            created_on = DateTime.Now,
            updated_by = 1,
            updated_on = DateTime.Now
        };

        var customer_address_model2 = new CustomerAddress()
        {
            customer_id = customer_model.id,
            address_id = address_model2.id,
            created_by = 1,
            created_on = DateTime.Now,
            updated_by = 1,
            updated_on = DateTime.Now
        };

        await _Context.CustomerAddresses.AddAsync(customer_address_model1);
        await _Context.CustomerAddresses.AddAsync(customer_address_model2);
        await _Context.SaveChangesAsync();

        customer_model = await _Context.Customers.FirstAsync();

        Assert.That(customer_model.addresses.Count() == 2);
    }

    [Test]
    public async Task Orders()
    {
        var address_model = new Address()
        {
            street_address1 = "123 St",
            city = "City",
            state = "TX",
            postal_code = "77777",
            country = "USA",
            created_by = 1,
            created_on = DateTime.Now,
            updated_by = 1,
            updated_on = DateTime.Now
        };

        await _Context.Addresses.AddAsync(address_model);
        await _Context.SaveChangesAsync();

        var order_model = new OrderHeader()
        {
            customer_id = 1,
            order_type = "Q",
            ship_to_address_id = address_model.id,
            shipping_method_id = 1,
            pay_method_id = 1,
            order_date = DateOnly.FromDateTime(DateTime.Now),
            required_date = DateOnly.FromDateTime(DateTime.Now.AddDays(1)),
            price = 1,
            tax = 1,
            created_by = 1,
            created_on = DateTime.Now,
            updated_by = 1,
            updated_on = DateTime.Now
        };

        await _Context.Orders.AddAsync(order_model);
        await _Context.SaveChangesAsync();

        var order_line = new OrderLine()
        { 
            order_id = order_model.id,
            product_id = 1,
            quantity = 1,
            line_number = 1,
            unit_price = 10,
            created_by = 1,
            created_on = DateTime.Now,
            updated_by = 1,
            updated_on = DateTime.Now
        };

        await _Context.OrderLines.AddAsync(order_line);
        await _Context.SaveChangesAsync();

        var line_attribute1 = new OrderLineAttribute()
        {
            order_line_id = order_line.id,
            attribute_name = "Weight",
            attribute_value = "10",
            created_by = 1,
            created_on = DateTime.Now,
            updated_by = 1,
            updated_on = DateTime.Now
        };

        var line_attribute2 = new OrderLineAttribute()
        {
            order_line_id = order_line.id,
            attribute_name = "Height",
            attribute_value = "1",
            created_by = 1,
            created_on = DateTime.Now,
            updated_by = 1,
            updated_on = DateTime.Now
        };

        await _Context.OrderLineAttributes.AddAsync(line_attribute1);
        await _Context.OrderLineAttributes.AddAsync(line_attribute2);
        await _Context.SaveChangesAsync();

        order_model = await _Context.Orders.SingleAsync(m => m.id == order_model.id);

        Assert.That(order_model.order_lines.Count() == 1);
        Assert.That(order_model.order_lines[0].attributes.Count() == 2);
    }

    [Test]
    public async Task APInvoices()
    {
        var invoice_header_model = new APInvoiceHeader()
        {
            invoice_number = "1",
            inv_due_date = DateTime.Now.AddDays(30),
            inv_received_date = DateTime.Now.AddDays(-10),
            inv_date = DateTime.Now,
            invoice_total = 100,
            vendor_id = 1,
            memo = "ASDASDSD",
            created_by = 1,
            created_on = DateTime.Now,
            updated_by = 1,
            updated_on = DateTime.Now
        };

        await _Context.APInvoiceHeaders.AddAsync(invoice_header_model);
        await _Context.SaveChangesAsync();

        var invoice_line_model = new APInvoiceLine()
        {
            ap_invoice_header_id = invoice_header_model.id,
            association_is_ar_invoice = false,
            association_is_purchase_order = true,
            association_is_sales_order = false,
            association_object_id = 1,
            gl_account_id = 1,
            description = "",
            line_total = 100,
            association_object_line_id = 1,
            qty_invoiced = 1,
            created_by = 1,
            created_on = DateTime.Now,
            updated_by = 1,
            updated_on = DateTime.Now
        };

        await _Context.APInvoiceLines.AddAsync(invoice_line_model);
        await _Context.SaveChangesAsync();

        invoice_header_model = await _Context.APInvoiceHeaders.SingleAsync(m => m.id == invoice_header_model.id);

        Assert.That(invoice_header_model.ap_invoice_lines.Count() == 1);
    }

    [Test]
    public async Task ARInvoices()
    {
        var order_model = new OrderHeader()
        {
            customer_id = 1,
            ship_to_address_id = 1,
            shipping_method_id = 1,
            pay_method_id = 1,
            order_type = "R",
            order_date = DateOnly.FromDateTime(DateTime.Now),
            required_date = DateOnly.FromDateTime(DateTime.Now.AddDays(1)),
            price = 1,
            tax = 1,
            created_by = 1,
            created_on = DateTime.Now,
            updated_by = 1,
            updated_on = DateTime.Now
        };

        await _Context.Orders.AddAsync(order_model);
        await _Context.SaveChangesAsync();

        var order_line_model = new OrderLine()
        {
            order_id = order_model.id,
            product_id = 1,
            quantity = 10,
            line_number = 1,
            unit_price = 10,
            created_by = 1,
            created_on = DateTime.Now,
            updated_by = 1,
            updated_on = DateTime.Now
        };

        await _Context.OrderLines.AddAsync(order_line_model);
        await _Context.SaveChangesAsync();

        var product_model = new Product()
        {
            vendor_id = 1,
            product_class = "Class",
            category = "Cat1",
            identifier1 = "SKU-1",
            internal_description = "A product description",
            product_name = "A product",
            created_by = 1,
            created_on = DateTime.Now,
            updated_by = 1,
            updated_on = DateTime.Now
        };

        await _Context.Products.AddAsync(product_model);
        await _Context.SaveChangesAsync();

        var invoice_header_model = new ARInvoiceHeader()
        {
            invoice_number = 10001,
            customer_id = 1,
            invoice_date = DateOnly.FromDateTime(DateTime.Now),
            invoice_total = 100,
            order_id = order_model.id,
            tax_percentage = 8.25M,
            is_taxable = true,
            created_by = 1,
            created_on = DateTime.Now,
            updated_by = 1,
            updated_on = DateTime.Now
        };

        await _Context.ARInvoiceHeaders.AddAsync(invoice_header_model);
        await _Context.SaveChangesAsync();

        var invoice_line_model = new ARInvoiceLine()
        {
            ar_invoice_header_id = invoice_header_model.id,
            product_id = product_model.id,
            order_line_id = order_line_model.id,
            order_qty = 10,
            invoice_qty = 10,
            line_total = 100,
            created_by = 1,
            created_on = DateTime.Now,
            updated_by = 1,
            updated_on = DateTime.Now
        };

        await _Context.ARInvoiceLines.AddAsync(invoice_line_model);
        await _Context.SaveChangesAsync();

        invoice_header_model = await _Context.ARInvoiceHeaders.SingleAsync(m => m.id == invoice_header_model.id);

        Assert.That(invoice_header_model.ar_invoice_lines.Count() == 1);
        Assert.NotNull(invoice_header_model.ar_invoice_lines[0].product);
        Assert.NotNull(invoice_header_model.ar_invoice_lines[0].order_line);
    }

    [Test]
    public async Task Shipments()
    {
        var order_model = new OrderHeader()
        {
            customer_id = 1,
            ship_to_address_id = 1,
            shipping_method_id = 1,
            pay_method_id = 1,
            order_type = "R",
            order_date = DateOnly.FromDateTime(DateTime.Now),
            required_date = DateOnly.FromDateTime(DateTime.Now.AddDays(1)),
            price = 1,
            tax = 1,
            created_by = 1,
            created_on = DateTime.Now,
            updated_by = 1,
            updated_on = DateTime.Now
        };

        await _Context.Orders.AddAsync(order_model);
        await _Context.SaveChangesAsync();

        var order_line_model = new OrderLine()
        {
            order_id = order_model.id,
            product_id = 1,
            quantity = 10,
            line_number = 1,
            unit_price = 10,
            created_by = 1,
            created_on = DateTime.Now,
            updated_by = 1,
            updated_on = DateTime.Now
        };

        await _Context.OrderLines.AddAsync(order_line_model);
        await _Context.SaveChangesAsync();

        var address_model = new Address()
        {
            street_address1 = "123 St",
            city = "City",
            state = "TX",
            postal_code = "77777",
            country = "USA",
            created_by = 1,
            created_on = DateTime.Now,
            updated_by = 1,
            updated_on = DateTime.Now
        };

        await _Context.Addresses.AddAsync(address_model);
        await _Context.SaveChangesAsync();

        var shipment_header_model = new ShipmentHeader()
        {
            order_id = order_model.id,
            shipment_number = 10001,
            tax = 10,
            ship_via = "UPS",
            units_shipped = 0,
            units_to_ship = 10,
            ship_attn = "Hello",
            address_id = address_model.id,
            freight_charge_amount = 100,
            created_by = 1,
            created_on = DateTime.Now,
            updated_by = 1,
            updated_on = DateTime.Now
        };

        await _Context.ShipmentHeaders.AddAsync(shipment_header_model);
        await _Context.SaveChangesAsync();

        var shipment_line_model = new ShipmentLine()
        {
            shipment_header_id = shipment_header_model.id,
            order_line_id = order_line_model.id,
            units_shipped = 0,
            units_to_ship = 10,
            created_by = 1,
            created_on = DateTime.Now,
            updated_by = 1,
            updated_on = DateTime.Now
        };

        await _Context.ShipmentLines.AddAsync(shipment_line_model);
        await _Context.SaveChangesAsync();

        shipment_header_model = await _Context.ShipmentHeaders.SingleAsync(m => m.id == shipment_header_model.id);
        
        Assert.NotNull(shipment_header_model.address);
        Assert.That(shipment_header_model.shipment_lines.Count() == 1);
        Assert.NotNull(shipment_header_model.shipment_lines[0].order_line);
    }

    [Test]
    public async Task PurchaseOrders()
    {
        var address_model = new Address()
        {
            street_address1 = "123 St",
            city = "City",
            state = "TX",
            postal_code = "77777",
            country = "USA",
            created_by = 1,
            created_on = DateTime.Now,
            updated_by = 1,
            updated_on = DateTime.Now
        };

        await _Context.Addresses.AddAsync(address_model);
        await _Context.SaveChangesAsync();

        var vendor_model = new Vendor()
        {
            vendor_name = "Vendor",
            address_id = address_model.id,
            phone = "123-456-7890",
            category = "Cat1",
            created_by = 1,
            created_on = DateTime.Now,
            updated_by = 1,
            updated_on = DateTime.Now
        };

        await _Context.Vendors.AddAsync(vendor_model);
        await _Context.SaveChangesAsync();

        var product_model = new Product()
        {
            vendor_id = vendor_model.id,
            product_class = "Class",
            category = "Cat1",
            identifier1 = "SKU-1",
            internal_description = "A product description",
            product_name = "A product",
            created_by = 1,
            created_on = DateTime.Now,
            updated_by = 1,
            updated_on = DateTime.Now
        };

        await _Context.Products.AddAsync(product_model);
        await _Context.SaveChangesAsync();

        var purchase_order_model = new PurchaseOrderHeader()
        {
            vendor_id = vendor_model.id,
            po_number = 10002,
            po_type = "R",
            revision_number = 2,
            created_by = 1,
            created_on = DateTime.Now,
            updated_by = 1,
            updated_on = DateTime.Now
        };

        await _Context.PurchaseOrderHeaders.AddAsync(purchase_order_model);
        await _Context.SaveChangesAsync();

        var purchase_order_line = new PurchaseOrderLine()
        {
            purchase_order_header_id = purchase_order_model.id,
            product_id = product_model.id,
            description = "Some product description",
            quantity = 12,
            unit_price = 10.2M,
            line_number = 1,
            revision_number = 1,
            is_taxable = true,
            tax = 1.9M,
            created_by = 1,
            created_on = DateTime.Now,
            updated_by = 1,
            updated_on = DateTime.Now
        };

        await _Context.PurchaseOrderLines.AddAsync(purchase_order_line);
        await _Context.SaveChangesAsync();

        purchase_order_model = await _Context.PurchaseOrderHeaders.SingleAsync(m => m.id == purchase_order_model.id);

        Assert.NotNull(purchase_order_model.vendor);
        Assert.That(purchase_order_model.purchase_order_lines.Count() == 1);
        Assert.NotNull(purchase_order_model.purchase_order_lines[0].product);
    }

    [Test]
    public async Task PurchaseOrderReceive()
    {
        var purchase_order_model = new PurchaseOrderHeader()
        {
            vendor_id = 1,
            po_number = 10002,
            po_type = "R",
            revision_number = 2,
            created_by = 1,
            created_on = DateTime.Now,
            updated_by = 1,
            updated_on = DateTime.Now
        };

        await _Context.PurchaseOrderHeaders.AddAsync(purchase_order_model);
        await _Context.SaveChangesAsync();

        var purchase_order_line = new PurchaseOrderLine()
        {
            purchase_order_header_id = purchase_order_model.id,
            product_id = 1,
            description = "Some product description",
            quantity = 12,
            unit_price = 10.2M,
            line_number = 1,
            revision_number = 1,
            is_taxable = true,
            tax = 1.9M,
            created_by = 1,
            created_on = DateTime.Now,
            updated_by = 1,
            updated_on = DateTime.Now
        };

        await _Context.PurchaseOrderLines.AddAsync(purchase_order_line);
        await _Context.SaveChangesAsync();


        var purchase_order_receive_model = new PurchaseOrderReceiveHeader()
        {
            purchase_order_id = purchase_order_line.id,
            units_ordered = 10,
            units_received = 2,
            created_by = 1,
            created_on = DateTime.Now,
            updated_by = 1,
            updated_on = DateTime.Now
        };

        await _Context.PurchaseOrderReceiveHeaders.AddAsync(purchase_order_receive_model);
        await _Context.SaveChangesAsync();

        var purchase_order_receive_line = new PurchaseOrderReceiveLine()
        {
            purchase_order_receive_header_id = purchase_order_receive_model.id,
            purchase_order_line_id = purchase_order_line.id,
            units_ordered = 10,
            units_received = 2,
            created_by = 1,
            created_on = DateTime.Now,
            updated_by = 1,
            updated_on = DateTime.Now
        };

        await _Context.PurchaseOrderReceiveLines.AddAsync(purchase_order_receive_line);
        await _Context.SaveChangesAsync();


        purchase_order_receive_model = await _Context.PurchaseOrderReceiveHeaders.SingleAsync(m => m.id == purchase_order_receive_model.id);

        Assert.NotNull(purchase_order_receive_model.purchase_order);
        Assert.That(purchase_order_receive_model.purchase_order_receive_lines.Count() == 1);
        Assert.NotNull(purchase_order_receive_model.purchase_order_receive_lines[0].purchase_order_line);
    }

    [Test]
    public async Task Opportunities()
    {
        var customer_model = new Customer()
        {
            customer_name = "Customer",
            phone = "123-456-7890",
            category = "Cat1",
            created_by = 1,
            created_on = DateTime.Now,
            updated_by = 1,
            updated_on = DateTime.Now
        };

        await _Context.Customers.AddAsync(customer_model);
        await _Context.SaveChangesAsync();

        var contact_model = new Contact()
        {
            customer_id = customer_model.id,
            first_name = "Bob",
            last_name = "Guy",
            title = "CEO",
            email = "test@test.com",
            cell_phone = "555-555-5555",
            created_by = 1,
            created_on = DateTime.Now,
            updated_by = 1,
            updated_on = DateTime.Now
        };

        await _Context.Contacts.AddAsync(contact_model);
        await _Context.SaveChangesAsync();

        var opportunity_model = new Opportunity()
        {
            contact_id = contact_model.id,
            customer_id = customer_model.id,
            opportunity_name = "Opportunity 1",
            amount = 100,
            win_chance = 50,
            stage = "Prospect",
            expected_close = DateOnly.FromDateTime(DateTime.Now),
            created_by = 1,
            created_on = DateTime.Now,
            updated_by = 1,
            updated_on = DateTime.Now
        };

        await _Context.Opportunities.AddAsync(opportunity_model);
        await _Context.SaveChangesAsync();

        var oppotunity_line = new OpportunityLine()
        {
            opportunity_id = opportunity_model.id,
            description = "A description line",
            unit_price = 100.20M,
            line_number = 1,
            quantity = 1,
            created_by = 1,
            created_on = DateTime.Now,
            updated_by = 1,
            updated_on = DateTime.Now
        };

        await _Context.OpportunityLines.AddAsync(oppotunity_line);
        await _Context.SaveChangesAsync();

        opportunity_model = await _Context.Opportunities.SingleAsync(m => m.id == opportunity_model.id);

        Assert.NotNull(opportunity_model.customer);
        Assert.NotNull(opportunity_model.contact);
        Assert.That(opportunity_model.opportunity_lines.Count() == 1);
    }

    [Test]
    public async Task Documents()
    {
        var document_upload_object = new DocumentUploadObject()
        {
            friendly_name = "AP Invoice",
            internal_name = "ap_invoice",
            requires_approval = false
        };

        await _Context.DocumentUploadObjects.AddAsync(document_upload_object);
        await _Context.SaveChangesAsync();

        var document_upload_tag = new DocumentUploadObjectTagTemplate()
        {
            document_object_id = document_upload_object.id,
            name = "PO Number"
        };

        await _Context.DocumentUploadObjectTags.AddAsync(document_upload_tag);
        await _Context.SaveChangesAsync();

        document_upload_object = await _Context.DocumentUploadObjects.SingleAsync(m => m.id == document_upload_object.id);

        Assert.That(document_upload_object.object_tags.Count() == 1);

        var document_upload = new DocumentUpload()
        {
            document_object_id = document_upload_object.id,
            rev_num = 1,
            created_by = 1,
            created_on = DateTime.Now,
            updated_by = 1,
            updated_on = DateTime.Now
        };

        await _Context.DocumentUploads.AddAsync(document_upload);
        await _Context.SaveChangesAsync();

        var document_revision = new DocumentUploadRevision()
        {
            document_upload_id = document_upload_object.id,
            document_name = "Document.jpg",
            document_path = "https://asdkasdjasdk.com/Document.jpg",
            rev_num = 1,
            created_by = 1,
            created_on = DateTime.Now,
            updated_by = 1,
            updated_on = DateTime.Now
        };

        await _Context.DocumentUploadRevisions.AddAsync(document_revision);
        await _Context.SaveChangesAsync();

        var document_revision_tag = new DocumentUploadRevisionTag()
        {
            document_upload_revision_id = document_revision.id,
            document_upload_object_tag_id = document_upload_tag.id,
            tag_name = "PO Number",
            tag_value = "123442",
            created_by = 1,
            created_on = DateTime.Now,
            updated_by = 1,
            updated_on = DateTime.Now
        };

        await _Context.DocumentUploadRevisionsTags.AddAsync(document_revision_tag);
        await _Context.SaveChangesAsync();

        document_upload = await _Context.DocumentUploads.SingleAsync(m => m.id == document_upload.id);

        Assert.That(document_upload.document_revisions.Count() == 1);
        Assert.That(document_upload.document_revisions[0].revision_tags.Count() == 1);
    }
}