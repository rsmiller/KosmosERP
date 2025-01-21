using Prometheus.Api.Models.Module.Vendor.Command.Create;
using Prometheus.Api.Models.Module.Vendor.Command.Delete;
using Prometheus.Api.Models.Module.Vendor.Command.Edit;
using Prometheus.Api.Models.Module.Vendor.Command.Find;
using Prometheus.Api.Models.Module.Vendor.Dto;
using Prometheus.Database.Models;
using Prometheus.Database;
using Prometheus.Models.Helpers;
using Prometheus.Models.Interfaces;
using Prometheus.Models;
using Prometheus.Module;
using Microsoft.EntityFrameworkCore;
using Prometheus.Api.Models.Module.Contact.Command.Create;
using Prometheus.Api.Models.Module.Contact.Dto;
using Prometheus.Api.Models.Module.Customer.Dto;

namespace Prometheus.Api.Modules;

public interface IVendorModule : IERPModule<
Vendor,
VendorDto,
VendorListDto,
VendorCreateCommand,
VendorEditCommand,
VendorDeleteCommand,
VendorFindCommand>
{
    // Add any vendor-specific methods if needed
}

public class VendorModule : BaseERPModule, IVendorModule
{
    public override Guid ModuleIdentifier => Guid.Parse("dae2593c-678b-4f6d-9c84-f4f74e066428");
    public override string ModuleName => "Vendors";

    private readonly IBaseERPContext _Context;
    private readonly IAddressModule _AddressModule;

    public VendorModule(IBaseERPContext context, IAddressModule address_module) : base(context)
    {
        _Context = context;
        _AddressModule = address_module;
    }

    public Vendor? Get(int object_id)
    {
        return _Context.Vendors.SingleOrDefault(m => m.id == object_id);
    }

    public async Task<Vendor?> GetAsync(int object_id)
    {
        return await _Context.Vendors.SingleOrDefaultAsync(m => m.id == object_id);
    }

    public async Task<Response<VendorDto>> GetDto(int object_id)
    {
        var entity = await GetAsync(object_id);
        if (entity == null)
            return new Response<VendorDto>("Vendor not found", ResultCode.NotFound);

        var dto = await MapToDto(entity);
        return new Response<VendorDto>(dto);
    }

    public async Task<Response<VendorDto>> Create(VendorCreateCommand commandModel)
    {
        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<VendorDto>(validationResult.Exception, ResultCode.DataValidationError);

        var vendor_permission_result = await base.HasPermission(commandModel.calling_user_id, "create_vendor", write: true);
        if (!vendor_permission_result)
            return new Response<VendorDto>("Invalid vendor permission", ResultCode.InvalidPermission);

        var address_permission_result = await base.HasPermission(commandModel.calling_user_id, "create_address", write: true);
        if (!address_permission_result)
            return new Response<VendorDto>("Invalid address permission", ResultCode.InvalidPermission);


        commandModel.address.calling_user_id = commandModel.calling_user_id;

        var address_response = await _AddressModule.Create(commandModel.address);

        if(!address_response.Success || address_response.Data == null)
            return new Response<VendorDto>(address_response.Exception, ResultCode.Error);


        var newVendor = this.MapForCreate(commandModel);
        newVendor.address_id = address_response.Data.id;


        _Context.Vendors.Add(newVendor);
        await _Context.SaveChangesAsync();

        var dto = await MapToDto(newVendor);
        return new Response<VendorDto>(dto);
    }

    public async Task<Response<VendorDto>> Edit(VendorEditCommand commandModel)
    {
        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<VendorDto>(validationResult.Exception, ResultCode.DataValidationError);

        var permission_result = await base.HasPermission(commandModel.calling_user_id, "edit_vendor", edit: true);
        if (!permission_result)
            return new Response<VendorDto>("Invalid permission", ResultCode.InvalidPermission);

        var existingEntity = await GetAsync(commandModel.id);
        if (existingEntity == null)
            return new Response<VendorDto>("Vendor not found", ResultCode.NotFound);

        if (existingEntity.vendor_name != commandModel.vendor_name)
            existingEntity.vendor_name = commandModel.vendor_name;

        if (existingEntity.vendor_description != commandModel.vendor_description)
            existingEntity.vendor_description = commandModel.vendor_description;

        if (existingEntity.phone != commandModel.phone)
            existingEntity.phone = commandModel.phone;

        if (commandModel.address_id.HasValue && existingEntity.address_id != commandModel.address_id)
            existingEntity.address_id = commandModel.address_id.Value;

        if (existingEntity.fax != commandModel.fax)
            existingEntity.fax = commandModel.fax;

        if (existingEntity.general_email != commandModel.general_email)
            existingEntity.general_email = commandModel.general_email;

        if (existingEntity.website != commandModel.website)
            existingEntity.website = commandModel.website;

        if (existingEntity.category != commandModel.category)
            existingEntity.category = commandModel.category;

        if (commandModel.is_critial_vendor.HasValue && existingEntity.is_critial_vendor != commandModel.is_critial_vendor)
            existingEntity.is_critial_vendor = commandModel.is_critial_vendor.HasValue;

        // Additional vendor-specific fields
        if (existingEntity.approved_on != commandModel.approved_on)
            existingEntity.approved_on = commandModel.approved_on;

        if (existingEntity.approved_by != commandModel.approved_by)
            existingEntity.approved_by = commandModel.approved_by;

        if (existingEntity.audit_on != commandModel.audit_on)
            existingEntity.audit_on = commandModel.audit_on;

        if (existingEntity.audit_by != commandModel.audit_by)
            existingEntity.audit_by = commandModel.audit_by;

        if (existingEntity.retired_on != commandModel.retired_on)
            existingEntity.retired_on = commandModel.retired_on;

        if (existingEntity.retired_by != commandModel.retired_by)
            existingEntity.retired_by = commandModel.retired_by;

        existingEntity.updated_on = DateTime.Now;
        existingEntity.updated_by = commandModel.calling_user_id;

        _Context.Vendors.Update(existingEntity);
        await _Context.SaveChangesAsync();

        var dto = await MapToDto(existingEntity);
        return new Response<VendorDto>(dto);
    }

    public async Task<Response<VendorDto>> Delete(VendorDeleteCommand commandModel)
    {
        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<VendorDto>(validationResult.Exception, ResultCode.DataValidationError);

        var permission_result = await base.HasPermission(commandModel.calling_user_id, "delete_vendor", delete: true);
        if (!permission_result)
            return new Response<VendorDto>("Invalid permission", ResultCode.InvalidPermission);

        var existingEntity = await GetAsync(commandModel.id);
        if (existingEntity == null)
            return new Response<VendorDto>("Vendor not found", ResultCode.NotFound);

        existingEntity.is_deleted = true;
        existingEntity.deleted_on = DateTime.Now;
        existingEntity.deleted_by = commandModel.calling_user_id;

        _Context.Vendors.Update(existingEntity);
        await _Context.SaveChangesAsync();

        var dto = await MapToDto(existingEntity);
        return new Response<VendorDto>(dto);
    }

    public async Task<PagingResult<VendorListDto>> Find(PagingSortingParameters parameters, VendorFindCommand commandModel)
    {
        var response = new PagingResult<VendorListDto>();
        try
        {
            var permission_result = await base.HasPermission(commandModel.calling_user_id, "read_vendor", read: true);
            if (!permission_result)
            {
                response.SetException("Invalid permission", ResultCode.InvalidPermission);
                return response;
            }

            var query = _Context.Vendors
                .Where(m => !m.is_deleted);

            if (!string.IsNullOrEmpty(commandModel.wildcard))
            {
                var wild = commandModel.wildcard.ToLower();
                query = query.Where(m =>
                    m.vendor_name.ToLower().Contains(wild)
                    || (m.vendor_description != null && m.vendor_description.ToLower().Contains(wild))
                    || m.phone.ToLower().Contains(wild)
                    || (m.fax != null && m.fax.ToLower().Contains(wild))
                    || (m.general_email != null && m.general_email.ToLower().Contains(wild))
                    || (m.website != null && m.website.ToLower().Contains(wild))
                    || m.category.ToLower().Contains(wild)
                    || m.guid.ToLower().Contains(wild)
                );
            }

            var totalCount = await query.CountAsync();
            var pagedItems = await query.SortAndPageBy(parameters).ToListAsync();

            var dtos = new List<VendorListDto>();
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

    public async Task<Response<List<VendorListDto>>> GlobalSearch(PagingSortingParameters parameters, string wildcard)
    {
        var response = new Response<List<VendorListDto>>();
        try
        {
            var query = _Context.Vendors
                .Where(m => !m.is_deleted);

            if (!string.IsNullOrEmpty(wildcard))
            {
                var lower = wildcard.ToLower();
                query = query.Where(m =>
                    m.vendor_name.ToLower().Contains(lower)
                    || (m.vendor_description != null && m.vendor_description.ToLower().Contains(lower))
                    || m.phone.ToLower().Contains(lower)
                    || (m.fax != null && m.fax.ToLower().Contains(lower))
                    || (m.general_email != null && m.general_email.ToLower().Contains(lower))
                    || (m.website != null && m.website.ToLower().Contains(lower))
                    || m.category.ToLower().Contains(lower)
                    || m.guid.ToLower().Contains(lower)
                );
            }

            var pagedItems = await query.SortAndPageBy(parameters).ToListAsync();
            var dtos = new List<VendorListDto>();
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

    public async Task<VendorListDto> MapToListDto(Vendor databaseModel)
    {
        var address = await _Context.Addresses.SingleAsync(m => m.id == databaseModel.address_id);
        var address_dto = await _AddressModule.MapToDto(address);

        return new VendorListDto
        {
            id = databaseModel.id,
            is_deleted = databaseModel.is_deleted,
            created_on = databaseModel.created_on,
            created_by = databaseModel.created_by,
            updated_on = databaseModel.updated_on,
            updated_by = databaseModel.updated_by,
            deleted_on = databaseModel.deleted_on,
            deleted_by = databaseModel.deleted_by,
            vendor_number = databaseModel.vendor_number,
            vendor_name = databaseModel.vendor_name,
            vendor_description = databaseModel.vendor_description,
            phone = databaseModel.phone,
            fax = databaseModel.fax,
            general_email = databaseModel.general_email,
            website = databaseModel.website,
            category = databaseModel.category,
            is_critial_vendor = databaseModel.is_critial_vendor,
            guid = databaseModel.guid,
            approved_on = databaseModel.approved_on,
            approved_by = databaseModel.approved_by,
            audit_on = databaseModel.audit_on,
            audit_by = databaseModel.audit_by,
            retired_on = databaseModel.retired_on,
            retired_by = databaseModel.retired_by,
            address_id = databaseModel.address_id,
            address = address_dto
        };
    }

    public async Task<VendorDto> MapToDto(Vendor databaseModel)
    {
        var address = await _Context.Addresses.SingleAsync(m => m.id == databaseModel.address_id);
        var address_dto = await _AddressModule.MapToDto(address);

        return new VendorDto
        {
            id = databaseModel.id,
            is_deleted = databaseModel.is_deleted,
            created_on = databaseModel.created_on,
            created_by = databaseModel.created_by,
            updated_on = databaseModel.updated_on,
            updated_by = databaseModel.updated_by,
            deleted_on = databaseModel.deleted_on,
            deleted_by = databaseModel.deleted_by,
            vendor_number = databaseModel.vendor_number,
            vendor_name = databaseModel.vendor_name,
            vendor_description = databaseModel.vendor_description,
            phone = databaseModel.phone,
            fax = databaseModel.fax,
            general_email = databaseModel.general_email,
            website = databaseModel.website,
            category = databaseModel.category,
            is_critial_vendor = databaseModel.is_critial_vendor,
            guid = databaseModel.guid,
            approved_on = databaseModel.approved_on,
            approved_by = databaseModel.approved_by,
            audit_on = databaseModel.audit_on,
            audit_by = databaseModel.audit_by,
            retired_on = databaseModel.retired_on,
            retired_by = databaseModel.retired_by,
            address_id = databaseModel.address_id,
            address = address_dto
        };
    }

    public Vendor MapToDatabaseModel(VendorDto dtoModel)
    {
        return new Vendor
        {
            id = dtoModel.id,
            is_deleted = dtoModel.is_deleted,
            created_on = dtoModel.created_on,
            created_by = dtoModel.created_by,
            updated_on = dtoModel.updated_on,
            updated_by = dtoModel.updated_by,
            deleted_on = dtoModel.deleted_on,
            deleted_by = dtoModel.deleted_by,
            vendor_number = dtoModel.vendor_number,
            vendor_name = dtoModel.vendor_name,
            vendor_description = dtoModel.vendor_description,
            address_id = dtoModel.address_id,
            phone = dtoModel.phone,
            fax = dtoModel.fax,
            general_email = dtoModel.general_email,
            website = dtoModel.website,
            category = dtoModel.category,
            is_critial_vendor = dtoModel.is_critial_vendor,
            guid = dtoModel.guid,
            approved_on = dtoModel.approved_on,
            approved_by = dtoModel.approved_by,
            audit_on = dtoModel.audit_on,
            audit_by = dtoModel.audit_by,
            retired_on = dtoModel.retired_on,
            retired_by = dtoModel.retired_by
        };
    }

    private Vendor MapForCreate(VendorCreateCommand createCommandModel)
    {
        var now = DateTime.Now;

        var vendor = new Vendor
        {
            vendor_name = createCommandModel.vendor_name,
            vendor_description = createCommandModel.vendor_description,
            phone = createCommandModel.phone,
            fax = createCommandModel.fax,
            general_email = createCommandModel.general_email,
            website = createCommandModel.website,
            category = createCommandModel.category,
            is_critial_vendor = createCommandModel.is_critial_vendor,
            approved_on = createCommandModel.approved_on,
            approved_by = createCommandModel.approved_by,
            audit_on = createCommandModel.audit_on,
            audit_by = createCommandModel.audit_by,
            retired_on = createCommandModel.retired_on,
            retired_by = createCommandModel.retired_by,
            is_deleted = false,
            created_on = now,
            created_by = createCommandModel.calling_user_id,
            updated_on = now,
            updated_by = createCommandModel.calling_user_id
        };

        return vendor;
    }
}
