using System.Data;
using Microsoft.EntityFrameworkCore;
using KosmosERP.Models;
using KosmosERP.Models.Permissions;
using KosmosERP.Tests.Modules.Shared;
using KosmosERP.BusinessLayer.Helpers;
using KosmosERP.BusinessLayer.Modules;
using KosmosERP.Database.Models;
using KosmosERP.BusinessLayer.Models.Module.PurchaseOrderReceive.Command.Create;
using KosmosERP.BusinessLayer.Models.Module.PurchaseOrderReceive.Command.Edit;
using KosmosERP.BusinessLayer.Models.Module.PurchaseOrderReceive.Command.Delete;
using KosmosERP.BusinessLayer.Models.Module.PurchaseOrderReceive.Command.Find;
using KosmosERP.BusinessLayer.Models.Module.PurchaseOrderReceive.Dto;
using KosmosERP.BusinessLayer.MessagePublisher;

namespace KosmosERP.Tests.Modules;

public class PurchaseOrderReceiveModuleTests : BaseTestModule<PurchaseOrderReceiveModule>, IModuleTest
{
    private Address _Address;
    private Vendor _Vendor;
    private Product _Product;
    private PurchaseOrderHeader _PurchaseOrderHeader;
    private PurchaseOrderLine _PurchaseOrderLine;
    private DocumentUploadObject _DocumentUploadObject;
    private DocumentUpload _DocumentUpload;

    [SetUp]
    public async Task SetupModule()
    {
        var the_module = new PurchaseOrderReceiveModule(base._Context, new MockMessagePublisher(new MessagePublisherSettings()));

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
        var read_permission = await _Context.ModulePermissions.FirstAsync(m => m.module_id == _Module.ModuleIdentifier.ToString() && m.internal_permission_name == PurchaseOrderReceivePermissions.Read);
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
        var create_permission = await _Context.ModulePermissions.FirstAsync(m => m.module_id == _Module.ModuleIdentifier.ToString() && m.internal_permission_name == PurchaseOrderReceivePermissions.Create);
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
        var edit_permission = await _Context.ModulePermissions.FirstAsync(m => m.module_id == _Module.ModuleIdentifier.ToString() && m.internal_permission_name == PurchaseOrderReceivePermissions.Edit);
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
        var delete_permission = await _Context.ModulePermissions.FirstAsync(m => m.module_id == _Module.ModuleIdentifier.ToString() && m.internal_permission_name == PurchaseOrderReceivePermissions.Delete);
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

        var document_upload_object = new DocumentUploadObject()
        {
            internal_name = "AR_Invoice",
            friendly_name = "AR Invoice",
        };

        _Context.DocumentUploadObjects.Add(document_upload_object);
        await _Context.SaveChangesAsync();

        _DocumentUploadObject = document_upload_object;

        var object_tag1 = new DocumentUploadObjectTagTemplate()
        {
            document_object_id = _DocumentUploadObject.id,
            is_required = true,
            name = "Invoice Number"
        };

        var object_tag2 = new DocumentUploadObjectTagTemplate()
        {
            document_object_id = _DocumentUploadObject.id,
            is_required = true,
            name = "Customer Number"
        };

        _Context.DocumentUploadObjectTags.Add(object_tag1);
        _Context.DocumentUploadObjectTags.Add(object_tag2);
        await _Context.SaveChangesAsync();

        var document_upload = CommonDataHelper<DocumentUpload>.FillCommonFields(new DocumentUpload()
        {
            document_object_id = _DocumentUploadObject.id,
            rev_num = 1
        }, 1);

        _Context.DocumentUploads.Add(document_upload);
        await _Context.SaveChangesAsync();

        _DocumentUpload = document_upload;
    }

    [Test]
    public async Task Get()
    {
        var new_result = await _Module.Create(new PurchaseOrderReceiveHeaderCreateCommand()
        {
            calling_user_id = 1,
            token = _SessionId,
            purchase_order_id = _PurchaseOrderHeader.id,
            document_upload_id = _DocumentUpload.id,
            received_lines = new List<PurchaseOrderReceiveLineCreateCommand>()
            {
                new PurchaseOrderReceiveLineCreateCommand()
                {
                    calling_user_id = 1,
                    token = _SessionId,
                    purchase_order_line_id = _PurchaseOrderLine.id,
                    purchase_order_receive_header_id = _PurchaseOrderHeader.id,
                    units_received = 10
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
        var result = await _Module.Create(new PurchaseOrderReceiveHeaderCreateCommand()
        {
            calling_user_id = 1,
            token = _SessionId,
            purchase_order_id = _PurchaseOrderHeader.id,
            document_upload_id = _DocumentUpload.id,
            received_lines = new List<PurchaseOrderReceiveLineCreateCommand>()
            {
                new PurchaseOrderReceiveLineCreateCommand()
                {
                    calling_user_id = 1,
                    token = _SessionId,
                    purchase_order_line_id = _PurchaseOrderLine.id,
                    purchase_order_receive_header_id = _PurchaseOrderHeader.id,
                    units_received = 10
                }
            }
        });

        ValidateMostDtoFields(result);
    }

    [Test]
    public async Task Edit()
    {
        var old_result = await _Module.Create(new PurchaseOrderReceiveHeaderCreateCommand()
        {
            calling_user_id = 1,
            token = _SessionId,
            purchase_order_id = _PurchaseOrderHeader.id,
            document_upload_id = _DocumentUpload.id,
            received_lines = new List<PurchaseOrderReceiveLineCreateCommand>()
            {
                new PurchaseOrderReceiveLineCreateCommand()
                {
                    calling_user_id = 1,
                    token = _SessionId,
                    purchase_order_line_id = _PurchaseOrderLine.id,
                    purchase_order_receive_header_id = _PurchaseOrderHeader.id,
                    units_received = 10
                }
            }
        });

        var edit_command = new PurchaseOrderReceiveHeaderEditCommand()
        {
            calling_user_id = 1,
            token = _SessionId,
            id = old_result.Data.id,
            received_lines = new List<PurchaseOrderReceiveLineEditCommand>()
            {
                new PurchaseOrderReceiveLineEditCommand()
                {
                    calling_user_id = 1,
                    token = _SessionId,
                    purchase_order_line_id = _PurchaseOrderLine.id,
                    units_received = 110,
                }
            }
        };

        var result = await _Module.Edit(edit_command);

        ValidateMostDtoFields(result);

        Assert.That(result.Data.received_lines.Count() == 2);
    }

    [Test]
    public async Task Delete()
    {
        var new_result = await _Module.Create(new PurchaseOrderReceiveHeaderCreateCommand()
        {
            calling_user_id = 1,
            token = _SessionId,
            purchase_order_id = _PurchaseOrderHeader.id,
            document_upload_id = _DocumentUpload.id,
            received_lines = new List<PurchaseOrderReceiveLineCreateCommand>()
            {
                new PurchaseOrderReceiveLineCreateCommand()
                {
                    calling_user_id = 1,
                    token = _SessionId,
                    purchase_order_line_id = _PurchaseOrderLine.id,
                    purchase_order_receive_header_id = _PurchaseOrderHeader.id,
                    units_received = 10
                }
            }
        });

        Assert.IsTrue(new_result.Success);
        Assert.IsNotNull(new_result.Data);

        var delete_result = await _Module.Delete(new PurchaseOrderReceiveHeaderDeleteCommand()
        {
            calling_user_id = 1,
            token = _SessionId,
            id = new_result.Data.id
        });

        Assert.IsTrue(delete_result.Success);
        Assert.IsNotNull(delete_result.Data);
        Assert.Zero(delete_result.Data.received_lines.Count());
        Assert.NotNull(delete_result.Data.deleted_by);
        Assert.NotNull(delete_result.Data.deleted_on);
        Assert.NotNull(delete_result.Data.deleted_on_string);
        Assert.NotNull(delete_result.Data.deleted_on_timezone);
    }

    [Test]
    public async Task Find()
    {
        var new_result = await _Module.Create(new PurchaseOrderReceiveHeaderCreateCommand()
        {
            calling_user_id = 1,
            token = _SessionId,
            purchase_order_id = _PurchaseOrderHeader.id,
            document_upload_id = _DocumentUpload.id,
            received_lines = new List<PurchaseOrderReceiveLineCreateCommand>()
            {
                new PurchaseOrderReceiveLineCreateCommand()
                {
                    calling_user_id = 1,
                    token = _SessionId,
                    purchase_order_line_id = _PurchaseOrderLine.id,
                    purchase_order_receive_header_id = _PurchaseOrderHeader.id,
                    units_received = 10
                }
            }
        });

        Assert.IsTrue(new_result.Success);
        Assert.IsNotNull(new_result.Data);

        ValidateMostDtoFields(new_result);

        var results = await _Module.Find(
                        new PagingSortingParameters() { ResultCount = 20, Start = 0 },
                        new PurchaseOrderReceiveHeaderFindCommand() { calling_user_id = 1, token = _SessionId, wildcard = "" });

        Assert.IsTrue(results.Success);
        Assert.IsNotNull(results.Data);
        Assert.NotZero(results.Data.Count());

        var first_result = results.Data.First();

        ValidateMostListFields(first_result);
    }

    
    [Test]
    public async Task CreateLine()
    {
        var create_result = await _Module.Create(new PurchaseOrderReceiveHeaderCreateCommand()
        {
            calling_user_id = 1,
            token = _SessionId,
            purchase_order_id = _PurchaseOrderHeader.id,
            document_upload_id = _DocumentUpload.id,
            received_lines = new List<PurchaseOrderReceiveLineCreateCommand>()
            {
                new PurchaseOrderReceiveLineCreateCommand()
                {
                    calling_user_id = 1,
                    token = _SessionId,
                    purchase_order_line_id = _PurchaseOrderLine.id,
                    purchase_order_receive_header_id = _PurchaseOrderHeader.id,
                    units_received = 10
                }
            }
        });

        Assert.IsTrue(create_result.Success);
        Assert.IsNotNull(create_result.Data);
        Assert.NotZero(create_result.Data.received_lines.Count());


        var create_command = new PurchaseOrderReceiveLineCreateCommand()
        {
            calling_user_id = 1,
            token = _SessionId,
            purchase_order_line_id = _PurchaseOrderLine.id,
            purchase_order_receive_header_id = _PurchaseOrderHeader.id,
            units_received = 101
        };

        var create_line_response = await _Module.CreateLine(create_command);


        Assert.IsTrue(create_line_response.Success);
        Assert.IsNotNull(create_line_response.Data);
        Assert.That(create_line_response.Data.purchase_order_line_id == _PurchaseOrderLine.id);
        Assert.That(create_line_response.Data.purchase_order_receive_header_id == _PurchaseOrderHeader.id);
        Assert.That(create_line_response.Data.units_received == create_command.units_received);
    }

    [Test]
    public async Task EditLine()
    {
        var create_result = await _Module.Create(new PurchaseOrderReceiveHeaderCreateCommand()
        {
            calling_user_id = 1,
            token = _SessionId,
            purchase_order_id = _PurchaseOrderHeader.id,
            document_upload_id = _DocumentUpload.id,
            received_lines = new List<PurchaseOrderReceiveLineCreateCommand>()
            {
                new PurchaseOrderReceiveLineCreateCommand()
                {
                    calling_user_id = 1,
                    token = _SessionId,
                    purchase_order_line_id = _PurchaseOrderLine.id,
                    purchase_order_receive_header_id = _PurchaseOrderHeader.id,
                    units_received = 10
                }
            }
        });

        Assert.IsTrue(create_result.Success);
        Assert.IsNotNull(create_result.Data);
        Assert.NotZero(create_result.Data.received_lines.Count());


        var edit_command = new PurchaseOrderReceiveLineEditCommand()
        {
            calling_user_id = 1,
            token = _SessionId,
            id = create_result.Data.received_lines[0].id,
            purchase_order_line_id = _PurchaseOrderLine.id,
            units_received = 1012
        };

        var edit_line_response = await _Module.EditLine(edit_command);


        Assert.IsTrue(edit_line_response.Success);
        Assert.IsNotNull(edit_line_response.Data);
        Assert.That(edit_line_response.Data.units_received == edit_command.units_received);
    }


    [Test]
    public async Task DeleteLine()
    {
        var create_result = await _Module.Create(new PurchaseOrderReceiveHeaderCreateCommand()
        {
            calling_user_id = 1,
            token = _SessionId,
            purchase_order_id = _PurchaseOrderHeader.id,
            document_upload_id = _DocumentUpload.id,
            received_lines = new List<PurchaseOrderReceiveLineCreateCommand>()
            {
                new PurchaseOrderReceiveLineCreateCommand()
                {
                    calling_user_id = 1,
                    token = _SessionId,
                    purchase_order_line_id = _PurchaseOrderLine.id,
                    purchase_order_receive_header_id = _PurchaseOrderHeader.id,
                    units_received = 10
                }
            }
        });

        Assert.IsTrue(create_result.Success);
        Assert.IsNotNull(create_result.Data);
        Assert.NotZero(create_result.Data.received_lines.Count());

        var response = await _Module.DeleteLine(new PurchaseOrderReceiveLineDeleteCommand()
        {
            calling_user_id = 1,
            token = _SessionId,
            id = create_result.Data.received_lines[0].id,
        });


        Assert.IsTrue(response.Success);
        Assert.IsNotNull(response.Data);
        Assert.NotNull(response.Data.deleted_by);
        Assert.NotNull(response.Data.deleted_on);
        Assert.NotNull(response.Data.deleted_on_string);
        Assert.NotNull(response.Data.deleted_on_timezone);
    }


    private void ValidateMostDtoFields(Response<PurchaseOrderReceiveHeaderDto> result)
    {
        Assert.IsTrue(result.Success);
        Assert.IsNotNull(result.Data);
        Assert.NotZero(result.Data.id);
        Assert.IsNotEmpty(result.Data.guid);
        Assert.NotZero(result.Data.units_ordered);
        Assert.NotZero(result.Data.units_received);
        Assert.NotZero(result.Data.purchase_order_id);
        Assert.NotZero(result.Data.created_by);
        Assert.NotNull(result.Data.created_on);
        Assert.NotNull(result.Data.created_on_string);
        Assert.NotNull(result.Data.created_on_timezone);
        Assert.NotNull(result.Data.updated_by);
        Assert.NotNull(result.Data.updated_on);
        Assert.NotNull(result.Data.updated_on_string);
        Assert.NotNull(result.Data.updated_on_timezone);
    }

    private void ValidateMostListFields(PurchaseOrderReceiveHeaderListDto result)
    {
        Assert.NotZero(result.id);
        Assert.IsNotEmpty(result.guid);
        Assert.NotZero(result.units_ordered);
        Assert.NotZero(result.units_received);
        Assert.NotZero(result.purchase_order_id);
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