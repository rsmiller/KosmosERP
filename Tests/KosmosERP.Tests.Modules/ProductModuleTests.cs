using System.Data;
using Microsoft.EntityFrameworkCore;
using KosmosERP.Models;
using KosmosERP.Models.Permissions;
using KosmosERP.Tests.Modules.Shared;
using KosmosERP.BusinessLayer.Helpers;
using KosmosERP.BusinessLayer.Modules;
using KosmosERP.Database.Models;
using KosmosERP.BusinessLayer.Models.Module.Product.Command.Create;
using KosmosERP.BusinessLayer.Models.Module.Product.Command.Edit;
using KosmosERP.BusinessLayer.Models.Module.Product.Command.Delete;
using KosmosERP.BusinessLayer.Models.Module.Product.Command.Find;
using KosmosERP.BusinessLayer.Models.Module.Product.Dto;

namespace KosmosERP.Tests.Modules;

public class ProductModuleTests : BaseTestModule<ProductModule>, IModuleTest
{
    private Vendor _Vendor;

    [SetUp]
    public async Task SetupModule()
    {
        var the_module = new ProductModule(base._Context);

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
        var read_permission = await _Context.ModulePermissions.FirstAsync(m => m.module_id == _Module.ModuleIdentifier.ToString() && m.internal_permission_name == ProductPermissions.Read);
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
        var create_permission = await _Context.ModulePermissions.FirstAsync(m => m.module_id == _Module.ModuleIdentifier.ToString() && m.internal_permission_name == ProductPermissions.Create);
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
        var edit_permission = await _Context.ModulePermissions.FirstAsync(m => m.module_id == _Module.ModuleIdentifier.ToString() && m.internal_permission_name == ProductPermissions.Edit);
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
        var delete_permission = await _Context.ModulePermissions.FirstAsync(m => m.module_id == _Module.ModuleIdentifier.ToString() && m.internal_permission_name == ProductPermissions.Delete);
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
        var vendor = CommonDataHelper<Vendor>.FillCommonFields(new Vendor()
        {
            category = "CAT1",
            fax = "123-123-1234",
            phone = "234-456-2312",
            general_email = "vendor@vendor.com",
            is_deleted = false,
            vendor_name = "Coolest vendor ever",
            vendor_description = "Supplies us with toliet paper",
            website = "google.com",
        }, 1);

        _Context.Vendors.Add(vendor);
        await _Context.SaveChangesAsync();

        _Vendor = vendor;
    }

    [Test]
    public async Task Get()
    {
        var new_result = await _Module.Create(new ProductCreateCommand()
        {
            calling_user_id = 1,
            token = _SessionId,
            product_name = "Some wire",
            product_class = "Copper",
            category = "Spool",
            identifier1 = "SPO-CU-100",
            is_sales_item = true,
            is_material = true,
            internal_description = "A big spool of copper wire used to make stuff",
            our_cost = 1,
            list_price = 5,
            sales_price = 3,
            unit_cost = 1,
            vendor_id = _Vendor.id,
            is_taxable = false,
            product_attributes = new List<ProductAttributeCreateCommand>()
            {
                new ProductAttributeCreateCommand()
                {
                    attribute_name = "Diameter",
                    attribute_value = "1in"
                }
            }
        });

        Assert.IsTrue(new_result.Success);
        Assert.IsNotNull(new_result.Data);

        var result = await _Module.GetDto(new_result.Data.id);

        ValidateMostDtoFields(result);
        Assert.That(result.Data.product_attributes.Count() > 0);
    }

    [Test]
    public async Task Create()
    {
        var result = await _Module.Create(new ProductCreateCommand()
        {
            calling_user_id = 1,
            token = _SessionId,
            product_name = "Some wire",
            product_class = "Copper",
            category = "Spool",
            identifier1 = "SPO-CU-100",
            is_sales_item = true,
            is_material = true,
            internal_description = "A big spool of copper wire used to make stuff",
            our_cost = 1,
            list_price = 5,
            sales_price = 3,
            unit_cost = 1,
            vendor_id = _Vendor.id,
            is_taxable = false,
            product_attributes = new List<ProductAttributeCreateCommand>()
            {
                new ProductAttributeCreateCommand()
                {
                    attribute_name = "Diameter",
                    attribute_value = "1in"
                }
            }
        });

        ValidateMostDtoFields(result);
    }

    [Test]
    public async Task Edit()
    {
        var new_result = await _Module.Create(new ProductCreateCommand()
        {
            calling_user_id = 1,
            token = _SessionId,
            product_name = "Some wire",
            product_class = "Copper",
            category = "Spool",
            identifier1 = "SPO-CU-100",
            is_sales_item = true,
            is_material = true,
            internal_description = "A big spool of copper wire used to make stuff",
            our_cost = 1,
            list_price = 5,
            sales_price = 3,
            unit_cost = 1,
            vendor_id = _Vendor.id,
            is_taxable = false,
            product_attributes = new List<ProductAttributeCreateCommand>()
            {
                new ProductAttributeCreateCommand()
                {
                    attribute_name = "Diameter",
                    attribute_value = "1in"
                }
            }
        });

        Assert.IsTrue(new_result.Success);
        Assert.IsNotNull(new_result.Data);

        var edit_command = new ProductEditCommand()
        {
            calling_user_id = 1,
            token = _SessionId,
            id = new_result.Data.id,
            product_name = "Some wire2",
            product_class = "Copper1",
            category = "Spool2",
            identifier1 = "SPO-CU-1001",
            is_sales_item = true,
            is_material = true,
            internal_description = "A big spool of copper wire used to make stuff22",
            our_cost = 2,
            list_price = 6,
            sales_price = 4,
            unit_cost = 2,
            vendor_id = _Vendor.id,
            is_taxable = true,
            identifier2 = "id1",
            identifier3 = "id3"
        };

        var edit_result = await _Module.Edit(edit_command);

        ValidateMostDtoFields(edit_result);

        Assert.That(edit_result.Data.product_name == edit_command.product_name);
        Assert.That(edit_result.Data.product_class == edit_command.product_class);
        Assert.That(edit_result.Data.category == edit_command.category);
        Assert.That(edit_result.Data.identifier1 == edit_command.identifier1);
        Assert.That(edit_result.Data.identifier2 == edit_command.identifier2);
        Assert.That(edit_result.Data.identifier3 == edit_command.identifier3);
        Assert.That(edit_result.Data.is_sales_item == edit_command.is_sales_item);
        Assert.That(edit_result.Data.is_material == edit_command.is_material);
        Assert.That(edit_result.Data.internal_description == edit_command.internal_description);
        Assert.That(edit_result.Data.our_cost == edit_command.our_cost);
        Assert.That(edit_result.Data.list_price == edit_command.list_price);
        Assert.That(edit_result.Data.sales_price == edit_command.sales_price);
        Assert.That(edit_result.Data.unit_cost == edit_command.unit_cost);
        Assert.That(edit_result.Data.is_taxable == edit_command.is_taxable);
    }

    [Test]
    public async Task Delete()
    {
        var new_result = await _Module.Create(new ProductCreateCommand()
        {
            calling_user_id = 1,
            token = _SessionId,
            product_name = "Some wire",
            product_class = "Copper",
            category = "Spool",
            identifier1 = "SPO-CU-100",
            is_sales_item = true,
            is_material = true,
            internal_description = "A big spool of copper wire used to make stuff",
            our_cost = 1,
            list_price = 5,
            sales_price = 3,
            unit_cost = 1,
            vendor_id = _Vendor.id,
            is_taxable = false,
        });

        Assert.IsTrue(new_result.Success);
        Assert.IsNotNull(new_result.Data);

        var delete_result = await _Module.Delete(new ProductDeleteCommand()
        {
            calling_user_id = 1,
            token = _SessionId,
            id = new_result.Data.id
        });

        Assert.IsTrue(delete_result.Success);
        Assert.IsNotNull(delete_result.Data);
        Assert.NotNull(delete_result.Data.deleted_by);
        Assert.NotNull(delete_result.Data.deleted_on);
        Assert.NotNull(delete_result.Data.deleted_on_string);
        Assert.NotNull(delete_result.Data.deleted_on_timezone);
    }

    [Test]
    public async Task Find()
    {
        var new_result = await _Module.Create(new ProductCreateCommand()
        {
            calling_user_id = 1,
            token = _SessionId,
            product_name = "Some wire",
            product_class = "Copper",
            category = "Spool",
            identifier1 = "SPO-CU-100",
            is_sales_item = true,
            is_material = true,
            internal_description = "A big spool of copper wire used to make stuff",
            our_cost = 1,
            list_price = 5,
            sales_price = 3,
            unit_cost = 1,
            vendor_id = _Vendor.id,
            is_taxable = false,
        });

        Assert.IsTrue(new_result.Success);
        Assert.IsNotNull(new_result.Data);

        var results = await _Module.Find(
                        new PagingSortingParameters() { ResultCount = 20, Start = 0 },
                        new ProductFindCommand() { calling_user_id = 1, token = _SessionId, wildcard = "PO-CU-100" });
        
        Assert.IsTrue(results.Success);
        Assert.IsNotNull(results.Data);
        Assert.NotZero(results.Data.Count());

        var first_result = results.Data.First();

        ValidateMostListFields(first_result);
    }

    [Test]
    public async Task CreateAttribute()
    {
        var result = await _Module.Create(new ProductCreateCommand()
        {
            calling_user_id = 1,
            token = _SessionId,
            product_name = "Some wire",
            product_class = "Copper",
            category = "Spool",
            identifier1 = "SPO-CU-100",
            is_sales_item = true,
            is_material = true,
            internal_description = "A big spool of copper wire used to make stuff",
            our_cost = 1,
            list_price = 5,
            sales_price = 3,
            unit_cost = 1,
            vendor_id = _Vendor.id,
            is_taxable = false,
            product_attributes = new List<ProductAttributeCreateCommand>()
            {
                new ProductAttributeCreateCommand()
                {
                    attribute_name = "Diameter",
                    attribute_value = "1in"
                }
            }
        });

        Assert.IsTrue(result.Success);
        Assert.IsNotNull(result.Data);

        var attribute_result = await _Module.CreateAttribute(new ProductAttributeCreateCommand()
        {
            calling_user_id = 1,
            token = _SessionId,
            attribute_name = "Length",
            attribute_value = "100000",
            product_id = result.Data.id
        });


        Assert.IsTrue(attribute_result.Success);
        Assert.IsNotNull(attribute_result.Data);
    }

    [Test]
    public async Task EditAttribute()
    {
        var result = await _Module.Create(new ProductCreateCommand()
        {
            calling_user_id = 1,
            token = _SessionId,
            product_name = "Some wire",
            product_class = "Copper",
            category = "Spool",
            identifier1 = "SPO-CU-100",
            is_sales_item = true,
            is_material = true,
            internal_description = "A big spool of copper wire used to make stuff",
            our_cost = 1,
            list_price = 5,
            sales_price = 3,
            unit_cost = 1,
            vendor_id = _Vendor.id,
            is_taxable = false,
            product_attributes = new List<ProductAttributeCreateCommand>()
            {
                new ProductAttributeCreateCommand()
                {
                    attribute_name = "Diameter",
                    attribute_value = "1in"
                }
            }
        });

        Assert.IsTrue(result.Success);
        Assert.IsNotNull(result.Data);
        Assert.NotZero(result.Data.product_attributes.Count());

        var attribute_to_edit = result.Data.product_attributes[0];


        var edit_command = new ProductAttributeEditCommand()
        {
            calling_user_id = 1,
            token = _SessionId,
            id = attribute_to_edit.id,
            attribute_name = "Length",
            attribute_value = "100000",
        };

        var attribute_result = await _Module.EditAttribute(edit_command);


        Assert.IsTrue(attribute_result.Success);
        Assert.IsNotNull(attribute_result.Data);
        Assert.That(attribute_result.Data.attribute_name == edit_command.attribute_name);
        Assert.That(attribute_result.Data.attribute_value == edit_command.attribute_value);
    }

    [Test]
    public async Task DeleteAttribute()
    {
        var result = await _Module.Create(new ProductCreateCommand()
        {
            calling_user_id = 1,
            token = _SessionId,
            product_name = "Some wire",
            product_class = "Copper",
            category = "Spool",
            identifier1 = "SPO-CU-100",
            is_sales_item = true,
            is_material = true,
            internal_description = "A big spool of copper wire used to make stuff",
            our_cost = 1,
            list_price = 5,
            sales_price = 3,
            unit_cost = 1,
            vendor_id = _Vendor.id,
            is_taxable = false,
            product_attributes = new List<ProductAttributeCreateCommand>()
            {
                new ProductAttributeCreateCommand()
                {
                    attribute_name = "Diameter",
                    attribute_value = "1in"
                }
            }
        });

        Assert.IsTrue(result.Success);
        Assert.IsNotNull(result.Data);
        Assert.NotZero(result.Data.product_attributes.Count());

        var attribute_to_edit = result.Data.product_attributes[0];

        var attribute_result = await _Module.DeleteAttribute(new ProductAttributeDeleteCommand()
        {
            calling_user_id = 1,
            token = _SessionId,
            id = attribute_to_edit.id,
        });


        Assert.IsTrue(attribute_result.Success);
        Assert.IsNotNull(attribute_result.Data);
    }

    private void ValidateMostDtoFields(Response<ProductDto> result)
    {
        Assert.IsTrue(result.Success);
        Assert.IsNotNull(result.Data);
        Assert.NotZero(result.Data.id);
        Assert.IsNotEmpty(result.Data.guid);
        Assert.IsNotEmpty(result.Data.product_name);
        Assert.IsNotEmpty(result.Data.category);
        Assert.IsNotEmpty(result.Data.identifier1);
        Assert.IsNotEmpty(result.Data.internal_description);
        Assert.NotZero(result.Data.vendor_id);
        Assert.NotZero(result.Data.list_price);
        Assert.NotZero(result.Data.sales_price);
        Assert.NotZero(result.Data.unit_cost);
        Assert.IsNotEmpty(result.Data.product_class);
        Assert.NotZero(result.Data.created_by);
        Assert.NotNull(result.Data.created_on);
        Assert.NotNull(result.Data.created_on_string);
        Assert.NotNull(result.Data.created_on_timezone);
        Assert.NotNull(result.Data.updated_by);
        Assert.NotNull(result.Data.updated_on);
        Assert.NotNull(result.Data.updated_on_string);
        Assert.NotNull(result.Data.updated_on_timezone);
    }

    private void ValidateMostListFields(ProductListDto result)
    {
        Assert.NotZero(result.id);
        Assert.IsNotEmpty(result.guid);
        Assert.IsNotEmpty(result.product_name);
        Assert.IsNotEmpty(result.category);
        Assert.IsNotEmpty(result.identifier1);
        Assert.IsNotEmpty(result.internal_description);
        Assert.NotZero(result.vendor_id);
        Assert.NotZero(result.list_price);
        Assert.NotZero(result.sales_price);
        Assert.NotZero(result.unit_cost);
        Assert.IsNotEmpty(result.product_class);
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