using Microsoft.EntityFrameworkCore;
using KosmosERP.BusinessLayer.Helpers;
using KosmosERP.BusinessLayer.Models.Module.Comment.Command.Create;
using KosmosERP.BusinessLayer.Models.Module.Comment.Command.Delete;
using KosmosERP.BusinessLayer.Models.Module.Comment.Command.Edit;
using KosmosERP.BusinessLayer.Models.Module.Comment.Command.Find;
using KosmosERP.BusinessLayer.Models.Module.Comment.Dto;
using KosmosERP.Database;
using KosmosERP.Database.Models;
using KosmosERP.Models;
using KosmosERP.Models.Helpers;
using KosmosERP.Models.Interfaces;
using KosmosERP.Module;

namespace KosmosERP.BusinessLayer.Modules;

public interface ICommentModule
        : IERPModule<Comment, CommentDto, CommentListDto, CommentCreateCommand, CommentEditCommand, CommentDeleteCommand, CommentFindCommand>, IBaseERPModule
{

}

public class CommentModule : BaseERPModule, ICommentModule
{
    private readonly IBaseERPContext _Context;

    public override Guid ModuleIdentifier => Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890");
    public override string ModuleName => "Comments";

    public CommentModule(IBaseERPContext context, ILogProviderFactory logProviderFactory) : base(logProviderFactory)
    {
        _Context = context;
    }

    public override void SeedPermissions()
    {
        var role = _Context.Roles.Any(m => m.name == "Comment Administrators");

        if (role == false)
        {
            _Context.Roles.Add(CommonDataHelper<Role>.FillCommonFields(new Role()
            {
                name = "Comment Administrators",
            }, 1));

            _Context.SaveChanges();
        }
    }

    public Comment? Get(int object_id)
    {
        return _Context.Comments.SingleOrDefault(m => m.id == object_id);
    }

    public async Task<Comment?> GetAsync(int object_id)
    {
        return await _Context.Comments.SingleOrDefaultAsync(m => m.id == object_id);
    }

    public async Task<Response<CommentDto>> GetDto(int object_id)
    {
        var entity = await GetAsync(object_id);
        if (entity == null)
            return new Response<CommentDto>("Comment not found", ResultCode.NotFound);

        var dto = await MapToDto(entity);
        return new Response<CommentDto>(dto);
    }

    public async Task<Response<CommentDto>> GetDtoByGuid(string guid)
    {
        var entity = await _Context.Comments.FirstOrDefaultAsync(c => c.guid == guid && !c.is_deleted);
        if (entity == null)
            return new Response<CommentDto>("Comment not found", ResultCode.NotFound);

        var dto = await MapToDto(entity);
        return new Response<CommentDto>(dto);
    }

    public async Task<Response<CommentDto>> Create(CommentCreateCommand commandModel)
    {
        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<CommentDto>(validationResult.Exception, ResultCode.DataValidationError);

        try
        {
            // Map command to a new Comment entity
            var newEntity = MapForCreate(commandModel);

            // Save to database
            await _Context.Comments.AddAsync(newEntity);
            await _Context.SaveChangesAsync();

            // Convert to DTO
            var dto = await MapToDto(newEntity);
            return new Response<CommentDto>(dto);
        }
        catch (Exception ex)
        {
            return new Response<CommentDto>(ex.Message, ResultCode.Error);
        }
    }

    public async Task<Response<CommentDto>> Edit(CommentEditCommand commandModel)
    {
        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<CommentDto>(validationResult.Exception, ResultCode.DataValidationError);

        // Retrieve the existing entity
        var existingEntity = await _Context.Comments.SingleOrDefaultAsync(m => m.id == commandModel.id);
        if (existingEntity == null)
            return new Response<CommentDto>("Comment not found", ResultCode.NotFound);

        // Compare each property and update if changed
        if (!string.IsNullOrEmpty(commandModel.object_guid) && existingEntity.object_guid != commandModel.object_guid)
            existingEntity.object_guid = commandModel.object_guid;

        if (!string.IsNullOrEmpty(commandModel.comment_text) && existingEntity.comment_text != commandModel.comment_text)
            existingEntity.comment_text = commandModel.comment_text;

        // Update auditing fields
        existingEntity = CommonDataHelper<Comment>.FillUpdateFields(existingEntity, commandModel.calling_user_id);

        // Persist
        _Context.Comments.Update(existingEntity);
        await _Context.SaveChangesAsync();

        // Convert to DTO
        var dto = await MapToDto(existingEntity);
        return new Response<CommentDto>(dto);
    }

    public async Task<Response<CommentDto>> Delete(CommentDeleteCommand commandModel)
    {
        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<CommentDto>(validationResult.Exception, ResultCode.DataValidationError);

        var existingEntity = await _Context.Comments.SingleOrDefaultAsync(m => m.id == commandModel.id);
        if (existingEntity == null)
            return new Response<CommentDto>("Comment not found", ResultCode.NotFound);

        // Delete
        existingEntity = CommonDataHelper<Comment>.FillDeleteFields(existingEntity, commandModel.calling_user_id);

        _Context.Comments.Update(existingEntity);
        await _Context.SaveChangesAsync();

        var dto = await MapToDto(existingEntity);
        return new Response<CommentDto>(dto);
    }

    public async Task<PagingResult<CommentListDto>> Find(PagingSortingParameters parameters, CommentFindCommand commandModel)
    {
        PagingResult<CommentListDto> response = new PagingResult<CommentListDto>();

        try
        {
            List<CommentListDto> dtos = new List<CommentListDto>();

            IQueryable<Comment> results;

            if (String.IsNullOrEmpty(commandModel.wildcard))
            {
                results = _Context.Comments.Where(m => m.is_deleted == false);
            }
            else
            {
                results = _Context.Comments.Where(m => m.is_deleted == false &&
                                        (m.comment_text.Contains(commandModel.wildcard)
                                        || m.object_guid.Contains(commandModel.wildcard)
                                        || m.guid.Contains(commandModel.wildcard)));
            }

            if (!string.IsNullOrEmpty(commandModel.object_guid))
            {
                results = results.Where(m => m.object_guid == commandModel.object_guid);
            }

            var sortedResults = await results.SortAndPageBy(parameters).ToListAsync();

            foreach (var result in sortedResults)
                dtos.Add(await this.MapToListDto(result));

            response.Data = dtos;
            response.TotalResultCount = results.Count();
        }
        catch (Exception ex)
        {
            await LogError(50, this.GetType().Name, "Find", ex);

            response.TotalResultCount = 0;
            response.SetException(ex.Message, ResultCode.Error);
        }

        return response;
    }

    public async Task<Response<List<CommentListDto>>> GlobalSearch(GlobalSearchFindCommand commandModel)
    {
        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<List<CommentListDto>>(validationResult.Exception, ResultCode.DataValidationError);

        var response = new Response<List<CommentListDto>>();
        try
        {
            var query = _Context.Comments
                .Where(m => !m.is_deleted);

            if (!string.IsNullOrEmpty(commandModel.wildcard))
            {
                var lower = commandModel.wildcard.ToLower();
                query = query.Where(m =>
                    m.comment_text.ToLower().Contains(lower)
                    || m.object_guid.ToLower().Contains(lower)
                    || m.guid.ToLower().Contains(lower)
                );
            }

            var pagedItems = await query.SortAndPageBy(commandModel.parameters).ToListAsync();

            var dtos = new List<CommentListDto>();
            foreach (var item in pagedItems)
            {
                dtos.Add(await MapToListDto(item));
            }

            response.Data = dtos;
        }
        catch (Exception ex)
        {
            await LogError(50, this.GetType().Name, nameof(GlobalSearch), ex);
            response.SetException(ex.Message, ResultCode.Error);
        }

        return response;
    }

    public async Task<CommentListDto> MapToListDto(Comment databaseModel)
    {
        var dto = new CommentListDto
        {
            id = databaseModel.id,
            object_guid = databaseModel.object_guid,
            comment_text = databaseModel.comment_text,
            guid = databaseModel.guid,
            is_deleted = databaseModel.is_deleted,
            created_on = databaseModel.created_on,
            created_by = databaseModel.created_by,
            updated_on = databaseModel.updated_on,
            updated_by = databaseModel.updated_by,
            deleted_by = databaseModel.deleted_by,
            deleted_on = databaseModel.deleted_on,
            created_on_string = databaseModel.created_on_string,
            created_on_timezone = databaseModel.created_on_timezone,
            updated_on_string = databaseModel.updated_on_string,
            updated_on_timezone = databaseModel.updated_on_timezone,
            deleted_on_string = databaseModel.deleted_on_string,
            deleted_on_timezone = databaseModel.deleted_on_timezone,
        };

        dto.comment_by_name = await _Context.Users.Where(m => m.external_id == databaseModel.created_by).Select(m => m.first_name + " " + m.last_name).SingleOrDefaultAsync();

        return dto;
    }

    public async Task<CommentDto> MapToDto(Comment databaseModel)
    {
        var dto = new CommentDto
        {
            id = databaseModel.id,
            object_guid = databaseModel.object_guid,
            comment_text = databaseModel.comment_text,
            guid = databaseModel.guid,
            is_deleted = databaseModel.is_deleted,
            created_on = databaseModel.created_on,
            created_by = databaseModel.created_by,
            updated_on = databaseModel.updated_on,
            updated_by = databaseModel.updated_by,
            deleted_on = databaseModel.deleted_on,
            deleted_by = databaseModel.deleted_by,
            created_on_string = databaseModel.created_on_string,
            created_on_timezone = databaseModel.created_on_timezone,
            updated_on_string = databaseModel.updated_on_string,
            updated_on_timezone = databaseModel.updated_on_timezone,
            deleted_on_string = databaseModel.deleted_on_string,
            deleted_on_timezone = databaseModel.deleted_on_timezone,
        };

        dto.comment_by_name = await _Context.Users.Where(m => m.external_id == databaseModel.created_by).Select(m => m.first_name + " " + m.last_name).SingleOrDefaultAsync();

        return dto;
    }

    public Comment MapToDatabaseModel(CommentDto dtoModel)
    {
        return new Comment
        {
            id = dtoModel.id,
            object_guid = dtoModel.object_guid,
            comment_text = dtoModel.comment_text,
            guid = dtoModel.guid,
            is_deleted = dtoModel.is_deleted,
            created_on = dtoModel.created_on,
            created_by = dtoModel.created_by,
            updated_on = dtoModel.updated_on,
            updated_by = dtoModel.updated_by,
            deleted_on = dtoModel.deleted_on,
            deleted_by = dtoModel.deleted_by,
        };
    }

    private Comment MapForCreate(CommentCreateCommand createCommandModel)
    {
        var comment = CommonDataHelper<Comment>.FillCommonFields(new Comment
        {
            object_guid = createCommandModel.object_guid,
            comment_text = createCommandModel.comment_text,
            guid = Guid.NewGuid().ToString(),
            is_deleted = false,
        }, createCommandModel.calling_user_id);

        return comment;
    }
} 