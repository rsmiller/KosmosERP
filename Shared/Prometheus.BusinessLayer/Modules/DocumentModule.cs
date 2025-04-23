using Prometheus.Database;
using Prometheus.Database.Models;
using Prometheus.Models;
using Prometheus.Models.Interfaces;
using Prometheus.Models.Helpers;
using Prometheus.Module;
using Microsoft.AspNetCore.Http;
using Prometheus.Models.Permissions;
using Microsoft.EntityFrameworkCore;
using Prometheus.BusinessLayer.Helpers;
using Prometheus.BusinessLayer.Interfaces;
using Prometheus.BusinessLayer.Models.Module.DocumentUpload.Dto;
using Prometheus.BusinessLayer.Models.Module.DocumentUpload.Command.Create;
using Prometheus.BusinessLayer.Models.Module.DocumentUpload.Command.Delete;
using Prometheus.BusinessLayer.Models.Module.DocumentUpload.Command.Edit;
using Prometheus.BusinessLayer.Models.Module.DocumentUpload.Command.Find;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;


namespace Prometheus.BusinessLayer.Modules;

public interface IDocumentUploadModule : IERPModule<DocumentUpload, DocumentUploadDto, DocumentUploadListDto, DocumentUploadCreateCommand, DocumentUploadEditCommand, DocumentUploadDeleteCommand, DocumentUploadFindCommand>, IBaseERPModule
{
	Task<Response<DocumentUploadDto>> CreateOverride(IFormFile file, DocumentUploadCreateCommand commandModel);
    Task<byte[]?> GetFile(int document_revision_id);
    
}

public class DocumentUploadModule : BaseERPModule, IDocumentUploadModule
{
    public override Guid ModuleIdentifier => Guid.Parse("4b0ce064-9c4b-4e39-8812-79cc3f69e945");
    public override string ModuleName => "Document Uploads";

    private IBaseERPContext _Context;
    private IStorageProvider _StorageProvider;

    public DocumentUploadModule(IBaseERPContext context) : base(context)
    {
        _Context = context;
    }

    public DocumentUploadModule(IBaseERPContext context, IStorageProvider storageProvider) : base(context)
    {
        _Context = context;
        _StorageProvider = storageProvider;

    }

    public override void SeedPermissions()
    {
        var role = _Context.Roles.Any(m => m.name == "Document Users");
        var read_permission = _Context.ModulePermissions.Any(m => m.module_id == this.ModuleIdentifier.ToString() && m.internal_permission_name == DocumentPermissions.Read);
        var create_permission = _Context.ModulePermissions.Any(m => m.module_id == this.ModuleIdentifier.ToString() && m.internal_permission_name == DocumentPermissions.Create);
        var edit_permission = _Context.ModulePermissions.Any(m => m.module_id == this.ModuleIdentifier.ToString() && m.internal_permission_name == DocumentPermissions.Edit);
        var delete_permission = _Context.ModulePermissions.Any(m => m.module_id == this.ModuleIdentifier.ToString() && m.internal_permission_name == DocumentPermissions.Delete);

        if (role == false)
        {
            _Context.Roles.Add(new Role()
            {
                name = "Document Users",
                created_by = 1,
                created_on = DateTime.UtcNow,
                updated_by = 1,
                updated_on = DateTime.UtcNow,
            });

            _Context.SaveChanges();
        }

        var role_id = _Context.Roles.Where(m => m.name == "Customer Users").Select(m => m.id).Single();

        if (read_permission == false)
        {
            _Context.ModulePermissions.Add(new ModulePermission()
            {
                permission_name = "Read Document",
                internal_permission_name = DocumentPermissions.Read,
                module_id = this.ModuleIdentifier.ToString(),
                module_name = this.ModuleName,
                read = true,
            });

            _Context.SaveChanges();

            var read_perm_id = _Context.ModulePermissions.Where(m => m.internal_permission_name == DocumentPermissions.Read).Select(m => m.id).Single();

            _Context.RolePermissions.Add(new RolePermission()
            {
                role_id = role_id,
                module_permission_id = read_perm_id,
                created_by = 1,
                created_on = DateTime.UtcNow,
                updated_by = 1,
                updated_on = DateTime.UtcNow,
            });

            _Context.SaveChanges();
        }

        if (create_permission == false)
        {
            _Context.ModulePermissions.Add(new ModulePermission()
            {
                permission_name = "Create Document",
                internal_permission_name = DocumentPermissions.Create,
                module_id = this.ModuleIdentifier.ToString(),
                module_name = this.ModuleName,
                write = true
            });

            _Context.SaveChanges();

            var create_perm_id = _Context.ModulePermissions.Where(m => m.internal_permission_name == DocumentPermissions.Create).Select(m => m.id).Single();

            _Context.RolePermissions.Add(new RolePermission()
            {
                role_id = role_id,
                module_permission_id = create_perm_id,
                created_by = 1,
                created_on = DateTime.UtcNow,
                updated_by = 1,
                updated_on = DateTime.UtcNow,
            });

            _Context.SaveChanges();
        }

        if (edit_permission == false)
        {
            _Context.ModulePermissions.Add(new ModulePermission()
            {
                permission_name = "Edit Document",
                internal_permission_name = DocumentPermissions.Delete,
                module_id = this.ModuleIdentifier.ToString(),
                module_name = this.ModuleName,
                edit = true
            });

            _Context.SaveChanges();

            var edit_perm_id = _Context.ModulePermissions.Where(m => m.internal_permission_name == DocumentPermissions.Edit).Select(m => m.id).Single();

            _Context.RolePermissions.Add(new RolePermission()
            {
                role_id = role_id,
                module_permission_id = edit_perm_id,
                created_by = 1,
                created_on = DateTime.UtcNow,
                updated_by = 1,
                updated_on = DateTime.UtcNow,
            });

            _Context.SaveChanges();
        }

        if (delete_permission == false)
        {
            _Context.ModulePermissions.Add(new ModulePermission()
            {
                permission_name = "Delete Document",
                internal_permission_name = DocumentPermissions.Delete,
                module_id = this.ModuleIdentifier.ToString(),
                module_name = this.ModuleName,
                delete = true
            });

            _Context.SaveChanges();

            var delete_perm_id = _Context.ModulePermissions.Where(m => m.internal_permission_name == DocumentPermissions.Delete).Select(m => m.id).Single();

            _Context.RolePermissions.Add(new RolePermission()
            {
                role_id = role_id,
                module_permission_id = delete_perm_id,
                created_by = 1,
                created_on = DateTime.UtcNow,
                updated_by = 1,
                updated_on = DateTime.UtcNow,
            });

            _Context.SaveChanges();
        }
    }

    public async Task<Response<DocumentUploadDto>> CreateOverride(IFormFile file, DocumentUploadCreateCommand commandModel)
    {
        throw new NotImplementedException();
    }


    public async Task<Response<DocumentUploadDto>> Create(DocumentUploadCreateCommand commandModel)
    {
        throw new NotImplementedException();
    }

    public async Task<Response<DocumentUploadDto>> Delete(DocumentUploadDeleteCommand commandModel)
    {
        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<DocumentUploadDto>(validationResult.Exception, ResultCode.DataValidationError);

        var permission_result = await base.HasPermission(commandModel.calling_user_id, commandModel.token, DocumentPermissions.Delete, delete: true);
        if (!permission_result)
            return new Response<DocumentUploadDto>("Invalid permission", ResultCode.InvalidPermission);

        var existingEntity = await _Context.DocumentUploads.SingleOrDefaultAsync(m => m.id == commandModel.id);
        if (existingEntity == null)
            return new Response<DocumentUploadDto>("Document not found", ResultCode.NotFound);


        // Delete
        existingEntity = CommonDataHelper<DocumentUpload>.FillDeleteFields(existingEntity, commandModel.calling_user_id);

        _Context.DocumentUploads.Update(existingEntity);
        await _Context.SaveChangesAsync();

        var dto = await MapToDto(existingEntity);
        return new Response<DocumentUploadDto>(dto);
    }

    public async Task<Response<DocumentUploadDto>> Edit(DocumentUploadEditCommand commandModel)
    {
        throw new NotImplementedException();
    }

    public async Task<PagingResult<DocumentUploadListDto>> Find(PagingSortingParameters parameters, DocumentUploadFindCommand commandModel)
    {
        PagingResult<DocumentUploadListDto> response = new PagingResult<DocumentUploadListDto>();

        try
        {
            var permission_result = await base.HasPermission(commandModel.calling_user_id, commandModel.token, DocumentPermissions.Read, read: true);
            if (!permission_result)
                return new PagingResult<DocumentUploadListDto>("Invalid permission", ResultCode.InvalidPermission);

            List<DocumentUploadListDto> dtos = new List<DocumentUploadListDto>();

            if (!String.IsNullOrEmpty(commandModel.wildcard))
            {
                var results = (from du in _Context.DocumentUploads
                               join dur in _Context.DocumentUploadRevisions on du.id equals dur.document_upload_id
                               join durt in _Context.DocumentUploadRevisionsTags on dur.id equals durt.document_upload_revision_id
                               where durt.is_deleted == false
                               && dur.is_deleted == false
                               && du.is_deleted == false
                               && (durt.tag_value.ToLower().Contains(commandModel.wildcard.ToLower())
                               || dur.document_name.ToLower().Contains(commandModel.wildcard.ToLower()))
                               select du);


                var sortedResults = await results.SortAndPageBy(parameters).ToListAsync();

                foreach (var result in results)
                    dtos.Add(await this.MapToListDto(result));

                response.Data = dtos;
                response.TotalResultCount = results.Count();
            }
            else
            {
                response.TotalResultCount = 0;
            }

        }
        catch (Exception ex)
        {
            await LogError(50, this.GetType().Name, "Find", ex);

            response.TotalResultCount = 0;
            response.SetException(ex.Message, ResultCode.Error);
        }

        return response;
    }

    public DocumentUpload? Get(int object_id)
    {
        return _Context.DocumentUploads.SingleOrDefault(m => m.id == object_id);
    }

    public async Task<DocumentUpload?> GetAsync(int object_id)
    {
        return await _Context.DocumentUploads.SingleOrDefaultAsync(m => m.id == object_id);
    }

    public async Task<Response<DocumentUploadDto>> GetDto(int object_id)
    {
        Response<DocumentUploadDto> response = new Response<DocumentUploadDto>();

        var result = await _Context.DocumentUploads.SingleOrDefaultAsync(m => m.id == object_id);
        if (result == null)
        {
            response.SetException("Document Upload not found", ResultCode.NotFound);
            return response;
        }

        response.Data = await this.MapToDto(result);

        return response;
    }

    public async Task<byte[]?> GetFile(int document_revision_id)
    {
        var current_revision = await _Context.DocumentUploadRevisions.Where(m => m.id == document_revision_id && m.is_deleted == false).FirstOrDefaultAsync();

        if (current_revision != null)
        {
            // Get the file contents
            return await _StorageProvider.GetFileAsync(current_revision.document_path);
            
        }
        else
        {
            return null;
        }
    }

	public async Task<Response<List<DocumentUploadListDto>>> GlobalSearch(PagingSortingParameters parameters, string wildcard)
	{
		throw new NotImplementedException();
	}

	public DocumentUpload MapToDatabaseModel(DocumentUploadDto dtoModel)
	{
		throw new NotImplementedException();
	}

    public DocumentUpload MapForCreate(DocumentUploadCreateCommand command, int calling_user_id)
    {
        var document = CommonDataHelper<DocumentUpload>.FillCommonFields(new DocumentUpload()
        {
            document_object_id = command.document_object_id,
            rev_num = 1,
        }, calling_user_id);

        return document;
    }

	public async Task<DocumentUploadDto> MapToDto(DocumentUpload databaseModel)
	{
        return new DocumentUploadDto()
        {
            id = databaseModel.id,
            document_object_id = databaseModel.document_object_id,
            rev_num = databaseModel.rev_num,
            guid = databaseModel.guid,
            is_deleted = databaseModel.is_deleted,
            created_on = databaseModel.created_on,
            created_by = databaseModel.created_by,
            updated_on = databaseModel.updated_on,
            updated_by = databaseModel.updated_by,
            updated_on_string = databaseModel.updated_on_string,
            updated_on_timezone = databaseModel.updated_on_timezone,
            deleted_by = databaseModel.deleted_by,
            deleted_on = databaseModel.deleted_on,
            deleted_on_string = databaseModel.deleted_on_string,
            deleted_on_timezone = databaseModel.deleted_on_timezone,
            created_on_string = databaseModel.created_on_string,
            created_on_timezone = databaseModel.created_on_timezone,
        };
    }

	public async Task<DocumentUploadListDto> MapToListDto(DocumentUpload databaseModel)
	{
        return new DocumentUploadListDto()
        {
            id = databaseModel.id,
            document_object_id = databaseModel.document_object_id,
            rev_num = databaseModel.rev_num,
            guid = databaseModel.guid,
            is_deleted = databaseModel.is_deleted,
            created_on = databaseModel.created_on,
            created_by = databaseModel.created_by,
            updated_on = databaseModel.updated_on,
            updated_by = databaseModel.updated_by,
            updated_on_string = databaseModel.updated_on_string,
            updated_on_timezone = databaseModel.updated_on_timezone,
            deleted_by = databaseModel.deleted_by,
            deleted_on = databaseModel.deleted_on,
            deleted_on_string = databaseModel.deleted_on_string,
            deleted_on_timezone = databaseModel.deleted_on_timezone,
            created_on_string = databaseModel.created_on_string,
            created_on_timezone = databaseModel.created_on_timezone,
        };
    }
}
