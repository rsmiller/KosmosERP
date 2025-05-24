using System.Data;
using Microsoft.EntityFrameworkCore;
using KosmosERP.Models;
using KosmosERP.Models.Permissions;
using KosmosERP.Tests.Modules.Shared;
using KosmosERP.BusinessLayer.Helpers;
using KosmosERP.BusinessLayer.Modules;
using KosmosERP.Database.Models;
using KosmosERP.BusinessLayer.Models.Module.PurchaseOrder.Command.Create;
using KosmosERP.BusinessLayer.Models.Module.PurchaseOrder.Command.Edit;
using KosmosERP.BusinessLayer.Models.Module.PurchaseOrder.Command.Delete;
using KosmosERP.BusinessLayer.Models.Module.PurchaseOrder.Command.Find;
using KosmosERP.BusinessLayer.Models.Module.PurchaseOrder.Dto;
using KosmosERP.BusinessLayer.MessagePublisher;

namespace KosmosERP.Tests.Modules;

public class PurchaseOrderModuleTests : BaseTestModule<PurchaseOrderModule>, IModuleTest
{
    private Address _Address;
    private Vendor _Vendor;
    private Product _Product;

    [SetUp]
    public async Task SetupModule()
    {
        var the_module = new PurchaseOrderModule(base._Context, new MockMessagePublisher(new MessagePublisherSettings()));

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
        var read_permission = await _Context.ModulePermissions.FirstAsync(m => m.module_id == _Module.ModuleIdentifier.ToString() && m.internal_permission_name == PurchaseOrderPermissions.Read);
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
        var create_permission = await _Context.ModulePermissions.FirstAsync(m => m.module_id == _Module.ModuleIdentifier.ToString() && m.internal_permission_name == PurchaseOrderPermissions.Create);
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
        var edit_permission = await _Context.ModulePermissions.FirstAsync(m => m.module_id == _Module.ModuleIdentifier.ToString() && m.internal_permission_name == PurchaseOrderPermissions.Edit);
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
        var delete_permission = await _Context.ModulePermissions.FirstAsync(m => m.module_id == _Module.ModuleIdentifier.ToString() && m.internal_permission_name == PurchaseOrderPermissions.Delete);
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
            category = "Raw Material",
            sales_price = 100,
            list_price = 150,
            product_class = "Copper",
            product_name = "100ft Copper Wire",
            internal_description = "This is copper",
            external_description = "This is copper",
            identifier1 = "RM-100-C",
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
    }

    [Test]
    public async Task Get()
    {
        var new_result = await _Module.Create(new PurchaseOrderHeaderCreateCommand()
        {
            calling_user_id = 1,
            token = _SessionId,
            po_type = "Build Material",
            vendor_id = _Vendor.id,
            purchase_order_lines = new List<PurchaseOrderLineCreateCommand>() {
                new PurchaseOrderLineCreateCommand()
                {
                    description = "Copper",
                    product_id = _Product.id,
                    quantity = 1,
                    unit_price = 10,
                    line_number = 1,
                    tax = 1,
                    is_taxable = true
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
        var result = await _Module.Create(new PurchaseOrderHeaderCreateCommand()
        {
            calling_user_id = 1,
            token = _SessionId,
            po_type = "Build Material",
            vendor_id = _Vendor.id,
            purchase_order_lines = new List<PurchaseOrderLineCreateCommand>() {
                new PurchaseOrderLineCreateCommand()
                {
                    description = "Copper",
                    product_id = _Product.id,
                    quantity = 1,
                    unit_price = 10,
                    line_number = 1,
                    tax = 1,
                    is_taxable = true
                }
            }
        });

        ValidateMostDtoFields(result);

        Assert.IsNotNull(result.Data);
        Assert.IsNotNull(result.Data.vendor_name);
        Assert.That(result.Data.purchase_order_lines.Count() == 1);
        Assert.IsNotNull(result.Data.purchase_order_lines[0].product_name);
    }

    [Test]
    public async Task Edit()
    {
        var old_result = await _Module.Create(new PurchaseOrderHeaderCreateCommand()
        {
            calling_user_id = 1,
            token = _SessionId,
            po_type = "Build Material",
            vendor_id = _Vendor.id,
            purchase_order_lines = new List<PurchaseOrderLineCreateCommand>() {
                new PurchaseOrderLineCreateCommand()
                {
                    description = "Copper",
                    product_id = _Product.id,
                    quantity = 1,
                    unit_price = 10,
                    line_number = 1,
                    tax = 1,
                    is_taxable = true
                }
            }
        });

        var edit_command = new PurchaseOrderHeaderEditCommand()
        {
            calling_user_id = 1,
            token = _SessionId,
            id = old_result.Data.id,
            po_type = "Office Use",
            vendor_id = _Vendor.id,
            purchase_order_lines = new List<PurchaseOrderLineEditCommand>() {
                new PurchaseOrderLineEditCommand()
                {
                    calling_user_id = 1,
                    token = _SessionId,
                    description = "Paper",
                    product_id = _Product.id,
                    quantity = 1,
                    unit_price = 10,
                    line_number = 1,
                    tax = 1,
                    is_taxable = true
                }
            }
        };

        var result = await _Module.Edit(edit_command);

        ValidateMostDtoFields(result);

        Assert.That(result.Data.po_type == edit_command.po_type);
        Assert.That(result.Data.purchase_order_lines.Count() == 2);
        Assert.IsNotNull(result.Data.vendor_name);
        Assert.IsNotNull(result.Data.purchase_order_lines[0].product_name);
    }

    [Test]
    public async Task Delete()
    {
        var new_result = await _Module.Create(new PurchaseOrderHeaderCreateCommand()
        {
            calling_user_id = 1,
            token = _SessionId,
            po_type = "Build Material",
            vendor_id = _Vendor.id,
            purchase_order_lines = new List<PurchaseOrderLineCreateCommand>() {
                new PurchaseOrderLineCreateCommand()
                {
                    description = "Copper",
                    product_id = _Product.id,
                    quantity = 1,
                    unit_price = 10,
                    line_number = 1,
                    tax = 1,
                    is_taxable = true
                }
            }
        });

        Assert.IsTrue(new_result.Success);
        Assert.IsNotNull(new_result.Data);

        var delete_result = await _Module.Delete(new PurchaseOrderHeaderDeleteCommand()
        {
            calling_user_id = 1,
            token = _SessionId,
            id = new_result.Data.id
        });

        Assert.IsTrue(delete_result.Success);
        Assert.IsNotNull(delete_result.Data);
        Assert.Zero(delete_result.Data.purchase_order_lines.Count());
        Assert.NotNull(delete_result.Data.deleted_by);
        Assert.NotNull(delete_result.Data.deleted_on);
        Assert.NotNull(delete_result.Data.deleted_on_string);
        Assert.NotNull(delete_result.Data.deleted_on_timezone);
    }

    [Test]
    public async Task Find()
    {
        var new_result = await _Module.Create(new PurchaseOrderHeaderCreateCommand()
        {
            calling_user_id = 1,
            token = _SessionId,
            po_type = "Build Material",
            vendor_id = _Vendor.id,
            purchase_order_lines = new List<PurchaseOrderLineCreateCommand>() {
                new PurchaseOrderLineCreateCommand()
                {
                    description = "Copper",
                    product_id = _Product.id,
                    quantity = 1,
                    unit_price = 10,
                    line_number = 1,
                    tax = 1,
                    is_taxable = true
                }
            }
        });

        Assert.IsTrue(new_result.Success);
        Assert.IsNotNull(new_result.Data);

        ValidateMostDtoFields(new_result);

        var results = await _Module.Find(
                        new PagingSortingParameters() { ResultCount = 20, Start = 0 },
                        new PurchaseOrderHeaderFindCommand() { calling_user_id = 1, token = _SessionId, wildcard = new_result.Data.po_number.ToString() });

        Assert.IsTrue(results.Success);
        Assert.IsNotNull(results.Data);
        Assert.NotZero(results.Data.Count());

        var first_result = results.Data.First();

        ValidateMostListFields(first_result);
    }


    [Test]
    public async Task CreateLine()
    {
        var create_result = await _Module.Create(new PurchaseOrderHeaderCreateCommand()
        {
            calling_user_id = 1,
            token = _SessionId,
            po_type = "Build Material",
            vendor_id = _Vendor.id,
            purchase_order_lines = new List<PurchaseOrderLineCreateCommand>() {
                new PurchaseOrderLineCreateCommand()
                {
                    description = "Copper",
                    product_id = _Product.id,
                    quantity = 1,
                    unit_price = 10,
                    line_number = 1,
                    tax = 1,
                    is_taxable = true
                }
            }
        });

        Assert.IsTrue(create_result.Success);
        Assert.IsNotNull(create_result.Data);
        Assert.NotZero(create_result.Data.purchase_order_lines.Count());


        var create_command = new PurchaseOrderLineCreateCommand()
        {
            calling_user_id = 1,
            token = _SessionId,
            description = "Copper",
            product_id = _Product.id,
            quantity = 12,
            unit_price = 101,
            line_number = 2,
            tax = 11,
            is_taxable = true
        };

        var create_line_response = await _Module.CreateLine(create_command);


        Assert.IsTrue(create_line_response.Success);
        Assert.IsNotNull(create_line_response.Data);
        Assert.That(create_line_response.Data.line_number == create_command.line_number);
        Assert.That(create_line_response.Data.quantity == create_command.quantity);
        Assert.That(create_line_response.Data.is_taxable == create_command.is_taxable);
        Assert.That(create_line_response.Data.product_id == create_command.product_id);
        Assert.That(create_line_response.Data.tax == create_command.tax);
        Assert.IsNotNull(create_line_response.Data.product_name);
    }

    [Test]
    public async Task EditLine()
    {
        var create_result = await _Module.Create(new PurchaseOrderHeaderCreateCommand()
        {
            calling_user_id = 1,
            token = _SessionId,
            po_type = "Build Material",
            vendor_id = _Vendor.id,
            purchase_order_lines = new List<PurchaseOrderLineCreateCommand>() {
                new PurchaseOrderLineCreateCommand()
                {
                    description = "Copper",
                    product_id = _Product.id,
                    quantity = 1,
                    unit_price = 10,
                    line_number = 1,
                    tax = 1,
                    is_taxable = true
                }
            }
        });

        Assert.IsTrue(create_result.Success);
        Assert.IsNotNull(create_result.Data);
        Assert.NotZero(create_result.Data.purchase_order_lines.Count());


        var edit_command = new PurchaseOrderLineEditCommand()
        {
            calling_user_id = 1,
            token = _SessionId,
            id = create_result.Data.purchase_order_lines[0].id,
            description = "Copper",
            product_id = _Product.id,
            quantity = 22,
            unit_price = 120,
            line_number = 11,
            tax = 11,
            is_taxable = true
        };

        var edit_line_response = await _Module.EditLine(edit_command);


        Assert.IsTrue(edit_line_response.Success);
        Assert.IsNotNull(edit_line_response.Data);
        Assert.That(edit_line_response.Data.line_number == edit_command.line_number);
        Assert.That(edit_line_response.Data.description == edit_command.description);
        Assert.That(edit_line_response.Data.is_taxable == edit_command.is_taxable);
        Assert.That(edit_line_response.Data.quantity == edit_command.quantity);
        Assert.That(edit_line_response.Data.unit_price == edit_command.unit_price);
        Assert.That(edit_line_response.Data.tax == edit_command.tax);
        Assert.IsNotNull(edit_line_response.Data.product_name);
    }


    [Test]
    public async Task DeleteLine()
    {
        var create_result = await _Module.Create(new PurchaseOrderHeaderCreateCommand()
        {
            calling_user_id = 1,
            token = _SessionId,
            po_type = "Build Material",
            vendor_id = _Vendor.id,
            purchase_order_lines = new List<PurchaseOrderLineCreateCommand>() {
                new PurchaseOrderLineCreateCommand()
                {
                    description = "Copper",
                    product_id = _Product.id,
                    quantity = 1,
                    unit_price = 10,
                    line_number = 1,
                    tax = 1,
                    is_taxable = true
                }
            }
        });

        Assert.IsTrue(create_result.Success);
        Assert.IsNotNull(create_result.Data);
        Assert.NotZero(create_result.Data.purchase_order_lines.Count());

        var response = await _Module.DeleteLine(new PurchaseOrderLineDeleteCommand()
        {
            calling_user_id = 1,
            token = _SessionId,
            id = create_result.Data.purchase_order_lines[0].id,
        });


        Assert.IsTrue(response.Success);
        Assert.IsNotNull(response.Data);
        Assert.NotNull(response.Data.deleted_by);
        Assert.NotNull(response.Data.deleted_on);
        Assert.NotNull(response.Data.deleted_on_string);
        Assert.NotNull(response.Data.deleted_on_timezone);
    }


    private void ValidateMostDtoFields(Response<PurchaseOrderHeaderDto> result)
    {
        Assert.IsTrue(result.Success);
        Assert.IsNotNull(result.Data);
        Assert.NotZero(result.Data.id);
        Assert.IsNotEmpty(result.Data.guid);
        Assert.NotZero(result.Data.revision_number);
        Assert.NotNull(result.Data.po_number);
        Assert.NotNull(result.Data.po_type);
        Assert.NotZero(result.Data.vendor_id);
        Assert.NotZero(result.Data.created_by);
        Assert.NotNull(result.Data.created_on);
        Assert.NotNull(result.Data.created_on_string);
        Assert.NotNull(result.Data.created_on_timezone);
        Assert.NotNull(result.Data.updated_by);
        Assert.NotNull(result.Data.updated_on);
        Assert.NotNull(result.Data.updated_on_string);
        Assert.NotNull(result.Data.updated_on_timezone);
    }

    private void ValidateMostListFields(PurchaseOrderHeaderListDto result)
    {
        Assert.NotZero(result.id);
        Assert.IsNotEmpty(result.guid);
        Assert.NotZero(result.revision_number);
        Assert.NotNull(result.po_number);
        Assert.NotNull(result.po_type);
        Assert.NotZero(result.vendor_id);
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