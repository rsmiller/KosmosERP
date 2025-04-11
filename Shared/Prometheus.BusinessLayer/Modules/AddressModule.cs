using Prometheus.BusinessLayer.Models.Module.Address.Command.Create;
using Prometheus.BusinessLayer.Models.Module.Address.Command.Delete;
using Prometheus.BusinessLayer.Models.Module.Address.Command.Edit;
using Prometheus.BusinessLayer.Models.Module.Address.Command.Find;
using Prometheus.BusinessLayer.Models.Module.Address.Dto;
using Prometheus.Database.Models;
using Prometheus.Database;
using Prometheus.Models.Helpers;
using Prometheus.Models.Interfaces;
using Prometheus.Models;
using Prometheus.Module;
using Microsoft.EntityFrameworkCore;
using Prometheus.Models.Permissions;
using Prometheus.BusinessLayer.Helpers;

namespace Prometheus.BusinessLayer.Modules;

public interface IAddressModule : IERPModule<
    Address,
    AddressDto,
    AddressListDto,
    AddressCreateCommand,
    AddressEditCommand,
    AddressDeleteCommand,
    AddressFindCommand>, IBaseERPModule
{
    // Add any address-specific methods here if needed
}

public class AddressModule : BaseERPModule, IAddressModule
{
    public override Guid ModuleIdentifier => Guid.Parse("185eb94f-f464-47af-ac74-fb4e1330ef3a");
    public override string ModuleName => "Addresses";

    private readonly IBaseERPContext _Context;

    public AddressModule(IBaseERPContext context) : base(context)
    {
        _Context = context;
    }

    public override void SeedPermissions()
    {
        var read_permission = _Context.ModulePermissions.Any(m => m.module_id == this.ModuleIdentifier.ToString() && m.internal_permission_name == AddressPermissions.Read);
        var create_permission = _Context.ModulePermissions.Any(m => m.module_id == this.ModuleIdentifier.ToString() && m.internal_permission_name == AddressPermissions.Create);
        var edit_permission = _Context.ModulePermissions.Any(m => m.module_id == this.ModuleIdentifier.ToString() && m.internal_permission_name == AddressPermissions.Edit);
        var delete_permission = _Context.ModulePermissions.Any(m => m.module_id == this.ModuleIdentifier.ToString() && m.internal_permission_name == AddressPermissions.Delete);

        if (read_permission == false)
        {
            _Context.ModulePermissions.Add(new ModulePermission()
            {
                permission_name = "Read Address",
                internal_permission_name = AddressPermissions.Read,
                module_id = this.ModuleIdentifier.ToString(),
                module_name = this.ModuleName,
                read = true,
            });

            _Context.SaveChanges();
        }

        if (create_permission == false)
        {
            _Context.ModulePermissions.Add(new ModulePermission()
            {
                permission_name = "Create Address",
                internal_permission_name = AddressPermissions.Create,
                module_id = this.ModuleIdentifier.ToString(),
                module_name = this.ModuleName,
                write = true
            });

            _Context.SaveChanges();
        }

        if (edit_permission == false)
        {
            _Context.ModulePermissions.Add(new ModulePermission()
            {
                permission_name = "Edit Address",
                internal_permission_name = AddressPermissions.Edit,
                module_id = this.ModuleIdentifier.ToString(),
                module_name = this.ModuleName,
                edit = true
            });

            _Context.SaveChanges();
        }

        if (delete_permission == false)
        {
            _Context.ModulePermissions.Add(new ModulePermission()
            {
                permission_name = "Delete Address",
                internal_permission_name = AddressPermissions.Delete,
                module_id = this.ModuleIdentifier.ToString(),
                module_name = this.ModuleName,
                delete = true
            });

            _Context.SaveChanges();
        }
    }

    public Address? Get(int object_id)
    {
        // SingleOrDefault instead of Find
        return _Context.Addresses
            .SingleOrDefault(m => m.id == object_id);
    }

    public async Task<Address?> GetAsync(int object_id)
    {
        return await _Context.Addresses
            .SingleOrDefaultAsync(m => m.id == object_id);
    }

    public async Task<Response<AddressDto>> GetDto(int object_id)
    {
        var entity = await GetAsync(object_id);
        if (entity == null)
            return new Response<AddressDto>("Address not found", ResultCode.NotFound);

        var dto = await MapToDto(entity);
        return new Response<AddressDto>(dto);
    }

    public async Task<Response<AddressDto>> Create(AddressCreateCommand commandModel)
    {
        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<AddressDto>(validationResult.Exception, ResultCode.DataValidationError);

        var permission_result = await base.HasPermission(commandModel.calling_user_id, commandModel.token,AddressPermissions.Create, write: true);
        if (!permission_result)
            return new Response<AddressDto>("Invalid permission", ResultCode.InvalidPermission);

        var newAddress = this.MapForCreate(commandModel);

        _Context.Addresses.Add(newAddress);
        await _Context.SaveChangesAsync();

        var dto = await MapToDto(newAddress);
        return new Response<AddressDto>(dto);
    }

    public async Task<Response<AddressDto>> Edit(AddressEditCommand commandModel)
    {
        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<AddressDto>(validationResult.Exception, ResultCode.DataValidationError);

        var permission_result = await base.HasPermission(commandModel.calling_user_id, commandModel.token,AddressPermissions.Edit, edit: true);
        if (!permission_result)
            return new Response<AddressDto>("Invalid permission", ResultCode.InvalidPermission);

        var existingEntity = await GetAsync(commandModel.id);
        if (existingEntity == null)
            return new Response<AddressDto>("Address not found", ResultCode.NotFound);

        if (existingEntity.street_address1 != commandModel.street_address1)
            existingEntity.street_address1 = commandModel.street_address1;

        if (existingEntity.street_address2 != commandModel.street_address2)
            existingEntity.street_address2 = commandModel.street_address2;

        if (existingEntity.city != commandModel.city)
            existingEntity.city = commandModel.city;

        if (existingEntity.state != commandModel.state)
            existingEntity.state = commandModel.state;

        if (existingEntity.postal_code != commandModel.postal_code)
            existingEntity.postal_code = commandModel.postal_code;

        if (existingEntity.country != commandModel.country)
            existingEntity.country = commandModel.country;

        // Update auditing fields
        existingEntity.updated_on = DateTime.UtcNow;
        existingEntity.updated_by = commandModel.calling_user_id;

        _Context.Addresses.Update(existingEntity);
        await _Context.SaveChangesAsync();

        var dto = await MapToDto(existingEntity);
        return new Response<AddressDto>(dto);
    }

    public async Task<Response<AddressDto>> Delete(AddressDeleteCommand commandModel)
    {
        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<AddressDto>(validationResult.Exception, ResultCode.DataValidationError);


        var permission_result = await base.HasPermission(commandModel.calling_user_id, commandModel.token,AddressPermissions.Delete, delete: true);
        if (!permission_result)
            return new Response<AddressDto>("Invalid permission", ResultCode.InvalidPermission);

        var existingEntity = await GetAsync(commandModel.id);
        if (existingEntity == null)
            return new Response<AddressDto>("Address not found", ResultCode.NotFound);

        // Soft-deletes
        existingEntity = CommonDataHelper<Address>.FillDeleteFields(existingEntity, commandModel.calling_user_id);

        _Context.Addresses.Update(existingEntity);
        await _Context.SaveChangesAsync();

        var dto = await MapToDto(existingEntity);
        return new Response<AddressDto>(dto);
    }

    public async Task<PagingResult<AddressListDto>> Find(PagingSortingParameters parameters, AddressFindCommand commandModel)
    {
        var response = new PagingResult<AddressListDto>();

        try
        {
            // Example permission check
            var permission_result = await base.HasPermission(commandModel.calling_user_id, commandModel.token,AddressPermissions.Read, read: true);
            if (!permission_result)
            {
                response.SetException("Invalid permission", ResultCode.InvalidPermission);
                return response;
            }

            var query = _Context.Addresses
                .Where(m => m.is_deleted == false);

            if (!string.IsNullOrEmpty(commandModel.wildcard))
            {
                var wild = commandModel.wildcard.ToLower();
                query = query.Where(m =>
                    (m.street_address1.ToLower().Contains(wild))
                    || (m.street_address2 != null && m.street_address2.ToLower().Contains(wild))
                    || (m.city.ToLower().Contains(wild))
                    || (m.state.ToLower().Contains(wild))
                    || (m.postal_code.ToLower().Contains(wild))
                    || (m.country.ToLower().Contains(wild))
                    || (m.guid.ToLower().Contains(wild))
                );
            }

            var totalCount = await query.CountAsync();
            var pagedItems = await query.SortAndPageBy(parameters).ToListAsync();

            var dtos = new List<AddressListDto>();
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

    public async Task<Response<List<AddressListDto>>> GlobalSearch(PagingSortingParameters parameters, string wildcard)
    {
        var response = new Response<List<AddressListDto>>();
        try
        {
            var query = _Context.Addresses
                .Where(m => !m.is_deleted);

            if (!string.IsNullOrEmpty(wildcard))
            {
                var lower = wildcard.ToLower();
                query = query.Where(m =>
                    m.street_address1.ToLower().Contains(lower)
                    || (m.street_address2 != null && m.street_address2.ToLower().Contains(lower))
                    || m.city.ToLower().Contains(lower)
                    || m.state.ToLower().Contains(lower)
                    || m.postal_code.ToLower().Contains(lower)
                    || m.country.ToLower().Contains(lower)
                    || m.guid.ToLower().Contains(lower)
                );
            }

            var pagedItems = await query.SortAndPageBy(parameters).ToListAsync();

            var dtos = new List<AddressListDto>();
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

    public async Task<AddressListDto> MapToListDto(Address databaseModel)
    {
        return new AddressListDto
        {
            id = databaseModel.id,
            is_deleted = databaseModel.is_deleted,
            created_on = databaseModel.created_on,
            created_by = databaseModel.created_by,
            updated_on = databaseModel.updated_on,
            updated_by = databaseModel.updated_by,
            deleted_on = databaseModel.deleted_on,
            deleted_by = databaseModel.deleted_by,
            street_address1 = databaseModel.street_address1,
            street_address2 = databaseModel.street_address2,
            city = databaseModel.city,
            state = databaseModel.state,
            postal_code = databaseModel.postal_code,
            country = databaseModel.country,
            guid = databaseModel.guid,
            created_on_string = databaseModel.created_on_string,
            created_on_timezone = databaseModel.created_on_timezone,
            updated_on_string = databaseModel.updated_on_string,
            updated_on_timezone = databaseModel.updated_on_timezone,
        };
    }

    public async Task<AddressDto> MapToDto(Address databaseModel)
    {
        return new AddressDto
        {
            id = databaseModel.id,
            is_deleted = databaseModel.is_deleted,
            street_address1 = databaseModel.street_address1,
            street_address2 = databaseModel.street_address2,
            city = databaseModel.city,
            state = databaseModel.state,
            postal_code = databaseModel.postal_code,
            country = databaseModel.country,
            guid = databaseModel.guid,
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
        };
    }

    public Address MapToDatabaseModel(AddressDto dtoModel)
    {
        return new Address
        {
            id = dtoModel.id,
            is_deleted = dtoModel.is_deleted,
            created_on = dtoModel.created_on,
            created_by = dtoModel.created_by,
            updated_on = dtoModel.updated_on,
            updated_by = dtoModel.updated_by,
            deleted_on = dtoModel.deleted_on,
            deleted_by = dtoModel.deleted_by,
            street_address1 = dtoModel.street_address1,
            street_address2 = dtoModel.street_address2,
            city = dtoModel.city,
            state = dtoModel.state,
            postal_code = dtoModel.postal_code,
            country = dtoModel.country,
            guid = dtoModel.guid
        };
    }

    private Address MapForCreate(AddressCreateCommand createCommandModel)
    {
        var address = CommonDataHelper<Address>.FillCommonFields(new Address
        {
            street_address1 = createCommandModel.street_address1,
            street_address2 = createCommandModel.street_address2,
            city = createCommandModel.city,
            state = createCommandModel.state,
            postal_code = createCommandModel.postal_code,
            country = createCommandModel.country,
            is_deleted = false,
        }, createCommandModel.calling_user_id);

        return address;
    }
}