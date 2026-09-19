
using KosmosERP.BusinessLayer.Helpers;
using KosmosERP.BusinessLayer.Models.Module;
using KosmosERP.BusinessLayer.Models.Module.KeyValue.Command.Create;
using KosmosERP.BusinessLayer.Models.Module.KeyValue.Command.Delete;
using KosmosERP.BusinessLayer.Models.Module.KeyValue.Command.Edit;
using KosmosERP.BusinessLayer.Models.Module.KeyValue.Command.Find;
using KosmosERP.BusinessLayer.Models.Module.KeyValue.Dto;
using KosmosERP.Database;
using KosmosERP.Database.Models;
using KosmosERP.Models;
using KosmosERP.Models.Helpers;
using KosmosERP.Models.Interfaces;
using KosmosERP.Module;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace KosmosERP.BusinessLayer.Modules;

public interface IKeyValueModule: IERPModule<
    KeyValueStore,
    KeyValueDto,
    KeyValueListDto,
    KeyValueCreateCommand,
    KeyValueEditCommand,
    KeyValueDeleteCommand,
    KeyValueFindCommand>, IBaseERPModule
{
    Task<Response<List<KeyValueDto>>> GetDtoByModule(string module_id);
    Task<Response<List<ModuleObjectDto>>> GetModuleInfo();
}

public class KeyValueModule : BaseERPModule, IKeyValueModule
{
    public override Guid ModuleIdentifier => Guid.Parse("8063f3ae-6b8f-4f23-90f0-1daccbd18e00");
    public override string ModuleName => "Key Value Module";
    
    private IBaseERPContext _Context;


    public KeyValueModule(IBaseERPContext context, ILogProviderFactory logProviderFactory) : base(context, logProviderFactory)
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

    public KeyValueStore? Get(int object_id)
    {
        return _Context.KeyValueStores.SingleOrDefault(m => m.id == object_id);
    }

    public async Task<KeyValueStore?> GetAsync(int object_id)
    {
        return await _Context.KeyValueStores.SingleOrDefaultAsync(m => m.id == object_id);
    }

    public async Task<Response<KeyValueDto>> GetDto(int object_id)
    {
        Response<KeyValueDto> response = new Response<KeyValueDto>();

        var result = await _Context.KeyValueStores.SingleOrDefaultAsync(m => m.id == object_id);
        if (result == null)
        {
            response.SetException("Key Value Store not found", ResultCode.NotFound);
            return response;
        }

        response.Data = await this.MapToDto(result);

        return response;
    }

    public async Task<Response<KeyValueDto>> GetDtoByGuid(string guid)
    {
        throw new NotImplementedException();
    }

    public async Task<Response<List<KeyValueDto>>> GetDtoByModule(string module_id)
    {
        Response<List<KeyValueDto>> response = new Response<List<KeyValueDto>>();
        response.Data = new List<KeyValueDto>();

        var results = await _Context.KeyValueStores.Where(m => m.module_id == module_id).ToListAsync();

        foreach (var result in results)
        {
            response.Data.Add(await this.MapToDto(result));
        }


        return response;
    }

    public async Task<KeyValueDto> MapToDto(KeyValueStore databaseModel)
    {
        var dto = new KeyValueDto()
        {
            id = databaseModel.id,
            key = databaseModel.key,
            value = databaseModel.value,
            int_value = databaseModel.int_value,
            module_id = databaseModel.module_id,
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

        return dto;
    }

    public async Task<Response<KeyValueDto>> Create(KeyValueCreateCommand commandModel)
    {
        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<KeyValueDto>(validationResult.Exception, ResultCode.DataValidationError);

        var keyvks = this.MapForCreate(commandModel);

        await _Context.KeyValueStores.AddAsync(keyvks);
        await _Context.SaveChangesAsync();

        var dto = await MapToDto(keyvks);
        return new Response<KeyValueDto>(dto);
    }

    public async Task<Response<KeyValueDto>> Edit(KeyValueEditCommand commandModel)
    {
        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<KeyValueDto>(validationResult.Exception, ResultCode.DataValidationError);

        var existingEntity = await GetAsync(commandModel.id);
        if (existingEntity == null)
            return new Response<KeyValueDto>("Key Value Store not found", ResultCode.NotFound);


        if (!String.IsNullOrEmpty(commandModel.key) && existingEntity.key != commandModel.key)
            existingEntity.key = commandModel.key;

        if (!String.IsNullOrEmpty(commandModel.value) && existingEntity.value != commandModel.value)
            existingEntity.value = commandModel.value;

        if (commandModel.int_value.HasValue && existingEntity.int_value != commandModel.int_value)
            existingEntity.int_value = commandModel.int_value;

        // Update auditing fields
        existingEntity = CommonDataHelper<KeyValueStore>.FillUpdateFields(existingEntity, commandModel.calling_user_id);


        _Context.KeyValueStores.Update(existingEntity);
        await _Context.SaveChangesAsync();

        var dto = await MapToDto(existingEntity);
        return new Response<KeyValueDto>(dto);
    }

    public async Task<Response<KeyValueDto>> Delete(KeyValueDeleteCommand commandModel)
    {
        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<KeyValueDto>(validationResult.Exception, ResultCode.DataValidationError);

        var existingEntity = await GetAsync(commandModel.id);
        if (existingEntity == null)
            return new Response<KeyValueDto>("Key Value not found", ResultCode.NotFound);

        //  Soft delete
        existingEntity = CommonDataHelper<KeyValueStore>.FillDeleteFields(existingEntity, commandModel.calling_user_id);

        _Context.KeyValueStores.Update(existingEntity);
        await _Context.SaveChangesAsync();

        var dto = await MapToDto(existingEntity);
        return new Response<KeyValueDto>(dto);
    }

    public async Task<Response<List<KeyValueListDto>>> GlobalSearch(GlobalSearchFindCommand commandModel)
    {
        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<List<KeyValueListDto>>(validationResult.Exception, ResultCode.DataValidationError);


        throw new NotImplementedException();
    }

    public async Task<KeyValueListDto> MapToListDto(KeyValueStore databaseModel)
    {
        var dto = new KeyValueListDto()
        {
            id = databaseModel.id,
            key = databaseModel.key,
            value = databaseModel.value,
            int_value = databaseModel.int_value,
            module_id = databaseModel.module_id,
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

        return dto;
    }

    public KeyValueStore MapToDatabaseModel(KeyValueDto dtoModel)
    {
        throw new NotImplementedException();
    }

    public async Task<PagingResult<KeyValueListDto>> Find(PagingSortingParameters parameters, KeyValueFindCommand commandModel)
    {
        var response = new PagingResult<KeyValueListDto>();

        var query = _Context.KeyValueStores.Where(m => !m.is_deleted);

        if (!String.IsNullOrEmpty(commandModel.wildcard))
        {
            var wild = commandModel.wildcard.ToLower();

            query = query.Where(m => m.key.ToLower() == wild || m.module_id.ToLower() == wild);
        }

        var totalCount = await query.CountAsync();
        var pagedItems = await query.SortAndPageBy(parameters).ToListAsync();

        var dtos = new List<KeyValueListDto>();
        foreach (var item in pagedItems)
        {
            dtos.Add(await MapToListDto(item));
        }

        response.Data = dtos;
        response.TotalResultCount = totalCount;

        return response;
    }
    
    public async Task<Response<List<ModuleObjectDto>>> GetModuleInfo()
    {
        Response<List<ModuleObjectDto>> response = new Response<List<ModuleObjectDto>>();
        response.Data = new List<ModuleObjectDto>();

        try
        {

            var assembly = Assembly.GetExecutingAssembly();
            var baseModuleType = typeof(BaseERPModule);

            var moduleTypes = assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && baseModuleType.IsAssignableFrom(t))
                .ToList();

            foreach (var moduleType in moduleTypes)
            {
                try
                {
                    // Create an instance of the module type to access the ModuleIdentifier property
                    var instance = Activator.CreateInstance(moduleType, _Context);
                    if (instance is BaseERPModule baseModule)
                    {
                        var mod = new ModuleObjectDto()
                        {
                            module_id = baseModule.ModuleIdentifier.ToString(),
                            module_name = baseModule.ModuleName,
                        };

                        response.Data.Add(mod);

                    }
                }
                catch (Exception ex)
                {
                    await LogError(30, this.GetType().Name, $"GetModuleInfo - Error creating instance of {moduleType.Name}", ex);
                }
            }
        }
        catch (Exception ex)
        {
            await LogError(50, this.GetType().Name, "GetModuleInfo", ex);
            response.SetException(ex.Message, ResultCode.Error);
        }

        return response;
    }

    private KeyValueStore MapForCreate(KeyValueCreateCommand createCommandModel)
    {
        var model = CommonDataHelper<KeyValueStore>.FillCommonFields(new KeyValueStore
        {
            module_id = createCommandModel.module_id,
            key = createCommandModel.key,
            value = createCommandModel.value,
            int_value = createCommandModel.int_value,
            is_deleted = false,
        }, createCommandModel.calling_user_id);

        return model;
    }
}