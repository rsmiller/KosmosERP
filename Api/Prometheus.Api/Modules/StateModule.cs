using Prometheus.Api.Models.Module.State.Command.Create;
using Prometheus.Api.Models.Module.State.Command.Delete;
using Prometheus.Api.Models.Module.State.Command.Edit;
using Prometheus.Api.Models.Module.State.Command.Find;
using Prometheus.Api.Models.Module.State.Dto;
using Prometheus.Database;
using Prometheus.Models.Helpers;
using Prometheus.Models.Interfaces;
using Prometheus.Models;
using Prometheus.Module;
using Microsoft.EntityFrameworkCore;

namespace Prometheus.Api.Modules;

public interface IStateModule : IERPModule<
Database.Models.State,
StateDto,
StateListDto,
StateCreateCommand,
StateEditCommand,
StateDeleteCommand,
StateFindCommand>, IBaseERPModule
{
    // Add any custom methods for State here if needed
}

public class StateModule : BaseERPModule, IStateModule
{
    private readonly IBaseERPContext _Context;

    public StateModule(IBaseERPContext context) : base(context)
    {
        _Context = context;
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

    // 4) Create
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

    // 5) Edit
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

        // Update auditing fields
        existingEntity.updated_on = DateTime.Now;
        existingEntity.updated_by = commandModel.calling_user_id;

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
        existingEntity.is_deleted = true;
        existingEntity.deleted_on = DateTime.Now;
        existingEntity.deleted_by = commandModel.calling_user_id;

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
            var permission_result = await base.HasPermission(commandModel.calling_user_id, "read_state", read: true);
            if (!permission_result)
            {
                response.SetException("Invalid permission", ResultCode.InvalidPermission);
                return response;
            }

            var query = _Context.States
                .Where(m => !m.is_deleted);

            if (!string.IsNullOrEmpty(commandModel.wildcard))
            {
                var wild = commandModel.wildcard.ToLower();
                query = query.Where(m =>
                    m.state_name.ToLower().Contains(wild)
                    || m.iso2.ToLower().Contains(wild)
                );
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

    public async Task<Response<List<StateListDto>>> GlobalSearch(PagingSortingParameters parameters, string wildcard)
    {
        var response = new Response<List<StateListDto>>();
        try
        {
            var query = _Context.States
                .Where(m => !m.is_deleted);

            if (!string.IsNullOrEmpty(wildcard))
            {
                var lower = wildcard.ToLower();
                query = query.Where(m =>
                    m.state_name.ToLower().Contains(lower)
                    || m.iso2.ToLower().Contains(lower)
                );
            }

            var pagedItems = await query.SortAndPageBy(parameters).ToListAsync();

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
            iso2 = databaseModel.iso2
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
            iso2 = databaseModel.iso2
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

            country_id = dtoModel.country_id,
            state_name = dtoModel.state_name,
            iso2 = dtoModel.iso2
        };
    }

    private Database.Models.State MapForCreate(StateCreateCommand createCommandModel)
    {
        var now = DateTime.Now;

        var state = new Database.Models.State
        {
            country_id = createCommandModel.country_id,
            state_name = createCommandModel.state_name,
            iso2 = createCommandModel.iso2,
            is_deleted = false,
            created_on = now,
            created_by = createCommandModel.calling_user_id,
            updated_on = now,
            updated_by = createCommandModel.calling_user_id
        };

        return state;
    }
}
