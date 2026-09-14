using System.Data;
using Microsoft.EntityFrameworkCore;
using KosmosERP.Models;
using KosmosERP.Tests.Modules.Shared;
using KosmosERP.BusinessLayer.Helpers;
using KosmosERP.BusinessLayer.Modules;
using KosmosERP.Database.Models;
using KosmosERP.BusinessLayer.Models.Module.PurchaseOrder.Command.Create;
using KosmosERP.BusinessLayer.Models.Module.PurchaseOrder.Command.Edit;
using KosmosERP.BusinessLayer.Models.Module.PurchaseOrder.Command.Delete;
using KosmosERP.BusinessLayer.Models.Module.PurchaseOrder.Command.Find;
using KosmosERP.BusinessLayer.Models.Module.PurchaseOrder.Dto;
using KosmosERP.BusinessLayer;

namespace KosmosERP.Tests.Modules;

public class PurchaseOrderModuleTests : BaseTestModule<PurchaseOrderModule>, IModuleTest
{
    private Address _Address;
    private Vendor _Vendor;
    private Product _Product;

    [SetUp]
    public async Task SetupModule()
    {
        var logProviderFactory = new LogProviderFactory(new LogProviderSettings() { log_provider = LogProviderType.MOCK }, _Context, null);
        var the_module = new PurchaseOrderModule(base._Context, 
                                                new MessageFactory(new MessagePublisherSettings() { account_provider = MessagePublisherType.MOCK, transaction_movement_topic = "test" }, base._Context), 
                                                new MessagePublisherSettings{ account_provider = MessagePublisherType.Database, transaction_movement_topic = "test" },
                                                logProviderFactory);

        await base.SetupModule(the_module);
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
        }, 1);

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
        }, 1);

        _Context.Vendors.Add(vendor);
        _Context.SaveChanges();

        _Vendor = vendor;
    }

    [Test]
    public async Task Get()
    {
        var new_result = await _Module.Create(new PurchaseOrderHeaderCreateCommand()
        {
            calling_user_id = _User.external_id,
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
                    is_taxable = true,
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
        var result = await _Module.Create(new PurchaseOrderHeaderCreateCommand()
        {
            calling_user_id = _User.external_id,
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
                    is_taxable = true,
                    calling_user_id = _User.external_id
                }
            }
        });

        ValidateMostDtoFields(result);

        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data.vendor_name, Is.Not.Null);
        Assert.That(result.Data.purchase_order_lines.Count() == 1);
        Assert.That(result.Data.purchase_order_lines[0].product_name, Is.Not.Null);
    }

    [Test]
    public async Task Edit()
    {
        var old_result = await _Module.Create(new PurchaseOrderHeaderCreateCommand()
        {
            calling_user_id = _User.external_id,
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
                    is_taxable = true,
                    calling_user_id = _User.external_id
                }
            }
        });

        var edit_command = new PurchaseOrderHeaderEditCommand()
        {
            calling_user_id = _User.external_id,
            id = old_result.Data.id,
            po_type = "Office Use",
            vendor_id = _Vendor.id,
            purchase_order_lines = new List<PurchaseOrderLineEditCommand>() {
                new PurchaseOrderLineEditCommand()
                {
                    calling_user_id = _User.external_id,
                    description = "Paper",
                    product_id = _Product.id,
                    quantity = 1,
                    unit_price = 10,
                    line_number = 1,
                    tax = 1,
                    is_taxable = true,
                }
            }
        };

        var result = await _Module.Edit(edit_command);

        ValidateMostDtoFields(result);

        Assert.That(result.Data.po_type == edit_command.po_type);
        Assert.That(result.Data.purchase_order_lines.Count() == 2);
        Assert.That(result.Data.vendor_name, Is.Not.Null);
        Assert.That(result.Data.purchase_order_lines[0].product_name, Is.Not.Null);
    }

    [Test]
    public async Task Delete()
    {
        var new_result = await _Module.Create(new PurchaseOrderHeaderCreateCommand()
        {
            calling_user_id = _User.external_id,
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
                    is_taxable = true,
                    calling_user_id = _User.external_id
                }
            }
        });

        Assert.That(new_result.Success, Is.True);
        Assert.That(new_result.Data, Is.Not.Null);

        var delete_result = await _Module.Delete(new PurchaseOrderHeaderDeleteCommand()
        {
            calling_user_id = _User.external_id,
            id = new_result.Data.id
        });

        Assert.That(delete_result.Success, Is.True);
        Assert.That(delete_result.Data, Is.Not.Null);
        Assert.That(delete_result.Data.purchase_order_lines.Count(), Is.Zero);
        Assert.That(delete_result.Data.deleted_by, Is.Not.Null);
        Assert.That(delete_result.Data.deleted_on, Is.Not.Null);
        Assert.That(delete_result.Data.deleted_on_string, Is.Not.Null);
        Assert.That(delete_result.Data.deleted_on_timezone, Is.Not.Null);
    }

    [Test]
    public async Task Find()
    {
        var new_result = await _Module.Create(new PurchaseOrderHeaderCreateCommand()
        {
            calling_user_id = _User.external_id,
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
                    is_taxable = true,
                    calling_user_id = _User.external_id
                }
            }
        });

        Assert.That(new_result.Success, Is.True);
        Assert.That(new_result.Data, Is.Not.Null);

        ValidateMostDtoFields(new_result);

        var results = await _Module.Find(
                        new PagingSortingParameters() { ResultCount = 20, Start = 0 },
                        new PurchaseOrderHeaderFindCommand() { calling_user_id = _User.external_id, wildcard = new_result.Data.po_number.ToString() });

        Assert.That(results.Success, Is.True);
        Assert.That(results.Data, Is.Not.Null);
        Assert.That(results.Data.Count(), Is.Not.Zero);

        var first_result = results.Data.First();

        ValidateMostListFields(first_result);
    }


    [Test]
    public async Task CreateLine()
    {
        var create_result = await _Module.Create(new PurchaseOrderHeaderCreateCommand()
        {
            calling_user_id = _User.external_id,
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
                    is_taxable = true,
                    calling_user_id = _User.external_id
                }
            }
        });

        Assert.That(create_result.Success, Is.True);
        Assert.That(create_result.Data, Is.Not.Null);
        Assert.That(create_result.Data.purchase_order_lines.Count(), Is.Not.Zero);


        var create_command = new PurchaseOrderLineCreateCommand()
        {
            calling_user_id = _User.external_id,
            purchase_order_header_id = create_result.Data.id,
            description = "Copper",
            product_id = _Product.id,
            quantity = 12,
            unit_price = 101,
            line_number = 2,
            tax = 11,
            is_taxable = true
        };

        var create_line_response = await _Module.CreateLine(create_command);


        Assert.That(create_line_response.Success, Is.True);
        Assert.That(create_line_response.Data, Is.Not.Null);
        Assert.That(create_line_response.Data.line_number == create_command.line_number);
        Assert.That(create_line_response.Data.quantity == create_command.quantity);
        Assert.That(create_line_response.Data.is_taxable == create_command.is_taxable);
        Assert.That(create_line_response.Data.product_id == create_command.product_id);
        Assert.That(create_line_response.Data.tax == create_command.tax);
        Assert.That(create_line_response.Data.product_name, Is.Not.Null);
    }

    [Test]
    public async Task EditLine()
    {
        var create_result = await _Module.Create(new PurchaseOrderHeaderCreateCommand()
        {
            calling_user_id = _User.external_id,
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
                    is_taxable = true,
                    calling_user_id = _User.external_id
                }
            }
        });

        Assert.That(create_result.Success, Is.True);
        Assert.That(create_result.Data, Is.Not.Null);
        Assert.That(create_result.Data.purchase_order_lines.Count(), Is.Not.Zero);


        var edit_command = new PurchaseOrderLineEditCommand()
        {
            calling_user_id = _User.external_id,
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


        Assert.That(edit_line_response.Success, Is.True);
        Assert.That(edit_line_response.Data, Is.Not.Null);
        Assert.That(edit_line_response.Data.line_number == edit_command.line_number);
        Assert.That(edit_line_response.Data.description == edit_command.description);
        Assert.That(edit_line_response.Data.is_taxable == edit_command.is_taxable);
        Assert.That(edit_line_response.Data.quantity == edit_command.quantity);
        Assert.That(edit_line_response.Data.unit_price == edit_command.unit_price);
        Assert.That(edit_line_response.Data.tax == edit_command.tax);
        Assert.That(edit_line_response.Data.product_name, Is.Not.Null);
    }


    [Test]
    public async Task DeleteLine()
    {
        var create_result = await _Module.Create(new PurchaseOrderHeaderCreateCommand()
        {
            calling_user_id = _User.external_id,
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
                    is_taxable = true,
                    calling_user_id = _User.external_id
                }
            }
        });

        Assert.That(create_result.Success, Is.True);
        Assert.That(create_result.Data, Is.Not.Null);
        Assert.That(create_result.Data.purchase_order_lines.Count(), Is.Not.Zero);

        var response = await _Module.DeleteLine(new PurchaseOrderLineDeleteCommand()
        {
            calling_user_id = _User.external_id,
            id = create_result.Data.purchase_order_lines[0].id,
        });


        Assert.That(response.Success, Is.True);
        Assert.That(response.Data, Is.Not.Null);
        Assert.That(response.Data.deleted_by, Is.Not.Null);
        Assert.That(response.Data.deleted_on, Is.Not.Null);
        Assert.That(response.Data.deleted_on_string, Is.Not.Null);
        Assert.That(response.Data.deleted_on_timezone, Is.Not.Null);
    }


    private void ValidateMostDtoFields(Response<PurchaseOrderHeaderDto> result)
    {
        Assert.That(result.Success, Is.True);
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data.id, Is.Not.Zero);
        Assert.That(result.Data.guid, Is.Not.Empty);
        Assert.That(result.Data.revision_number, Is.Not.Zero);
        Assert.That(result.Data.po_type, Is.Not.Null);
        Assert.That(result.Data.vendor_id, Is.Not.Zero);
        Assert.That(result.Data.created_by, Is.Not.Zero);
        Assert.That(result.Data.created_on, Is.GreaterThan(DateTime.MinValue));
        Assert.That(result.Data.created_on_string, Is.Not.Null);
        Assert.That(result.Data.created_on_timezone, Is.Not.Null);
        Assert.That(result.Data.updated_by, Is.Not.Null);
        Assert.That(result.Data.updated_on, Is.Not.Null);
        Assert.That(result.Data.updated_on_string, Is.Not.Null);
        Assert.That(result.Data.updated_on_timezone, Is.Not.Null);
    }

    private void ValidateMostListFields(PurchaseOrderHeaderListDto result)
    {
        Assert.That(result.id, Is.Not.Zero);
        Assert.That(result.guid, Is.Not.Empty);
        Assert.That(result.revision_number, Is.Not.Zero);
        Assert.That(result.po_type, Is.Not.Null);
        Assert.That(result.vendor_id, Is.Not.Zero);
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