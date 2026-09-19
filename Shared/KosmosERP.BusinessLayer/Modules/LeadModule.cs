using Microsoft.EntityFrameworkCore;
using KosmosERP.BusinessLayer.Helpers;
using KosmosERP.BusinessLayer.Models.Module.Lead.Command.Create;
using KosmosERP.BusinessLayer.Models.Module.Lead.Command.Delete;
using KosmosERP.BusinessLayer.Models.Module.Lead.Command.Edit;
using KosmosERP.BusinessLayer.Models.Module.Lead.Command.Find;
using KosmosERP.BusinessLayer.Models.Module.Lead.Dto;
using KosmosERP.Database;
using KosmosERP.Database.Models;
using KosmosERP.Models;
using KosmosERP.Models.Helpers;
using KosmosERP.Models.Interfaces;
using KosmosERP.Module;

namespace KosmosERP.BusinessLayer.Modules;

public interface ILeadModule
        : IERPModule<Lead, LeadDto, LeadListDto, LeadCreateCommand, LeadEditCommand, LeadDeleteCommand, LeadFindCommand>, IBaseERPModule
{

}

public class LeadModule : BaseERPModule, ILeadModule
{
    private readonly IBaseERPContext _Context;

    public override Guid ModuleIdentifier => Guid.Parse("6a4897a8-571d-4f5b-99c4-9eb15f56f619");
    public override string ModuleName => "Leads";

    public LeadModule(IBaseERPContext context, ILogProviderFactory logProviderFactory) : base(context, logProviderFactory)
    {
        _Context = context;
    }

    public override void SeedPermissions()
    {
        var role = _Context.Roles.Any(m => m.name == "CRM Administrators");

        if (role == false)
        {
            _Context.Roles.Add(CommonDataHelper<Role>.FillCommonFields(new Role()
            {
                name = "CRM Administrators",
            }, 1));

            _Context.SaveChanges();

            base.CreateFirstRunRolePermissions();
        }


        var new_stage = _Context.KeyValueStores.Where(m => m.module_id == this.ModuleIdentifier.ToString() && m.key == "lead_stage_new").SingleOrDefault();
        var contacted_stage = _Context.KeyValueStores.Where(m => m.module_id == this.ModuleIdentifier.ToString() && m.key == "lead_stage_contacted").SingleOrDefault();
        var qualified_stage = _Context.KeyValueStores.Where(m => m.module_id == this.ModuleIdentifier.ToString() && m.key == "lead_stage_qualified").SingleOrDefault();
        var unqualified_stage = _Context.KeyValueStores.Where(m => m.module_id == this.ModuleIdentifier.ToString() && m.key == "lead_stage_unqualified").SingleOrDefault();
        var working_stage = _Context.KeyValueStores.Where(m => m.module_id == this.ModuleIdentifier.ToString() && m.key == "lead_stage_working").SingleOrDefault();
        var reopen_stage = _Context.KeyValueStores.Where(m => m.module_id == this.ModuleIdentifier.ToString() && m.key == "lead_stage_reopen").SingleOrDefault();
        var lost_stage = _Context.KeyValueStores.Where(m => m.module_id == this.ModuleIdentifier.ToString() && m.key == "lead_stage_lost").SingleOrDefault();

        if(new_stage == null)
        {
            _Context.KeyValueStores.Add(CommonDataHelper<KeyValueStore>.FillCommonFields(new KeyValueStore()
            {
                key = "lead_stage_new",
                value = "New",
                module_id = this.ModuleIdentifier.ToString()
            }, 1));

            _Context.SaveChanges();
        }

        if (contacted_stage == null)
        {
            _Context.KeyValueStores.Add(CommonDataHelper<KeyValueStore>.FillCommonFields(new KeyValueStore()
            {
                key = "lead_stage_contacted",
                value = "Contacted",
                module_id = this.ModuleIdentifier.ToString()
            }, 1));

            _Context.SaveChanges();
        }

        if (qualified_stage == null)
        {
            _Context.KeyValueStores.Add(CommonDataHelper<KeyValueStore>.FillCommonFields(new KeyValueStore()
            {
                key = "lead_stage_qualified",
                value = "Qualified",
                module_id = this.ModuleIdentifier.ToString()
            }, 1));

            _Context.SaveChanges();
        }

        if (unqualified_stage == null)
        {
            _Context.KeyValueStores.Add(CommonDataHelper<KeyValueStore>.FillCommonFields(new KeyValueStore()
            {
                key = "lead_stage_unqualified",
                value = "Unqualified",
                module_id = this.ModuleIdentifier.ToString()
            }, 1));

            _Context.SaveChanges();
        }

        if (working_stage == null)
        {
            _Context.KeyValueStores.Add(CommonDataHelper<KeyValueStore>.FillCommonFields(new KeyValueStore()
            {
                key = "lead_stage_working",
                value = "Working",
                module_id = this.ModuleIdentifier.ToString()
            }, 1));

            _Context.SaveChanges();
        }

        if (reopen_stage == null)
        {
            _Context.KeyValueStores.Add(CommonDataHelper<KeyValueStore>.FillCommonFields(new KeyValueStore()
            {
                key = "lead_stage_reopen",
                value = "Reopen",
                module_id = this.ModuleIdentifier.ToString()
            }, 1));

            _Context.SaveChanges();
        }

        if (lost_stage == null)
        {
            _Context.KeyValueStores.Add(CommonDataHelper<KeyValueStore>.FillCommonFields(new KeyValueStore()
            {
                key = "lead_stage_lost",
                value = "Lost",
                module_id = this.ModuleIdentifier.ToString()
            }, 1));

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

    public async Task<Response<LeadDto>> GetDtoByGuid(string guid)
    {
        var entity = await _Context.Leads.FirstOrDefaultAsync(c => c.guid == guid && !c.is_deleted);
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
        existingEntity = CommonDataHelper<Lead>.FillUpdateFields(existingEntity, commandModel.calling_user_id);


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

        var existingEntity = await _Context.Leads.SingleOrDefaultAsync(m => m.id == commandModel.id);
        if (existingEntity == null)
            return new Response<LeadDto>("Lead not found", ResultCode.NotFound);


        // Delete
        existingEntity = CommonDataHelper<Lead>.FillDeleteFields(existingEntity, commandModel.calling_user_id);

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
            List<LeadListDto> dtos = new List<LeadListDto>();


            IQueryable<Lead> results;

            if (String.IsNullOrEmpty(commandModel.wildcard))
            {
                results = _Context.Leads.Where(m => m.is_deleted == false);
            }
            else
            {
                results = _Context.Leads.Where(m => m.is_deleted == false &&
                                        (m.first_name.Contains(commandModel.wildcard)
                                        || m.last_name.Contains(commandModel.wildcard)
                                        || m.company_name.Contains(commandModel.wildcard)
                                        || m.email.Contains(commandModel.wildcard)));
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


    public async Task<Response<List<LeadListDto>>> GlobalSearch(GlobalSearchFindCommand commandModel)
    {
        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<List<LeadListDto>>(validationResult.Exception, ResultCode.DataValidationError);

        throw new NotImplementedException();
    }

    public async Task<LeadListDto> MapToListDto(Lead databaseModel)
    {
        var dto = new LeadListDto
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
            created_on_string = databaseModel.created_on_string,
            created_on_timezone = databaseModel.created_on_timezone,
            updated_on_string = databaseModel.updated_on_string,
            updated_on_timezone = databaseModel.updated_on_timezone,
            is_deleted = databaseModel.is_deleted,
            created_on = databaseModel.created_on,
            created_by = databaseModel.created_by,
            updated_on = databaseModel.updated_on,
            updated_by = databaseModel.updated_by,
            deleted_by = databaseModel.deleted_by,
            deleted_on = databaseModel.deleted_on,
            deleted_on_string = databaseModel.deleted_on_string,
            deleted_on_timezone = databaseModel.deleted_on_timezone,
        };

        // THese need to be cached
        dto.owner_name = await _Context.Users.Where(m => m.id == databaseModel.owner_id).Select(m => m.first_name + " " + m.last_name).SingleOrDefaultAsync();
        dto.stage_name = await _Context.KeyValueStores.Where(m => m.module_id == this.ModuleIdentifier.ToString() && m.key == databaseModel.lead_stage).Select(m => m.value).SingleOrDefaultAsync();

        return dto;
    }

    public async Task<LeadDto> MapToDto(Lead databaseModel)
    {
        var dto = new LeadDto
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

        dto.owner_name = await _Context.Users.Where(m => m.id == databaseModel.owner_id).Select(m => m.first_name + " " + m.last_name).SingleOrDefaultAsync();
        dto.stage_name = await _Context.KeyValueStores.Where(m => m.module_id == this.ModuleIdentifier.ToString() && m.key == databaseModel.lead_stage).Select(m => m.value).SingleOrDefaultAsync();

        return dto;
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
            is_deleted = dtoModel.is_deleted,
            created_on = dtoModel.created_on,
            created_by = dtoModel.created_by,
            updated_on = dtoModel.updated_on,
            updated_by = dtoModel.updated_by,
            created_on_string = dtoModel.created_on_string,
            created_on_timezone = dtoModel.created_on_timezone,
            deleted_by = dtoModel.deleted_by,
            deleted_on = dtoModel.deleted_on,
            deleted_on_string = dtoModel.deleted_on_string,
            deleted_on_timezone = dtoModel.deleted_on_timezone,
            updated_on_string = dtoModel.updated_on_string,
            updated_on_timezone = dtoModel.updated_on_timezone,
            
        };
    }


    private Lead MapForCreate(LeadCreateCommand createCommandModel)
    {
        var now = DateTime.UtcNow;

        var lead = CommonDataHelper<Lead>.FillCommonFields(new Lead
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
        }, createCommandModel.calling_user_id);

        return lead;
    }
}
