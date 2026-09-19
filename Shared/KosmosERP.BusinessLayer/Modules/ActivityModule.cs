using Microsoft.EntityFrameworkCore;
using KosmosERP.Database.Models;
using KosmosERP.Database;
using KosmosERP.Models.Helpers;
using KosmosERP.Models.Interfaces;
using KosmosERP.Models;
using KosmosERP.Module;
using KosmosERP.BusinessLayer.Models.Module.Activity.Command.Create;
using KosmosERP.BusinessLayer.Models.Module.Activity.Command.Delete;
using KosmosERP.BusinessLayer.Models.Module.Activity.Command.Edit;
using KosmosERP.BusinessLayer.Models.Module.Activity.Command.Find;
using KosmosERP.BusinessLayer.Models.Module.Activity.Dto;
using KosmosERP.BusinessLayer.Helpers;

namespace KosmosERP.BusinessLayer.Modules;

public interface IActivityModule : IERPModule<
Activity,
ActivityDto,
ActivityListDto,
ActivityCreateCommand,
ActivityEditCommand,
ActivityDeleteCommand,
ActivityFindCommand>, IBaseERPModule
{

}

public class ActivityModule : BaseERPModule, IActivityModule
{
    public override Guid ModuleIdentifier => Guid.Parse("1e8f4af3-a0bc-4958-8996-b39dbf6d0084");
    public override string ModuleName => "Activities";

    private readonly IBaseERPContext _Context;

    public ActivityModule(IBaseERPContext context, ILogProviderFactory logProviderFactory) : base(context, logProviderFactory)
    {
        _Context = context;
    }

    public override void SeedPermissions()
    {
        var role = _Context.Roles.Any(m => m.name == "Activity Administrators");

        if (role == false)
        {
            _Context.Roles.Add(CommonDataHelper<Role>.FillCommonFields(new Role()
            {
                name = "Activity Administrators",
            }, 1));

            _Context.SaveChanges();

            base.CreateFirstRunRolePermissions();
        }
    }

    public Activity? Get(int object_id)
    {
        return _Context.Activities
            .SingleOrDefault(m => m.id == object_id);
    }

    public async Task<Activity?> GetAsync(int object_id)
    {
        return await _Context.Activities
            .SingleOrDefaultAsync(m => m.id == object_id);
    }

    public async Task<Response<ActivityDto>> GetDto(int object_id)
    {
        var entity = await GetAsync(object_id);
        if (entity == null)
            return new Response<ActivityDto>("Activity not found", ResultCode.NotFound);

        var dto = await MapToDto(entity);
        return new Response<ActivityDto>(dto);
    }
    
    public async Task<Response<ActivityDto>> GetDtoByGuid(string guid)
    {
        var entity = await _Context.Activities.FirstOrDefaultAsync(c => c.guid == guid && !c.is_deleted);
        if (entity == null)
            return new Response<ActivityDto>("Activity not found", ResultCode.NotFound);

        var dto = await MapToDto(entity);
        return new Response<ActivityDto>(dto);
    }

    public async Task<Response<ActivityDto>> Create(ActivityCreateCommand commandModel)
    {
        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<ActivityDto>(validationResult.Exception, ResultCode.DataValidationError);

        var newActivity = this.MapForCreate(commandModel);

        _Context.Activities.Add(newActivity);
        await _Context.SaveChangesAsync();

        var dto = await MapToDto(newActivity);
        return new Response<ActivityDto>(dto);
    }

    public async Task<Response<ActivityDto>> Edit(ActivityEditCommand commandModel)
    {
        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<ActivityDto>(validationResult.Exception, ResultCode.DataValidationError);

        var existingEntity = await GetAsync(commandModel.id);
        if (existingEntity == null)
            return new Response<ActivityDto>("Activity not found", ResultCode.NotFound);

        if (existingEntity.subject != commandModel.subject)
            existingEntity.subject = commandModel.subject;

        if (existingEntity.description != commandModel.description)
            existingEntity.description = commandModel.description;

        if (existingEntity.activity_type != commandModel.activity_type)
            existingEntity.activity_type = commandModel.activity_type;

        if (existingEntity.status != commandModel.status)
            existingEntity.status = commandModel.status;

        if (existingEntity.owner_id != commandModel.owner_id)
            existingEntity.owner_id = commandModel.owner_id;

        if (existingEntity.start_date != commandModel.start_date)
            existingEntity.start_date = commandModel.start_date;

        if (existingEntity.end_date != commandModel.end_date)
            existingEntity.end_date = commandModel.end_date;

        if (existingEntity.priority != commandModel.priority)
            existingEntity.priority = commandModel.priority;

        if (existingEntity.customer_id != commandModel.customer_id)
            existingEntity.customer_id = commandModel.customer_id;

        if (existingEntity.contact_id != commandModel.contact_id)
            existingEntity.contact_id = commandModel.contact_id;

        if (existingEntity.opportunity_id != commandModel.opportunity_id)
            existingEntity.opportunity_id = commandModel.opportunity_id;

        if (existingEntity.lead_id != commandModel.lead_id)
            existingEntity.lead_id = commandModel.lead_id;

        if (existingEntity.related_entity_id != commandModel.related_entity_id)
            existingEntity.related_entity_id = commandModel.related_entity_id;

        if (existingEntity.related_entity_type != commandModel.related_entity_type)
            existingEntity.related_entity_type = commandModel.related_entity_type;

        if (existingEntity.is_all_day != commandModel.is_all_day)
            existingEntity.is_all_day = commandModel.is_all_day;

        if (existingEntity.location != commandModel.location)
            existingEntity.location = commandModel.location;

        if (existingEntity.reminder_type != commandModel.reminder_type)
            existingEntity.reminder_type = commandModel.reminder_type;

        if (existingEntity.reminder_time != commandModel.reminder_time)
            existingEntity.reminder_time = commandModel.reminder_time;

        // Update auditing fields
        existingEntity = CommonDataHelper<Activity>.FillUpdateFields(existingEntity, commandModel.calling_user_id);

        _Context.Activities.Update(existingEntity);
        await _Context.SaveChangesAsync();

        var dto = await MapToDto(existingEntity);
        return new Response<ActivityDto>(dto);
    }

    public async Task<Response<ActivityDto>> Delete(ActivityDeleteCommand commandModel)
    {
        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<ActivityDto>(validationResult.Exception, ResultCode.DataValidationError);

        var existingEntity = await GetAsync(commandModel.id);
        if (existingEntity == null)
            return new Response<ActivityDto>("Activity not found", ResultCode.NotFound);

        // Soft delete
        existingEntity = CommonDataHelper<Activity>.FillDeleteFields(existingEntity, commandModel.calling_user_id);

        _Context.Activities.Update(existingEntity);
        await _Context.SaveChangesAsync();

        var dto = await MapToDto(existingEntity);
        return new Response<ActivityDto>(dto);
    }

    public async Task<PagingResult<ActivityListDto>> Find(PagingSortingParameters parameters, ActivityFindCommand commandModel)
    {
        var response = new PagingResult<ActivityListDto>();
        try
        {

            var query = _Context.Activities
                .Where(m => !m.is_deleted);

            if (commandModel.owner_id.HasValue)
            {
                query = query.Where(m => m.owner_id == commandModel.owner_id.Value);
            }

            if (commandModel.customer_id.HasValue)
            {
                query = query.Where(m => m.customer_id == commandModel.customer_id.Value);
            }

            if (commandModel.contact_id.HasValue)
            {
                query = query.Where(m => m.contact_id == commandModel.contact_id.Value);
            }

            if (commandModel.opportunity_id.HasValue)
            {
                query = query.Where(m => m.opportunity_id == commandModel.opportunity_id.Value);
            }

            if (commandModel.lead_id.HasValue)
            {
                query = query.Where(m => m.lead_id == commandModel.lead_id.Value);
            }

            if (!string.IsNullOrEmpty(commandModel.activity_type))
            {
                query = query.Where(m => m.activity_type == commandModel.activity_type);
            }

            if (!string.IsNullOrEmpty(commandModel.status))
            {
                query = query.Where(m => m.status == commandModel.status);
            }

            if (commandModel.start_date_from.HasValue)
            {
                query = query.Where(m => m.start_date >= commandModel.start_date_from.Value);
            }

            if (commandModel.start_date_to.HasValue)
            {
                query = query.Where(m => m.start_date <= commandModel.start_date_to.Value);
            }

            if (!string.IsNullOrEmpty(commandModel.wildcard))
            {
                var wild = commandModel.wildcard.ToLower();
                query = query.Where(m =>
                    m.subject.ToLower().Contains(wild)
                    || m.description.ToLower().Contains(wild)
                    || m.activity_type.ToLower().Contains(wild)
                    || m.status.ToLower().Contains(wild)
                    || (m.location != null && m.location.ToLower().Contains(wild))
                    || m.guid.ToLower().Contains(wild)
                );
            }

            var totalCount = await query.CountAsync();
            var pagedItems = await query.SortAndPageBy(parameters).ToListAsync();

            var dtos = new List<ActivityListDto>();
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

    public async Task<Response<List<ActivityListDto>>> GlobalSearch(GlobalSearchFindCommand commandModel)
    {
        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<List<ActivityListDto>>(validationResult.Exception, ResultCode.DataValidationError);

        var response = new Response<List<ActivityListDto>>();
        try
        {
            var query = _Context.Activities
                .Where(m => !m.is_deleted);

            if (!string.IsNullOrEmpty(commandModel.wildcard))
            {
                var lower = commandModel.wildcard.ToLower();
                query = query.Where(m =>
                    m.subject.ToLower().Contains(lower)
                    || m.description.ToLower().Contains(lower)
                    || m.activity_type.ToLower().Contains(lower)
                    || m.status.ToLower().Contains(lower)
                    || (m.location != null && m.location.ToLower().Contains(lower))
                    || m.guid.ToLower().Contains(lower)
                );
            }

            var pagedItems = await query.SortAndPageBy(commandModel.parameters).ToListAsync();

            var dtos = new List<ActivityListDto>();
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

    public async Task<ActivityListDto> MapToListDto(Activity databaseModel)
    {
        var dto = new ActivityListDto
        {
            id = databaseModel.id,
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
            subject = databaseModel.subject,
            description = databaseModel.description,
            activity_type = databaseModel.activity_type,
            status = databaseModel.status,
            owner_id = databaseModel.owner_id,
            start_date = databaseModel.start_date,
            end_date = databaseModel.end_date,
            priority = databaseModel.priority,
            customer_id = databaseModel.customer_id,
            contact_id = databaseModel.contact_id,
            opportunity_id = databaseModel.opportunity_id,
            lead_id = databaseModel.lead_id,
            related_entity_id = databaseModel.related_entity_id,
            related_entity_type = databaseModel.related_entity_type,
            is_all_day = databaseModel.is_all_day,
            location = databaseModel.location,
            reminder_type = databaseModel.reminder_type,
            reminder_time = databaseModel.reminder_time,
            guid = databaseModel.guid
        };

        // Load navigation properties
        dto.owner_name = await _Context.Users.Where(m => m.id == databaseModel.owner_id).Select(m => m.first_name + " " + m.last_name).SingleOrDefaultAsync();
        
        if (databaseModel.customer_id.HasValue)
            dto.customer_name = await _Context.Customers.Where(m => m.id == databaseModel.customer_id).Select(m => m.customer_name).SingleOrDefaultAsync();
        
        if (databaseModel.contact_id.HasValue)
            dto.contact_name = await _Context.Contacts.Where(m => m.id == databaseModel.contact_id).Select(m => m.first_name + " " + m.last_name).SingleOrDefaultAsync();
        
        if (databaseModel.opportunity_id.HasValue)
            dto.opportunity_name = await _Context.Opportunities.Where(m => m.id == databaseModel.opportunity_id).Select(m => m.opportunity_name).SingleOrDefaultAsync();
        
        if (databaseModel.lead_id.HasValue)
            dto.lead_name = await _Context.Leads.Where(m => m.id == databaseModel.lead_id).Select(m => m.first_name + " " + m.last_name).SingleOrDefaultAsync();

        return dto;
    }

    public async Task<ActivityDto> MapToDto(Activity databaseModel)
    {
        var dto = new ActivityDto
        {
            id = databaseModel.id,
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
            subject = databaseModel.subject,
            description = databaseModel.description,
            activity_type = databaseModel.activity_type,
            status = databaseModel.status,
            owner_id = databaseModel.owner_id,
            start_date = databaseModel.start_date,
            end_date = databaseModel.end_date,
            priority = databaseModel.priority,
            customer_id = databaseModel.customer_id,
            contact_id = databaseModel.contact_id,
            opportunity_id = databaseModel.opportunity_id,
            lead_id = databaseModel.lead_id,
            related_entity_id = databaseModel.related_entity_id,
            related_entity_type = databaseModel.related_entity_type,
            is_all_day = databaseModel.is_all_day,
            location = databaseModel.location,
            reminder_type = databaseModel.reminder_type,
            reminder_time = databaseModel.reminder_time,
            guid = databaseModel.guid
        };

        // Load navigation properties
        dto.owner_name = await _Context.Users.Where(m => m.id == databaseModel.owner_id).Select(m => m.first_name + " " + m.last_name).SingleOrDefaultAsync();
        
        if (databaseModel.customer_id.HasValue)
            dto.customer_name = await _Context.Customers.Where(m => m.id == databaseModel.customer_id).Select(m => m.customer_name).SingleOrDefaultAsync();
        
        if (databaseModel.contact_id.HasValue)
            dto.contact_name = await _Context.Contacts.Where(m => m.id == databaseModel.contact_id).Select(m => m.first_name + " " + m.last_name).SingleOrDefaultAsync();
        
        if (databaseModel.opportunity_id.HasValue)
            dto.opportunity_name = await _Context.Opportunities.Where(m => m.id == databaseModel.opportunity_id).Select(m => m.opportunity_name).SingleOrDefaultAsync();
        
        if (databaseModel.lead_id.HasValue)
            dto.lead_name = await _Context.Leads.Where(m => m.id == databaseModel.lead_id).Select(m => m.first_name + " " + m.last_name).SingleOrDefaultAsync();

        return dto;
    }

    public Activity MapToDatabaseModel(ActivityDto dtoModel)
    {
        return new Activity
        {
            id = dtoModel.id,
            is_deleted = dtoModel.is_deleted,
            created_on = dtoModel.created_on,
            created_by = dtoModel.created_by,
            updated_on = dtoModel.updated_on,
            updated_by = dtoModel.updated_by,
            deleted_on = dtoModel.deleted_on,
            deleted_by = dtoModel.deleted_by,
            subject = dtoModel.subject,
            description = dtoModel.description,
            activity_type = dtoModel.activity_type,
            status = dtoModel.status,
            owner_id = dtoModel.owner_id,
            start_date = dtoModel.start_date,
            end_date = dtoModel.end_date,
            priority = dtoModel.priority,
            customer_id = dtoModel.customer_id,
            contact_id = dtoModel.contact_id,
            opportunity_id = dtoModel.opportunity_id,
            lead_id = dtoModel.lead_id,
            related_entity_id = dtoModel.related_entity_id,
            related_entity_type = dtoModel.related_entity_type,
            is_all_day = dtoModel.is_all_day,
            location = dtoModel.location,
            reminder_type = dtoModel.reminder_type,
            reminder_time = dtoModel.reminder_time,
            guid = dtoModel.guid
        };
    }

    private Activity MapForCreate(ActivityCreateCommand createCommandModel)
    {
        var now = DateTime.UtcNow;

        var activity = CommonDataHelper<Activity>.FillCommonFields(new Activity
        {
            subject = createCommandModel.subject,
            description = createCommandModel.description,
            activity_type = createCommandModel.activity_type,
            status = createCommandModel.status,
            owner_id = createCommandModel.owner_id,
            start_date = createCommandModel.start_date,
            end_date = createCommandModel.end_date,
            priority = createCommandModel.priority,
            customer_id = createCommandModel.customer_id,
            contact_id = createCommandModel.contact_id,
            opportunity_id = createCommandModel.opportunity_id,
            lead_id = createCommandModel.lead_id,
            related_entity_id = createCommandModel.related_entity_id,
            related_entity_type = createCommandModel.related_entity_type,
            is_all_day = createCommandModel.is_all_day,
            location = createCommandModel.location,
            reminder_type = createCommandModel.reminder_type,
            reminder_time = createCommandModel.reminder_time,
            is_deleted = false,
        }, createCommandModel.calling_user_id);

        return activity;
    }
} 