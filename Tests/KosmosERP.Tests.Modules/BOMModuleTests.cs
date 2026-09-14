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
using KosmosERP.BusinessLayer;

namespace KosmosERP.Tests.Modules;

public class BOMModuleTests : BaseTestModule<BOMModule>, IModuleTest
{
    private Product _Product1;
    private Product _Product2;

    [SetUp]
    public async Task SetupModule()
    {
        var logProviderFactory = new LogProviderFactory(new LogProviderSettings() { log_provider = LogProviderType.MOCK }, _Context, null);
        var the_module = new BOMModule(base._Context, logProviderFactory);

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
            calling_user_id = _User.external_id,
            parent_product_id = _Product1.id,
            quantity = 100,
            instructions = "Do stuff and things"
        });

        Assert.That(new_result.Success, Is.True);
        Assert.That(new_result.Data, Is.Not.Null);

        var result = await _Module.GetDto(new_result.Data.id);

        ValidateMostDtoFields(result);
    }

    [Test]
    public async Task Create()
    {
        var result = await _Module.Create(new BOMCreateCommand()
        {
            calling_user_id = _User.external_id,
            parent_product_id = _Product1.id,
            quantity = 100,
            instructions = "Do stuff and things",
            child_bom = new BOMCreateCommand()
            {
                parent_product_id = _Product2.id,
                quantity = 1,
                instructions = "This is a sub item",
                calling_user_id = _User.external_id
            }
        });

        ValidateMostDtoFields(result);
        Assert.That(result.Data.child_boms.Count(), Is.Not.Zero);
        Assert.That(result.Data.child_boms[0].parent_product_id == _Product2.id);

    }

    [Test]
    public async Task Edit()
    {
        var new_result = await _Module.Create(new BOMCreateCommand()
        {
            calling_user_id = _User.external_id,
            parent_product_id = _Product1.id,
            quantity = 100,
            instructions = "Do stuff and things",
            child_bom = new BOMCreateCommand()
            {
                parent_product_id = _Product2.id,
                quantity = 1,
                instructions = "This is a sub item",
                calling_user_id = _User.external_id
            }
        });

        Assert.That(new_result.Success, Is.True);
        Assert.That(new_result.Data, Is.Not.Null);

        var edit_command = new BOMEditCommand()
        {
            calling_user_id = _User.external_id,
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
            calling_user_id = _User.external_id,
            parent_product_id = _Product1.id,
            quantity = 100,
            instructions = "Do stuff and things",
            child_bom = new BOMCreateCommand()
            {
                parent_product_id = _Product2.id,
                quantity = 1,
                instructions = "This is a sub item",
                calling_user_id = _User.external_id
            }
        });

        Assert.That(new_result.Success, Is.True);
        Assert.That(new_result.Data, Is.Not.Null);

        var delete_result = await _Module.Delete(new BOMDeleteCommand()
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
        var new_result = await _Module.Create(new BOMCreateCommand()
        {
            calling_user_id = _User.external_id,
            parent_product_id = _Product1.id,
            product_id = _Product2.id,
            quantity = 100,
            instructions = "Do stuff and things"
        });

        Assert.That(new_result.Success, Is.True);
        Assert.That(new_result.Data, Is.Not.Null);

        var results = await _Module.Find(
                        new PagingSortingParameters() { ResultCount = 20, Start = 0 },
                        new BOMFindCommand() { calling_user_id = _User.external_id, parent_product_id = _Product1.id });
        
        Assert.That(results.Success, Is.True);
        Assert.That(results.Data, Is.Not.Null);
        Assert.That(results.Data.Count(), Is.Not.Zero);

        var first_result = results.Data.First();

        ValidateMostListFields(first_result);
    }

    private void ValidateMostDtoFields(Response<BOMDto> result)
    {
        Assert.That(result.Success, Is.True);
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data.id, Is.Not.Zero);
        Assert.That(result.Data.guid, Is.Not.Empty);
        Assert.That(result.Data.parent_product_id, Is.Not.Zero);
        Assert.That(result.Data.quantity, Is.Not.Zero);
        Assert.That(result.Data.instructions, Is.Not.Empty);
        Assert.That(result.Data.created_by, Is.Not.Zero);
        Assert.That(result.Data.created_on, Is.GreaterThan(DateTime.MinValue));
        Assert.That(result.Data.created_on_string, Is.Not.Null);
        Assert.That(result.Data.created_on_timezone, Is.Not.Null);
        Assert.That(result.Data.updated_by, Is.Not.Null);
        Assert.That(result.Data.updated_on, Is.Not.Null);
        Assert.That(result.Data.updated_on_string, Is.Not.Null);
        Assert.That(result.Data.updated_on_timezone, Is.Not.Null);
    }

    private void ValidateMostListFields(BOMListDto result)
    {
        Assert.That(result.id, Is.Not.Zero);
        Assert.That(result.guid, Is.Not.Empty);
        Assert.That(result.parent_product_id, Is.Not.Zero);
        Assert.That(result.quantity, Is.Not.Zero);
        Assert.That(result.instructions, Is.Not.Empty);
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