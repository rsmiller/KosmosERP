using KosmosERP.Database;
using KosmosERP.Database.Models;
using KosmosERP.Models;
using KosmosERP.Models.Interfaces;
using KosmosERP.Models.Helpers;
using KosmosERP.Module;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using KosmosERP.BusinessLayer.Helpers;
using KosmosERP.BusinessLayer.Models.Module.DocumentUpload.Dto;
using KosmosERP.BusinessLayer.Models.Module.DocumentUpload.Command.Create;
using KosmosERP.BusinessLayer.Models.Module.DocumentUpload.Command.Delete;
using KosmosERP.BusinessLayer.Models.Module.DocumentUpload.Command.Edit;
using KosmosERP.BusinessLayer.Models.Module.DocumentUpload.Command.Find;
using KosmosERP.BusinessLayer.StorageProviders;

namespace KosmosERP.BusinessLayer.Modules;

public interface IDocumentUploadModule : IERPModule<DocumentUpload, DocumentUploadDto, DocumentUploadListDto, DocumentUploadCreateCommand, DocumentUploadEditCommand, DocumentUploadDeleteCommand, DocumentUploadFindCommand>, IBaseERPModule
{
    Task<Response<DocumentUploadDto>> CreateOverride(IFormFile file, DocumentUploadCreateCommand commandModel);
    Task<Response<DocumentUploadDto>> CreateNewFileRevision(IFormFile file, DocumentUploadEditCommand commandModel);
    Task<Tuple<byte[]?, DocumentUploadRevisionDto>> GetFile(int document_revision_id);
    Task<Tuple<byte[]?, DocumentUploadRevisionDto>> GetFileByGuid(string document_revision_guid);
    Task<PagingResult<DocumentUploadDto>> FindOverride(PagingSortingParameters parameters, DocumentUploadFindCommand commandModel);
    Task<Response<List<DocumentUploadCategoryDto>>> GetObjectCategories();
    Task<Response<DocumentUploadCategoryDto>> CreateCategory(DocumentUploadCategoryCreateCommand commandModel);
    Task<Response<DocumentUploadCategoryDto>> EditCategory(DocumentUploadCategoryEditCommand commandModel);
    Task<Response<DocumentUploadCategoryDto>> DeleteCategory(DocumentUploadCategoryDeleteCommand commandModel);
    Task<Response<List<DocumentUploadObjectDto>>> GetUploadObjects();
    Task<Response<DocumentUploadObjectDto>> EditUploadObject(DocumentUploadObjectEditCommand commandModel);
    Task<Response<DocumentUploadObjectDto>> CreateUploadObject(DocumentUploadObjectCreateCommand commandModel);
    Task<Response<DocumentUploadObjectDto>> DeleteUploadObject(DocumentUploadObjectDeleteCommand commandModel);
    Task<Response<List<DocumentUploadObjectTagDto>>> GetDocumentObjectTags(int document_object_id);
    Task<Response<DocumentUploadObjectTagDto>> EditDocumentObjectTags(DocumentUploadObjectTagEditCommand commandModel);
    Task<Response<DocumentUploadObjectTagDto>> CreateDocumentObjectTags(DocumentUploadObjectTagCreateCommand commandModel);
    Task<Response<DocumentUploadObjectTagDto>> DeleteDocumentObjectTags(DocumentUploadObjectTagDeleteCommand commandModel);
}

public class DocumentUploadModule : BaseERPModule, IDocumentUploadModule
{
    public override Guid ModuleIdentifier => Guid.Parse("4b0ce064-9c4b-4e39-8812-79cc3f69e945");
    public override string ModuleName => "Document Uploads";

    private IBaseERPContext _Context;
    private IStorageProvider _StorageProvider;

    public DocumentUploadModule(IBaseERPContext context, ILogProviderFactory logProviderFactory) : base(logProviderFactory)
    {
        _Context = context;
    }

    public DocumentUploadModule(IBaseERPContext context, IStorageProvider storageProvider, ILogProviderFactory logProviderFactory) : base(logProviderFactory)
    {
        _Context = context;
        _StorageProvider = storageProvider;

    }

    public override void SeedPermissions()
    {
        var role = _Context.Roles.Any(m => m.name == "Document Administrators");

        if (role == false)
        {
            _Context.Roles.Add(CommonDataHelper<Role>.FillCommonFields(new Role()
            {
                name = "Document Administrators",
            }, 1));

            _Context.SaveChanges();
        }
    }

    public async Task<Response<DocumentUploadDto>> CreateOverride(IFormFile file, DocumentUploadCreateCommand commandModel)
    {

        try
        {
            var validationResult = ModelValidationHelper.ValidateModel(commandModel);
            if (!validationResult.Success)
                return new Response<DocumentUploadDto>(validationResult.Exception, ResultCode.DataValidationError);


            var new_document = this.MapForCreate(commandModel, commandModel.calling_user_id);

            _Context.DocumentUploads.Add(new_document);
            await _Context.SaveChangesAsync();


            // Upload the document to get the 'path'
            string filePath = "";
            string extention = "";

            if (commandModel.document_name.Contains("."))
                extention = commandModel.document_name.Substring(commandModel.document_name.LastIndexOf("."));


            string internalId = Guid.NewGuid().ToString().ToLower() + extention;


            if (_StorageProvider != null && _StorageProvider.GetType() != typeof(MockStorageProvider))
            {
                using (var memoryStream = new MemoryStream())
                {
                    await file.CopyToAsync(memoryStream);
                    memoryStream.Position = 0;

                    filePath = await _StorageProvider.UploadFileAsync(memoryStream.ToArray(), internalId);
                }
            }

            var revision = this.MapForRevisionCreate(file, new_document, filePath, new_document.rev_num, commandModel.calling_user_id);
            revision.document_name = commandModel.document_name;

            _Context.DocumentUploadRevisions.Add(revision);
            await _Context.SaveChangesAsync();

            foreach (var tag in commandModel.revision_tags)
            {
                var newTag = this.MapForRevisionTagCreate(tag, revision, commandModel.calling_user_id);

                _Context.DocumentUploadRevisionsTags.Add(newTag);
                await _Context.SaveChangesAsync();
            }


            var dto = await MapToDto(new_document);
            return new Response<DocumentUploadDto>(dto);
        }
        catch (Exception ex)
        {
            await LogError(50, this.GetType().Name, "Create", ex);
            return new Response<DocumentUploadDto>(ex.Message, ResultCode.Error);
        }
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

        var existingEntity = await _Context.DocumentUploads.SingleOrDefaultAsync(m => m.id == commandModel.id);
        if (existingEntity == null)
            return new Response<DocumentUploadDto>("Document not found", ResultCode.NotFound);


        // Delete
        existingEntity = CommonDataHelper<DocumentUpload>.FillDeleteFields(existingEntity, commandModel.calling_user_id);

        _Context.DocumentUploads.Update(existingEntity);
        await _Context.SaveChangesAsync();

        // Delete revisions
        var revisions = await _Context.DocumentUploadRevisions.Where(m => m.document_upload_id == existingEntity.id).ToListAsync();
        foreach (var revision in revisions)
        {
            var rev = CommonDataHelper<DocumentUploadRevision>.FillDeleteFields(revision, commandModel.calling_user_id);

            _Context.DocumentUploadRevisions.Update(rev);
            await _Context.SaveChangesAsync();
        }

        var dto = await MapToDto(existingEntity);
        return new Response<DocumentUploadDto>(dto);
    }

    public async Task<Response<DocumentUploadDto>> Edit(DocumentUploadEditCommand commandModel)
    {
        throw new NotImplementedException();
    }

    public async Task<Response<DocumentUploadDto>> CreateNewFileRevision(IFormFile file, DocumentUploadEditCommand commandModel)
    {
        Response<DocumentUploadDto> response = new Response<DocumentUploadDto>();

        try
        {
            var validationResult = ModelValidationHelper.ValidateModel(commandModel);
            if (!validationResult.Success)
                return new Response<DocumentUploadDto>(validationResult.Exception, ResultCode.DataValidationError);

            var existingEntity = await GetAsync(commandModel.id);
            if (existingEntity == null)
                return new Response<DocumentUploadDto>("Document not found", ResultCode.NotFound);

            int old_rev_num = existingEntity.rev_num;

            existingEntity.rev_num += 1;
            
            existingEntity = CommonDataHelper<DocumentUpload>.FillUpdateFields(existingEntity, commandModel.calling_user_id);

            _Context.DocumentUploads.Update(existingEntity);
            await _Context.SaveChangesAsync();


            // Upload the document to get the 'path'
            string filePath = "";
            string internalId = Guid.NewGuid().ToString().ToLower();

            if (_StorageProvider != null && _StorageProvider.GetType() != typeof(MockStorageProvider))
            {
                using (var memoryStream = new MemoryStream())
                {
                    await file.CopyToAsync(memoryStream);
                    memoryStream.Position = 0;

                    filePath = await _StorageProvider.UploadFileAsync(memoryStream.ToArray(), internalId);
                }
            }


            var old_revision = await _Context.DocumentUploadRevisions.Where(m => m.document_upload_id == existingEntity.id && m.rev_num == old_rev_num).FirstAsync();
            var new_revision = this.MapForRevisionCreate(file, existingEntity, filePath, existingEntity.rev_num, commandModel.calling_user_id);

            _Context.DocumentUploadRevisions.Add(new_revision);
            await _Context.SaveChangesAsync();

            //var existing_tags = await _Context.DocumentUploadRevisionsTags.Where(m => m.document_upload_revision_id == old_revision.id).ToListAsync();
            //foreach(var existing_tag in existing_tags)
            //{
            //    var copy_tag = this.CopyRevisionTag(existing_tag);
            //    copy_tag.document_upload_revision_id = new_revision.id;
            //
            //    _Context.DocumentUploadRevisionsTags.Add(copy_tag);
            //    await _Context.SaveChangesAsync();
            //}

            foreach (var tag in commandModel.revision_tags)
            {
                var newTag = this.MapForRevisionTagCreate(tag, new_revision, commandModel.calling_user_id);

                _Context.DocumentUploadRevisionsTags.Add(newTag);
                await _Context.SaveChangesAsync();
            }

            var dto = await MapToDto(existingEntity);
            return new Response<DocumentUploadDto>(dto);
        }
        catch (Exception ex)
        {
            await LogError(50, this.GetType().Name, "Edit", ex);
            response.SetException(ex);
        }

        return response;
    }

    public async Task<PagingResult<DocumentUploadDto>> FindOverride(PagingSortingParameters parameters, DocumentUploadFindCommand commandModel)
    {
        PagingResult<DocumentUploadDto> response = new PagingResult<DocumentUploadDto>();

        try
        {
            List<DocumentUploadDto> dtos = new List<DocumentUploadDto>();

            IQueryable<DocumentUpload> query = _Context.DocumentUploads.Where(m => m.is_deleted == false);

            if (!String.IsNullOrEmpty(commandModel.wildcard))
            {
                query = (from du in _Context.DocumentUploads
                         join dur in _Context.DocumentUploadRevisions on du.id equals dur.document_upload_id
                         join durt in _Context.DocumentUploadRevisionsTags on dur.id equals durt.document_upload_revision_id
                         where durt.is_deleted == false
                         && dur.is_deleted == false
                         && du.is_deleted == false
                         && dur.rev_num == du.rev_num
                         && (durt.tag_value.ToLower().Contains(commandModel.wildcard.ToLower())
                         || dur.document_name.ToLower().Contains(commandModel.wildcard.ToLower()))
                         select du);

            }

            if (commandModel.category_id.HasValue && !commandModel.object_id.HasValue)
            {
                query = (from ud in _Context.DocumentUploads
                         join duac in _Context.DocumentUploadObjectCategories on ud.document_object_id equals duac.document_upload_object_id
                         where ud.is_deleted == false
                         && duac.is_deleted == false
                         && duac.document_upload_category_id == commandModel.category_id
                         select ud);
            }

            if (!commandModel.category_id.HasValue && commandModel.object_id.HasValue)
            {
                query = (from ud in _Context.DocumentUploads
                         join duac in _Context.DocumentUploadObjectCategories on ud.document_object_id equals duac.document_upload_object_id
                         where ud.is_deleted == false
                         && duac.is_deleted == false
                         && duac.document_upload_object_id == commandModel.object_id
                         select ud);
            }

            if (commandModel.category_id.HasValue && commandModel.object_id.HasValue)
            {
                query = (from ud in _Context.DocumentUploads
                         join duac in _Context.DocumentUploadObjectCategories on ud.document_object_id equals duac.document_upload_object_id
                         where ud.is_deleted == false
                         && duac.is_deleted == false
                         && duac.document_upload_object_id == commandModel.object_id
                         && duac.document_upload_category_id == commandModel.category_id
                         select ud);
            }

            var sortedResults = await query.SortAndPageBy(parameters).ToListAsync();
            var count = query.Count();

            foreach (var result in sortedResults)
                dtos.Add(await this.MapToDto(result));

            response.Data = dtos;
            response.TotalResultCount = count;

        }
        catch (Exception ex)
        {
            await LogError(50, this.GetType().Name, "Find", ex);

            response.TotalResultCount = 0;
            response.SetException(ex.Message, ResultCode.Error);
        }

        return response;
    }

    public async Task<PagingResult<DocumentUploadListDto>> Find(PagingSortingParameters parameters, DocumentUploadFindCommand commandModel)
    {
        PagingResult<DocumentUploadListDto> response = new PagingResult<DocumentUploadListDto>();

        try
        {
            List<DocumentUploadListDto> dtos = new List<DocumentUploadListDto>();

            if (!String.IsNullOrEmpty(commandModel.wildcard))
            {
                var revisions_result = (from du in _Context.DocumentUploads
                               join dur in _Context.DocumentUploadRevisions on du.id equals dur.document_upload_id
                               where dur.is_deleted == false
                               && du.is_deleted == false
                               && dur.rev_num == du.rev_num
                               && dur.document_name.ToLower().Contains(commandModel.wildcard.ToLower())
                               select du);


                var tags_results = (from du in _Context.DocumentUploads
                              join dur in _Context.DocumentUploadRevisions on du.id equals dur.document_upload_id
                               join durt in _Context.DocumentUploadRevisionsTags on dur.id equals durt.document_upload_revision_id
                               where durt.is_deleted == false
                               && dur.is_deleted == false
                               && du.is_deleted == false
                               && dur.rev_num == du.rev_num
                               && (durt.tag_value.ToLower().Contains(commandModel.wildcard.ToLower())
                               || dur.document_name.ToLower().Contains(commandModel.wildcard.ToLower()))
                               select du);

                var results = revisions_result.Union(tags_results);


                var sortedResults = await results.SortAndPageBy(parameters).ToListAsync();
                var count = results.Count();

                foreach (var result in sortedResults)
                    dtos.Add(await this.MapToListDto(result));

                response.Data = dtos;
                response.TotalResultCount = count;
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

    public async Task<Response<DocumentUploadDto>> GetDtoByGuid(string guid)
    {
        var entity = await _Context.DocumentUploads.FirstOrDefaultAsync(c => c.guid == guid && !c.is_deleted);
        if (entity == null)
            return new Response<DocumentUploadDto>("DocumentUpload not found", ResultCode.NotFound);

        var dto = await MapToDto(entity);
        return new Response<DocumentUploadDto>(dto);
    }

    public async Task<Tuple<byte[]?, DocumentUploadRevisionDto>> GetFile(int document_revision_id)
    {
        var current_revision = await _Context.DocumentUploadRevisions.Where(m => m.id == document_revision_id && m.is_deleted == false).FirstOrDefaultAsync();

        if (current_revision != null)
        {
            // Get the file contents
            var dto = await this.MapToRevisionDto(current_revision);
            var data = await _StorageProvider.GetFileAsync(current_revision.document_path);

            return Tuple.Create<byte[]?, DocumentUploadRevisionDto>(data, dto);

        }
        else
        {
            return Tuple.Create<byte[]?, DocumentUploadRevisionDto>(null, null);
        }
    }

    public async Task<Tuple<byte[]?, DocumentUploadRevisionDto>> GetFileByGuid(string document_revision_guid)
    {
        try
        {
            var current_revision = await _Context.DocumentUploadRevisions.Where(m => m.guid == document_revision_guid && m.is_deleted == false).FirstOrDefaultAsync();

            if (current_revision != null)
            {
                // Get the file contents
                var dto = await this.MapToRevisionDto(current_revision);
                var data = await _StorageProvider.GetFileAsync(current_revision.document_path);

                return Tuple.Create<byte[]?, DocumentUploadRevisionDto>(data, dto);

            }
        }
        catch(Exception ex)
        {
            await LogError(50, this.GetType().Name, nameof(GetFileByGuid), ex);
        }
        
        return Tuple.Create<byte[]?, DocumentUploadRevisionDto>(null, null);
    }

    public async Task<Response<List<DocumentUploadListDto>>> GlobalSearch(GlobalSearchFindCommand commandModel)
    {
        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<List<DocumentUploadListDto>>(validationResult.Exception, ResultCode.DataValidationError);

        var response = new Response<List<DocumentUploadListDto>>();

        try
        {
            var filter = PredicateBuilder.True<DocumentUpload>();
            filter = filter.And(m => m.is_deleted == false);

            if (!string.IsNullOrEmpty(commandModel.wildcard))
            {
                var lower = commandModel.wildcard.ToLower();
                filter = filter.And(m => m.document_revisions.Where(x => x.document_name.ToLower().Contains(lower)).Any()
                    || m.document_revisions.Where(x => x.revision_tags.Where(t => t.tag_value.ToLower().Contains(lower)).Any()).Any());
            }

            var results = _Context.DocumentUploads.Where(filter);
            var pagedItems = await results.SortAndPageBy(commandModel.parameters).ToListAsync();

            var dtos = new List<DocumentUploadListDto>();
            foreach (var item in pagedItems)
                dtos.Add(await MapToListDto(item));
                

            response.Data = dtos;
        }
        catch (Exception ex)
        {
            await LogError(50, this.GetType().Name, nameof(GlobalSearch), ex);
            response.SetException(ex.Message, ResultCode.Error);
        }

        return response;
    }


    public async Task<Response<List<DocumentUploadCategoryDto>>> GetObjectCategories()
    {
        Response<List<DocumentUploadCategoryDto>> response = new Response<List<DocumentUploadCategoryDto>>();
        response.Data = new List<DocumentUploadCategoryDto>();

        var results = await _Context.DocumentUploadCategories.Where(m => !m.is_deleted).ToListAsync();
        foreach (var result in results)
            response.Data.Add(await this.MapToCategoryDto(result));


        return response;
    }

    public async Task<Response<DocumentUploadObjectDto>> EditUploadObject(DocumentUploadObjectEditCommand commandModel)
    {
        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<DocumentUploadObjectDto>(validationResult.Exception, ResultCode.DataValidationError);

        var existingEntity = await _Context.DocumentUploadObjects.Where(m => m.id == commandModel.id).SingleOrDefaultAsync();
        if (existingEntity == null)
            return new Response<DocumentUploadObjectDto>("Document Object not found", ResultCode.NotFound);

        try
        {
            // Update fields
            if (commandModel.is_deleted.HasValue)
                existingEntity.is_deleted = commandModel.is_deleted.Value;

            if (!string.IsNullOrEmpty(commandModel.friendly_name) && existingEntity.friendly_name != commandModel.friendly_name)
                existingEntity.friendly_name = commandModel.friendly_name;
            if (!string.IsNullOrEmpty(commandModel.internal_name) && existingEntity.internal_name != commandModel.internal_name)
                existingEntity.internal_name = commandModel.internal_name;

            existingEntity = CommonDataHelper<DocumentUploadObject>.FillUpdateFields(existingEntity, commandModel.calling_user_id);

            _Context.DocumentUploadObjects.Update(existingEntity);
            await _Context.SaveChangesAsync();

            var dto = await this.MapToDocumentUploadObjectDto(existingEntity);
            return new Response<DocumentUploadObjectDto>(dto);
        }
        catch (Exception ex)
        {
            await LogError(50, this.GetType().Name, "EditUploadObject", ex);
            return new Response<DocumentUploadObjectDto>(ex.Message, ResultCode.Error);
        }
    }

    public async Task<Response<DocumentUploadObjectDto>> CreateUploadObject(DocumentUploadObjectCreateCommand commandModel)
    {
        try
        {
            var validationResult = ModelValidationHelper.ValidateModel(commandModel);
            if (!validationResult.Success)
                return new Response<DocumentUploadObjectDto>(validationResult.Exception, ResultCode.DataValidationError);

            var new_duo = this.MapForDocumentUploadObjectCreate(commandModel, commandModel.calling_user_id);

            _Context.DocumentUploadObjects.Add(new_duo);
            await _Context.SaveChangesAsync();

            var dto = await this.MapToDocumentUploadObjectDto(new_duo);
            return new Response<DocumentUploadObjectDto>(dto);
        }
        catch (Exception ex)
        {
            await LogError(50, this.GetType().Name, "CreateUploadObject", ex);
            return new Response<DocumentUploadObjectDto>(ex.Message, ResultCode.Error);
        }
    }

    public async Task<Response<DocumentUploadObjectDto>> DeleteUploadObject(DocumentUploadObjectDeleteCommand commandModel)
    {

        try
        {
            var validationResult = ModelValidationHelper.ValidateModel(commandModel);
            if (!validationResult.Success)
                return new Response<DocumentUploadObjectDto>(validationResult.Exception, ResultCode.DataValidationError);

            var existingEntity = await _Context.DocumentUploadObjects.SingleOrDefaultAsync(m => m.id == commandModel.id);
            if (existingEntity == null)
                return new Response<DocumentUploadObjectDto>("Document Upload Object not found", ResultCode.NotFound);

            // Soft delete
            existingEntity = CommonDataHelper<DocumentUploadObject>.FillDeleteFields(existingEntity, commandModel.calling_user_id);

            _Context.DocumentUploadObjects.Update(existingEntity);
            await _Context.SaveChangesAsync();

            var dto = await this.MapToDocumentUploadObjectDto(existingEntity);
            return new Response<DocumentUploadObjectDto>(dto);
        }
        catch (Exception ex)
        {
            await LogError(50, this.GetType().Name, "DeleteUploadObject", ex);
            return new Response<DocumentUploadObjectDto>(ex.Message, ResultCode.Error);
        }
    }

    public async Task<Response<DocumentUploadCategoryDto>> CreateCategory(DocumentUploadCategoryCreateCommand commandModel)
    {
        try
        {
            var validationResult = ModelValidationHelper.ValidateModel(commandModel);
            if (!validationResult.Success)
                return new Response<DocumentUploadCategoryDto>(validationResult.Exception, ResultCode.DataValidationError);


            var new_category = this.MapForCategoryCreate(commandModel, commandModel.calling_user_id);

            _Context.DocumentUploadCategories.Add(new_category);
            await _Context.SaveChangesAsync();

            var dto = await this.MapToCategoryDto(new_category);
            return new Response<DocumentUploadCategoryDto>(dto);
        }
        catch (Exception ex)
        {
            await LogError(50, this.GetType().Name, "CreateObjectCategory", ex);
            return new Response<DocumentUploadCategoryDto>(ex.Message, ResultCode.Error);
        }
    }

    public async Task<Response<DocumentUploadCategoryDto>> EditCategory(DocumentUploadCategoryEditCommand commandModel)
    {
        try
        {
            var validationResult = ModelValidationHelper.ValidateModel(commandModel);
            if (!validationResult.Success)
                return new Response<DocumentUploadCategoryDto>(validationResult.Exception, ResultCode.DataValidationError);

            var existingEntity = await _Context.DocumentUploadCategories.SingleOrDefaultAsync(m => m.id == commandModel.id);
            if (existingEntity == null)
                return new Response<DocumentUploadCategoryDto>("Document Upload Object Category not found", ResultCode.NotFound);

            // Update fields
            if (commandModel.parent_category_id.HasValue)
                existingEntity.parent_category_id = commandModel.parent_category_id.Value;
            if (!string.IsNullOrEmpty(commandModel.category_name))
                existingEntity.category_name = commandModel.category_name;
            if (!string.IsNullOrEmpty(commandModel.internal_category_name))
                existingEntity.internal_category_name = commandModel.internal_category_name;

            existingEntity = CommonDataHelper<DocumentUploadCategory>.FillUpdateFields(existingEntity, commandModel.calling_user_id);

            _Context.DocumentUploadCategories.Update(existingEntity);
            await _Context.SaveChangesAsync();

            var dto = await this.MapToCategoryDto(existingEntity);
            return new Response<DocumentUploadCategoryDto>(dto);
        }
        catch (Exception ex)
        {
            await LogError(50, this.GetType().Name, "EditObjectCategory", ex);
            return new Response<DocumentUploadCategoryDto>(ex.Message, ResultCode.Error);
        }
    }

    public async Task<Response<DocumentUploadCategoryDto>> DeleteCategory(DocumentUploadCategoryDeleteCommand commandModel)
    {
        try
        {
            var validationResult = ModelValidationHelper.ValidateModel(commandModel);
            if (!validationResult.Success)
                return new Response<DocumentUploadCategoryDto>(validationResult.Exception, ResultCode.DataValidationError);

            var existingEntity = await _Context.DocumentUploadCategories.SingleOrDefaultAsync(m => m.id == commandModel.id);
            if (existingEntity == null)
                return new Response<DocumentUploadCategoryDto>("Document Upload Category not found", ResultCode.NotFound);

            // Soft delete
            existingEntity = CommonDataHelper<DocumentUploadCategory>.FillDeleteFields(existingEntity, commandModel.calling_user_id);

            _Context.DocumentUploadCategories.Update(existingEntity);
            await _Context.SaveChangesAsync();

            var dto = await this.MapToCategoryDto(existingEntity);
            return new Response<DocumentUploadCategoryDto>(dto);
        }
        catch (Exception ex)
        {
            await LogError(50, this.GetType().Name, "DeleteObjectCategory", ex);
            return new Response<DocumentUploadCategoryDto>(ex.Message, ResultCode.Error);
        }
    }

    public async Task<Response<List<DocumentUploadObjectDto>>> GetUploadObjects()
    {
        Response<List<DocumentUploadObjectDto>> response = new Response<List<DocumentUploadObjectDto>>();
        response.Data = new List<DocumentUploadObjectDto>();

        var upload_objects = await _Context.DocumentUploadObjects.Where(m => m.is_deleted == false).ToListAsync();

        foreach (var obj in upload_objects)
        {
            response.Data.Add(await this.MapToDocumentUploadObjectDto(obj));
        }

        return response;
    }

    public async Task<Response<List<DocumentUploadObjectTagDto>>> GetDocumentObjectTags(int document_object_id)
    {
        Response<List<DocumentUploadObjectTagDto>> response = new Response<List<DocumentUploadObjectTagDto>>();
        response.Data = new List<DocumentUploadObjectTagDto>();

        var results = await _Context.DocumentUploadObjectTags.Where(m => m.document_object_id == document_object_id && m.is_deleted == false).ToListAsync();

        foreach (var uot in results)
            response.Data.Add(this.MapToObjectTagDto(uot));


        return response;
    }

    public async Task<Response<DocumentUploadObjectTagDto>> EditDocumentObjectTags(DocumentUploadObjectTagEditCommand commandModel)
    {
        try
        {
            var validationResult = ModelValidationHelper.ValidateModel(commandModel);
            if (!validationResult.Success)
                return new Response<DocumentUploadObjectTagDto>(validationResult.Exception, ResultCode.DataValidationError);

            var existingEntity = await _Context.DocumentUploadObjectTags.SingleOrDefaultAsync(m => m.id == commandModel.id);
            if (existingEntity == null)
                return new Response<DocumentUploadObjectTagDto>("Document Upload Object Tag not found", ResultCode.NotFound);

            // Update fields
            if (commandModel.is_required.HasValue)
                existingEntity.is_required = commandModel.is_required.Value;
            if (!string.IsNullOrEmpty(commandModel.name) && commandModel.name != existingEntity.name)
                existingEntity.name = commandModel.name;

            //existingEntity = CommonDataHelper<DocumentUploadObjectTagTemplate>.FillUpdateFields(existingEntity, commandModel.calling_user_id);

            _Context.DocumentUploadObjectTags.Update(existingEntity);
            await _Context.SaveChangesAsync();

            var dto = this.MapToObjectTagDto(existingEntity);
            return new Response<DocumentUploadObjectTagDto>(dto);
        }
        catch (Exception ex)
        {
            await LogError(50, this.GetType().Name, "EditObjectCategory", ex);
            return new Response<DocumentUploadObjectTagDto>(ex.Message, ResultCode.Error);
        }
    }

    public async Task<Response<DocumentUploadObjectTagDto>> CreateDocumentObjectTags(DocumentUploadObjectTagCreateCommand commandModel)
    {
        try
        {
            var validationResult = ModelValidationHelper.ValidateModel(commandModel);
            if (!validationResult.Success)
                return new Response<DocumentUploadObjectTagDto>(validationResult.Exception, ResultCode.DataValidationError);


            var new_tag = this.MapForTagCreate(commandModel, commandModel.calling_user_id);

            _Context.DocumentUploadObjectTags.Add(new_tag);
            await _Context.SaveChangesAsync();

            var dto = this.MapToObjectTagDto(new_tag);
            return new Response<DocumentUploadObjectTagDto>(dto);
        }
        catch (Exception ex)
        {
            await LogError(50, this.GetType().Name, "CreateDocumentObjectTags", ex);
            return new Response<DocumentUploadObjectTagDto>(ex.Message, ResultCode.Error);
        }
    }

    public async Task<Response<DocumentUploadObjectTagDto>> DeleteDocumentObjectTags(DocumentUploadObjectTagDeleteCommand commandModel)
    {
        try
        {
            var validationResult = ModelValidationHelper.ValidateModel(commandModel);
            if (!validationResult.Success)
                return new Response<DocumentUploadObjectTagDto>(validationResult.Exception, ResultCode.DataValidationError);

            var existingEntity = await _Context.DocumentUploadObjectTags.SingleOrDefaultAsync(m => m.id == commandModel.id);
            if (existingEntity == null)
                return new Response<DocumentUploadObjectTagDto>("Document Upload Tag not found", ResultCode.NotFound);

            // Soft delete
            //existingEntity = CommonDataHelper<DocumentUploadCategory>.FillDeleteFields(existingEntity, commandModel.calling_user_id);

            existingEntity.is_deleted = true;


            _Context.DocumentUploadObjectTags.Update(existingEntity);
            await _Context.SaveChangesAsync();

            var dto = this.MapToObjectTagDto(existingEntity);
            return new Response<DocumentUploadObjectTagDto>(dto);
        }
        catch (Exception ex)
        {
            await LogError(50, this.GetType().Name, "DeleteDocumentObjectTags", ex);
            return new Response<DocumentUploadObjectTagDto>(ex.Message, ResultCode.Error);
        }
    }

    public DocumentUpload MapToDatabaseModel(DocumentUploadDto dtoModel)
    {
        throw new NotImplementedException();
    }

    public DocumentUpload MapForCreate(DocumentUploadCreateCommand command, string calling_user_id)
    {
        var document = CommonDataHelper<DocumentUpload>.FillCommonFields(new DocumentUpload()
        {
            document_object_id = command.document_object_id,
            rev_num = 1,
        }, calling_user_id);

        return document;
    }

    public DocumentUploadRevisionTag MapForTagCreate(DocumentUploadRevisionTagCreateCommand command, string calling_user_id)
    {
        var tag = CommonDataHelper<DocumentUploadRevisionTag>.FillCommonFields(new DocumentUploadRevisionTag()
        {
            tag_name = command.tag_name,
            tag_value = command.tag_value,
        }, calling_user_id);

        return tag;
    }

    public DocumentUploadRevision MapForRevisionCreate(IFormFile file, DocumentUpload documentUpload, string filePath, int rev_num, string calling_user_id)
    {
        var revision = CommonDataHelper<DocumentUploadRevision>.FillCommonFields(new DocumentUploadRevision()
        {
            rev_num = rev_num,
            document_upload_id = documentUpload.id,
            document_name = file.FileName,
            document_type = file.ContentType,
            document_path = filePath,
        }, calling_user_id);

        return revision;
    }

    public DocumentUploadRevisionTag MapForRevisionTagCreate(DocumentUploadRevisionTagCreateCommand command, DocumentUploadRevision documentRevision, string calling_user_id)
    {
        var newTag = CommonDataHelper<DocumentUploadRevisionTag>.FillCommonFields(new DocumentUploadRevisionTag()
        {
            tag_name = command.tag_name,
            tag_value = command.tag_value,
            document_upload_object_tag_id = command.document_upload_object_tag_id,
            document_upload_revision_id = documentRevision.id
        }, calling_user_id);

        return newTag;
    }

    public DocumentUploadObjectTagTemplate MapForTagCreate(DocumentUploadObjectTagCreateCommand command, string calling_user_id)
    {
        // TODO: THis probably needs to be a standard object with delete and update data
        var newTag = new DocumentUploadObjectTagTemplate()
        {
            name = command.name,
            document_object_id = command.document_object_id,
            is_required = command.is_required,
            is_deleted = false,

        };

        return newTag;
    }

    public async Task<DocumentUploadDto> MapToDto(DocumentUpload databaseModel)
    {
        var dto = new DocumentUploadDto()
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

        dto.tag_templates = await _Context.DocumentUploadObjectTags.Where(m => m.document_object_id == databaseModel.document_object_id && m.is_deleted == false).ToListAsync();

        var revisions = await _Context.DocumentUploadRevisions.Where(m => m.document_upload_id == databaseModel.id).ToListAsync();

        foreach (var revision in revisions)
        {
            dto.document_revisions.Add(await this.MapToRevisionDto(revision));
        }


        return dto;
    }

    public async Task<DocumentUploadListDto> MapToListDto(DocumentUpload databaseModel)
    {
        var dto = new DocumentUploadListDto()
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

        var revision_tags = await _Context.DocumentUploadRevisionsTags.Where(m => m.document_upload_revision_id == databaseModel.id).ToListAsync();

        foreach (var tag in revision_tags)
            dto.revision_tags.Add(this.MapToRevisionTagDto(tag));

        var recent_revision = await _Context.DocumentUploadRevisions.Where(m => m.document_upload_id == databaseModel.id && m.rev_num == databaseModel.rev_num && m.is_deleted == false).FirstOrDefaultAsync();
        if(recent_revision != null)
            dto.document_name = recent_revision.document_name;


        return dto;
    }

    public async Task<DocumentUploadRevisionDto> MapToRevisionDto(DocumentUploadRevision databaseModel)
    {
        var dto = new DocumentUploadRevisionDto()
        {
            id = databaseModel.id,
            document_name = databaseModel.document_name,
            document_upload_id = databaseModel.document_upload_id,
            document_path = databaseModel.document_path,
            approved_by = databaseModel.approved_by,
            approved_on = databaseModel.approved_on,
            rejected_by = databaseModel.rejected_by,
            rejected_on = databaseModel.rejected_on,
            rejected_reason = databaseModel.rejected_reason,
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

        var revision_tags = await _Context.DocumentUploadRevisionsTags.Where(m => m.document_upload_revision_id == databaseModel.id).ToListAsync();

        foreach (var tag in revision_tags)
        {
            dto.revision_tags.Add(this.MapToRevisionTagDto(tag));
        }


        return dto;
    }

    public DocumentUploadRevisionTagDto MapToRevisionTagDto(DocumentUploadRevisionTag databaseModel)
    {
        return new DocumentUploadRevisionTagDto()
        {
            id = databaseModel.id,
            document_upload_object_tag_id = databaseModel.document_upload_object_tag_id,
            document_upload_revision_id = databaseModel.document_upload_revision_id,
            is_required = databaseModel.is_required,
            tag_name = databaseModel.tag_name,
            tag_value = databaseModel.tag_value,
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

    private DocumentUploadRevisionTag CopyRevisionTag(DocumentUploadRevisionTag databaseModel)
    {
        return new DocumentUploadRevisionTag()
        {
            document_upload_object_tag_id = databaseModel.document_upload_object_tag_id,
            document_upload_revision_id = databaseModel.document_upload_revision_id,
            tag_name = databaseModel.tag_name,
            tag_value = databaseModel.tag_value,
            is_required = databaseModel.is_required,
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

    // DocumentUploadObjectCategory mapping methods
    public DocumentUploadCategory MapForCategoryCreate(DocumentUploadCategoryCreateCommand command, string calling_user_id)
    {
        var category = CommonDataHelper<DocumentUploadCategory>.FillCommonFields(new DocumentUploadCategory()
        {
            parent_category_id = command.parent_category_id,
            category_name = command.category_name,
            internal_category_name = command.internal_category_name,
        }, calling_user_id);

        return category;
    }

    public async Task<DocumentUploadCategoryDto> MapToCategoryDto(DocumentUploadCategory databaseModel)
    {
        var dto = new DocumentUploadCategoryDto()
        {
            id = databaseModel.id,
            parent_category_id = databaseModel.parent_category_id,
            category_name = databaseModel.category_name,
            internal_category_name = databaseModel.internal_category_name,
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

        var du_objects = await (from duo in _Context.DocumentUploadObjects
                                join duoc in _Context.DocumentUploadObjectCategories on duo.id equals duoc.document_upload_object_id
                                where duoc.document_upload_category_id == databaseModel.id
                                select duo).ToListAsync();

        foreach (var obj in du_objects)
        {
            dto.document_objects.Add(await this.MapToDocumentUploadObjectDto(obj));
        }



        return dto;
    }

    public async Task<DocumentUploadObjectDto> MapToDocumentUploadObjectDto(DocumentUploadObject databaseModel)
    {
        var dto = new DocumentUploadObjectDto()
        {
            id = databaseModel.id,
            friendly_name = databaseModel.friendly_name,
            internal_name = databaseModel.internal_name,
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

        // TODO: Make these dto objects
        dto.tag_templates = await _Context.DocumentUploadObjectTags.Where(m => m.document_object_id == databaseModel.id && m.is_deleted == false).ToListAsync();


        return dto;
    }

    public DocumentUploadObject MapForDocumentUploadObjectCreate(DocumentUploadObjectCreateCommand createCommand, string calling_user_id)
    {
        var duo = CommonDataHelper<DocumentUploadObject>.FillCommonFields(new DocumentUploadObject()
        {
            internal_name = createCommand.internal_name,
            friendly_name = createCommand.friendly_name,
        }, calling_user_id);

        return duo;
    }

    public DocumentUploadObjectTagDto MapToObjectTagDto(DocumentUploadObjectTagTemplate model)
    {
        return new DocumentUploadObjectTagDto()
        {
            id = model.id,
            name = model.name,
            is_required = model.is_required,
            is_deleted = model.is_deleted
        };
    }
}