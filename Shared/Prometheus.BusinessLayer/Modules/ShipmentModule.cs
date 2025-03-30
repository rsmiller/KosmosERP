using Prometheus.BusinessLayer.Models.Module.Shipment.Command.Create;
using Prometheus.BusinessLayer.Models.Module.Shipment.Command.Delete;
using Prometheus.BusinessLayer.Models.Module.Shipment.Command.Find;
using Prometheus.BusinessLayer.Models.Module.Shipment.Dto;
using Prometheus.Database.Models;
using Prometheus.Database;
using Prometheus.Models.Helpers;
using Prometheus.Models.Interfaces;
using Prometheus.Models;
using Prometheus.Module;
using Microsoft.EntityFrameworkCore;

namespace Prometheus.BusinessLayer.Modules;

public interface IShipmentModule : IERPModule<
    ShipmentHeader,
    ShipmentDto,
    ShipmentListDto,
    ShipmentHeaderCreateCommand,
    ShipmentHeaderEditCommand,
    ShipmentHeaderDeleteCommand,
    ShipmentHeaderFindCommand>, IBaseERPModule
{
    // Any ShipmentHeader-specific methods can be added here
    ShipmentLine? GetLine(int object_id);
    Task<ShipmentLine?> GetLineAsync(int object_id);
    Task<Response<ShipmentLineDto>> GetLineDto(int object_id);
    Task<Response<ShipmentLineDto>> CreateLine(ShipmentLineCreateCommand commandModel);
    Task<Response<ShipmentLineDto>> EditLine(ShipmentLineEditCommand commandModel);
    Task<Response<ShipmentLineDto>> DeleteLine(ShipmentLineDeleteCommand commandModel);
}

public class ShipmentModule : BaseERPModule, IShipmentModule
{
    public override Guid ModuleIdentifier => Guid.Parse("9d624ee2-6433-49f0-bc6c-3e6978e2ac9c");
    public override string ModuleName => "Shipment";

    private readonly IBaseERPContext _Context;

    public ShipmentModule(IBaseERPContext context) : base(context)
    {
        _Context = context;
    }

    public override void SeedPermissions()
    {
        var role = _Context.Roles.Any(m => m.name == "Shipping Users");
        var read_permission = _Context.ModulePermissions.Any(m => m.module_id == this.ModuleIdentifier.ToString() && m.internal_permission_name == "read_shipping");
        var create_permission = _Context.ModulePermissions.Any(m => m.module_id == this.ModuleIdentifier.ToString() && m.internal_permission_name == "create_shipping");
        var edit_permission = _Context.ModulePermissions.Any(m => m.module_id == this.ModuleIdentifier.ToString() && m.internal_permission_name == "edit_shipping");
        var delete_permission = _Context.ModulePermissions.Any(m => m.module_id == this.ModuleIdentifier.ToString() && m.internal_permission_name == "delete_shipping");

        if (role == false)
        {
            _Context.Roles.Add(new Role()
            {
                name = "Shipping Users",
                created_by = 1,
                created_on = DateTime.Now,
                updated_by = 1,
                updated_on = DateTime.Now,
            });

            _Context.SaveChanges();
        }

        var role_id = _Context.Roles.Where(m => m.name == "Shipping Users").Select(m => m.id).Single();

        if (read_permission == false)
        {
            _Context.ModulePermissions.Add(new ModulePermission()
            {
                permission_name = "Read Shipments",
                internal_permission_name = "read_shipping",
                module_id = this.ModuleIdentifier.ToString(),
                module_name = this.ModuleName,
                read = true,
            });

            _Context.SaveChanges();

            var read_perm_id = _Context.ModulePermissions.Where(m => m.internal_permission_name == "read_shipping").Select(m => m.id).Single();

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
                permission_name = "Create Shipment",
                internal_permission_name = "create_shipping",
                module_id = this.ModuleIdentifier.ToString(),
                module_name = this.ModuleName,
                write = true
            });

            _Context.SaveChanges();

            var create_perm_id = _Context.ModulePermissions.Where(m => m.internal_permission_name == "create_shipping").Select(m => m.id).Single();

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
                permission_name = "Edit Shipment",
                internal_permission_name = "edit_shipping",
                module_id = this.ModuleIdentifier.ToString(),
                module_name = this.ModuleName,
                edit = true
            });

            _Context.SaveChanges();

            var edit_perm_id = _Context.ModulePermissions.Where(m => m.internal_permission_name == "edit_shipping").Select(m => m.id).Single();

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
                permission_name = "Delete Shipment",
                internal_permission_name = "delete_shipping",
                module_id = this.ModuleIdentifier.ToString(),
                module_name = this.ModuleName,
                delete = true
            });

            _Context.SaveChanges();

            var delete_perm_id = _Context.ModulePermissions.Where(m => m.internal_permission_name == "delete_shipping").Select(m => m.id).Single();

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


    public ShipmentHeader? Get(int object_id)
    {
        return _Context.ShipmentHeaders.SingleOrDefault(m => m.id == object_id);
    }

    public ShipmentLine? GetLine(int object_id)
    {
        return _Context.ShipmentLines.SingleOrDefault(m => m.id == object_id);
    }

    public async Task<ShipmentHeader?> GetAsync(int object_id)
    {
        return await _Context.ShipmentHeaders.SingleOrDefaultAsync(m => m.id == object_id);
    }

    public async Task<ShipmentLine?> GetLineAsync(int object_id)
    {
        return await _Context.ShipmentLines.SingleOrDefaultAsync(m => m.id == object_id);
    }

    public async Task<Response<ShipmentDto>> GetDto(int object_id)
    {
        var entity = await GetAsync(object_id);
        if (entity == null)
            return new Response<ShipmentDto>("Shipment Header not found", ResultCode.NotFound);

        var dto = await MapToDto(entity);
        return new Response<ShipmentDto>(dto);
    }

    public async Task<Response<ShipmentLineDto>> GetLineDto(int object_id)
    {
        var entity = await GetLineAsync(object_id);
        if (entity == null)
            return new Response<ShipmentLineDto>("Shipment Line not found", ResultCode.NotFound);

        var dto = await MapToLineDto(entity);
        return new Response<ShipmentLineDto>(dto);
    }

    public async Task<Response<ShipmentDto>> Create(ShipmentHeaderCreateCommand commandModel)
    {
        try
        {
            var validationResult = ModelValidationHelper.ValidateModel(commandModel);
            if (!validationResult.Success)
                return new Response<ShipmentDto>(validationResult.Exception, ResultCode.DataValidationError);

            var permission_result = await base.HasPermission(commandModel.calling_user_id, "create_shipment", write: true);
            if (!permission_result)
                return new Response<ShipmentDto>("Invalid permission", ResultCode.InvalidPermission);


            var newShipmentHeader = this.MapForCreate(commandModel);

            var db_transaction = await _Context.Database.BeginTransactionAsync();

            _Context.ShipmentHeaders.Add(newShipmentHeader);
            await _Context.SaveChangesAsync();

            foreach (var shipment_line in commandModel.shipment_lines)
            {
                var line = this.MapForCreate(shipment_line, newShipmentHeader.id);

                _Context.ShipmentLines.Add(line);
                await _Context.SaveChangesAsync();
            }

            await db_transaction.CommitAsync();

            var dto = await MapToDto(newShipmentHeader);
            return new Response<ShipmentDto>(dto);
        }
        catch (Exception ex)
        {
            return new Response<ShipmentDto>(ex.Message, ResultCode.Error);
        }
    }

    public async Task<Response<ShipmentLineDto>> CreateLine(ShipmentLineCreateCommand commandModel)
    {
        try
        {
            var validationResult = ModelValidationHelper.ValidateModel(commandModel);
            if (!validationResult.Success)
                return new Response<ShipmentLineDto>(validationResult.Exception, ResultCode.DataValidationError);

            var permission_result = await base.HasPermission(commandModel.calling_user_id, "create_shipment", write: true);
            if (!permission_result)
                return new Response<ShipmentLineDto>("Invalid permission", ResultCode.InvalidPermission);


            var line = this.MapForCreate(commandModel, commandModel.shipment_header_id);

            _Context.ShipmentLines.Add(line);
            await _Context.SaveChangesAsync();

            var dto = await MapToLineDto(line);
            return new Response<ShipmentLineDto>(dto);
        }
        catch (Exception ex)
        {
            return new Response<ShipmentLineDto>(ex.Message, ResultCode.Error);
        }
    }

    public async Task<Response<ShipmentDto>> Edit(ShipmentHeaderEditCommand commandModel)
    {
        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<ShipmentDto>(validationResult.Exception, ResultCode.DataValidationError);

        var permission_result = await base.HasPermission(commandModel.calling_user_id, "edit_shipment", edit: true);
        if (!permission_result)
            return new Response<ShipmentDto>("Invalid permission", ResultCode.InvalidPermission);


        var existingEntity = await GetAsync(commandModel.id);
        if (existingEntity == null)
            return new Response<ShipmentDto>("Shipment Header not found", ResultCode.NotFound);


        if (commandModel.order_id.HasValue && existingEntity.order_id != commandModel.order_id)
            existingEntity.order_id = commandModel.order_id.Value;

        if (commandModel.address_id.HasValue && existingEntity.address_id != commandModel.address_id)
            existingEntity.address_id = commandModel.address_id.Value;

        if (commandModel.is_complete.HasValue && existingEntity.is_complete != commandModel.is_complete)
            existingEntity.is_complete = commandModel.is_complete.Value;

        if (commandModel.is_canceled.HasValue && existingEntity.is_canceled != commandModel.is_canceled)
            existingEntity.is_canceled = commandModel.is_canceled.Value;

        if (existingEntity.ship_via != commandModel.ship_via)
            existingEntity.ship_via = commandModel.ship_via;

        if (existingEntity.ship_attn != commandModel.ship_attn)
            existingEntity.ship_attn = commandModel.ship_attn;

        if (existingEntity.freight_carrier != commandModel.freight_carrier)
            existingEntity.freight_carrier = commandModel.freight_carrier;

        if (commandModel.freight_charge_amount.HasValue && existingEntity.freight_charge_amount != commandModel.freight_charge_amount)
            existingEntity.freight_charge_amount = commandModel.freight_charge_amount.Value;

        if (commandModel.tax.HasValue && existingEntity.tax != commandModel.tax)
            existingEntity.tax = commandModel.tax.Value;

        if (existingEntity.canceled_reason != commandModel.canceled_reason)
            existingEntity.canceled_reason = commandModel.canceled_reason;

        existingEntity.updated_on = DateTime.Now;
        existingEntity.updated_by = commandModel.calling_user_id;

        existingEntity.revision_number = existingEntity.revision_number + 1;

        _Context.ShipmentHeaders.Update(existingEntity);
        await _Context.SaveChangesAsync();

        var dto = await MapToDto(existingEntity);
        return new Response<ShipmentDto>(dto);
    }

    public async Task<Response<ShipmentLineDto>> EditLine(ShipmentLineEditCommand commandModel)
    {
        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<ShipmentLineDto>(validationResult.Exception, ResultCode.DataValidationError);

        var permission_result = await base.HasPermission(commandModel.calling_user_id, "edit_shipment", edit: true);
        if (!permission_result)
            return new Response<ShipmentLineDto>("Invalid permission", ResultCode.InvalidPermission);

        var existingEntity = await GetLineAsync(commandModel.id);
        if (existingEntity == null)
            return new Response<ShipmentLineDto>("Shipment Line not found", ResultCode.NotFound);


        if (commandModel.order_line_id.HasValue && existingEntity.order_line_id != commandModel.order_line_id)
            existingEntity.order_line_id = commandModel.order_line_id.Value;

        if (commandModel.units_to_ship.HasValue && existingEntity.units_to_ship != commandModel.units_to_ship)
            existingEntity.units_to_ship = commandModel.units_to_ship.Value;

        if (commandModel.units_shipped.HasValue && existingEntity.units_shipped != commandModel.units_shipped)
            existingEntity.units_shipped = commandModel.units_shipped.Value;

        if (commandModel.is_complete.HasValue && existingEntity.is_complete != commandModel.is_complete)
            existingEntity.is_complete = commandModel.is_complete.Value;

        if (commandModel.is_canceled.HasValue && existingEntity.is_canceled != commandModel.is_canceled)
            existingEntity.is_canceled = commandModel.is_canceled.Value;

        if (existingEntity.canceled_reason != commandModel.canceled_reason)
            existingEntity.canceled_reason = commandModel.canceled_reason;

        existingEntity.updated_on = DateTime.Now;
        existingEntity.updated_by = commandModel.calling_user_id;

        existingEntity.revision_number = existingEntity.revision_number + 1;

        _Context.ShipmentLines.Update(existingEntity);
        await _Context.SaveChangesAsync();

        var dto = await MapToLineDto(existingEntity);
        return new Response<ShipmentLineDto>(dto);
    }

    public async Task<Response<ShipmentDto>> Delete(ShipmentHeaderDeleteCommand commandModel)
    {
        var permission_result = await base.HasPermission(commandModel.calling_user_id, "delete_shipment", delete: true);
        if (!permission_result)
            return new Response<ShipmentDto>("Invalid permission", ResultCode.InvalidPermission);

        var existingEntity = await GetAsync(commandModel.id);
        if (existingEntity == null)
            return new Response<ShipmentDto>("Shipment Header not found", ResultCode.NotFound);

        existingEntity.is_deleted = true;
        existingEntity.deleted_on = DateTime.Now;
        existingEntity.deleted_by = commandModel.calling_user_id;

        _Context.ShipmentHeaders.Update(existingEntity);
        await _Context.SaveChangesAsync();

        var dto = await MapToDto(existingEntity);
        return new Response<ShipmentDto>(dto);
    }

    public async Task<Response<ShipmentLineDto>> DeleteLine(ShipmentLineDeleteCommand commandModel)
    {
        var permission_result = await base.HasPermission(commandModel.calling_user_id, "delete_shipment", delete: true);
        if (!permission_result)
            return new Response<ShipmentLineDto>("Invalid permission", ResultCode.InvalidPermission);

        var existingEntity = await GetLineAsync(commandModel.id);
        if (existingEntity == null)
            return new Response<ShipmentLineDto>("Shipment Line not found", ResultCode.NotFound);

        existingEntity.is_deleted = true;
        existingEntity.deleted_on = DateTime.Now;
        existingEntity.deleted_by = commandModel.calling_user_id;

        _Context.ShipmentLines.Update(existingEntity);
        await _Context.SaveChangesAsync();

        var dto = await MapToLineDto(existingEntity);
        return new Response<ShipmentLineDto>(dto);
    }

    public async Task<PagingResult<ShipmentListDto>> Find(PagingSortingParameters parameters, ShipmentHeaderFindCommand commandModel)
    {
        var response = new PagingResult<ShipmentListDto>();
        try
        {
            var permission_result = await base.HasPermission(commandModel.calling_user_id, "read_shipment", read: true);
            if (!permission_result)
            {
                response.SetException("Invalid permission", ResultCode.InvalidPermission);
                return response;
            }

            var query = _Context.ShipmentHeaders
                .Where(m => !m.is_deleted);

            if (!string.IsNullOrEmpty(commandModel.wildcard))
            {
                var wild = commandModel.wildcard.ToLower();
                query = query.Where(m =>
                    (m.ship_via.ToLower().Contains(wild))
                    || (m.ship_attn != null && m.ship_attn.ToLower().Contains(wild))
                    || (m.freight_carrier != null && m.freight_carrier.ToLower().Contains(wild))
                    || (m.canceled_reason != null && m.canceled_reason.ToLower().Contains(wild))
                    || m.guid.ToLower().Contains(wild)
                );
            }

            var totalCount = await query.CountAsync();
            var pagedItems = await query.SortAndPageBy(parameters).ToListAsync();

            var dtos = new List<ShipmentListDto>();
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

    public async Task<Response<List<ShipmentListDto>>> GlobalSearch(PagingSortingParameters parameters, string wildcard)
    {
        var response = new Response<List<ShipmentListDto>>();
        try
        {
            var query = _Context.ShipmentHeaders
                .Where(m => !m.is_deleted);

            if (!string.IsNullOrEmpty(wildcard))
            {
                var lower = wildcard.ToLower();
                query = query.Where(m =>
                    m.ship_via.ToLower().Contains(lower)
                    || (m.ship_attn != null && m.ship_attn.ToLower().Contains(lower))
                    || (m.freight_carrier != null && m.freight_carrier.ToLower().Contains(lower))
                    || (m.canceled_reason != null && m.canceled_reason.ToLower().Contains(lower))
                    || m.guid.ToLower().Contains(lower)
                );
            }

            var pagedItems = await query.SortAndPageBy(parameters).ToListAsync();

            var dtos = new List<ShipmentListDto>();
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

    public async Task<ShipmentListDto> MapToListDto(ShipmentHeader databaseModel)
    {
        return new ShipmentListDto
        {
            id = databaseModel.id,
            is_deleted = databaseModel.is_deleted,
            created_on = databaseModel.created_on,
            created_by = databaseModel.created_by,
            updated_on = databaseModel.updated_on,
            updated_by = databaseModel.updated_by,
            deleted_on = databaseModel.deleted_on,
            deleted_by = databaseModel.deleted_by,
            order_id = databaseModel.order_id,
            shipment_number = databaseModel.shipment_number,
            address_id = databaseModel.address_id,
            units_to_ship = databaseModel.units_to_ship,
            units_shipped = databaseModel.units_shipped,
            is_complete = databaseModel.is_complete,
            is_canceled = databaseModel.is_canceled,
            ship_via = databaseModel.ship_via,
            ship_attn = databaseModel.ship_attn,
            freight_carrier = databaseModel.freight_carrier,
            freight_charge_amount = databaseModel.freight_charge_amount,
            tax = databaseModel.tax,
            completed_on = databaseModel.completed_on,
            completed_by = databaseModel.completed_by,
            canceled_on = databaseModel.canceled_on,
            canceled_by = databaseModel.canceled_by,
            canceled_reason = databaseModel.canceled_reason,
            guid = databaseModel.guid
        };
    }

    public async Task<ShipmentDto> MapToDto(ShipmentHeader databaseModel)
    {
        return new ShipmentDto
        {
            id = databaseModel.id,
            is_deleted = databaseModel.is_deleted,
            created_on = databaseModel.created_on,
            created_by = databaseModel.created_by,
            updated_on = databaseModel.updated_on,
            updated_by = databaseModel.updated_by,
            deleted_on = databaseModel.deleted_on,
            deleted_by = databaseModel.deleted_by,
            order_id = databaseModel.order_id,
            shipment_number = databaseModel.shipment_number,
            address_id = databaseModel.address_id,
            units_to_ship = databaseModel.units_to_ship,
            units_shipped = databaseModel.units_shipped,
            is_complete = databaseModel.is_complete,
            is_canceled = databaseModel.is_canceled,
            ship_via = databaseModel.ship_via,
            ship_attn = databaseModel.ship_attn,
            freight_carrier = databaseModel.freight_carrier,
            freight_charge_amount = databaseModel.freight_charge_amount,
            tax = databaseModel.tax,
            completed_on = databaseModel.completed_on,
            completed_by = databaseModel.completed_by,
            canceled_on = databaseModel.canceled_on,
            canceled_by = databaseModel.canceled_by,
            canceled_reason = databaseModel.canceled_reason,
            guid = databaseModel.guid
        };
    }

    public async Task<ShipmentLineDto> MapToLineDto(ShipmentLine databaseModel)
    {
        return new ShipmentLineDto
        {
            id = databaseModel.id,
            is_deleted = databaseModel.is_deleted,
            created_on = databaseModel.created_on,
            created_by = databaseModel.created_by,
            updated_on = databaseModel.updated_on,
            updated_by = databaseModel.updated_by,
            deleted_on = databaseModel.deleted_on,
            deleted_by = databaseModel.deleted_by,
            shipment_header_id = databaseModel.shipment_header_id,
            order_line_id = databaseModel.order_line_id,
            units_to_ship = databaseModel.units_to_ship,
            units_shipped = databaseModel.units_shipped,
            is_complete = databaseModel.is_complete,
            is_canceled = databaseModel.is_canceled,
            completed_on = databaseModel.completed_on,
            completed_by = databaseModel.completed_by,
            canceled_on = databaseModel.canceled_on,
            canceled_by = databaseModel.canceled_by,
            canceled_reason = databaseModel.canceled_reason,
            guid = databaseModel.guid
        };
    }

    public ShipmentHeader MapToDatabaseModel(ShipmentDto dtoModel)
    {
        return new ShipmentHeader
        {
            id = dtoModel.id,
            is_deleted = dtoModel.is_deleted,
            created_on = dtoModel.created_on,
            created_by = dtoModel.created_by,
            updated_on = dtoModel.updated_on,
            updated_by = dtoModel.updated_by,
            deleted_on = dtoModel.deleted_on,
            deleted_by = dtoModel.deleted_by,
            order_id = dtoModel.order_id,
            shipment_number = dtoModel.shipment_number,
            address_id = dtoModel.address_id,
            units_to_ship = dtoModel.units_to_ship,
            units_shipped = dtoModel.units_shipped,
            is_complete = dtoModel.is_complete,
            is_canceled = dtoModel.is_canceled,
            ship_via = dtoModel.ship_via,
            ship_attn = dtoModel.ship_attn,
            freight_carrier = dtoModel.freight_carrier,
            freight_charge_amount = dtoModel.freight_charge_amount,
            tax = dtoModel.tax,
            completed_on = dtoModel.completed_on,
            completed_by = dtoModel.completed_by,
            canceled_on = dtoModel.canceled_on,
            canceled_by = dtoModel.canceled_by,
            canceled_reason = dtoModel.canceled_reason,
            guid = dtoModel.guid
        };
    }

    private ShipmentHeader MapForCreate(ShipmentHeaderCreateCommand createCommandModel)
    {
        var now = DateTime.Now;

        var shipment_header = new ShipmentHeader
        {
            order_id = createCommandModel.order_id,
            address_id = createCommandModel.address_id,
            ship_via = createCommandModel.ship_via,
            ship_attn = createCommandModel.ship_attn,
            freight_carrier = createCommandModel.freight_carrier,
            freight_charge_amount = createCommandModel.freight_charge_amount,
            tax = createCommandModel.tax,
            is_deleted = false,
            created_on = now,
            created_by = createCommandModel.calling_user_id,
            updated_on = now,
            updated_by = createCommandModel.calling_user_id
        };

        return shipment_header;
    }

    private ShipmentLine MapForCreate(ShipmentLineCreateCommand createCommandModel, int shipment_header_id)
    {
        var now = DateTime.Now;

        var shipment_line = new ShipmentLine
        {
            shipment_header_id = shipment_header_id,
            order_line_id = createCommandModel.order_line_id,
            units_to_ship = createCommandModel.units_to_ship,
            units_shipped = createCommandModel.units_shipped,
            is_deleted = false,
            created_on = now,
            created_by = createCommandModel.calling_user_id,
            updated_on = now,
            updated_by = createCommandModel.calling_user_id
        };

        return shipment_line;
    }
}

