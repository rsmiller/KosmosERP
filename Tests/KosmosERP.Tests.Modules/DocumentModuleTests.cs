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
using KosmosERP.BusinessLayer;

namespace KosmosERP.Tests.Modules;

public class DocumentUploadModuleTests : BaseTestModule<DocumentUploadModule>, IModuleTest
{
    private DocumentUploadObject _DocumentUploadObject;
    private Dictionary<string, DocumentUploadObjectTagTemplate> _ObjectTags = new Dictionary<string, DocumentUploadObjectTagTemplate>();

    [SetUp]
    public async Task SetupModule()
    {
        var logProviderFactory = new LogProviderFactory(new LogProviderSettings() { log_provider = LogProviderType.MOCK }, _Context, null);
        var the_module = new DocumentUploadModule(base._Context, logProviderFactory);

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
        var role = await _Context.Roles.Where(m => m.name == "Module Admin").FirstAsync();

        var role_module_permission = CommonDataHelper<RolePermission>.FillCommonFields(new RolePermission()
        {
            module_id = _Module.ModuleIdentifier.ToString(),
            role_id = role.id,
            read = true
        }, 1);

        _Context.RolePermissions.Add(role_module_permission);
        await _Context.SaveChangesAsync();
    }

    protected override async Task SetupCreatePermissions()
    {
        var role = await _Context.Roles.Where(m => m.name == "Module Admin").FirstAsync();

        var role_module_permission = CommonDataHelper<RolePermission>.FillCommonFields(new RolePermission()
        {
            module_id = _Module.ModuleIdentifier.ToString(),
            role_id = role.id,
            write = true
        }, 1);

        _Context.RolePermissions.Add(role_module_permission);
        await _Context.SaveChangesAsync();
    }

    protected override async Task SetupEditPermissions()
    {
        var role = await _Context.Roles.Where(m => m.name == "Module Admin").FirstAsync();

        var role_module_permission = CommonDataHelper<RolePermission>.FillCommonFields(new RolePermission()
        {
            module_id = _Module.ModuleIdentifier.ToString(),
            role_id = role.id,
            edit = true
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
            delete = true
        }, 1);

        _Context.RolePermissions.Add(role_module_permission);
        await _Context.SaveChangesAsync();
    }

    protected override async Task SetupData()
    {
        if (_DocumentUploadObject != null)
            return;

        var document_upload_object = CommonDataHelper<DocumentUploadObject>.FillCommonFields(new DocumentUploadObject()
        {
            internal_name = "AR_Invoice",
            friendly_name = "AR Invoice",
        }, 1);

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
            calling_user_id = _User.external_id,
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

        Assert.That(new_result.Success, Is.True);
        Assert.That(new_result.Data, Is.Not.Null);

        var result = await _Module.GetDto(new_result.Data.id);

        ValidateMostDtoFields(result);
    }

    [Test]
    public async Task Create()
    {
        var create_command = new DocumentUploadCreateCommand()
        {
            calling_user_id = _User.external_id,
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

        Assert.That(result.Data.document_revisions.Count(), Is.Not.Zero);

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
            calling_user_id = _User.external_id,
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

        Assert.That(new_result.Success, Is.True);
        Assert.That(new_result.Data, Is.Not.Null);

        var edit_command = new DocumentUploadEditCommand()
        {
            calling_user_id = _User.external_id,
            id = new_result.Data.id,
            document_name = "Fake file2.txt",
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
        Assert.That(latest_revision, Is.Not.Null);
        Assert.That(edit_result.Data.document_revisions.Count(), Is.Not.Zero);

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
            calling_user_id = _User.external_id,
            document_name = "A cool document 99",
            document_object_id = _DocumentUploadObject.id,
        });

        Assert.That(new_result.Success, Is.True);
        Assert.That(new_result.Data, Is.Not.Null);

        var delete_result = await _Module.Delete(new DocumentUploadDeleteCommand()
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
        var new_result = await _Module.CreateOverride(new MockFileUpload()
        {
            Name = "Fake file.txt",
            FileName = "Fake file.txt",
            ContentType = "text"
        }, new DocumentUploadCreateCommand()
        {
            calling_user_id = _User.external_id,
            document_name = "Fake file.txt",
            document_object_id = _DocumentUploadObject.id,
        });

        Assert.That(new_result.Success, Is.True);
        Assert.That(new_result.Data, Is.Not.Null);

        var results = await _Module.Find(
                        new PagingSortingParameters() { ResultCount = 20, Start = 0 },
                        new DocumentUploadFindCommand() { calling_user_id = _User.external_id, wildcard = "Fake file" });
        
        Assert.That(results.Success, Is.True);
        Assert.That(results.Data, Is.Not.Null);
        Assert.That(results.Data.Count(), Is.Not.Zero);

        var first_result = results.Data.First();

        ValidateMostListFields(first_result);
    }

    private void ValidateMostDtoFields(Response<DocumentUploadDto> result)
    {
        Assert.That(result.Success, Is.True);
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data.id, Is.Not.Zero);
        Assert.That(result.Data.guid, Is.Not.Empty);
        Assert.That(result.Data.document_object_id, Is.Not.Zero);
        Assert.That(result.Data.rev_num, Is.Not.Zero);
        Assert.That(result.Data.document_revisions.Count(), Is.Not.Zero);
        Assert.That(result.Data.created_by, Is.Not.Zero);
        Assert.That(result.Data.created_on, Is.GreaterThan(DateTime.MinValue));
        Assert.That(result.Data.created_on_string, Is.Not.Null);
        Assert.That(result.Data.created_on_timezone, Is.Not.Null);
        Assert.That(result.Data.updated_by, Is.Not.Null);
        Assert.That(result.Data.updated_on, Is.Not.Null);
        Assert.That(result.Data.updated_on_string, Is.Not.Null);
        Assert.That(result.Data.updated_on_timezone, Is.Not.Null);
    }

    private void ValidateMostListFields(DocumentUploadListDto result)
    {
        Assert.That(result.id, Is.Not.Zero);
        Assert.That(result.guid, Is.Not.Empty);
        Assert.That(result.document_object_id, Is.Not.Zero);
        Assert.That(result.rev_num, Is.Not.Zero);
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