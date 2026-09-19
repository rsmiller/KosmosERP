using Microsoft.EntityFrameworkCore;
using KosmosERP.Database;
using KosmosERP.Models.Helpers;
using KosmosERP.Models.Interfaces;
using KosmosERP.Models;
using KosmosERP.Module;
using KosmosERP.BusinessLayer.Models.Module.State.Command.Create;
using KosmosERP.BusinessLayer.Models.Module.State.Command.Delete;
using KosmosERP.BusinessLayer.Models.Module.State.Command.Edit;
using KosmosERP.BusinessLayer.Models.Module.State.Command.Find;
using KosmosERP.BusinessLayer.Models.Module.State.Dto;
using KosmosERP.BusinessLayer.Helpers;
using KosmosERP.Database.Models;

namespace KosmosERP.BusinessLayer.Modules;

public interface IStateModule : IERPModule<
Database.Models.State,
StateDto,
StateListDto,
StateCreateCommand,
StateEditCommand,
StateDeleteCommand,
StateFindCommand>, IBaseERPModule
{
    Task<Response<StateDto>> GetByISOAsync(int country_id, string iso2);
}

public class StateModule : BaseERPModule, IStateModule
{
    public override Guid ModuleIdentifier => Guid.Parse("87fa499a-1240-43fe-8457-11d367d4eb2e");
    public override string ModuleName => "State";

    private readonly IBaseERPContext _Context;

    public StateModule(IBaseERPContext context, ILogProviderFactory logProviderFactory) : base(context, logProviderFactory)
    {
        _Context = context;
    }

    public override void SeedPermissions()
    {
        var role = _Context.Roles.Any(m => m.name == "Data Type Administrators");

        if (role == false)
        {
            _Context.Roles.Add(CommonDataHelper<Role>.FillCommonFields(new Role()
            {
                name = "Data Type Administrators",
            }, 1));

            _Context.SaveChanges();
        }
    }

    public Database.Models.State? Get(int object_id)
    {
        return _Context.States.SingleOrDefault(m => m.id == object_id);
    }

    public async Task<Database.Models.State?> GetAsync(int object_id)
    {
        return await _Context.States.SingleOrDefaultAsync(m => m.id == object_id);
    }

    public async Task<Response<StateDto>> GetDto(int object_id)
    {
        var entity = await GetAsync(object_id);
        if (entity == null)
            return new Response<StateDto>("State not found", ResultCode.NotFound);

        var dto = await MapToDto(entity);
        return new Response<StateDto>(dto);
    }

    public async Task<Response<StateDto>> GetDtoByGuid(string guid)
    {
        throw new NotImplementedException();
    }

    public async Task<Response<StateDto>> GetByISOAsync(int country_id, string iso2)
    {
        var entity = await _Context.States.SingleOrDefaultAsync(m => m.country_id == country_id && m.iso2 == iso2);
        if (entity == null)
            return new Response<StateDto>("State not found", ResultCode.NotFound);

        var dto = await MapToDto(entity);
        return new Response<StateDto>(dto);
    }
    
    public async Task<Response<StateDto>> Create(StateCreateCommand commandModel)
    {
        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<StateDto>(validationResult.Exception, ResultCode.DataValidationError);

        // Build new State entity from command
        var newState = this.MapForCreate(commandModel);

        _Context.States.Add(newState);
        await _Context.SaveChangesAsync();

        var dto = await MapToDto(newState);
        return new Response<StateDto>(dto);
    }

    public async Task<Response<StateDto>> Edit(StateEditCommand commandModel)
    {
        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<StateDto>(validationResult.Exception, ResultCode.DataValidationError);

        var existingEntity = await GetAsync(commandModel.id);
        if (existingEntity == null)
            return new Response<StateDto>("State not found", ResultCode.NotFound);

        // Compare and update
        if (commandModel.country_id.HasValue && existingEntity.country_id != commandModel.country_id)
            existingEntity.country_id = commandModel.country_id.Value;

        if (existingEntity.state_name != commandModel.state_name)
            existingEntity.state_name = commandModel.state_name;

        if (existingEntity.iso2 != commandModel.iso2)
            existingEntity.iso2 = commandModel.iso2;


        existingEntity = CommonDataHelper<State>.FillUpdateFields(existingEntity, commandModel.calling_user_id);



        _Context.States.Update(existingEntity);
        await _Context.SaveChangesAsync();

        var dto = await MapToDto(existingEntity);
        return new Response<StateDto>(dto);
    }

    public async Task<Response<StateDto>> Delete(StateDeleteCommand commandModel)
    {
        var existingEntity = await GetAsync(commandModel.id);
        if (existingEntity == null)
            return new Response<StateDto>("State not found", ResultCode.NotFound);

        // Soft-delete
        existingEntity = CommonDataHelper<State>.FillDeleteFields(existingEntity, commandModel.calling_user_id);

        _Context.States.Update(existingEntity);
        await _Context.SaveChangesAsync();

        var dto = await MapToDto(existingEntity);
        return new Response<StateDto>(dto);
    }

    public async Task<PagingResult<StateListDto>> Find(PagingSortingParameters parameters, StateFindCommand commandModel)
    {
        var response = new PagingResult<StateListDto>();

        try
        {
            var query = _Context.States
                .Where(m => !m.is_deleted);

            if (commandModel.country_id.HasValue)
            {
                query = query.Where(m => m.country_id == commandModel.country_id.Value);
            }

            if (!string.IsNullOrEmpty(commandModel.wildcard))
            {
                var wild = commandModel.wildcard.ToLower();
                query = query.Where(m => m.state_name.ToLower().Contains(wild) || m.iso2.ToLower() == wild.ToLower());
            }

            var totalCount = await query.CountAsync();
            var pagedItems = await query.SortAndPageBy(parameters).ToListAsync();

            var dtos = new List<StateListDto>();
            foreach (var item in pagedItems)
            {
                dtos.Add(await MapToListDto(item));
            }

            response.Data = dtos;
            response.TotalResultCount = totalCount;
        }
        catch (Exception ex)
        {
            await LogError(50, this.GetType().Name, nameof(Find), ex);
            response.SetException(ex.Message, ResultCode.Error);
            response.TotalResultCount = 0;
        }

        return response;
    }

    public async Task<Response<List<StateListDto>>> GlobalSearch(GlobalSearchFindCommand commandModel)
    {
        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<List<StateListDto>>(validationResult.Exception, ResultCode.DataValidationError);


        var response = new Response<List<StateListDto>>();
        try
        {
            var query = _Context.States
                .Where(m => !m.is_deleted);

            if (!string.IsNullOrEmpty(commandModel.wildcard))
            {
                var lower = commandModel.wildcard.ToLower();
                query = query.Where(m =>
                    m.state_name.ToLower().Contains(lower)
                    || m.iso2.ToLower().Contains(lower)
                );
            }

            var pagedItems = await query.SortAndPageBy(commandModel.parameters).ToListAsync();

            var dtos = new List<StateListDto>();
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

    public async Task<StateListDto> MapToListDto(Database.Models.State databaseModel)
    {
        return new StateListDto
        {
            id = databaseModel.id,
            is_deleted = databaseModel.is_deleted,
            created_on = databaseModel.created_on,
            created_by = databaseModel.created_by,
            updated_on = databaseModel.updated_on,
            updated_by = databaseModel.updated_by,
            deleted_on = databaseModel.deleted_on,
            deleted_by = databaseModel.deleted_by,
            country_id = databaseModel.country_id,
            state_name = databaseModel.state_name,
            iso2 = databaseModel.iso2,
            created_on_string = databaseModel.created_on_string,
            created_on_timezone = databaseModel.created_on_timezone,
            updated_on_string = databaseModel.updated_on_string,
            updated_on_timezone = databaseModel.updated_on_timezone,
        };
    }

    public async Task<StateDto> MapToDto(Database.Models.State databaseModel)
    {
        return new StateDto
        {
            id = databaseModel.id,
            is_deleted = databaseModel.is_deleted,
            created_on = databaseModel.created_on,
            created_by = databaseModel.created_by,
            updated_on = databaseModel.updated_on,
            updated_by = databaseModel.updated_by,
            deleted_on = databaseModel.deleted_on,
            deleted_by = databaseModel.deleted_by,
            country_id = databaseModel.country_id,
            state_name = databaseModel.state_name,
            iso2 = databaseModel.iso2,
            created_on_string = databaseModel.created_on_string,
            created_on_timezone = databaseModel.created_on_timezone,
            updated_on_string = databaseModel.updated_on_string,
            updated_on_timezone = databaseModel.updated_on_timezone,
            deleted_on_timezone = databaseModel.deleted_on_timezone,
            deleted_on_string = databaseModel.deleted_on_string,
        };
    }

    public Database.Models.State MapToDatabaseModel(StateDto dtoModel)
    {
        return new Database.Models.State
        {
            id = dtoModel.id,
            is_deleted = dtoModel.is_deleted,
            created_on = dtoModel.created_on,
            created_by = dtoModel.created_by,
            updated_on = dtoModel.updated_on,
            updated_by = dtoModel.updated_by,
            deleted_on = dtoModel.deleted_on,
            deleted_by = dtoModel.deleted_by,
            deleted_on_string = dtoModel.deleted_on_string,
            deleted_on_timezone = dtoModel.deleted_on_timezone,
            created_on_string = dtoModel.created_on_string,
            created_on_timezone = dtoModel.created_on_timezone,
            updated_on_string = dtoModel.updated_on_string,
            updated_on_timezone = dtoModel.updated_on_timezone,
            country_id = dtoModel.country_id,
            state_name = dtoModel.state_name,
            iso2 = dtoModel.iso2
        };
    }

    private Database.Models.State MapForCreate(StateCreateCommand createCommandModel)
    {
        var state = CommonDataHelper<Database.Models.State>.FillCommonFields(new Database.Models.State
        {
            country_id = createCommandModel.country_id,
            state_name = createCommandModel.state_name,
            iso2 = createCommandModel.iso2,
            is_deleted = false,
        }, createCommandModel.calling_user_id);

        return state;
    }
}
