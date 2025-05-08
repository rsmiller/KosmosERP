using System.Data;
using Microsoft.EntityFrameworkCore;
using KosmosERP.Models;
using KosmosERP.Models.Permissions;
using KosmosERP.Tests.Modules.Shared;
using KosmosERP.BusinessLayer.Helpers;
using KosmosERP.BusinessLayer.Modules;
using KosmosERP.Database.Models;
using KosmosERP.BusinessLayer.Models.Module.DocumentUpload.Command.Create;
using KosmosERP.BusinessLayer.Models.Module.DocumentUpload.Command.Edit;
using KosmosERP.BusinessLayer.Models.Module.DocumentUpload.Command.Delete;
using KosmosERP.BusinessLayer.Models.Module.DocumentUpload.Command.Find;
using KosmosERP.BusinessLayer.Models.Module.DocumentUpload.Dto;

namespace KosmosERP.Tests.Modules;

public class DocumentUploadModuleTests : BaseTestModule<DocumentUploadModule>, IModuleTest
{
    private DocumentUploadObject _DocumentUploadObject;
    private Dictionary<string, DocumentUploadObjectTagTemplate> _ObjectTags = new Dictionary<string, DocumentUploadObjectTagTemplate>();

    [SetUp]
    public async Task SetupModule()
    {
        var the_module = new DocumentUploadModule(base._Context);

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
        var read_permission = await _Context.ModulePermissions.FirstAsync(m => m.module_id == _Module.ModuleIdentifier.ToString() && m.internal_permission_name == DocumentPermissions.Read);
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
        var create_permission = await _Context.ModulePermissions.FirstAsync(m => m.module_id == _Module.ModuleIdentifier.ToString() && m.internal_permission_name == DocumentPermissions.Create);
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
        var edit_permission = await _Context.ModulePermissions.FirstAsync(m => m.module_id == _Module.ModuleIdentifier.ToString() && m.internal_permission_name == DocumentPermissions.Edit);
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
        var delete_permission = await _Context.ModulePermissions.FirstAsync(m => m.module_id == _Module.ModuleIdentifier.ToString() && m.internal_permission_name == DocumentPermissions.Delete);
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
        if (_DocumentUploadObject != null)
            return;

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

        _ObjectTags.Add(object_tag1.name, object_tag1);
        _ObjectTags.Add(object_tag2.name, object_tag2);
    }

    [Test]
    public async Task Get()
    {
        var new_result = await _Module.CreateOverride(new MockFileUpload()
        {
            Name = "Fake file.txt",
            FileName = "Fake file.txt",
            ContentType = "text"
        }, new DocumentUploadCreateCommand()
        {
            calling_user_id = 1,
            token = _SessionId,
            document_name = "A cool document 1",
            document_object_id = _DocumentUploadObject.id,
            revision_tags = new List<DocumentUploadRevisionTagCreateCommand>()
            {
                new DocumentUploadRevisionTagCreateCommand()
                {
                    document_upload_object_tag_id = _ObjectTags["Invoice Number"].id,
                    is_required=true,
                    tag_name = _ObjectTags["Invoice Number"].name,
                    tag_value = "12345"
                },
                new DocumentUploadRevisionTagCreateCommand()
                {
                    document_upload_object_tag_id = _ObjectTags["Customer Number"].id,
                    is_required=true,
                    tag_name = _ObjectTags["Customer Number"].name,
                    tag_value = "667788"
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
        var create_command = new DocumentUploadCreateCommand()
        {
            calling_user_id = 1,
            token = _SessionId,
            document_name = "A cool document 2",
            document_object_id = _DocumentUploadObject.id,
            revision_tags = new List<DocumentUploadRevisionTagCreateCommand>()
            {
                new DocumentUploadRevisionTagCreateCommand()
                {
                    document_upload_object_tag_id = _ObjectTags["Invoice Number"].id,
                    is_required=true,
                    tag_name = _ObjectTags["Invoice Number"].name,
                    tag_value = "11111"
                },
                new DocumentUploadRevisionTagCreateCommand()
                {
                    document_upload_object_tag_id = _ObjectTags["Customer Number"].id,
                    is_required=true,
                    tag_name = _ObjectTags["Customer Number"].name,
                    tag_value = "22222"
                }
            }
        };

        var result = await _Module.CreateOverride(new MockFileUpload()
        {
            Name = "Fake file.txt",
            FileName = "Fake file.txt",
            ContentType = "text"
        }, create_command);

        ValidateMostDtoFields(result);

        Assert.NotZero(result.Data.document_revisions.Count());

    }

    [Test]
    public async Task Edit()
    {
        var new_result = await _Module.CreateOverride(new MockFileUpload()
        {
            Name = "Fake file.txt",
            FileName = "Fake file.txt",
            ContentType = "text"
        }, new DocumentUploadCreateCommand()
        {
            calling_user_id = 1,
            token = _SessionId,
            document_name = "Fake file.txt",
            document_object_id = _DocumentUploadObject.id,
            revision_tags = new List<DocumentUploadRevisionTagCreateCommand>()
            {
                new DocumentUploadRevisionTagCreateCommand()
                {
                    document_upload_object_tag_id = _ObjectTags["Invoice Number"].id,
                    is_required=true,
                    tag_name = _ObjectTags["Invoice Number"].name,
                    tag_value = "9999"
                },
                new DocumentUploadRevisionTagCreateCommand()
                {
                    document_upload_object_tag_id = _ObjectTags["Customer Number"].id,
                    is_required=true,
                    tag_name = _ObjectTags["Customer Number"].name,
                    tag_value = "88888"
                }
            }
        });

        Assert.IsTrue(new_result.Success);
        Assert.IsNotNull(new_result.Data);

        var edit_command = new DocumentUploadEditCommand()
        {
            calling_user_id = 1,
            token = _SessionId,
            id = new_result.Data.id,
            document_name = "Fake file1.txt",
            document_object_id = _DocumentUploadObject.id,
            revision_tags = new List<DocumentUploadRevisionTagCreateCommand>()
            {
                new DocumentUploadRevisionTagCreateCommand()
                {
                    document_upload_object_tag_id = _ObjectTags["Invoice Number"].id,
                    is_required=true,
                    tag_name = _ObjectTags["Invoice Number"].name,
                    tag_value = "90876"
                },
                new DocumentUploadRevisionTagCreateCommand()
                {
                    document_upload_object_tag_id = _ObjectTags["Customer Number"].id,
                    is_required=true,
                    tag_name = _ObjectTags["Customer Number"].name,
                    tag_value = "54321"
                }
            }
        };

        var edit_result = await _Module.CreateNewFileRevision(new MockFileUpload()
        {
            Name = "Fake file1.txt",
            FileName = "Fake file2.txt",
            ContentType = "text"
        }, edit_command);

        ValidateMostDtoFields(edit_result);

        var latest_revision = edit_result.Data.document_revisions.Where(m => m.rev_num == 2).FirstOrDefault();
        Assert.NotNull(latest_revision);
        Assert.NotZero(edit_result.Data.document_revisions.Count());

        Assert.That(edit_result.Data.rev_num == 2);
        Assert.That(edit_result.Data.document_object_id == _DocumentUploadObject.id);
        Assert.That(latest_revision.document_name == edit_command.document_name);
        Assert.That(latest_revision.revision_tags.Count() == 2);
    }

    [Test]
    public async Task Delete()
    {
        var new_result = await _Module.CreateOverride(new MockFileUpload()
        {
            Name = "Fake file.txt",
            FileName = "Fake file.txt",
            ContentType = "text"
        }, new DocumentUploadCreateCommand()
        {
            calling_user_id = 1,
            token = _SessionId,
            document_name = "A cool document 99",
            document_object_id = _DocumentUploadObject.id,
        });

        Assert.IsTrue(new_result.Success);
        Assert.IsNotNull(new_result.Data);

        var delete_result = await _Module.Delete(new DocumentUploadDeleteCommand()
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
        var new_result = await _Module.CreateOverride(new MockFileUpload()
        {
            Name = "Fake file.txt",
            FileName = "Fake file.txt",
            ContentType = "text"
        }, new DocumentUploadCreateCommand()
        {
            calling_user_id = 1,
            token = _SessionId,
            document_name = "Fake file.txt",
            document_object_id = _DocumentUploadObject.id,
        });

        Assert.IsTrue(new_result.Success);
        Assert.IsNotNull(new_result.Data);

        var results = await _Module.Find(
                        new PagingSortingParameters() { ResultCount = 20, Start = 0 },
                        new DocumentUploadFindCommand() { calling_user_id = 1, token = _SessionId, wildcard = "Fake file" });
        
        Assert.IsTrue(results.Success);
        Assert.IsNotNull(results.Data);
        Assert.NotZero(results.Data.Count());

        var first_result = results.Data.First();

        ValidateMostListFields(first_result);
    }

    private void ValidateMostDtoFields(Response<DocumentUploadDto> result)
    {
        Assert.IsTrue(result.Success);
        Assert.IsNotNull(result.Data);
        Assert.NotZero(result.Data.id);
        Assert.IsNotEmpty(result.Data.guid);
        Assert.NotZero(result.Data.document_object_id);
        Assert.NotZero(result.Data.rev_num);
        Assert.NotZero(result.Data.document_revisions.Count());
        Assert.NotZero(result.Data.created_by);
        Assert.NotNull(result.Data.created_on);
        Assert.NotNull(result.Data.created_on_string);
        Assert.NotNull(result.Data.created_on_timezone);
        Assert.NotNull(result.Data.updated_by);
        Assert.NotNull(result.Data.updated_on);
        Assert.NotNull(result.Data.updated_on_string);
        Assert.NotNull(result.Data.updated_on_timezone);
    }

    private void ValidateMostListFields(DocumentUploadListDto result)
    {
        Assert.NotZero(result.id);
        Assert.IsNotEmpty(result.guid);
        Assert.NotZero(result.document_object_id);
        Assert.NotZero(result.rev_num);
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