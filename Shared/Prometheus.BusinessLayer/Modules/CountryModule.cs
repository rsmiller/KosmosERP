using Microsoft.EntityFrameworkCore;
using Prometheus.Models;
using Prometheus.Module;
using Prometheus.Database;
using Prometheus.Models.Helpers;
using Prometheus.Models.Interfaces;
using Prometheus.BusinessLayer.Models.Module.Country.Command.Create;
using Prometheus.BusinessLayer.Models.Module.Country.Command.Delete;
using Prometheus.BusinessLayer.Models.Module.Country.Command.Edit;
using Prometheus.BusinessLayer.Models.Module.Country.Command.Find;
using Prometheus.BusinessLayer.Models.Module.Country.Dto;
using Prometheus.Models.Permissions;

namespace Prometheus.BusinessLayer.Modules;

public interface ICountryModule : IERPModule<
Database.Models.Country,
CountryDto,
CountryListDto,
CountryCreateCommand,
CountryEditCommand,
CountryDeleteCommand,
CountryFindCommand>, IBaseERPModule
{

}

public class CountryModule : BaseERPModule, ICountryModule
{
    public override Guid ModuleIdentifier => Guid.Parse("170f420e-645b-4a05-b8dc-73e9c3cb9748");
    public override string ModuleName => "Country";

    private readonly IBaseERPContext _Context;

    public CountryModule(IBaseERPContext context) : base(context)
    {
        _Context = context;
    }


    public Database.Models.Country? Get(int object_id)
    {
        return _Context.Countries.SingleOrDefault(m => m.id == object_id);
    }

    public async Task<Database.Models.Country?> GetAsync(int object_id)
    {
        return await _Context.Countries.SingleOrDefaultAsync(m => m.id == object_id);
    }

    public async Task<Response<CountryDto>> GetDto(int object_id)
    {
        var entity = await GetAsync(object_id);
        if (entity == null)
            return new Response<CountryDto>("Country not found", ResultCode.NotFound);

        var dto = await MapToDto(entity);
        return new Response<CountryDto>(dto);
    }

    public async Task<Response<CountryDto>> Create(CountryCreateCommand commandModel)
    {
        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<CountryDto>(validationResult.Exception, ResultCode.DataValidationError);


        // Build new Country entity from command
        var newCountry = this.MapForCreate(commandModel);

        _Context.Countries.Add(newCountry);
        await _Context.SaveChangesAsync();

        var dto = await MapToDto(newCountry);
        return new Response<CountryDto>(dto);
    }

    public async Task<Response<CountryDto>> Edit(CountryEditCommand commandModel)
    {
        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<CountryDto>(validationResult.Exception, ResultCode.DataValidationError);

        var permission_result = await base.HasPermission(commandModel.calling_user_id, commandModel.token,CountryPermissions.Edit, edit: true);
        if (!permission_result)
            return new Response<CountryDto>("Invalid permission", ResultCode.InvalidPermission);

        var existingEntity = await GetAsync(commandModel.id);
        if (existingEntity == null)
            return new Response<CountryDto>("Country not found", ResultCode.NotFound);

        // Compare and update
        if (existingEntity.country_name != commandModel.country_name)
            existingEntity.country_name = commandModel.country_name;

        if (existingEntity.iso3 != commandModel.iso3)
            existingEntity.iso3 = commandModel.iso3;

        if (existingEntity.phonecode != commandModel.phonecode)
            existingEntity.phonecode = commandModel.phonecode;

        if (existingEntity.currency != commandModel.currency)
            existingEntity.currency = commandModel.currency;

        if (existingEntity.currency_symbol != commandModel.currency_symbol)
            existingEntity.currency_symbol = commandModel.currency_symbol;

        if (existingEntity.region != commandModel.region)
            existingEntity.region = commandModel.region;

        // Update auditing fields
        existingEntity.updated_on = DateTime.Now;
        existingEntity.updated_by = commandModel.calling_user_id;

        _Context.Countries.Update(existingEntity);
        await _Context.SaveChangesAsync();

        var dto = await MapToDto(existingEntity);
        return new Response<CountryDto>(dto);
    }

    public async Task<Response<CountryDto>> Delete(CountryDeleteCommand commandModel)
    {
        var permission_result = await base.HasPermission(commandModel.calling_user_id, commandModel.token,CountryPermissions.Delete, delete: true);
        if (!permission_result)
            return new Response<CountryDto>("Invalid permission", ResultCode.InvalidPermission);

        var existingEntity = await GetAsync(commandModel.id);
        if (existingEntity == null)
            return new Response<CountryDto>("Country not found", ResultCode.NotFound);

        // Soft-delete
        existingEntity.is_deleted = true;
        existingEntity.deleted_on = DateTime.Now;
        existingEntity.deleted_by = commandModel.calling_user_id;

        _Context.Countries.Update(existingEntity);
        await _Context.SaveChangesAsync();

        var dto = await MapToDto(existingEntity);
        return new Response<CountryDto>(dto);
    }

    public async Task<PagingResult<CountryListDto>> Find(PagingSortingParameters parameters, CountryFindCommand commandModel)
    {
        var response = new PagingResult<CountryListDto>();

        try
        {
            var query = _Context.Countries
                .Where(m => !m.is_deleted);

            if (!string.IsNullOrEmpty(commandModel.wildcard))
            {
                var wild = commandModel.wildcard.ToLower();
                query = query.Where(m =>
                    m.country_name.ToLower().Contains(wild)
                    || m.iso3.ToLower().Contains(wild)
                );
            }

            var totalCount = await query.CountAsync();
            var pagedItems = await query.SortAndPageBy(parameters).ToListAsync();

            var dtos = new List<CountryListDto>();
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

    public async Task<Response<List<CountryListDto>>> GlobalSearch(PagingSortingParameters parameters, string wildcard)
    {
        var response = new Response<List<CountryListDto>>();
        try
        {
            var query = _Context.Countries
                .Where(m => !m.is_deleted);

            if (!string.IsNullOrEmpty(wildcard))
            {
                var lower = wildcard.ToLower();
                query = query.Where(m =>
                    m.country_name.ToLower().Contains(lower)
                    || m.iso3.ToLower().Contains(lower)
                );
            }

            var pagedItems = await query.SortAndPageBy(parameters).ToListAsync();

            var dtos = new List<CountryListDto>();
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

    public async Task<CountryListDto> MapToListDto(Database.Models.Country databaseModel)
    {
        return new CountryListDto
        {
            id = databaseModel.id,
            is_deleted = databaseModel.is_deleted,
            created_on = databaseModel.created_on,
            created_by = databaseModel.created_by,
            updated_on = databaseModel.updated_on,
            updated_by = databaseModel.updated_by,
            deleted_on = databaseModel.deleted_on,
            deleted_by = databaseModel.deleted_by,
            country_name = databaseModel.country_name,
            iso3 = databaseModel.iso3,
            phonecode = databaseModel.phonecode,
            currency = databaseModel.currency,
            currency_symbol = databaseModel.currency_symbol,
            region = databaseModel.region,
        };
    }

    public async Task<CountryDto> MapToDto(Database.Models.Country databaseModel)
    {
        return new CountryDto
        {
            id = databaseModel.id,
            is_deleted = databaseModel.is_deleted,
            created_on = databaseModel.created_on,
            created_by = databaseModel.created_by,
            updated_on = databaseModel.updated_on,
            updated_by = databaseModel.updated_by,
            deleted_on = databaseModel.deleted_on,
            deleted_by = databaseModel.deleted_by,
            country_name = databaseModel.country_name,
            iso3 = databaseModel.iso3,
            phonecode = databaseModel.phonecode,
            currency = databaseModel.currency,
            currency_symbol = databaseModel.currency_symbol,
            region = databaseModel.region,
        };
    }

    public Database.Models.Country MapToDatabaseModel(CountryDto dtoModel)
    {
        return new Database.Models.Country
        {
            id = dtoModel.id,
            is_deleted = dtoModel.is_deleted,
            created_on = dtoModel.created_on,
            created_by = dtoModel.created_by,
            updated_on = dtoModel.updated_on,
            updated_by = dtoModel.updated_by,
            deleted_on = dtoModel.deleted_on,
            deleted_by = dtoModel.deleted_by,
            country_name = dtoModel.country_name,
            iso3 = dtoModel.iso3,
            phonecode = dtoModel.phonecode,
            currency = dtoModel.currency,
            currency_symbol = dtoModel.currency_symbol,
            region = dtoModel.region,
        };
    }

    private Database.Models.Country MapForCreate(CountryCreateCommand createCommandModel)
    {
        var now = DateTime.Now;

        var Country = new Database.Models.Country
        {
            country_name = createCommandModel.country_name,
            iso3 = createCommandModel.iso3,
            phonecode = createCommandModel.phonecode,
            currency = createCommandModel.currency,
            currency_symbol = createCommandModel.currency_symbol,
            region = createCommandModel.region,
            is_deleted = false,
            created_on = now,
            created_by = createCommandModel.calling_user_id,
            updated_on = now,
            updated_by = createCommandModel.calling_user_id
        };

        return Country;
    }
}
