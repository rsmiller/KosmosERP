using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
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
using KosmosERP.BusinessLayer;

namespace KosmosERP.Tests.Modules;

public class ProductModuleTests : BaseTestModule<ProductModule>, IModuleTest
{
    private Vendor _Vendor;

    [SetUp]
    public async Task SetupModule()
    {
        Console.WriteLine("ProductModuleTests.SetupModule called");
        var logProviderFactory = new LogProviderFactory(new LogProviderSettings() { log_provider = LogProviderType.MOCK }, _Context, null);
        var the_module = new ProductModule(base._Context, 
                                        new MessageFactory(new MessagePublisherSettings() { account_provider = MessagePublisherType.MOCK, transaction_movement_topic = "test" }, base._Context), 
                                        new MessagePublisherSettings{ account_provider = MessagePublisherType.Database, transaction_movement_topic = "test" },
                                        new MemoryCacheService<KeyValueStore>(new MemoryCache(new MemoryCacheOptions()), base._Context),
                                        logProviderFactory);

        await base.SetupModule(the_module);
        Console.WriteLine("ProductModuleTests.SetupModule completed");
        
        // Debug: Check if the setup actually worked
        var roleCount = await _Context.Roles.CountAsync();
        var permissionCount = await _Context.ModulePermissions.CountAsync();
        var rolePermissionCount = await _Context.RolePermissions.CountAsync();
        
        Console.WriteLine($"After setup - Roles: {roleCount}, ModulePermissions: {permissionCount}, RolePermissions: {rolePermissionCount}");
        
        // This will fail if setup didn't work
        Assert.That(roleCount, Is.GreaterThan(0), "No roles were created");
        Assert.That(permissionCount, Is.GreaterThan(0), "No module permissions were created");
        Assert.That(rolePermissionCount, Is.GreaterThan(0), "No role permissions were created");
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
        
        Console.WriteLine($"Created role: {admin_role.name} with ID {admin_role.id}, assigned to user {_User.id}");
    }

    protected override async Task SetupReadPermissions()
    {
        var role = await _Context.Roles.Where(m => m.name == "Module Admin").FirstAsync();

        var role_module_permission = CommonDataHelper<RolePermission>.FillCommonFields(new RolePermission()
        {
            module_id = _Module.ModuleIdentifier.ToString(),
            role_id = role.id,
            read = true,
        }, 1);

        _Context.RolePermissions.Add(role_module_permission);
        await _Context.SaveChangesAsync();
    }

    protected override async Task SetupCreatePermissions()
    {
        var role = await _Context.Roles.Where(m => m.name == "Module Admin").FirstAsync();

        Console.WriteLine($"Role ID: {role.id}");

        var role_module_permission = CommonDataHelper<RolePermission>.FillCommonFields(new RolePermission()
        {
            module_id = _Module.ModuleIdentifier.ToString(),
            role_id = role.id,
            write = true,
        }, 1);

        _Context.RolePermissions.Add(role_module_permission);
        await _Context.SaveChangesAsync();
        
        Console.WriteLine($"Added role permission: module_id={_Module.ModuleIdentifier}, role_id={role.id}, write=true");
    }

    protected override async Task SetupEditPermissions()
    {
        var role = await _Context.Roles.Where(m => m.name == "Module Admin").FirstAsync();

        var role_module_permission = CommonDataHelper<RolePermission>.FillCommonFields(new RolePermission()
        {
            module_id = _Module.ModuleIdentifier.ToString(),
            role_id = role.id,
            edit = true,
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
            delete = true,
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
            calling_user_id = _User.external_id,
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

        if (!new_result.Success)
        {
            Console.WriteLine($"Create failed: {new_result.Exception}");
        }
        Assert.That(new_result.Success, Is.True);
        Assert.That(new_result.Data, Is.Not.Null);

        var result = await _Module.GetDto(new_result.Data.id);

        ValidateMostDtoFields(result);
        Assert.That(result.Data.product_attributes.Count() > 0);
    }

    [Test]
    public async Task Create()
    {
        var result = await _Module.Create(new ProductCreateCommand()
        {
            calling_user_id = _User.external_id,
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

        if (!result.Success)
        {
            Console.WriteLine($"Create failed: {result.Exception}");
        }
        ValidateMostDtoFields(result);
    }

    [Test]
    public async Task Edit()
    {
        var new_result = await _Module.Create(new ProductCreateCommand()
        {
            calling_user_id = _User.external_id,
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

        Assert.That(new_result.Success, Is.True);
        Assert.That(new_result.Data, Is.Not.Null);

        var edit_command = new ProductEditCommand()
        {
            calling_user_id = _User.external_id,
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
            calling_user_id = _User.external_id,
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

        Assert.That(new_result.Success, Is.True);
        Assert.That(new_result.Data, Is.Not.Null);

        var delete_result = await _Module.Delete(new ProductDeleteCommand()
        {
            calling_user_id = _User.external_id,
            id = new_result.Data.id
        });

        Assert.That(delete_result.Success, Is.True);
        Assert.That(delete_result.Data, Is.Not.Null);
        Assert.That(delete_result.Data.deleted_by, Is.Not.Null);
        Assert.That(delete_result.Data.deleted_on, Is.Not.Null);
        Assert.That(delete_result.Data.deleted_on_string, Is.Not.Null);
        Assert.That(delete_result.Data.deleted_on_timezone, Is.Not.Null);
    }

    [Test]
    public async Task Find()
    {
        var new_result = await _Module.Create(new ProductCreateCommand()
        {
            calling_user_id = _User.external_id,
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

        Assert.That(new_result.Success, Is.True);
        Assert.That(new_result.Data, Is.Not.Null);

        var results = await _Module.Find(
                        new PagingSortingParameters() { ResultCount = 20, Start = 0 },
                        new ProductFindCommand() { calling_user_id = _User.external_id, wildcard = "PO-CU-100" });
        
        Assert.That(results.Success, Is.True);
        Assert.That(results.Data, Is.Not.Null);
        Assert.That(results.Data.Count(), Is.Not.Zero);

        var first_result = results.Data.First();

        ValidateMostListFields(first_result);
    }

    [Test]
    public async Task CreateAttribute()
    {
        var result = await _Module.Create(new ProductCreateCommand()
        {
            calling_user_id = _User.external_id,
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

        Assert.That(result.Success, Is.True);
        Assert.That(result.Data, Is.Not.Null);

        var attribute_result = await _Module.CreateAttribute(new ProductAttributeCreateCommand()
        {
            calling_user_id = _User.external_id,
            attribute_name = "Length",
            attribute_value = "100000",
            product_id = result.Data.id
        });


        Assert.That(attribute_result.Success, Is.True);
        Assert.That(attribute_result.Data, Is.Not.Null);
    }

    [Test]
    public async Task EditAttribute()
    {
        var result = await _Module.Create(new ProductCreateCommand()
        {
            calling_user_id = _User.external_id,
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

        Assert.That(result.Success, Is.True);
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data.product_attributes.Count(), Is.Not.Zero);

        var attribute_to_edit = result.Data.product_attributes[0];


        var edit_command = new ProductAttributeEditCommand()
        {
            calling_user_id = _User.external_id,
            id = attribute_to_edit.id,
            attribute_name = "Length",
            attribute_value = "100000",
        };

        var attribute_result = await _Module.EditAttribute(edit_command);


        Assert.That(attribute_result.Success, Is.True);
        Assert.That(attribute_result.Data, Is.Not.Null);
        Assert.That(attribute_result.Data.attribute_name == edit_command.attribute_name);
        Assert.That(attribute_result.Data.attribute_value == edit_command.attribute_value);
    }

    [Test]
    public async Task DeleteAttribute()
    {
        var result = await _Module.Create(new ProductCreateCommand()
        {
            calling_user_id = _User.external_id,
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

        Assert.That(result.Success, Is.True);
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data.product_attributes.Count(), Is.Not.Zero);

        var attribute_to_edit = result.Data.product_attributes[0];

        var attribute_result = await _Module.DeleteAttribute(new ProductAttributeDeleteCommand()
        {
            calling_user_id = _User.external_id,
            id = attribute_to_edit.id,
        });


        Assert.That(attribute_result.Success, Is.True);
        Assert.That(attribute_result.Data, Is.Not.Null);
    }

    private void ValidateMostDtoFields(Response<ProductDto> result)
    {
        Assert.That(result.Success, Is.True);
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data.id, Is.Not.Zero);
        Assert.That(result.Data.guid, Is.Not.Empty);
        Assert.That(result.Data.product_name, Is.Not.Empty);
        Assert.That(result.Data.category, Is.Not.Empty);
        Assert.That(result.Data.identifier1, Is.Not.Empty);
        Assert.That(result.Data.internal_description, Is.Not.Empty);
        Assert.That(result.Data.vendor_id, Is.Not.Zero);
        Assert.That(result.Data.list_price, Is.Not.Zero);
        Assert.That(result.Data.sales_price, Is.Not.Zero);
        Assert.That(result.Data.unit_cost, Is.Not.Zero);
        Assert.That(result.Data.product_class, Is.Not.Empty);
        Assert.That(result.Data.created_by, Is.Not.Zero);
        Assert.That(result.Data.created_on, Is.GreaterThan(DateTime.MinValue));
        Assert.That(result.Data.created_on_string, Is.Not.Null);
        Assert.That(result.Data.created_on_timezone, Is.Not.Null);
        Assert.That(result.Data.updated_by, Is.Not.Null);
        Assert.That(result.Data.updated_on, Is.Not.Null);
        Assert.That(result.Data.updated_on_string, Is.Not.Null);
        Assert.That(result.Data.updated_on_timezone, Is.Not.Null);
    }

    private void ValidateMostListFields(ProductListDto result)
    {
        Assert.That(result.id, Is.Not.Zero);
        Assert.That(result.guid, Is.Not.Empty);
        Assert.That(result.product_name, Is.Not.Empty);
        Assert.That(result.category, Is.Not.Empty);
        Assert.That(result.identifier1, Is.Not.Empty);
        Assert.That(result.internal_description, Is.Not.Empty);
        Assert.That(result.vendor_id, Is.Not.Zero);
        Assert.That(result.list_price, Is.Not.Zero);
        Assert.That(result.sales_price, Is.Not.Zero);
        Assert.That(result.unit_cost, Is.Not.Zero);
        Assert.That(result.product_class, Is.Not.Empty);
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