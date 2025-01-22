using Prometheus.Database.Models;
using Prometheus.Database;
using Prometheus.Models.Helpers;
using Prometheus.Models.Interfaces;
using Prometheus.Models;
using Prometheus.Module;
using Microsoft.EntityFrameworkCore;
using Prometheus.Api.Models.Module.Opportunity.Dto;
using Prometheus.Api.Models.Module.Opportunity.Command.Create;
using Prometheus.Api.Models.Module.Opportunity.Command.Edit;
using Prometheus.Api.Models.Module.Opportunity.Command.Delete;
using Prometheus.Api.Models.Module.Opportunity.Command.Find;
using Prometheus.Api.Models.Module.Customer.Dto;
using Prometheus.Api.Models.Module.Lead.Dto;

namespace Prometheus.Api.Modules;

public interface IOpportunityModule : IERPModule<
    Opportunity,
    OpportunityDto,
    OpportunityListDto,
    OpportunityCreateCommand,
    OpportunityEditCommand,
    OpportunityDeleteCommand,
    OpportunityFindCommand>, IBaseERPModule
{
    // Add any opportunity-specific methods here if needed
}

public class OpportunityModule : BaseERPModule, IOpportunityModule
{
    public override Guid ModuleIdentifier => Guid.Parse("0c3959c3-15dc-44ab-8e2c-9b9e2773e65f");
    public override string ModuleName => "Opportunities";

    private readonly IBaseERPContext _Context;

    public OpportunityModule(IBaseERPContext context) : base(context)
    {
        _Context = context;
    }

    public override void SeedPermissions()
    {
        var role = _Context.Roles.Any(m => m.name == "CRM Users");
        var read_permission = _Context.ModulePermissions.Any(m => m.module_id == this.ModuleIdentifier.ToString() && m.internal_permission_name == "read_opportunity");
        var create_permission = _Context.ModulePermissions.Any(m => m.module_id == this.ModuleIdentifier.ToString() && m.internal_permission_name == "create_opportunity");
        var edit_permission = _Context.ModulePermissions.Any(m => m.module_id == this.ModuleIdentifier.ToString() && m.internal_permission_name == "edit_opportunity");
        var delete_permission = _Context.ModulePermissions.Any(m => m.module_id == this.ModuleIdentifier.ToString() && m.internal_permission_name == "delete_opportunity");

        if (role == false)
        {
            _Context.Roles.Add(new Role()
            {
                name = "CRM Users",
                created_by = 1,
                created_on = DateTime.Now,
                updated_by = 1,
                updated_on = DateTime.Now,
            });

            _Context.SaveChanges();
        }

        var role_id = _Context.Roles.Where(m => m.name == "CRM Users").Select(m => m.id).Single();

        if (read_permission == false)
        {
            _Context.ModulePermissions.Add(new ModulePermission()
            {
                permission_name = "Read Opportunity",
                internal_permission_name = "read_opportunity",
                module_id = this.ModuleIdentifier.ToString(),
                module_name = this.ModuleName,
                read = true,
            });

            _Context.SaveChanges();

            var read_perm_id = _Context.ModulePermissions.Where(m => m.internal_permission_name == "read_opportunity").Select(m => m.id).Single();

            _Context.RolePermissions.Add(new RolePermission()
            {
                role_id = role_id,
                module_permission_id = read_perm_id,
                created_by = 1,
                created_on = DateTime.Now,
                updated_by = 1,
                updated_on = DateTime.Now,
            });

            _Context.SaveChanges();
        }

        if (create_permission == false)
        {
            _Context.ModulePermissions.Add(new ModulePermission()
            {
                permission_name = "Create Opportunity",
                internal_permission_name = "create_opportunity",
                module_id = this.ModuleIdentifier.ToString(),
                module_name = this.ModuleName,
                write = true
            });

            _Context.SaveChanges();

            var create_perm_id = _Context.ModulePermissions.Where(m => m.internal_permission_name == "create_opportunity").Select(m => m.id).Single();

            _Context.RolePermissions.Add(new RolePermission()
            {
                role_id = role_id,
                module_permission_id = create_perm_id,
                created_by = 1,
                created_on = DateTime.Now,
                updated_by = 1,
                updated_on = DateTime.Now,
            });

            _Context.SaveChanges();
        }

        if (edit_permission == false)
        {
            _Context.ModulePermissions.Add(new ModulePermission()
            {
                permission_name = "Edit Opportunity",
                internal_permission_name = "edit_opportunity",
                module_id = this.ModuleIdentifier.ToString(),
                module_name = this.ModuleName,
                edit = true
            });

            _Context.SaveChanges();

            var edit_perm_id = _Context.ModulePermissions.Where(m => m.internal_permission_name == "edit_opportunity").Select(m => m.id).Single();

            _Context.RolePermissions.Add(new RolePermission()
            {
                role_id = role_id,
                module_permission_id = edit_perm_id,
                created_by = 1,
                created_on = DateTime.Now,
                updated_by = 1,
                updated_on = DateTime.Now,
            });

            _Context.SaveChanges();
        }

        if (delete_permission == false)
        {
            _Context.ModulePermissions.Add(new ModulePermission()
            {
                permission_name = "Delete Opportunity",
                internal_permission_name = "delete_opportunity",
                module_id = this.ModuleIdentifier.ToString(),
                module_name = this.ModuleName,
                delete = true
            });

            _Context.SaveChanges();

            var delete_perm_id = _Context.ModulePermissions.Where(m => m.internal_permission_name == "delete_opportunity").Select(m => m.id).Single();

            _Context.RolePermissions.Add(new RolePermission()
            {
                role_id = role_id,
                module_permission_id = delete_perm_id,
                created_by = 1,
                created_on = DateTime.Now,
                updated_by = 1,
                updated_on = DateTime.Now,
            });

            _Context.SaveChanges();
        }
    }

    public Opportunity? Get(int object_id)
    {
        return _Context.Opportunities
            .SingleOrDefault(m => m.id == object_id);
    }

    public async Task<Opportunity?> GetAsync(int object_id)
    {
        return await _Context.Opportunities
            .SingleOrDefaultAsync(m => m.id == object_id);
    }

    public async Task<Response<OpportunityDto>> GetDto(int object_id)
    {
        var entity = await GetAsync(object_id);
        if (entity == null)
            return new Response<OpportunityDto>("Opportunity not found", ResultCode.NotFound);

        var dto = await MapToDto(entity);
        return new Response<OpportunityDto>(dto);
    }

    public async Task<Response<OpportunityDto>> Create(OpportunityCreateCommand commandModel)
    {
        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<OpportunityDto>(validationResult.Exception, ResultCode.DataValidationError);

        var permission_result = await base.HasPermission(commandModel.calling_user_id, "create_opportunity", write: true);
        if (!permission_result)
            return new Response<OpportunityDto>("Invalid permission", ResultCode.InvalidPermission);

        var record = this.MapToDatabaseModel(commandModel);
        record.owner_id = commandModel.calling_user_id;

        _Context.Opportunities.Add(record);
        await _Context.SaveChangesAsync();

        var dto = await MapToDto(record);
        return new Response<OpportunityDto>(dto);
    }

    public async Task<Response<OpportunityDto>> Edit(OpportunityEditCommand commandModel)
    {
        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<OpportunityDto>(validationResult.Exception, ResultCode.DataValidationError);

        var permission_result = await base.HasPermission(commandModel.calling_user_id, "edit_opportunity", edit: true);
        if (!permission_result)
            return new Response<OpportunityDto>("Invalid permission", ResultCode.InvalidPermission);

        var existingEntity = await GetAsync(commandModel.id);
        if (existingEntity == null)
            return new Response<OpportunityDto>("Opportunity not found", ResultCode.NotFound);

        if (!string.IsNullOrEmpty(commandModel.opportunity_name)
            && existingEntity.opportunity_name != commandModel.opportunity_name)
        {
            existingEntity.opportunity_name = commandModel.opportunity_name;
        }

        if (commandModel.customer_id.HasValue && existingEntity.customer_id != commandModel.customer_id)
            existingEntity.customer_id = commandModel.customer_id.Value;

        if (commandModel.contact_id.HasValue && existingEntity.contact_id != commandModel.contact_id)
            existingEntity.contact_id = commandModel.contact_id.Value;

        if (commandModel.amount.HasValue && existingEntity.amount != commandModel.amount)
            existingEntity.amount = commandModel.amount.Value;

        if (!string.IsNullOrEmpty(commandModel.stage)
            && existingEntity.stage != commandModel.stage)
        {
            existingEntity.stage = commandModel.stage;
        }

        if (commandModel.win_chance.HasValue && existingEntity.win_chance != commandModel.win_chance)
            existingEntity.win_chance = commandModel.win_chance.Value;

        if (commandModel.expected_close.HasValue && existingEntity.expected_close != commandModel.expected_close)
            existingEntity.expected_close = commandModel.expected_close.Value;

        if (commandModel.owner_id.HasValue && existingEntity.owner_id != commandModel.owner_id)
            existingEntity.owner_id = commandModel.owner_id.Value;

        existingEntity.updated_on = DateTime.Now;
        existingEntity.updated_by = commandModel.calling_user_id;

        _Context.Opportunities.Update(existingEntity);
        await _Context.SaveChangesAsync();

        var dto = await MapToDto(existingEntity);
        return new Response<OpportunityDto>(dto);
    }

    public async Task<Response<OpportunityDto>> Delete(OpportunityDeleteCommand commandModel)
    {
        var permission_result = await base.HasPermission(commandModel.calling_user_id, "delete_opportunity", delete: true);
        if (!permission_result)
            return new Response<OpportunityDto>("Invalid permission", ResultCode.InvalidPermission);

        var existingEntity = await GetAsync(commandModel.id);
        if (existingEntity == null)
            return new Response<OpportunityDto>("Opportunity not found", ResultCode.NotFound);

        existingEntity.is_deleted = true;
        existingEntity.deleted_on = DateTime.Now;
        existingEntity.deleted_by = commandModel.calling_user_id;

        _Context.Opportunities.Update(existingEntity);
        await _Context.SaveChangesAsync();

        var dto = await MapToDto(existingEntity);
        return new Response<OpportunityDto>(dto);
    }

    public async Task<PagingResult<OpportunityListDto>> Find(PagingSortingParameters parameters, OpportunityFindCommand commandModel)
    {
        var response = new PagingResult<OpportunityListDto>();

        try
        {
            // Example permission check (could be read_opportunity, etc.)
            var permission_result = await base.HasPermission(commandModel.calling_user_id, "read_opportunity", read: true);
            if (!permission_result)
            {
                response.SetException("Invalid permission", ResultCode.InvalidPermission);
                return response;
            }

            var query = _Context.Opportunities
                .Where(m => m.is_deleted == false);

            // If wildcard is not empty, filter by string fields
            if (!string.IsNullOrEmpty(commandModel.wildcard))
            {
                var wild = commandModel.wildcard.ToLower();
                query = query.Where(m =>
                    (m.opportunity_name.ToLower().Contains(wild))
                    || (m.stage.ToLower().Contains(wild))
                    || (m.guid.ToLower().Contains(wild))
                );
            }

            // Sort and page
            var totalCount = await query.CountAsync();
            var pagedItems = await query.SortAndPageBy(parameters).ToListAsync();

            // Convert to DTO
            var dtos = new List<OpportunityListDto>();
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

    public async Task<Response<List<OpportunityListDto>>> GlobalSearch(PagingSortingParameters parameters, string wildcard)
    {
        // Simple example
        var response = new Response<List<OpportunityListDto>>();
        try
        {
            // If you do permission checks here, implement similarly

            var query = _Context.Opportunities
                .Where(m => !m.is_deleted);

            if (!string.IsNullOrEmpty(wildcard))
            {
                var lower = wildcard.ToLower();
                query = query.Where(m =>
                    m.opportunity_name.ToLower().Contains(lower)
                    || m.stage.ToLower().Contains(lower)
                    || m.guid.ToLower().Contains(lower));
            }

            var pagedItems = await query.SortAndPageBy(parameters).ToListAsync();
            var dtos = new List<OpportunityListDto>();
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

    public async Task<OpportunityListDto> MapToListDto(Opportunity databaseModel)
    {
        return new OpportunityListDto
        {
            id = databaseModel.id,
            is_deleted = databaseModel.is_deleted,
            created_on = databaseModel.created_on,
            created_by = databaseModel.created_by,
            updated_on = databaseModel.updated_on,
            updated_by = databaseModel.updated_by,
            deleted_on = databaseModel.deleted_on,
            deleted_by = databaseModel.deleted_by,
            opportunity_name = databaseModel.opportunity_name,
            customer_id = databaseModel.customer_id,
            contact_id = databaseModel.contact_id,
            amount = databaseModel.amount,
            stage = databaseModel.stage,
            win_chance = databaseModel.win_chance,
            expected_close = databaseModel.expected_close,
            owner_id = databaseModel.owner_id,
            guid = databaseModel.guid
        };
    }

    public async Task<OpportunityDto> MapToDto(Opportunity databaseModel)
    {
        return new OpportunityDto
        {
            id = databaseModel.id,
            is_deleted = databaseModel.is_deleted,
            created_on = databaseModel.created_on,
            created_by = databaseModel.created_by,
            updated_on = databaseModel.updated_on,
            updated_by = databaseModel.updated_by,
            deleted_on = databaseModel.deleted_on,
            deleted_by = databaseModel.deleted_by,
            opportunity_name = databaseModel.opportunity_name,
            customer_id = databaseModel.customer_id,
            contact_id = databaseModel.contact_id,
            amount = databaseModel.amount,
            stage = databaseModel.stage,
            win_chance = databaseModel.win_chance,
            expected_close = databaseModel.expected_close,
            owner_id = databaseModel.owner_id,
            guid = databaseModel.guid
        };
    }

    public Opportunity MapToDatabaseModel(OpportunityDto dtoModel)
    {
        return new Opportunity
        {
            id = dtoModel.id,
            opportunity_name = dtoModel.opportunity_name,
            customer_id = dtoModel.customer_id,
            contact_id = dtoModel.contact_id,
            amount = dtoModel.amount,
            stage = dtoModel.stage,
            win_chance = dtoModel.win_chance,
            expected_close = dtoModel.expected_close,
            owner_id = dtoModel.owner_id,
        };
    }

    public Opportunity MapToDatabaseModel(OpportunityCreateCommand createCommand)
    {
        return new Opportunity
        {
            opportunity_name = createCommand.opportunity_name,
            customer_id = createCommand.customer_id,
            contact_id = createCommand.contact_id,
            amount = createCommand.amount,
            stage = createCommand.stage,
            win_chance = createCommand.win_chance,
            expected_close = createCommand.expected_close,
        };
    }
}
