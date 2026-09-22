using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using KosmosERP.Database.Models;
using KosmosERP.Database;
using KosmosERP.Models.Helpers;
using KosmosERP.Models.Interfaces;
using KosmosERP.Models;
using KosmosERP.Module;
using KosmosERP.BusinessLayer.Models.Module.Shipment.Command.Create;
using KosmosERP.BusinessLayer.Models.Module.Shipment.Command.Delete;
using KosmosERP.BusinessLayer.Models.Module.Shipment.Command.Find;
using KosmosERP.BusinessLayer.Models.Module.Shipment.Command.Edit;
using KosmosERP.BusinessLayer.Models.Module.Shipment.Dto;
using KosmosERP.BusinessLayer.Models.Module.Transaction.Command.Create;
using KosmosERP.BusinessLayer.Models.Module.Transaction.Command.Delete;
using KosmosERP.BusinessLayer.Helpers;
using KosmosERP.Database.Views;



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
    Task<Response<List<vw_ReadyToShip>>> GetReadyToShip();
}

public class ShipmentModule : BaseERPModule, IShipmentModule
{
    public override Guid ModuleIdentifier => Guid.Parse("9d624ee2-6433-49f0-bc6c-3e6978e2ac9c");
    public override string ModuleName => "Shipments";

    private readonly IBaseERPContext _Context;
    private IMessageFactory? _MessageFactory;

    private IAddressModule? _AddressModule;
    private IMessagePublisherSettings? _MessagePublisherSettings;
    private IMemoryCacheService<KeyValueStore> _KVMemoryService;

    public ShipmentModule(IBaseERPContext context, ILogProviderFactory logProviderFactory) : base(context, logProviderFactory)
    {
        _Context = context;
    }

    public ShipmentModule(IBaseERPContext context, 
                            IMessageFactory messageFactory, 
                            IAddressModule addressModule, 
                            IMessagePublisherSettings messageSettings, 
                            ILogProviderFactory logProviderFactory,
                            IMemoryCacheService<KeyValueStore> kvMService) : base(context, logProviderFactory)
    {
        _Context = context;
        _MessageFactory = messageFactory;
        _AddressModule = addressModule;
        _MessagePublisherSettings = messageSettings;
        _KVMemoryService = kvMService;
    }

    public override void SeedPermissions()
    {
        var role = _Context.Roles.Any(m => m.name == "Shipping Administrators");

        if (role == false)
        {
            _Context.Roles.Add(CommonDataHelper<Role>.FillCommonFields(new Role()
            {
                name = "Shipping Administrators",
            }, 1));

            _Context.SaveChanges();

            base.CreateFirstRunRolePermissions();
        }

        
        var pickup_shipping_method = _Context.KeyValueStores.Where(m => m.module_id == KeyValueIds.ShippingMethods && m.key == "shipping_method_pickup").SingleOrDefault();
        var carrier_shipping_method = _Context.KeyValueStores.Where(m => m.module_id == KeyValueIds.ShippingMethods && m.key == "shipping_method_carrier").SingleOrDefault();
        var dispatch_shipping_method = _Context.KeyValueStores.Where(m => m.module_id == KeyValueIds.ShippingMethods && m.key == "shipping_method_dispatch").SingleOrDefault();


        if (pickup_shipping_method == null)
        {
            _Context.KeyValueStores.Add(CommonDataHelper<KeyValueStore>.FillCommonFields(new KeyValueStore()
            {
                key = "shipping_method_pickup",
                value = "Pickup",
                module_id = KeyValueIds.ShippingMethods.ToString(),
                int_value = 1
            }, 1));

            _Context.SaveChanges();
        }

        if (carrier_shipping_method == null)
        {
            _Context.KeyValueStores.Add(CommonDataHelper<KeyValueStore>.FillCommonFields(new KeyValueStore()
            {
                key = "shipping_method_carrier",
                value = "Carrier",
                module_id = KeyValueIds.ShippingMethods.ToString(),
                int_value = 2
            }, 1));

            _Context.SaveChanges();
        }

        if (dispatch_shipping_method == null)
        {
            _Context.KeyValueStores.Add(CommonDataHelper<KeyValueStore>.FillCommonFields(new KeyValueStore()
            {
                key = "shipping_method_dispatch",
                value = "Dispatch",
                module_id = KeyValueIds.ShippingMethods.ToString(),
                int_value = 3
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
        return _Context.ShipmentLines.Include("order_line").SingleOrDefault(m => m.id == object_id);
    }

    public async Task<ShipmentHeader?> GetAsync(int object_id)
    {
        return await _Context.ShipmentHeaders.SingleOrDefaultAsync(m => m.id == object_id);
    }

    public async Task<ShipmentLine?> GetLineAsync(int object_id)
    {
        return await _Context.ShipmentLines.Include("order_line").SingleOrDefaultAsync(m => m.id == object_id);
    }

    public async Task<Response<ShipmentHeaderDto>> GetDto(int object_id)
    {
        var entity = await GetAsync(object_id);
        if (entity == null)
            return new Response<ShipmentHeaderDto>("Shipment Header not found", ResultCode.NotFound);

        var dto = await MapToDto(entity);
        return new Response<ShipmentHeaderDto>(dto);
    }
    
    public async Task<Response<ShipmentHeaderDto>> GetDtoByGuid(string guid)
    {
        var entity = await _Context.ShipmentHeaders.FirstOrDefaultAsync(c => c.guid == guid && !c.is_deleted);
        if (entity == null)
            return new Response<ShipmentHeaderDto>("ShipmentHeader not found", ResultCode.NotFound);

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

            var existingOrderEntity = await _Context.OrderHeaders.Where(m => m.id == commandModel.order_header_id).SingleOrDefaultAsync();
            if (existingOrderEntity == null)
                return new Response<ShipmentHeaderDto>("Order Header not found", ResultCode.NotFound);

            var units_sold = await _Context.OrderLines.Where(m => m.order_header_id == existingOrderEntity.id).SumAsync(m => m.quantity);

            var newShipmentHeader = this.MapForCreate(commandModel);

            newShipmentHeader.units_to_ship = units_sold;

            newShipmentHeader.shipment_number = await this.ManuallyGenerateAShipmentNumber();

            _Context.ShipmentHeaders.Add(newShipmentHeader);
            await _Context.SaveChangesAsync();


            // Lines
            foreach (var shipment_line in commandModel.shipment_lines)
            {
                shipment_line.calling_user_id = commandModel.calling_user_id;
                
                var line = this.MapForCreate(shipment_line, newShipmentHeader.id);

                _Context.ShipmentLines.Add(line);
                await _Context.SaveChangesAsync();

                var order_line_product = await _Context.OrderLines.Where(m => m.id == line.order_line_id).Select(m => m.product_id).SingleOrDefaultAsync();

                await _MessageFactory!.GetPublisher().PublishAsync(new Models.MessageObject()
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
                }, _MessagePublisherSettings!.transaction_movement_topic!);
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

            var line = this.MapForCreate(commandModel, commandModel.shipment_header_id);

            _Context.ShipmentLines.Add(line);
            await _Context.SaveChangesAsync();

            var order_line_product = await _Context.OrderLines.Where(m => m.id == line.order_line_id).Select(m => m.product_id).SingleOrDefaultAsync();

            await _MessageFactory!.GetPublisher().PublishAsync(new Models.MessageObject()
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
            }, _MessagePublisherSettings!.transaction_movement_topic!);

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

        var existingEntity = await GetAsync(commandModel.id);
        if (existingEntity == null)
            return new Response<ShipmentHeaderDto>("Shipment Header not found", ResultCode.NotFound);

        if (existingEntity.is_released == true)
            return new Response<ShipmentHeaderDto>("Shipment header has been released and can not be modified", ResultCode.DataValidationError);

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

            if (commandModel.is_released.HasValue && existingEntity.is_released != commandModel.is_released)
                existingEntity.is_released = commandModel.is_released.Value;

            if (!String.IsNullOrEmpty(commandModel.ship_via) && existingEntity.ship_via != commandModel.ship_via)
                existingEntity.ship_via = commandModel.ship_via;

            if (!String.IsNullOrEmpty(commandModel.ship_attn) && existingEntity.ship_attn != commandModel.ship_attn)
                existingEntity.ship_attn = commandModel.ship_attn;

            if (!String.IsNullOrEmpty(commandModel.freight_carrier) && existingEntity.freight_carrier != commandModel.freight_carrier)
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


            foreach (var line in commandModel.shipment_lines)
            {
                // Edit lines
                if (!line.id.HasValue)
                {
                    line.calling_user_id = commandModel.calling_user_id;
                    var add_line = this.MapForNewLine(line, existingEntity.id, commandModel.calling_user_id);
                    await _Context.ShipmentLines.AddAsync(add_line);
                    await _Context.SaveChangesAsync();
                }
                else
                {
                    line.calling_user_id = commandModel.calling_user_id;
                    var edit_response = await this.EditLine(line);

                    if (!edit_response.Success)
                        return new Response<ShipmentHeaderDto>(edit_response.Exception, ResultCode.Error);
                }
            }


            // Released logic
            if (commandModel.is_released.HasValue && commandModel.is_released == true)
            {
                //ensuring all lines for this shipment
                var all_lines = await _Context.ShipmentLines.Include("order_line").Where(m => m.shipment_header_id == commandModel.id && m.is_deleted == false).ToListAsync();

                foreach (var line in all_lines)
                {
                    // Publish this data to a message queue to be processed for transactions
                    await _MessageFactory!.GetPublisher().PublishAsync(new Models.MessageObject()
                    {
                        created_on = DateTime.UtcNow,
                        object_type = "TransactionCreateCommand",
                        body = JsonSerializer.Serialize(new TransactionCreateCommand()
                        {
                            transaction_type = TransactionType.Outbound,
                            transaction_date = DateTime.UtcNow,
                            object_reference_id = commandModel.id,
                            object_sub_reference_id = line.id,
                            units_shipped = line.units_to_ship,
                            product_id = line.order_line.product_id,
                            sold_unit_price = line.order_line.unit_price,
                            calling_user_id = commandModel.calling_user_id,
                        })
                    }, _MessagePublisherSettings!.transaction_movement_topic!);
                }
            }

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

        var existingEntity = await GetLineAsync(commandModel.id.Value);
        if (existingEntity == null)
            return new Response<ShipmentLineDto>("Shipment Line not found", ResultCode.NotFound);

        var existingHeaderEntity = await GetAsync(existingEntity.shipment_header_id);
        if (existingHeaderEntity == null)
            return new Response<ShipmentLineDto>("Shipment header not found", ResultCode.NotFound);

        if (existingHeaderEntity.is_released == true)
            return new Response<ShipmentLineDto>("Shipment header has been released and lines can not be modified", ResultCode.DataValidationError);

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
        var existingEntity = await GetAsync(commandModel.id);
        if (existingEntity == null)
            return new Response<ShipmentHeaderDto>("Shipment Header not found", ResultCode.NotFound);

        try
        {
            // DO delete
            existingEntity = CommonDataHelper<ShipmentHeader>.FillDeleteFields(existingEntity, commandModel.calling_user_id);

            _Context.ShipmentHeaders.Update(existingEntity);
            await _Context.SaveChangesAsync();


            var lines = await _Context.ShipmentLines.Where(m => m.shipment_header_id == existingEntity.id).ToListAsync();
            foreach (var line in lines)
            {
                await this.DeleteLine(new ShipmentLineDeleteCommand()
                {
                    calling_user_id = commandModel.calling_user_id,
                    id = line.id,
                });
            }


            await _MessageFactory!.GetPublisher().PublishAsync(new Models.MessageObject()
            {
                created_on = DateTime.UtcNow,
                object_type = "TransactionDeleteCommand",
                body = JsonSerializer.Serialize(new TransactionDeleteCommand()
                {
                    object_reference_id = existingEntity.id,
                    calling_user_id = commandModel.calling_user_id,
                })
            }, _MessagePublisherSettings!.transaction_movement_topic!);

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
        var existingEntity = await GetLineAsync(commandModel.id);
        if (existingEntity == null)
            return new Response<ShipmentLineDto>("Shipment Line not found", ResultCode.NotFound);

        try
        {
            // Delete line
            existingEntity = CommonDataHelper<ShipmentLine>.FillDeleteFields(existingEntity, commandModel.calling_user_id);

            _Context.ShipmentLines.Update(existingEntity);
            await _Context.SaveChangesAsync();

            await _MessageFactory!.GetPublisher().PublishAsync(new Models.MessageObject()
            {
                created_on = DateTime.UtcNow,
                object_type = "TransactionDeleteCommand",
                body = JsonSerializer.Serialize(new TransactionDeleteCommand()
                {
                    object_reference_id = existingEntity.shipment_header_id,
                    object_sub_reference_id = existingEntity.id,
                    calling_user_id = commandModel.calling_user_id,
                })
            }, _MessagePublisherSettings!.transaction_movement_topic!);

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

            var query = _Context.ShipmentHeaders
                .Where(m => !m.is_deleted);

            int the_num = 0;

            
            if(int.TryParse(commandModel.wildcard, out the_num))
            {
                query = query.Where(m => m.shipment_number == the_num);
            }
            else if(!string.IsNullOrEmpty(commandModel.order_guid))
            {
                var order_header = await _Context.OrderHeaders.Where(m => m.guid == commandModel.order_guid && !m.is_deleted).SingleOrDefaultAsync();
                if (order_header != null)
                {
                    query = query.Where(m => m.order_header_id == order_header.id);
                }
            }
            else if (!string.IsNullOrEmpty(commandModel.wildcard) && the_num == 0)
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

    public async Task<Response<List<ShipmentHeaderListDto>>> GlobalSearch(GlobalSearchFindCommand commandModel)
    {
        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<List<ShipmentHeaderListDto>>(validationResult.Exception, ResultCode.DataValidationError);

        var response = new Response<List<ShipmentHeaderListDto>>();

        try
        {
            var query = _Context.ShipmentHeaders
                .Where(m => !m.is_deleted);

            if (!string.IsNullOrEmpty(commandModel.wildcard))
            {
                var lower = commandModel.wildcard.ToLower();
                query = query.Where(m =>
                    m.ship_via.ToLower().Contains(lower)
                    || (m.ship_attn != null && m.ship_attn.ToLower().Contains(lower))
                    || (m.freight_carrier != null && m.freight_carrier.ToLower().Contains(lower))
                    || (m.canceled_reason != null && m.canceled_reason.ToLower().Contains(lower))
                    || m.guid.ToLower().Contains(lower)
                );
            }

            var pagedItems = await query.SortAndPageBy(commandModel.parameters).ToListAsync();

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

    public async Task<Response<List<vw_ReadyToShip>>> GetReadyToShip()
    {
        Response<List<vw_ReadyToShip>> response = new Response<List<vw_ReadyToShip>>();

        try
        {
            // Units produced per order line, counting only production lines that are ready to ship.
            var produced = _Context.ProductionOrderLines
                .Where(m => m.status == "production_order_status_ready_to_ship" && !m.is_deleted)
                .GroupBy(m => m.order_line_id)
                .Select(g => new { order_line_id = g.Key, produced_quantity = g.Sum(m => m.quantity) });

            response.Data = await (
                from p in produced
                join ol in _Context.OrderLines on p.order_line_id equals ol.id
                join o in _Context.OrderHeaders on ol.order_header_id equals o.id
                join c in _Context.Customers on o.customer_id equals c.id
                join pr in _Context.Products on ol.product_id equals pr.id
                where !ol.is_deleted && !o.is_deleted
                orderby o.order_number, ol.line_number
                select new vw_ReadyToShip
                {
                    order_number = o.order_number,
                    order_guid = o.guid,
                    customer_name = c.customer_name,
                    product_name = pr.product_name,
                    sold_quantity = ol.quantity,
                    produced_quantity = p.produced_quantity,
                    shipped_quantity = _Context.ShipmentLines
                        .Where(sl => sl.order_line_id == ol.id && !sl.is_deleted && !sl.is_canceled)
                        .Sum(sl => (int?)sl.units_shipped) ?? 0
                })
                .AsNoTracking()
                .ToListAsync();
        }
        catch (Exception ex)
        {
            await LogError(50, this.GetType().Name, nameof(GetReadyToShip), ex);
            response.SetException(ex.Message, ResultCode.Error);
        }

        return response;
    }

    public ShipmentLine MapForNewLine(ShipmentLineEditCommand commandModel, int shipment_header_id, string calling_user_id)
    {
        var line = CommonDataHelper<ShipmentLine>.FillCommonFields(new ShipmentLine()
        {
            shipment_header_id = shipment_header_id,
            revision_number = 1,
            is_deleted = false,
        }, calling_user_id);

        if (commandModel.order_line_id.HasValue)
            line.order_line_id = commandModel.order_line_id.Value;
        if (commandModel.units_to_ship.HasValue)
            line.units_to_ship = commandModel.units_to_ship.Value;
        if (commandModel.units_shipped.HasValue)
            line.units_shipped = commandModel.units_shipped.Value;
        if (commandModel.is_complete.HasValue)
            line.is_complete = commandModel.is_complete.Value;
        if (commandModel.is_canceled.HasValue)
            line.is_canceled = commandModel.is_canceled.Value;

        return line;
    }

    public async Task<ShipmentHeaderListDto> MapToListDto(ShipmentHeader databaseModel)
    {
        var dto = new ShipmentHeaderListDto
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
            is_released = databaseModel.is_released,
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

        var freight_carrier_val = await _KVMemoryService.GetKeyValue(databaseModel.freight_carrier);
        if (freight_carrier_val != null)
            dto.freight_carrier_name = freight_carrier_val.value;

        var address_result = await _AddressModule!.GetDto(databaseModel.address_id);
        if (address_result.Success && address_result.Data != null)
            dto.address = address_result.Data;


        var lines = await _Context.ShipmentLines.Where(m => m.shipment_header_id == databaseModel.id && m.is_deleted == false).ToListAsync();

        foreach(var line in lines)
            dto.shipment_lines.Add(await this.MapToLineListDto(line));

        return dto;
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
            is_released = databaseModel.is_released,
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

        var lines = await _Context.ShipmentLines.Include("order_line").Where(m => m.shipment_header_id == databaseModel.id && m.is_deleted == false).ToListAsync();
        foreach(var line in lines)
            dto.shipment_lines.Add(await this.MapToLineDto(line));


        var address_result = await _AddressModule!.GetDto(databaseModel.address_id);
        if (address_result.Success && address_result.Data != null)
            dto.address = address_result.Data;


        var order_customer = await (from c in _Context.Customers
                                   join o in _Context.OrderHeaders on c.id equals o.customer_id
                                   where o.id == databaseModel.order_header_id
                                   select new {c, o}).SingleOrDefaultAsync();

        if (order_customer != null)
        {
            dto.customer_name = order_customer.c.customer_name;
            dto.po_number = order_customer.o.po_number;
            dto.order_date = order_customer.o.order_date;
            dto.order_number = order_customer.o.order_number;
        }
        
        
        return dto;
    }

    public async Task<ShipmentLineDto> MapToLineDto(ShipmentLine databaseModel)
    {
        var dto = new ShipmentLineDto
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

        if (databaseModel.order_line != null)
        {
            dto.line_description = databaseModel.order_line.line_description;
            dto.line_number = databaseModel.order_line.line_number;
            dto.units_ordered = databaseModel.order_line.quantity;
            
            dto.identifier1 = await _Context.Products.Where(m => m.id == databaseModel.order_line.product_id).Select(m => m.identifier1).SingleOrDefaultAsync();
        }
        

        return dto;
    }

    public async Task<ShipmentLineListDto> MapToLineListDto(ShipmentLine databaseModel)
    {
        var dto = new ShipmentLineListDto
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

        return dto;
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


    private async Task<int> ManuallyGenerateAShipmentNumber()
    {
        var total_records = await _Context.ShipmentHeaders.CountAsync();
        int start = DatabaseStartNumbers.Shipments;

        return (total_records + start + 1);
    }
}

