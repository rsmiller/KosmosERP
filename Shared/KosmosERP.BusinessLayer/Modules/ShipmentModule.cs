using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using KosmosERP.Database.Models;
using KosmosERP.Database;
using KosmosERP.Models.Helpers;
using KosmosERP.Models.Interfaces;
using KosmosERP.Models;
using KosmosERP.Module;
using KosmosERP.Models.Permissions;
using KosmosERP.BusinessLayer.Interfaces;
using KosmosERP.BusinessLayer.Models.Module.Shipment.Command.Create;
using KosmosERP.BusinessLayer.Models.Module.Shipment.Command.Delete;
using KosmosERP.BusinessLayer.Models.Module.Shipment.Command.Find;
using KosmosERP.BusinessLayer.Models.Module.Shipment.Command.Edit;
using KosmosERP.BusinessLayer.Models.Module.Shipment.Dto;
using KosmosERP.BusinessLayer.Models.Module.Transaction.Command.Create;
using KosmosERP.BusinessLayer.Models.Module.Transaction.Command.Edit;
using KosmosERP.BusinessLayer.Models.Module.Transaction.Command.Delete;
using KosmosERP.BusinessLayer.Helpers;



namespace KosmosERP.BusinessLayer.Modules;

public interface IShipmentModule : IERPModule<
    ShipmentHeader,
    ShipmentHeaderDto,
    ShipmentHeaderListDto,
    ShipmentHeaderCreateCommand,
    ShipmentHeaderEditCommand,
    ShipmentHeaderDeleteCommand,
    ShipmentHeaderFindCommand>, IBaseERPModule
{
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
    public override string ModuleName => "Shipments";

    private readonly IBaseERPContext _Context;
    private IMessagePublisher _MessagePublisher;

    public ShipmentModule(IBaseERPContext context, IMessagePublisher messagePublisher) : base(context)
    {
        _Context = context;
        _MessagePublisher = messagePublisher;
    }

    public override void SeedPermissions()
    {
        var role = _Context.Roles.Any(m => m.name == "Shipping Users");
        var read_permission = _Context.ModulePermissions.Any(m => m.module_id == this.ModuleIdentifier.ToString() && m.internal_permission_name == ShipmentPermissions.Read);
        var create_permission = _Context.ModulePermissions.Any(m => m.module_id == this.ModuleIdentifier.ToString() && m.internal_permission_name == ShipmentPermissions.Create);
        var edit_permission = _Context.ModulePermissions.Any(m => m.module_id == this.ModuleIdentifier.ToString() && m.internal_permission_name == ShipmentPermissions.Edit);
        var delete_permission = _Context.ModulePermissions.Any(m => m.module_id == this.ModuleIdentifier.ToString() && m.internal_permission_name == ShipmentPermissions.Delete);

        if (role == false)
        {
            _Context.Roles.Add(CommonDataHelper<Role>.FillCommonFields(new Role()
            {
                name = "Shipping Users",
            }, 1));

            _Context.SaveChanges();
        }

        var role_id = _Context.Roles.Where(m => m.name == "Shipping Users").Select(m => m.id).Single();

        if (read_permission == false)
        {
            _Context.ModulePermissions.Add(new ModulePermission()
            {
                permission_name = "Read Shipments",
                internal_permission_name = ShipmentPermissions.Read,
                module_id = this.ModuleIdentifier.ToString(),
                module_name = this.ModuleName,
                read = true,
            });

            _Context.SaveChanges();

            var read_perm_id = _Context.ModulePermissions.Where(m => m.internal_permission_name == ShipmentPermissions.Read).Select(m => m.id).Single();

            _Context.RolePermissions.Add(CommonDataHelper<RolePermission>.FillCommonFields(new RolePermission()
            {
                role_id = role_id,
                module_permission_id = read_perm_id,
            }, 1));

            _Context.SaveChanges();
        }

        if (create_permission == false)
        {
            _Context.ModulePermissions.Add(new ModulePermission()
            {
                permission_name = "Create Shipment",
                internal_permission_name = ShipmentPermissions.Create,
                module_id = this.ModuleIdentifier.ToString(),
                module_name = this.ModuleName,
                write = true
            });

            _Context.SaveChanges();

            var create_perm_id = _Context.ModulePermissions.Where(m => m.internal_permission_name == ShipmentPermissions.Create).Select(m => m.id).Single();

            _Context.RolePermissions.Add(CommonDataHelper<RolePermission>.FillCommonFields(new RolePermission()
            {
                role_id = role_id,
                module_permission_id = create_perm_id,
            }, 1));

            _Context.SaveChanges();
        }

        if (edit_permission == false)
        {
            _Context.ModulePermissions.Add(new ModulePermission()
            {
                permission_name = "Edit Shipment",
                internal_permission_name = ShipmentPermissions.Edit,
                module_id = this.ModuleIdentifier.ToString(),
                module_name = this.ModuleName,
                edit = true
            });

            _Context.SaveChanges();

            var edit_perm_id = _Context.ModulePermissions.Where(m => m.internal_permission_name == ShipmentPermissions.Edit).Select(m => m.id).Single();

            _Context.RolePermissions.Add(CommonDataHelper<RolePermission>.FillCommonFields(new RolePermission()
            {
                role_id = role_id,
                module_permission_id = edit_perm_id,
            }, 1));

            _Context.SaveChanges();
        }

        if (delete_permission == false)
        {
            _Context.ModulePermissions.Add(new ModulePermission()
            {
                permission_name = "Delete Shipment",
                internal_permission_name = ShipmentPermissions.Delete,
                module_id = this.ModuleIdentifier.ToString(),
                module_name = this.ModuleName,
                delete = true
            });

            _Context.SaveChanges();

            var delete_perm_id = _Context.ModulePermissions.Where(m => m.internal_permission_name == ShipmentPermissions.Delete).Select(m => m.id).Single();

            _Context.RolePermissions.Add(CommonDataHelper<RolePermission>.FillCommonFields(new RolePermission()
            {
                role_id = role_id,
                module_permission_id = delete_perm_id,
            }, 1));

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

    public async Task<Response<ShipmentHeaderDto>> GetDto(int object_id)
    {
        var entity = await GetAsync(object_id);
        if (entity == null)
            return new Response<ShipmentHeaderDto>("Shipment Header not found", ResultCode.NotFound);

        var dto = await MapToDto(entity);
        return new Response<ShipmentHeaderDto>(dto);
    }

    public async Task<Response<ShipmentLineDto>> GetLineDto(int object_id)
    {
        var entity = await GetLineAsync(object_id);
        if (entity == null)
            return new Response<ShipmentLineDto>("Shipment Line not found", ResultCode.NotFound);

        var dto = await MapToLineDto(entity);
        return new Response<ShipmentLineDto>(dto);
    }

    public async Task<Response<ShipmentHeaderDto>> Create(ShipmentHeaderCreateCommand commandModel)
    {
        try
        {
            var validationResult = ModelValidationHelper.ValidateModel(commandModel);
            if (!validationResult.Success)
                return new Response<ShipmentHeaderDto>(validationResult.Exception, ResultCode.DataValidationError);

            var permission_result = await base.HasPermission(commandModel.calling_user_id, commandModel.token,ShipmentPermissions.Create, write: true);
            if (!permission_result)
                return new Response<ShipmentHeaderDto>("Invalid permission", ResultCode.InvalidPermission);

            var existingOrderEntity = await _Context.OrderHeaders.Where(m => m.id == commandModel.order_header_id).SingleOrDefaultAsync();
            if (existingOrderEntity == null)
                return new Response<ShipmentHeaderDto>("Order Header not found", ResultCode.NotFound);

            var units_sold = await _Context.OrderLines.Where(m => m.order_header_id == existingOrderEntity.id).SumAsync(m => m.quantity);

            var newShipmentHeader = this.MapForCreate(commandModel);

            newShipmentHeader.units_to_ship = units_sold;

            _Context.ShipmentHeaders.Add(newShipmentHeader);
            await _Context.SaveChangesAsync();

            // Check shipment number
            if (newShipmentHeader.shipment_number == 0)
            {
                newShipmentHeader.shipment_number = await this.ManuallyGenerateAShipmentNumber();

                _Context.ShipmentHeaders.Update(newShipmentHeader);
                await _Context.SaveChangesAsync();
            }

            // Lines
            foreach (var shipment_line in commandModel.shipment_lines)
            {
                var line = this.MapForCreate(shipment_line, newShipmentHeader.id);

                _Context.ShipmentLines.Add(line);
                await _Context.SaveChangesAsync();

                var order_line_product = await _Context.OrderLines.Where(m => m.id == line.order_line_id).Select(m => m.product_id).SingleOrDefaultAsync();
                await _MessagePublisher.PublishAsync(new Models.MessageObject()
                {
                    created_on = DateTime.UtcNow,
                    object_type = "TransactionCreateCommand",
                    body = JsonSerializer.Serialize(new TransactionCreateCommand()
                    {
                        transaction_type = TransactionType.Outbound,
                        transaction_date = DateTime.UtcNow,
                        object_reference_id = line.shipment_header_id,
                        object_sub_reference_id = line.id,
                        units_shipped = line.units_to_ship,
                        product_id = order_line_product,
                        calling_user_id = commandModel.calling_user_id,
                    })
                }, RequiredMessageTopics.TransactionMovementTopic);
            }


            var dto = await MapToDto(newShipmentHeader);
            return new Response<ShipmentHeaderDto>(dto);
        }
        catch (Exception ex)
        {
            await LogError(80, this.GetType().Name, nameof(Create), ex);
            return new Response<ShipmentHeaderDto>(ex.Message, ResultCode.Error);
        }
    }

    public async Task<Response<ShipmentLineDto>> CreateLine(ShipmentLineCreateCommand commandModel)
    {
        try
        {
            var validationResult = ModelValidationHelper.ValidateModel(commandModel);
            if (!validationResult.Success)
                return new Response<ShipmentLineDto>(validationResult.Exception, ResultCode.DataValidationError);

            var permission_result = await base.HasPermission(commandModel.calling_user_id, commandModel.token,ShipmentPermissions.Create, write: true);
            if (!permission_result)
                return new Response<ShipmentLineDto>("Invalid permission", ResultCode.InvalidPermission);


            var line = this.MapForCreate(commandModel, commandModel.shipment_header_id);

            _Context.ShipmentLines.Add(line);
            await _Context.SaveChangesAsync();

            var order_line_product = await _Context.OrderLines.Where(m => m.id == line.order_line_id).Select(m => m.product_id).SingleOrDefaultAsync();
            await _MessagePublisher.PublishAsync(new Models.MessageObject()
            {
                created_on = DateTime.UtcNow,
                object_type = "TransactionCreateCommand",
                body = JsonSerializer.Serialize(new TransactionCreateCommand()
                {
                    transaction_type = TransactionType.Outbound,
                    transaction_date = DateTime.UtcNow,
                    object_reference_id = line.shipment_header_id,
                    object_sub_reference_id = line.id,
                    units_shipped = line.units_shipped,
                    product_id = order_line_product,
                    calling_user_id = commandModel.calling_user_id,
                })
            }, RequiredMessageTopics.TransactionMovementTopic);

            var dto = await MapToLineDto(line);
            return new Response<ShipmentLineDto>(dto);
        }
        catch (Exception ex)
        {
            await LogError(80, this.GetType().Name, nameof(CreateLine), ex);
            return new Response<ShipmentLineDto>(ex.Message, ResultCode.Error);
        }
    }

    public async Task<Response<ShipmentHeaderDto>> Edit(ShipmentHeaderEditCommand commandModel)
    {
        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<ShipmentHeaderDto>(validationResult.Exception, ResultCode.DataValidationError);

        var permission_result = await base.HasPermission(commandModel.calling_user_id, commandModel.token,ShipmentPermissions.Edit, edit: true);
        if (!permission_result)
            return new Response<ShipmentHeaderDto>("Invalid permission", ResultCode.InvalidPermission);


        var existingEntity = await GetAsync(commandModel.id);
        if (existingEntity == null)
            return new Response<ShipmentHeaderDto>("Shipment Header not found", ResultCode.NotFound);

        try
        {
            if (commandModel.order_id.HasValue && existingEntity.order_header_id != commandModel.order_id)
                existingEntity.order_header_id = commandModel.order_id.Value;

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


            existingEntity = CommonDataHelper<ShipmentHeader>.FillUpdateFields(existingEntity, commandModel.calling_user_id);


            existingEntity.revision_number = existingEntity.revision_number + 1;

            _Context.ShipmentHeaders.Update(existingEntity);
            await _Context.SaveChangesAsync();

            var dto = await MapToDto(existingEntity);
            return new Response<ShipmentHeaderDto>(dto);
        }
        catch (Exception ex)
        {
            await LogError(80, this.GetType().Name, nameof(CreateLine), ex);
            return new Response<ShipmentHeaderDto>(ex.Message, ResultCode.Error);
        }
    }

    public async Task<Response<ShipmentLineDto>> EditLine(ShipmentLineEditCommand commandModel)
    {
        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<ShipmentLineDto>(validationResult.Exception, ResultCode.DataValidationError);

        var permission_result = await base.HasPermission(commandModel.calling_user_id, commandModel.token,ShipmentPermissions.Edit, edit: true);
        if (!permission_result)
            return new Response<ShipmentLineDto>("Invalid permission", ResultCode.InvalidPermission);

        var existingEntity = await GetLineAsync(commandModel.id);
        if (existingEntity == null)
            return new Response<ShipmentLineDto>("Shipment Line not found", ResultCode.NotFound);

        try
        {
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

            existingEntity = CommonDataHelper<ShipmentLine>.FillUpdateFields(existingEntity, commandModel.calling_user_id);


            existingEntity.revision_number = existingEntity.revision_number + 1;

            _Context.ShipmentLines.Update(existingEntity);
            await _Context.SaveChangesAsync();


            // Publish this data to a message queue to be processed for transactions
            var purchase_line_product = await _Context.OrderLines.Where(m => m.id == existingEntity.order_line_id).Select(m => m.product_id).SingleOrDefaultAsync();
            await _MessagePublisher.PublishAsync(new Models.MessageObject()
            {
                created_on = DateTime.UtcNow,
                object_type = "TransactionEditCommand",
                body = JsonSerializer.Serialize(new TransactionEditCommand()
                {
                    object_reference_id = existingEntity.shipment_header_id,
                    object_sub_reference_id = existingEntity.id,
                    units_shipped = existingEntity.units_shipped,
                    product_id = purchase_line_product,
                    calling_user_id = commandModel.calling_user_id,
                })
            }, RequiredMessageTopics.TransactionMovementTopic);

            var dto = await MapToLineDto(existingEntity);
            return new Response<ShipmentLineDto>(dto);
        }
        catch (Exception ex)
        {
            await LogError(80, this.GetType().Name, nameof(EditLine), ex);
            return new Response<ShipmentLineDto>(ex.Message, ResultCode.Error);
        }
    }

    public async Task<Response<ShipmentHeaderDto>> Delete(ShipmentHeaderDeleteCommand commandModel)
    {
        var permission_result = await base.HasPermission(commandModel.calling_user_id, commandModel.token,ShipmentPermissions.Delete, delete: true);
        if (!permission_result)
            return new Response<ShipmentHeaderDto>("Invalid permission", ResultCode.InvalidPermission);

        var existingEntity = await GetAsync(commandModel.id);
        if (existingEntity == null)
            return new Response<ShipmentHeaderDto>("Shipment Header not found", ResultCode.NotFound);

        try
        {
            // DO delete
            existingEntity = CommonDataHelper<ShipmentHeader>.FillDeleteFields(existingEntity, commandModel.calling_user_id);

            _Context.ShipmentHeaders.Update(existingEntity);
            await _Context.SaveChangesAsync();


            var lines = await _Context.ShipmentLines.Where(m => m.shipment_header_id == existingEntity.order_header_id).ToListAsync();
            foreach (var line in lines)
            {
                await this.DeleteLine(new ShipmentLineDeleteCommand()
                {
                    calling_user_id = commandModel.calling_user_id,
                    token = commandModel.token,
                    id = line.id,
                });
            }


            await _MessagePublisher.PublishAsync(new Models.MessageObject()
            {
                created_on = DateTime.UtcNow,
                object_type = "TransactionDeleteCommand",
                body = JsonSerializer.Serialize(new TransactionDeleteCommand()
                {
                    object_reference_id = existingEntity.id,
                    calling_user_id = commandModel.calling_user_id,
                })
            }, RequiredMessageTopics.TransactionMovementTopic);

            var dto = await MapToDto(existingEntity);
            return new Response<ShipmentHeaderDto>(dto);

        }
        catch (Exception ex)
        {
            await LogError(80, this.GetType().Name, nameof(Delete), ex);
            return new Response<ShipmentHeaderDto>(ex.Message, ResultCode.Error);
        }
    }

    public async Task<Response<ShipmentLineDto>> DeleteLine(ShipmentLineDeleteCommand commandModel)
    {
        var permission_result = await base.HasPermission(commandModel.calling_user_id, commandModel.token,ShipmentPermissions.Delete, delete: true);
        if (!permission_result)
            return new Response<ShipmentLineDto>("Invalid permission", ResultCode.InvalidPermission);

        var existingEntity = await GetLineAsync(commandModel.id);
        if (existingEntity == null)
            return new Response<ShipmentLineDto>("Shipment Line not found", ResultCode.NotFound);

        try
        {
            // Delete line
            existingEntity = CommonDataHelper<ShipmentLine>.FillDeleteFields(existingEntity, commandModel.calling_user_id);

            _Context.ShipmentLines.Update(existingEntity);
            await _Context.SaveChangesAsync();

            await _MessagePublisher.PublishAsync(new Models.MessageObject()
            {
                created_on = DateTime.UtcNow,
                object_type = "TransactionDeleteCommand",
                body = JsonSerializer.Serialize(new TransactionDeleteCommand()
                {
                    object_reference_id = existingEntity.shipment_header_id,
                    object_sub_reference_id = existingEntity.id,
                    calling_user_id = commandModel.calling_user_id,
                })
            }, RequiredMessageTopics.TransactionMovementTopic);

            var dto = await MapToLineDto(existingEntity);
            return new Response<ShipmentLineDto>(dto);
        }
        catch (Exception ex)
        {
            await LogError(80, this.GetType().Name, nameof(DeleteLine), ex);
            return new Response<ShipmentLineDto>(ex.Message, ResultCode.Error);
        }
    }

    public async Task<PagingResult<ShipmentHeaderListDto>> Find(PagingSortingParameters parameters, ShipmentHeaderFindCommand commandModel)
    {
        var response = new PagingResult<ShipmentHeaderListDto>();
        try
        {
            var permission_result = await base.HasPermission(commandModel.calling_user_id, commandModel.token,ShipmentPermissions.Read, read: true);
            if (!permission_result)
            {
                response.SetException("Invalid permission", ResultCode.InvalidPermission);
                return response;
            }

            var query = _Context.ShipmentHeaders
                .Where(m => !m.is_deleted);

            int the_num = 0;
            int.TryParse(commandModel.wildcard, out the_num);

            if (!string.IsNullOrEmpty(commandModel.wildcard) && the_num == 0)
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
            else
            {
                query = query.Where(m => m.shipment_number == the_num);
            }

            var totalCount = await query.CountAsync();
            var pagedItems = await query.SortAndPageBy(parameters).ToListAsync();

            var dtos = new List<ShipmentHeaderListDto>();
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

    public async Task<Response<List<ShipmentHeaderListDto>>> GlobalSearch(PagingSortingParameters parameters, string wildcard)
    {
        var response = new Response<List<ShipmentHeaderListDto>>();
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

            var dtos = new List<ShipmentHeaderListDto>();
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

    public async Task<ShipmentHeaderListDto> MapToListDto(ShipmentHeader databaseModel)
    {
        return new ShipmentHeaderListDto
        {
            id = databaseModel.id,
            is_deleted = databaseModel.is_deleted,
            created_on = databaseModel.created_on,
            created_by = databaseModel.created_by,
            updated_on = databaseModel.updated_on,
            updated_by = databaseModel.updated_by,
            deleted_on = databaseModel.deleted_on,
            deleted_by = databaseModel.deleted_by,
            order_header_id = databaseModel.order_header_id,
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
            guid = databaseModel.guid,
            deleted_on_string = databaseModel.deleted_on_string,
            deleted_on_timezone = databaseModel.deleted_on_timezone,
            updated_on_string = databaseModel.updated_on_string,
            updated_on_timezone = databaseModel.updated_on_timezone,
            created_on_string = databaseModel.created_on_string,
            created_on_timezone = databaseModel.created_on_timezone,
        };
    }

    public async Task<ShipmentHeaderDto> MapToDto(ShipmentHeader databaseModel)
    {
        var dto = new ShipmentHeaderDto
        {
            id = databaseModel.id,
            is_deleted = databaseModel.is_deleted,
            created_on = databaseModel.created_on,
            created_by = databaseModel.created_by,
            updated_on = databaseModel.updated_on,
            updated_by = databaseModel.updated_by,
            deleted_on = databaseModel.deleted_on,
            deleted_by = databaseModel.deleted_by,
            order_header_id = databaseModel.order_header_id,
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
            guid = databaseModel.guid,
            deleted_on_string = databaseModel.deleted_on_string,
            deleted_on_timezone = databaseModel.deleted_on_timezone,
            updated_on_string = databaseModel.updated_on_string,
            updated_on_timezone = databaseModel.updated_on_timezone,
            created_on_string = databaseModel.created_on_string,
            created_on_timezone = databaseModel.created_on_timezone,
        };

        var lines = await _Context.ShipmentLines.Where(m => m.shipment_header_id == databaseModel.order_header_id && m.is_deleted == false).ToListAsync();
        foreach(var line in lines)
            dto.shipment_lines.Add(await this.MapToLineDto(line));


        return dto;
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
            guid = databaseModel.guid,
            deleted_on_string = databaseModel.deleted_on_string,
            deleted_on_timezone = databaseModel.deleted_on_timezone,
            updated_on_string = databaseModel.updated_on_string,
            updated_on_timezone = databaseModel.updated_on_timezone,
            created_on_string = databaseModel.created_on_string,
            created_on_timezone = databaseModel.created_on_timezone,
        };
    }

    public ShipmentHeader MapToDatabaseModel(ShipmentHeaderDto dtoModel)
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
            order_header_id = dtoModel.order_header_id,
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
            guid = dtoModel.guid,
            deleted_on_string = dtoModel.deleted_on_string,
            deleted_on_timezone = dtoModel.deleted_on_timezone,
            updated_on_string = dtoModel.updated_on_string,
            updated_on_timezone = dtoModel.updated_on_timezone,
            created_on_string = dtoModel.created_on_string,
            created_on_timezone = dtoModel.created_on_timezone,
        };
    }

    private ShipmentHeader MapForCreate(ShipmentHeaderCreateCommand createCommandModel)
    {
        var shipment_header = CommonDataHelper<ShipmentHeader>.FillCommonFields(new ShipmentHeader
        {
            order_header_id = createCommandModel.order_header_id,
            address_id = createCommandModel.address_id,
            ship_via = createCommandModel.ship_via,
            ship_attn = createCommandModel.ship_attn,
            freight_carrier = createCommandModel.freight_carrier,
            freight_charge_amount = createCommandModel.freight_charge_amount,
            tax = createCommandModel.tax,
            is_deleted = false,
        }, 1);

        return shipment_header;
    }

    private ShipmentLine MapForCreate(ShipmentLineCreateCommand createCommandModel, int shipment_header_id)
    {
        var shipment_line = CommonDataHelper<ShipmentLine>.FillCommonFields(new ShipmentLine
        {
            shipment_header_id = shipment_header_id,
            order_line_id = createCommandModel.order_line_id,
            units_to_ship = createCommandModel.units_to_ship,
            units_shipped = createCommandModel.units_shipped,
            is_deleted = false,
        }, createCommandModel.calling_user_id);

        return shipment_line;
    }

    /// <summary>
    /// This method is used when the auto-gen field on Shipment Number fails. Some databases like memory databases don't auto-incremenet fields.
    /// </summary>
    /// <returns>Shipment Number</returns>
    private async Task<int> ManuallyGenerateAShipmentNumber()
    {
        var total_records = await _Context.ShipmentHeaders.CountAsync();
        return (total_records + 1);
    }
}

