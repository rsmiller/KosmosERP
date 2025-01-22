using Microsoft.EntityFrameworkCore;
using Prometheus.Api.Models.Module.Lead.Command.Create;
using Prometheus.Api.Models.Module.Lead.Command.Delete;
using Prometheus.Api.Models.Module.Lead.Command.Edit;
using Prometheus.Api.Models.Module.Lead.Command.Find;
using Prometheus.Api.Models.Module.Lead.Dto;
using Prometheus.Database;
using Prometheus.Database.Models;
using Prometheus.Models;
using Prometheus.Models.Helpers;
using Prometheus.Models.Interfaces;
using Prometheus.Module;

namespace Prometheus.Api.Modules;

public interface ILeadModule
        : IERPModule<Lead, LeadDto, LeadListDto, LeadCreateCommand, LeadEditCommand, LeadDeleteCommand, LeadFindCommand>, IBaseERPModule
{
    // Add any lead-specific methods here if needed
}

public class LeadModule : BaseERPModule, ILeadModule
{
    private readonly IBaseERPContext _Context;

    public override Guid ModuleIdentifier => Guid.Parse("9d624ee2-6433-49f0-bc6c-3e6978e2ac9c");
    public override string ModuleName => "Leads";

    public LeadModule(IBaseERPContext context) : base(context)
    {
        _Context = context;
    }

    public override void SeedPermissions()
    {
        var role = _Context.Roles.Any(m => m.name == "CRM Users");
        var read_permission = _Context.ModulePermissions.Any(m => m.module_id == this.ModuleIdentifier.ToString() && m.internal_permission_name == "read_lead");
        var create_permission = _Context.ModulePermissions.Any(m => m.module_id == this.ModuleIdentifier.ToString() && m.internal_permission_name == "create_lead");
        var edit_permission = _Context.ModulePermissions.Any(m => m.module_id == this.ModuleIdentifier.ToString() && m.internal_permission_name == "edit_lead");
        var delete_permission = _Context.ModulePermissions.Any(m => m.module_id == this.ModuleIdentifier.ToString() && m.internal_permission_name == "delete_lead");
        
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
                permission_name = "Read Lead",
                internal_permission_name = "read_lead",
                module_id = this.ModuleIdentifier.ToString(),
                module_name = this.ModuleName,
                read = true,
            });

            _Context.SaveChanges();

            var read_perm_id = _Context.ModulePermissions.Where(m => m.internal_permission_name == "read_lead").Select(m => m.id).Single();

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
                permission_name = "Create Lead",
                internal_permission_name = "create_lead",
                module_id = this.ModuleIdentifier.ToString(),
                module_name = this.ModuleName,
                write = true
            });

            _Context.SaveChanges();

            var create_perm_id = _Context.ModulePermissions.Where(m => m.internal_permission_name == "create_lead").Select(m => m.id).Single();

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
                permission_name = "Edit Lead",
                internal_permission_name = "edit_lead",
                module_id = this.ModuleIdentifier.ToString(),
                module_name = this.ModuleName,
                edit = true
            });

            _Context.SaveChanges();

            var edit_perm_id = _Context.ModulePermissions.Where(m => m.internal_permission_name == "edit_lead").Select(m => m.id).Single();

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
                permission_name = "Delete Lead",
                internal_permission_name = "delete_lead",
                module_id = this.ModuleIdentifier.ToString(),
                module_name = this.ModuleName,
                delete = true
            });

            _Context.SaveChanges();

            var delete_perm_id = _Context.ModulePermissions.Where(m => m.internal_permission_name == "delete_lead").Select(m => m.id).Single();

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


    public Lead? Get(int object_id)
    {
        return _Context.Leads.SingleOrDefault(m => m.id == object_id);
    }


    public async Task<Lead?> GetAsync(int object_id)
    {
        return await _Context.Leads.SingleOrDefaultAsync(m => m.id == object_id);
    }


    public async Task<Response<LeadDto>> GetDto(int object_id)
    {
        var entity = await GetAsync(object_id);
        if (entity == null)
            return new Response<LeadDto>("Lead not found", ResultCode.NotFound);

        var dto = await MapToDto(entity);
        return new Response<LeadDto>(dto);
    }


    public async Task<Response<LeadDto>> Create(LeadCreateCommand commandModel)
    {
        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<LeadDto>(validationResult.Exception, ResultCode.DataValidationError);

        var permission_result = await base.HasPermission(commandModel.calling_user_id, "create_lead", write: true);
        if (!permission_result)
            return new Response<LeadDto>("Invalid permission", ResultCode.InvalidPermission);

        try
        {
            // Map command to a new Lead entity
            var newEntity = MapForCreate(commandModel);

            // Save to database
            await _Context.Leads.AddAsync(newEntity);
            await _Context.SaveChangesAsync();

            // Convert to DTO
            var dto = await MapToDto(newEntity);
            return new Response<LeadDto>(dto);
        }
        catch (Exception ex)
        {
            return new Response<LeadDto>(ex.Message, ResultCode.Error);
        }
    }


    public async Task<Response<LeadDto>> Edit(LeadEditCommand commandModel)
    {
        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<LeadDto>(validationResult.Exception, ResultCode.DataValidationError);

        var permission_result = await base.HasPermission(commandModel.calling_user_id, "edit_lead", edit: true);
        if (!permission_result)
            return new Response<LeadDto>("Invalid permission", ResultCode.InvalidPermission);


        // Retrieve the existing entity
        var existingEntity = await _Context.Leads.SingleOrDefaultAsync(m => m.id == commandModel.id);
        if (existingEntity == null)
            return new Response<LeadDto>("Lead not found", ResultCode.NotFound);

        // Compare each property and update if changed
        if (!string.IsNullOrEmpty(commandModel.first_name) && existingEntity.first_name != commandModel.first_name)
            existingEntity.first_name = commandModel.first_name;

        if (!string.IsNullOrEmpty(commandModel.last_name) && existingEntity.last_name != commandModel.last_name)
            existingEntity.last_name = commandModel.last_name;

        if (!string.IsNullOrEmpty(commandModel.title) && existingEntity.title != commandModel.title)
            existingEntity.title = commandModel.title;

        if (!string.IsNullOrEmpty(commandModel.email) && existingEntity.email != commandModel.email)
            existingEntity.email = commandModel.email;

        if (!string.IsNullOrEmpty(commandModel.phone) && existingEntity.phone != commandModel.phone)
            existingEntity.phone = commandModel.phone;

        if (!string.IsNullOrEmpty(commandModel.cell_phone) && existingEntity.cell_phone != commandModel.cell_phone)
            existingEntity.cell_phone = commandModel.cell_phone;

        if (!string.IsNullOrEmpty(commandModel.company_name) && existingEntity.company_name != commandModel.company_name)
            existingEntity.company_name = commandModel.company_name;

        if (!string.IsNullOrEmpty(commandModel.lead_stage) && existingEntity.lead_stage != commandModel.lead_stage)
            existingEntity.lead_stage = commandModel.lead_stage;

        if (!string.IsNullOrEmpty(commandModel.time_zone) && existingEntity.time_zone != commandModel.time_zone)
            existingEntity.time_zone = commandModel.time_zone;

        if (!string.IsNullOrEmpty(commandModel.address_line1) && existingEntity.address_line1 != commandModel.address_line1)
            existingEntity.address_line1 = commandModel.address_line1;

        if (!string.IsNullOrEmpty(commandModel.address_line2) && existingEntity.address_line2 != commandModel.address_line2)
            existingEntity.address_line2 = commandModel.address_line2;

        if (!string.IsNullOrEmpty(commandModel.city) && existingEntity.city != commandModel.city)
            existingEntity.city = commandModel.city;

        if (!string.IsNullOrEmpty(commandModel.state) && existingEntity.state != commandModel.state)
            existingEntity.state = commandModel.state;

        if (!string.IsNullOrEmpty(commandModel.zip) && existingEntity.zip != commandModel.zip)
            existingEntity.zip = commandModel.zip;

        if (!string.IsNullOrEmpty(commandModel.country) && existingEntity.country != commandModel.country)
            existingEntity.country = commandModel.country;

        if (commandModel.owner_id.HasValue && existingEntity.owner_id != commandModel.owner_id.Value)
            existingEntity.owner_id = commandModel.owner_id.Value;

        // Update auditing fields
        existingEntity.updated_on = DateTime.Now;
        existingEntity.updated_by = commandModel.calling_user_id;

        // Persist
        _Context.Leads.Update(existingEntity);
        await _Context.SaveChangesAsync();

        // Convert to DTO
        var dto = await MapToDto(existingEntity);
        return new Response<LeadDto>(dto);
    }

    public async Task<Response<LeadDto>> Delete(LeadDeleteCommand commandModel)
    {
        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<LeadDto>(validationResult.Exception, ResultCode.DataValidationError);

        var permission_result = await base.HasPermission(commandModel.calling_user_id, "delete_lead", delete: true);
        if (!permission_result)
            return new Response<LeadDto>("Invalid permission", ResultCode.InvalidPermission);

        var existingEntity = await _Context.Leads.SingleOrDefaultAsync(m => m.id == commandModel.id);
        if (existingEntity == null)
            return new Response<LeadDto>("Lead not found", ResultCode.NotFound);

        existingEntity.is_deleted = true;
        existingEntity.deleted_on = DateTime.Now;
        existingEntity.deleted_by = commandModel.calling_user_id;

        _Context.Leads.Update(existingEntity);
        await _Context.SaveChangesAsync();

        var dto = await MapToDto(existingEntity);
        return new Response<LeadDto>(dto);
    }


    public async Task<PagingResult<LeadListDto>> Find(PagingSortingParameters parameters, LeadFindCommand commandModel)
    {
        PagingResult<LeadListDto> response = new PagingResult<LeadListDto>();

        try
        {
            var permission_result = await base.HasPermission(commandModel.calling_user_id, "read_lead", read: true);
            if (!permission_result)
                return new PagingResult<LeadListDto>("Invalid permission", ResultCode.InvalidPermission);

            List<LeadListDto> dtos = new List<LeadListDto>();


            var results = _Context.Leads.Where(m => m.is_deleted == false &&
                                        (m.first_name.Contains(commandModel.wildcard)
                                        || m.last_name.Contains(commandModel.wildcard)
                                        || m.company_name.Contains(commandModel.wildcard)
                                        || m.email.Contains(commandModel.wildcard)));


            var sortedResults = await results.SortAndPageBy(parameters).ToListAsync();

            foreach (var result in results)
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


    public async Task<Response<List<LeadListDto>>> GlobalSearch(PagingSortingParameters parameters, string wildcard)
    {
        throw new NotImplementedException();
    }

    public async Task<LeadListDto> MapToListDto(Lead databaseModel)
    {
        return new LeadListDto
        {
            id = databaseModel.id,
            first_name = databaseModel.first_name,
            last_name = databaseModel.last_name,
            title = databaseModel.title,
            email = databaseModel.email,
            phone = databaseModel.phone,
            cell_phone = databaseModel.cell_phone,
            company_name = databaseModel.company_name,
            lead_stage = databaseModel.lead_stage,
            time_zone = databaseModel.time_zone,
            address_line1 = databaseModel.address_line1,
            address_line2 = databaseModel.address_line2,
            city = databaseModel.city,
            state = databaseModel.state,
            zip = databaseModel.zip,
            country = databaseModel.country,
            is_converted = databaseModel.is_converted,
            converted_customer_id = databaseModel.converted_customer_id,
            converted_contact_id = databaseModel.converted_contact_id,
            owner_id = databaseModel.owner_id,
            guid = databaseModel.guid,

            // BaseDatabaseModel fields
            is_deleted = databaseModel.is_deleted,
            created_on = databaseModel.created_on,
            created_by = databaseModel.created_by,
            updated_on = databaseModel.updated_on,
            updated_by = databaseModel.updated_by
        };
    }

    public async Task<LeadDto> MapToDto(Lead databaseModel)
    {
        return new LeadDto
        {
            id = databaseModel.id,
            first_name = databaseModel.first_name,
            last_name = databaseModel.last_name,
            title = databaseModel.title,
            email = databaseModel.email,
            phone = databaseModel.phone,
            cell_phone = databaseModel.cell_phone,
            company_name = databaseModel.company_name,
            lead_stage = databaseModel.lead_stage,
            time_zone = databaseModel.time_zone,
            address_line1 = databaseModel.address_line1,
            address_line2 = databaseModel.address_line2,
            city = databaseModel.city,
            state = databaseModel.state,
            zip = databaseModel.zip,
            country = databaseModel.country,
            is_converted = databaseModel.is_converted,
            converted_customer_id = databaseModel.converted_customer_id,
            converted_contact_id = databaseModel.converted_contact_id,
            owner_id = databaseModel.owner_id,
            guid = databaseModel.guid,
            is_deleted = databaseModel.is_deleted,
            created_on = databaseModel.created_on,
            created_by = databaseModel.created_by,
            updated_on = databaseModel.updated_on,
            updated_by = databaseModel.updated_by
        };
    }

    public Lead MapToDatabaseModel(LeadDto dtoModel)
    {
        return new Lead
        {
            id = dtoModel.id,
            first_name = dtoModel.first_name,
            last_name = dtoModel.last_name,
            title = dtoModel.title,
            email = dtoModel.email,
            phone = dtoModel.phone,
            cell_phone = dtoModel.cell_phone,
            company_name = dtoModel.company_name,
            lead_stage = dtoModel.lead_stage,
            time_zone = dtoModel.time_zone,
            address_line1 = dtoModel.address_line1,
            address_line2 = dtoModel.address_line2,
            city = dtoModel.city,
            state = dtoModel.state,
            zip = dtoModel.zip,
            country = dtoModel.country,
            is_converted = dtoModel.is_converted,
            converted_customer_id = dtoModel.converted_customer_id,
            converted_contact_id = dtoModel.converted_contact_id,
            owner_id = dtoModel.owner_id,
            guid = dtoModel.guid,

            // BaseDatabaseModel fields
            is_deleted = dtoModel.is_deleted,
            created_on = dtoModel.created_on,
            created_by = dtoModel.created_by,
            updated_on = dtoModel.updated_on,
            updated_by = dtoModel.updated_by
        };
    }


    private Lead MapForCreate(LeadCreateCommand createCommandModel)
    {
        var now = DateTime.Now;

        var lead = new Lead
        {
            first_name = createCommandModel.first_name,
            last_name = createCommandModel.last_name,
            title = createCommandModel.title,
            email = createCommandModel.email,
            phone = createCommandModel.phone,
            cell_phone = createCommandModel.cell_phone,
            company_name = createCommandModel.company_name,
            lead_stage = createCommandModel.lead_stage,
            time_zone = createCommandModel.time_zone,
            address_line1 = createCommandModel.address_line1,
            address_line2 = createCommandModel.address_line2,
            city = createCommandModel.city,
            state = createCommandModel.state,
            zip = createCommandModel.zip,
            country = createCommandModel.country,
            created_on = now,
            created_by = createCommandModel.calling_user_id,
            updated_on = now,
            updated_by = createCommandModel.calling_user_id
        };

        return lead;
    }
}
