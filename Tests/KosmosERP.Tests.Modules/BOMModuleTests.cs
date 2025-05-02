using System.Data;
using Microsoft.EntityFrameworkCore;
using KosmosERP.Models;
using KosmosERP.Models.Permissions;
using KosmosERP.Tests.Modules.Shared;
using KosmosERP.BusinessLayer.Helpers;
using KosmosERP.BusinessLayer.Modules;
using KosmosERP.Database.Models;
using KosmosERP.BusinessLayer.Models.Module.BOM.Command.Create;
using KosmosERP.BusinessLayer.Models.Module.BOM.Command.Edit;
using KosmosERP.BusinessLayer.Models.Module.BOM.Command.Delete;
using KosmosERP.BusinessLayer.Models.Module.BOM.Command.Find;
using KosmosERP.BusinessLayer.Models.Module.BOM.Dto;

namespace KosmosERP.Tests.Modules;

public class BOMModuleTests : BaseTestModule<BOMModule>, IModuleTest
{
    private Product _Product1;
    private Product _Product2;

    [SetUp]
    public async Task SetupModule()
    {
        var the_module = new BOMModule(base._Context);

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
        var read_permission = await _Context.ModulePermissions.FirstAsync(m => m.module_id == _Module.ModuleIdentifier.ToString() && m.internal_permission_name == BOMPermissions.Read);
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
        var create_permission = await _Context.ModulePermissions.FirstAsync(m => m.module_id == _Module.ModuleIdentifier.ToString() && m.internal_permission_name == BOMPermissions.Create);
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
        var edit_permission = await _Context.ModulePermissions.FirstAsync(m => m.module_id == _Module.ModuleIdentifier.ToString() && m.internal_permission_name == BOMPermissions.Edit);
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
        var delete_permission = await _Context.ModulePermissions.FirstAsync(m => m.module_id == _Module.ModuleIdentifier.ToString() && m.internal_permission_name == BOMPermissions.Delete);
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

        _Product1 = product;


        var product2 = CommonDataHelper<Product>.FillCommonFields(new Product()
        {
            category = "Wirerope",
            sales_price = 4,
            list_price = 5,
            product_class = "Rope",
            product_name = "Wirerope Sling",
            internal_description = "This is wirerope",
            external_description = "This is wirerope",
            identifier1 = "SL-100",
            our_cost = 1,
            unit_cost = 1,
            is_taxable = true,
            is_shippable = true,
            is_material = true,
        }, 1);

        _Context.Products.Add(product2);
        await _Context.SaveChangesAsync();

        _Product2 = product2;


        var BOM1 = CommonDataHelper<BOM>.FillCommonFields(new BOM
        {
            parent_product_id = _Product1.id,
            is_deleted = false,
        }, 1);

        _Context.BOMs.Add(BOM1);
        await _Context.SaveChangesAsync();

        var BOM2 = CommonDataHelper<BOM>.FillCommonFields(new BOM
        {
            parent_bom_id = BOM1.id,
            parent_product_id = _Product2.id,
            quantity = 100,
            instructions = "Cut 100 feet of wirerope",
            is_deleted = false,
        }, 1);

        _Context.BOMs.Add(BOM2);

        await _Context.SaveChangesAsync();
    }

    [Test]
    public async Task Get()
    {
        var new_result = await _Module.Create(new BOMCreateCommand()
        {
            calling_user_id = 1,
            token = _SessionId,
            parent_product_id = _Product1.id,
            quantity = 100,
            instructions = "Do stuff and things"
        });

        Assert.IsTrue(new_result.Success);
        Assert.IsNotNull(new_result.Data);

        var result = await _Module.GetDto(new_result.Data.id);

        ValidateMostDtoFields(result);
    }

    [Test]
    public async Task Create()
    {
        var result = await _Module.Create(new BOMCreateCommand()
        {
            calling_user_id = 1,
            token = _SessionId,
            parent_product_id = _Product1.id,
            quantity = 100,
            instructions = "Do stuff and things",
            child_bom = new BOMCreateCommand()
            {
                parent_product_id = _Product2.id,
                quantity = 1,
                instructions = "This is a sub item"
            }
        });

        ValidateMostDtoFields(result);
        Assert.NotZero(result.Data.child_boms.Count());
        Assert.That(result.Data.child_boms[0].parent_product_id == _Product2.id);

    }

    [Test]
    public async Task Edit()
    {
        var new_result = await _Module.Create(new BOMCreateCommand()
        {
            calling_user_id = 1,
            token = _SessionId,
            parent_product_id = _Product1.id,
            quantity = 100,
            instructions = "Do stuff and things",
            child_bom = new BOMCreateCommand()
            {
                parent_product_id = _Product2.id,
                quantity = 1,
                instructions = "This is a sub item"
            }
        });

        Assert.IsTrue(new_result.Success);
        Assert.IsNotNull(new_result.Data);

        var edit_command = new BOMEditCommand()
        {
            calling_user_id = 1,
            token = _SessionId,
            id = new_result.Data.id,
            parent_product_id = _Product2.id,
            quantity = 11,
            instructions = "Do stasdasd"
        };

        var edit_result = await _Module.Edit(edit_command);

        ValidateMostDtoFields(edit_result);

        Assert.That(edit_result.Data.parent_product_id == edit_command.parent_product_id);
        Assert.That(edit_result.Data.quantity == edit_command.quantity);
        Assert.That(edit_result.Data.instructions == edit_command.instructions);
        Assert.That(edit_result.Data.bom_id == new_result.Data.bom_id);
    }

    [Test]
    public async Task Delete()
    {
        var new_result = await _Module.Create(new BOMCreateCommand()
        {
            calling_user_id = 1,
            token = _SessionId,
            parent_product_id = _Product1.id,
            quantity = 100,
            instructions = "Do stuff and things",
            child_bom = new BOMCreateCommand()
            {
                parent_product_id = _Product2.id,
                quantity = 1,
                instructions = "This is a sub item"
            }
        });

        Assert.IsTrue(new_result.Success);
        Assert.IsNotNull(new_result.Data);

        var delete_result = await _Module.Delete(new BOMDeleteCommand()
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
        var new_result = await _Module.Create(new BOMCreateCommand()
        {
            calling_user_id = 1,
            token = _SessionId,
            parent_product_id = _Product1.id,
            quantity = 100,
            instructions = "Do stuff and things"
        });

        Assert.IsTrue(new_result.Success);
        Assert.IsNotNull(new_result.Data);

        var results = await _Module.Find(
                        new PagingSortingParameters() { ResultCount = 20, Start = 0 },
                        new BOMFindCommand() { calling_user_id = 1, token = _SessionId, wildcard = "stuff" });
        
        Assert.IsTrue(results.Success);
        Assert.IsNotNull(results.Data);
        Assert.NotZero(results.Data.Count());

        var first_result = results.Data.First();

        ValidateMostListFields(first_result);
    }

    private void ValidateMostDtoFields(Response<BOMDto> result)
    {
        Assert.IsTrue(result.Success);
        Assert.IsNotNull(result.Data);
        Assert.NotZero(result.Data.id);
        Assert.IsNotEmpty(result.Data.guid);
        Assert.NotZero(result.Data.parent_product_id);
        Assert.NotZero(result.Data.quantity);
        Assert.IsNotEmpty(result.Data.instructions);
        Assert.NotZero(result.Data.created_by);
        Assert.NotNull(result.Data.created_on);
        Assert.NotNull(result.Data.created_on_string);
        Assert.NotNull(result.Data.created_on_timezone);
        Assert.NotNull(result.Data.updated_by);
        Assert.NotNull(result.Data.updated_on);
        Assert.NotNull(result.Data.updated_on_string);
        Assert.NotNull(result.Data.updated_on_timezone);
    }

    private void ValidateMostListFields(BOMListDto result)
    {
        Assert.NotZero(result.id);
        Assert.IsNotEmpty(result.guid);
        Assert.NotZero(result.parent_product_id);
        Assert.NotZero(result.quantity);
        Assert.IsNotEmpty(result.instructions);
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