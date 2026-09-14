using Microsoft.EntityFrameworkCore;
using KosmosERP.Database.Models;
using KosmosERP.Database;
using KosmosERP.Models.Helpers;
using KosmosERP.Models.Interfaces;
using KosmosERP.Models;
using KosmosERP.Module;
using KosmosERP.BusinessLayer.Models.Module.Settings.Command.Create;
using KosmosERP.BusinessLayer.Models.Module.Settings.Command.Delete;
using KosmosERP.BusinessLayer.Models.Module.Settings.Command.Edit;
using KosmosERP.BusinessLayer.Models.Module.Settings.Command.Find;
using KosmosERP.BusinessLayer.Models.Module.Settings.Dto;
using KosmosERP.BusinessLayer.Helpers;


namespace KosmosERP.BusinessLayer.Modules;

public interface ISettingsModule : IERPModule<
    Settings,
    SettingsDto,
    SettingsListDto,
    SettingsCreateCommand,
    SettingsEditCommand,
    SettingsDeleteCommand,
    SettingsFindCommand>, IBaseERPModule
{
    Task<Response<SettingsDto>> GetBaseSettings();
}

public class SettingsModule : BaseERPModule, ISettingsModule
{
    public override Guid ModuleIdentifier => Guid.Parse("3b6f3d9a-0000-4c7f-9000-7a1d00000001");
    public override string ModuleName => "Settings";

    private readonly IBaseERPContext _Context;

    public SettingsModule(IBaseERPContext context, ILogProviderFactory logProviderFactory) : base(logProviderFactory)
    {
        _Context = context;
    }

    public override void SeedPermissions()
    {
        var initial_record = _Context.Settings.FirstOrDefault(c => !c.is_deleted);

        if(initial_record == null)
        {
            var settings = new Settings
            {
                company_name = "My Company",
                company_address1 = "123 Main St",
                company_city = "Anytown",
                company_state = "TX",
                company_zip = "12345",
                company_country = "USA",
                fiscal_year_start = DateTime.Now.Year.ToString() + "-01-01",
                is_deleted = false
            };

            settings = CommonDataHelper<Settings>.FillCommonFields(settings, 1);

            _Context.Settings.Add(settings);
            _Context.SaveChanges();
        }
    }

    public async Task<Response<SettingsDto>> GetBaseSettings()
    {
        var entity = await _Context.Settings.FirstOrDefaultAsync(c => !c.is_deleted);

        if (entity == null)
            return new Response<SettingsDto>("Settings not found", ResultCode.NotFound);

        var dto = await MapToDto(entity);
        return new Response<SettingsDto>(dto);
    }

    public Settings? Get(int object_id)
    {
        return _Context.Settings.SingleOrDefault(m => m.id == object_id);
    }

    public async Task<Settings?> GetAsync(int object_id)
    {
        return await _Context.Settings.SingleOrDefaultAsync(m => m.id == object_id);
    }

    public async Task<Response<SettingsDto>> GetDto(int object_id)
    {
        var entity = await GetAsync(object_id);
        if (entity == null)
            return new Response<SettingsDto>("Settings not found", ResultCode.NotFound);

        var dto = await MapToDto(entity);
        return new Response<SettingsDto>(dto);
    }

    public async Task<Response<SettingsDto>> GetDtoByGuid(string guid)
    {
        var entity = await _Context.Settings.FirstOrDefaultAsync(c => c.guid == guid && !c.is_deleted);
        if (entity == null)
            return new Response<SettingsDto>("Settings not found", ResultCode.NotFound);

        var dto = await MapToDto(entity);
        return new Response<SettingsDto>(dto);
    }

    public async Task<Response<SettingsDto>> Create(SettingsCreateCommand commandModel)
    {
        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<SettingsDto>(validationResult.Exception, ResultCode.DataValidationError);

        var newSettings = this.MapForCreate(commandModel);

        _Context.Settings.Add(newSettings);
        await _Context.SaveChangesAsync();

        var dto = await MapToDto(newSettings);
        return new Response<SettingsDto>(dto);
    }

    public async Task<Response<SettingsDto>> Edit(SettingsEditCommand commandModel)
    {
        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<SettingsDto>(validationResult.Exception, ResultCode.DataValidationError);

        var existingEntity = await GetAsync(commandModel.id);
        if (existingEntity == null)
            return new Response<SettingsDto>("Settings not found", ResultCode.NotFound);

        if (commandModel.company_name != null && existingEntity.company_name != commandModel.company_name)
            existingEntity.company_name = commandModel.company_name;

        if (commandModel.company_address1 != null && existingEntity.company_address1 != commandModel.company_address1)
            existingEntity.company_address1 = commandModel.company_address1;

        if (commandModel.company_address2 != null && existingEntity.company_address2 != commandModel.company_address2)
            existingEntity.company_address2 = commandModel.company_address2;

        if (commandModel.company_city != null && existingEntity.company_city != commandModel.company_city)
            existingEntity.company_city = commandModel.company_city;

        if (commandModel.company_state != null && existingEntity.company_state != commandModel.company_state)
            existingEntity.company_state = commandModel.company_state;

        if (commandModel.company_zip != null && existingEntity.company_zip != commandModel.company_zip)
            existingEntity.company_zip = commandModel.company_zip;

        if (commandModel.company_country != null && existingEntity.company_country != commandModel.company_country)
            existingEntity.company_country = commandModel.company_country;

        if (commandModel.company_phone != null && existingEntity.company_phone != commandModel.company_phone)
            existingEntity.company_phone = commandModel.company_phone;

        if (commandModel.company_ar_email != null && existingEntity.company_ar_email != commandModel.company_ar_email)
            existingEntity.company_ar_email = commandModel.company_ar_email;

        if (commandModel.company_ap_email != null && existingEntity.company_ap_email != commandModel.company_ap_email)
            existingEntity.company_ap_email = commandModel.company_ap_email;

        if (commandModel.company_general_email != null && existingEntity.company_general_email != commandModel.company_general_email)
            existingEntity.company_general_email = commandModel.company_general_email;

        if (commandModel.company_website != null && existingEntity.company_website != commandModel.company_website)
            existingEntity.company_website = commandModel.company_website;

        if (commandModel.tax_id != null && existingEntity.tax_id != commandModel.tax_id)
            existingEntity.tax_id = commandModel.tax_id;

        if (commandModel.fiscal_year_start != null && existingEntity.fiscal_year_start != commandModel.fiscal_year_start)
            existingEntity.fiscal_year_start = commandModel.fiscal_year_start;


        existingEntity = CommonDataHelper<Settings>.FillUpdateFields(existingEntity, commandModel.calling_user_id);

        _Context.Settings.Update(existingEntity);
        await _Context.SaveChangesAsync();

        var dto = await MapToDto(existingEntity);
        return new Response<SettingsDto>(dto);
    }

    public async Task<Response<SettingsDto>> Delete(SettingsDeleteCommand commandModel)
    {
        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<SettingsDto>(validationResult.Exception, ResultCode.DataValidationError);


        var existingEntity = await GetAsync(commandModel.id);
        if (existingEntity == null)
            return new Response<SettingsDto>("Settings not found", ResultCode.NotFound);

        // Soft delete
        existingEntity = CommonDataHelper<Settings>.FillDeleteFields(existingEntity, commandModel.calling_user_id);

        _Context.Settings.Update(existingEntity);
        await _Context.SaveChangesAsync();

        var dto = await MapToDto(existingEntity);
        return new Response<SettingsDto>(dto);
    }

    public async Task<PagingResult<SettingsListDto>> Find(PagingSortingParameters parameters, SettingsFindCommand commandModel)
    {
        var response = new PagingResult<SettingsListDto>();
        try
        {

            var query = _Context.Settings.Where(m => !m.is_deleted);

            if (!string.IsNullOrEmpty(commandModel.wildcard))
            {
                var wild = commandModel.wildcard.ToLower();
                query = query.Where(m =>
                    (m.company_name != null && m.company_name.ToLower().Contains(wild))
                    || (m.company_address1 != null && m.company_address1.ToLower().Contains(wild))
                    || (m.company_address2 != null && m.company_address2.ToLower().Contains(wild))
                    || (m.company_city != null && m.company_city.ToLower().Contains(wild))
                    || (m.company_state != null && m.company_state.ToLower().Contains(wild))
                    || (m.company_country != null && m.company_country.ToLower().Contains(wild))
                    || (m.guid != null && m.guid.ToLower().Contains(wild))
                );
            }

            var totalCount = await query.CountAsync();
            var pagedItems = await query.SortAndPageBy(parameters).ToListAsync();

            var dtos = new List<SettingsListDto>();
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

    public async Task<Response<List<SettingsListDto>>> GlobalSearch(GlobalSearchFindCommand commandModel)
    {
        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<List<SettingsListDto>>(validationResult.Exception, ResultCode.DataValidationError);

        var response = new Response<List<SettingsListDto>>();
        try
        {
            var query = _Context.Settings.Where(m => !m.is_deleted);

            if (!string.IsNullOrEmpty(commandModel.wildcard))
            {
                var lower = commandModel.wildcard.ToLower();
                query = query.Where(m =>
                    (m.company_name != null && m.company_name.ToLower().Contains(lower))
                    || (m.company_address1 != null && m.company_address1.ToLower().Contains(lower))
                    || (m.company_city != null && m.company_city.ToLower().Contains(lower))
                    || (m.company_country != null && m.company_country.ToLower().Contains(lower))
                    || (m.guid != null && m.guid.ToLower().Contains(lower))
                );
            }

            var pagedItems = await query.SortAndPageBy(commandModel.parameters).ToListAsync();

            var dtos = new List<SettingsListDto>();
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

    public async Task<SettingsListDto> MapToListDto(Settings databaseModel)
    {
        var dto = new SettingsListDto
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
            company_name = databaseModel.company_name,
            company_address1 = databaseModel.company_address1,
            company_address2 = databaseModel.company_address2,
            company_city = databaseModel.company_city,
            company_state = databaseModel.company_state,
            company_zip = databaseModel.company_zip,
            company_country = databaseModel.company_country,
            company_phone = databaseModel.company_phone,
            company_ar_email = databaseModel.company_ar_email,
            company_ap_email = databaseModel.company_ap_email,
            company_general_email = databaseModel.company_general_email,
            company_website = databaseModel.company_website,
            tax_id = databaseModel.tax_id,
            fiscal_year_start = databaseModel.fiscal_year_start,
            guid = databaseModel.guid
        };

        return dto;
    }

    public async Task<SettingsDto> MapToDto(Settings databaseModel)
    {
        var dto = new SettingsDto
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
            company_name = databaseModel.company_name,
            company_address1 = databaseModel.company_address1,
            company_address2 = databaseModel.company_address2,
            company_city = databaseModel.company_city,
            company_state = databaseModel.company_state,
            company_zip = databaseModel.company_zip,
            company_country = databaseModel.company_country,
            company_phone = databaseModel.company_phone,
            company_ar_email = databaseModel.company_ar_email,
            company_ap_email = databaseModel.company_ap_email,
            company_general_email = databaseModel.company_general_email,
            company_website = databaseModel.company_website,
            tax_id = databaseModel.tax_id,
            fiscal_year_start = databaseModel.fiscal_year_start,
            guid = databaseModel.guid
        };

        return dto;
    }

    public Settings MapToDatabaseModel(SettingsDto dtoModel)
    {
        return new Settings
        {
            id = dtoModel.id,
            is_deleted = dtoModel.is_deleted,
            created_on = dtoModel.created_on,
            created_by = dtoModel.created_by,
            updated_on = dtoModel.updated_on,
            updated_by = dtoModel.updated_by,
            deleted_on = dtoModel.deleted_on,
            deleted_by = dtoModel.deleted_by,
            company_name = dtoModel.company_name,
            company_address1 = dtoModel.company_address1,
            company_address2 = dtoModel.company_address2,
            company_city = dtoModel.company_city,
            company_state = dtoModel.company_state,
            company_zip = dtoModel.company_zip,
            company_country = dtoModel.company_country,
            company_phone = dtoModel.company_phone,
            company_ar_email = dtoModel.company_ar_email,
            company_ap_email = dtoModel.company_ap_email,
            company_general_email = dtoModel.company_general_email,
            company_website = dtoModel.company_website,
            tax_id = dtoModel.tax_id,
            fiscal_year_start = dtoModel.fiscal_year_start,
            guid = dtoModel.guid
        };
    }

    private Settings MapForCreate(SettingsCreateCommand createCommandModel)
    {
        var settings = CommonDataHelper<Settings>.FillCommonFields(new Settings
        {
            company_name = createCommandModel.company_name,
            company_address1 = createCommandModel.company_address1,
            company_address2 = createCommandModel.company_address2,
            company_city = createCommandModel.company_city,
            company_state = createCommandModel.company_state,
            company_zip = createCommandModel.company_zip,
            company_country = createCommandModel.company_country,
            company_phone = createCommandModel.company_phone,
            company_ar_email = createCommandModel.company_ar_email,
            company_ap_email = createCommandModel.company_ap_email,
            company_general_email = createCommandModel.company_general_email,
            company_website = createCommandModel.company_website,
            tax_id = createCommandModel.tax_id,
            fiscal_year_start = createCommandModel.fiscal_year_start,
            is_deleted = false,
        }, createCommandModel.calling_user_id);

        return settings;
    }
}
