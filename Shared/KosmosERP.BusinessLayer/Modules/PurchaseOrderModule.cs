using KosmosERP.Database;
using KosmosERP.Models.Helpers;
using KosmosERP.Models.Interfaces;
using KosmosERP.Models;
using KosmosERP.Module;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using KosmosERP.BusinessLayer.Models.Module.Transaction.Command.Create;
using KosmosERP.BusinessLayer.Helpers;
using KosmosERP.BusinessLayer.Models.Module.Transaction.Command.Edit;
using KosmosERP.BusinessLayer.Models.Module.Transaction.Command.Delete;
using KosmosERP.BusinessLayer.Models.Module.PurchaseOrder.Command.Create;
using KosmosERP.BusinessLayer.Models.Module.PurchaseOrder.Command.Delete;
using KosmosERP.BusinessLayer.Models.Module.PurchaseOrder.Command.Edit;
using KosmosERP.BusinessLayer.Models.Module.PurchaseOrder.Command.Find;
using KosmosERP.BusinessLayer.Models.Module.PurchaseOrder.Dto;
using KosmosERP.Database.Models;


namespace KosmosERP.BusinessLayer.Modules;

public interface IPurchaseOrderModule : IERPModule<
    PurchaseOrderHeader,
    PurchaseOrderHeaderDto,
    PurchaseOrderHeaderListDto,
    PurchaseOrderHeaderCreateCommand,
    PurchaseOrderHeaderEditCommand,
    PurchaseOrderHeaderDeleteCommand,
    PurchaseOrderHeaderFindCommand>, IBaseERPModule
{
    PurchaseOrderLine? GetLine(int object_id);
    Task<PurchaseOrderLine?> GetLineAsync(int object_id);
    Task<Response<PurchaseOrderLineDto>> GetLineDto(int object_id);
    Task<Response<PurchaseOrderHeaderDto>> GetByPONumber(int po_number);
    Task<Response<PurchaseOrderLineDto>> CreateLine(PurchaseOrderLineCreateCommand commandModel);
    Task<Response<PurchaseOrderLineDto>> EditLine(PurchaseOrderLineEditCommand commandModel);
    Task<Response<PurchaseOrderLineDto>> DeleteLine(PurchaseOrderLineDeleteCommand commandModel);

    Task<PurchaseOrderLineDto> MapToLineDto(PurchaseOrderLine databaseModel);
}

public class PurchaseOrderModule : BaseERPModule, IPurchaseOrderModule
{
    public override Guid ModuleIdentifier => Guid.Parse("78c4861d-1252-4cac-9461-0e1e0399cd83");
    public override string ModuleName => "Purchase Orders";

    private readonly IBaseERPContext _Context;
    private IMessageFactory? _MessageFactory;
    private IMessagePublisherSettings? _MessagePublisherSettings;

    public PurchaseOrderModule(IBaseERPContext context, ILogProviderFactory logProviderFactory) : base(context, logProviderFactory)
    {
        _Context = context;
    }

    public PurchaseOrderModule(IBaseERPContext context, 
                                    IMessageFactory messageFactory, 
                                    IMessagePublisherSettings messageSettings, 
                                    ILogProviderFactory logProviderFactory) : base(context, logProviderFactory)
    {
        _Context = context;
        _MessageFactory = messageFactory;
        _MessagePublisherSettings = messageSettings;
    }

    public override void SeedPermissions()
    {
        var role = _Context.Roles.Any(m => m.name == "Purchase Order Administrators");

        if (role == false)
        {
            _Context.Roles.Add(CommonDataHelper<Role>.FillCommonFields(new Role()
            {
                name = "Purchase Order Administrators",
            }, 1));

            _Context.SaveChanges();

            base.CreateFirstRunRolePermissions();
        }


        var material_po_type = _Context.KeyValueStores.Where(m => m.module_id == this.ModuleIdentifier.ToString() && m.key == "po_type_materials").SingleOrDefault();
        var office_po_type = _Context.KeyValueStores.Where(m => m.module_id == this.ModuleIdentifier.ToString() && m.key == "po_type_office_suplies").SingleOrDefault();

        if (material_po_type == null)
        {
            _Context.KeyValueStores.Add(CommonDataHelper<KeyValueStore>.FillCommonFields(new KeyValueStore()
            {
                key = "po_type_materials",
                value = "Build Material",
                int_value = 1,
                module_id = this.ModuleIdentifier.ToString()
            }, 1));

            _Context.SaveChanges();
        }

        if (office_po_type == null)
        {
            _Context.KeyValueStores.Add(CommonDataHelper<KeyValueStore>.FillCommonFields(new KeyValueStore()
            {
                key = "po_type_office_suplies",
                value = "Office Use",
                int_value = 2,
                module_id = this.ModuleIdentifier.ToString()
            }, 1));

            _Context.SaveChanges();
        }
    }
    public PurchaseOrderHeader? Get(int object_id)
    {
        return _Context.PurchaseOrderHeaders.Include("vendor").SingleOrDefault(m => m.id == object_id);
    }

    public PurchaseOrderLine? GetLine(int object_id)
    {
        return _Context.PurchaseOrderLines.Include("product").SingleOrDefault(m => m.id == object_id);
    }

    public async Task<PurchaseOrderHeader?> GetAsync(int object_id)
    {
        return await _Context.PurchaseOrderHeaders.Include("vendor").SingleOrDefaultAsync(m => m.id == object_id);
    }

    public async Task<PurchaseOrderLine?> GetLineAsync(int object_id)
    {
        return await _Context.PurchaseOrderLines.Include("product").SingleOrDefaultAsync(m => m.id == object_id);
    }

    public async Task<Response<PurchaseOrderHeaderDto>> GetDto(int object_id)
    {
        var entity = await GetAsync(object_id);
        if (entity == null)
            return new Response<PurchaseOrderHeaderDto>("PurchaseOrderHeader not found", ResultCode.NotFound);

        var dto = await MapToDto(entity);
        return new Response<PurchaseOrderHeaderDto>(dto);
    }

    public async Task<Response<PurchaseOrderHeaderDto>> GetDtoByGuid(string guid)
    {
        var entity = await _Context.PurchaseOrderHeaders.FirstOrDefaultAsync(c => c.guid == guid && !c.is_deleted);
        if (entity == null)
            return new Response<PurchaseOrderHeaderDto>("PurchaseOrderHeader not found", ResultCode.NotFound);

        var dto = await MapToDto(entity);
        return new Response<PurchaseOrderHeaderDto>(dto);
    }

    public async Task<Response<PurchaseOrderHeaderDto>> GetByPONumber(int po_number)
    {
        var entity = await _Context.PurchaseOrderHeaders
            .Include("vendor")
            .FirstOrDefaultAsync(c => c.po_number == po_number && !c.is_deleted);
        
        if (entity == null)
            return new Response<PurchaseOrderHeaderDto>("PurchaseOrderHeader not found", ResultCode.NotFound);

        var dto = await MapToDto(entity);
        return new Response<PurchaseOrderHeaderDto>(dto);
    }

    public async Task<Response<PurchaseOrderLineDto>> GetLineDto(int object_id)
    {
        var entity = await GetLineAsync(object_id);
        if (entity == null)
            return new Response<PurchaseOrderLineDto>("PurchaseOrderLine not found", ResultCode.NotFound);

        var dto = await MapToLineDto(entity);
        return new Response<PurchaseOrderLineDto>(dto);
    }

    public async Task<Response<PurchaseOrderHeaderDto>> Create(PurchaseOrderHeaderCreateCommand commandModel)
    {
        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<PurchaseOrderHeaderDto>(validationResult.Exception, ResultCode.DataValidationError);

        try
        {
            var newPoHeader = this.MapForCreate(commandModel);

            _Context.PurchaseOrderHeaders.Add(newPoHeader);
            await _Context.SaveChangesAsync();

            foreach (var po_line in commandModel.purchase_order_lines)
            {
                var line = this.MapForCreateLine(po_line, newPoHeader.id);

                _Context.PurchaseOrderLines.Add(line);
                await _Context.SaveChangesAsync();


                // Publish this data to a message queue to be processed for transactions
                await _MessageFactory!.GetPublisher().PublishAsync(new Models.MessageObject()
                {
                    created_on = DateTime.UtcNow,
                    object_type = "TransactionCreateCommand",
                    body = JsonSerializer.Serialize(new TransactionCreateCommand()
                    {
                        transaction_type = TransactionType.Planned,
                        transaction_date = DateTime.UtcNow,
                        object_reference_id = newPoHeader.id,
                        object_sub_reference_id = line.id,
                        purchased_unit_cost = line.unit_price,
                        units_purchased = line.quantity,
                        product_id = line.product_id,
                        calling_user_id = commandModel.calling_user_id,
                    })
                }, _MessagePublisherSettings!.transaction_movement_topic!);
            }

            await PerformPriceRecalculation(newPoHeader, commandModel.calling_user_id);

            var dto = await MapToDto(newPoHeader);
            return new Response<PurchaseOrderHeaderDto>(dto);
        }
        catch (Exception ex)
        {
            await LogError(80, this.GetType().Name, nameof(Create), ex);
            return new Response<PurchaseOrderHeaderDto>(ex.Message, ResultCode.Error);
        }
    }

    public async Task<Response<PurchaseOrderLineDto>> CreateLine(PurchaseOrderLineCreateCommand commandModel)
    {
        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<PurchaseOrderLineDto>(validationResult.Exception, ResultCode.DataValidationError);

        try
        {
            var line = this.MapForCreateLine(commandModel, commandModel.purchase_order_header_id);

            _Context.PurchaseOrderLines.Add(line);
            await _Context.SaveChangesAsync();

            await PerformPriceRecalculation(line.purchase_order_header_id, commandModel.calling_user_id);

            // Publish this data to a message queue to be processed for transactions
            await _MessageFactory!.GetPublisher().PublishAsync(new Models.MessageObject()
            {
                created_on = DateTime.UtcNow,
                object_type = "TransactionCreateCommand",
                body = JsonSerializer.Serialize(new TransactionCreateCommand()
                {
                    transaction_type = TransactionType.Planned,
                    transaction_date = DateTime.UtcNow,
                    object_reference_id = line.purchase_order_header_id,
                    object_sub_reference_id = line.id,
                    purchased_unit_cost = line.unit_price,
                    units_purchased = line.quantity,
                    product_id = line.product_id,
                    calling_user_id = commandModel.calling_user_id,
                })
            }, _MessagePublisherSettings!.transaction_movement_topic!);


            await PerformPriceRecalculation(line.purchase_order_header_id, commandModel.calling_user_id);


            var dto = await MapToLineDto(line);
            return new Response<PurchaseOrderLineDto>(dto);
        }
        catch (Exception ex)
        {
            await LogError(80, this.GetType().Name, nameof(CreateLine), ex);
            return new Response<PurchaseOrderLineDto>(ex.Message, ResultCode.Error);
        }
    }

    public async Task<Response<PurchaseOrderHeaderDto>> Edit(PurchaseOrderHeaderEditCommand commandModel)
    {
        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<PurchaseOrderHeaderDto>(validationResult.Exception, ResultCode.DataValidationError);

        // Check for validation issues with the lines
        foreach (var line in commandModel.purchase_order_lines)
        {
            var line_validation = ModelValidationHelper.ValidateModel(line);
            if (!line_validation.Success)
                return new Response<PurchaseOrderHeaderDto>(line_validation.Exception, ResultCode.DataValidationError);

            var new_edited_lines = commandModel.purchase_order_lines.Where(m => !m.id.HasValue).ToList();
            foreach (var new_edit in new_edited_lines)
            {
                // These fields must be set to be considered a new line
                if (!new_edit.product_id.HasValue
                    && !new_edit.tax.HasValue
                    && !new_edit.is_taxable.HasValue
                    && !new_edit.quantity.HasValue
                    && !new_edit.unit_price.HasValue)
                {
                    return new Response<PurchaseOrderHeaderDto>("Required field not set on new line", ResultCode.DataValidationError);
                }
            }
        }


        try
        {
            var existingEntity = await GetAsync(commandModel.id);
            if (existingEntity == null)
                return new Response<PurchaseOrderHeaderDto>("PurchaseOrderHeader not found", ResultCode.NotFound);

            if (commandModel.vendor_id.HasValue && existingEntity.vendor_id != commandModel.vendor_id)
                existingEntity.vendor_id = commandModel.vendor_id.Value;

            if (existingEntity.po_type != commandModel.po_type)
                existingEntity.po_type = commandModel.po_type;

            if (existingEntity.deleted_reason != commandModel.deleted_reason)
                existingEntity.deleted_reason = commandModel.deleted_reason;

            if (existingEntity.canceled_reason != commandModel.canceled_reason)
                existingEntity.canceled_reason = commandModel.canceled_reason;

            if (commandModel.is_complete.HasValue && existingEntity.is_complete != commandModel.is_complete)
                existingEntity.is_complete = commandModel.is_complete.Value;

            if (commandModel.is_canceled.HasValue && existingEntity.is_canceled != commandModel.is_canceled)
                existingEntity.is_canceled = commandModel.is_canceled.Value;


            existingEntity = CommonDataHelper<PurchaseOrderHeader>.FillUpdateFields(existingEntity, commandModel.calling_user_id);


            existingEntity.revision_number = existingEntity.revision_number + 1;

            _Context.PurchaseOrderHeaders.Update(existingEntity);
            await _Context.SaveChangesAsync();

            // Create or update lines
            foreach (var line in commandModel.purchase_order_lines)
            {
                // Edit lines
                if (!line.id.HasValue)
                {
                    var add_line = this.MapForEditLine(line, existingEntity.id, commandModel.calling_user_id);
                    await _Context.PurchaseOrderLines.AddAsync(add_line);
                    await _Context.SaveChangesAsync();


                    // Publish this data to a message queue to be processed for transactions
                    await _MessageFactory!.GetPublisher().PublishAsync(new Models.MessageObject()
                    {
                        created_on = DateTime.UtcNow,
                        object_type = "TransactionCreateCommand",
                        body = JsonSerializer.Serialize(new TransactionCreateCommand()
                        {
                            transaction_type = TransactionType.Planned,
                            transaction_date = DateTime.UtcNow,
                            object_reference_id = add_line.purchase_order_header_id,
                            object_sub_reference_id = add_line.id,
                            purchased_unit_cost = add_line.unit_price,
                            units_purchased = add_line.quantity,
                            product_id = add_line.product_id,
                            calling_user_id = commandModel.calling_user_id,
                        })
                    }, _MessagePublisherSettings!.transaction_movement_topic!);
                }
                else
                {
                    var edit_response = await this.EditLine(line);

                    if (!edit_response.Success)
                        return new Response<PurchaseOrderHeaderDto>(edit_response.Exception, ResultCode.Error);
                }
            }

            await PerformPriceRecalculation(existingEntity, commandModel.calling_user_id);

            var dto = await MapToDto(existingEntity);
            return new Response<PurchaseOrderHeaderDto>(dto);
        }
        catch (Exception ex)
        {
            await LogError(80, this.GetType().Name, nameof(Edit), ex);
            return new Response<PurchaseOrderHeaderDto>(ex.Message, ResultCode.Error);
        }
    }

    public async Task<Response<PurchaseOrderLineDto>> EditLine(PurchaseOrderLineEditCommand commandModel)
    {
        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<PurchaseOrderLineDto>(validationResult.Exception, ResultCode.DataValidationError);


        if (!commandModel.id.HasValue)
            return new Response<PurchaseOrderLineDto>("Purchase Order Line must have an id", ResultCode.DataValidationError);

        var existingEntity = await GetLineAsync(commandModel.id.Value);
        if (existingEntity == null)
            return new Response<PurchaseOrderLineDto>("Purchase Order Line not found", ResultCode.NotFound);

        try
        {

            if (commandModel.product_id.HasValue && existingEntity.product_id != commandModel.product_id)
                existingEntity.product_id = commandModel.product_id.Value;

            if (existingEntity.line_number != commandModel.line_number)
                existingEntity.line_number = commandModel.line_number;

            if (commandModel.quantity.HasValue && existingEntity.quantity != commandModel.quantity)
                existingEntity.quantity = commandModel.quantity.Value;

            if (existingEntity.description != commandModel.description)
                existingEntity.description = commandModel.description;

            if (commandModel.unit_price.HasValue && existingEntity.unit_price != commandModel.unit_price)
                existingEntity.unit_price = commandModel.unit_price.Value;

            if (commandModel.tax.HasValue && existingEntity.tax != commandModel.tax)
                existingEntity.tax = commandModel.tax.Value;

            if (commandModel.is_taxable.HasValue && existingEntity.is_taxable != commandModel.is_taxable)
                existingEntity.is_taxable = commandModel.is_taxable.Value;


            existingEntity = CommonDataHelper<PurchaseOrderLine>.FillUpdateFields(existingEntity, commandModel.calling_user_id);


            existingEntity.revision_number = existingEntity.revision_number + 1;

            _Context.PurchaseOrderLines.Update(existingEntity);
            await _Context.SaveChangesAsync();

            await PerformPriceRecalculation(existingEntity.purchase_order_header_id, commandModel.calling_user_id);

            // Publish this data to a message queue to be processed for transactions
            await _MessageFactory!.GetPublisher().PublishAsync(new Models.MessageObject()
            {
                created_on = DateTime.UtcNow,
                object_type = "TransactionEditCommand",
                body = JsonSerializer.Serialize(new TransactionEditCommand()
                {
                    object_reference_id = existingEntity.purchase_order_header_id,
                    object_sub_reference_id = existingEntity.id,
                    purchased_unit_cost = existingEntity.unit_price,
                    units_purchased = existingEntity.quantity,
                    product_id = existingEntity.product_id,
                    calling_user_id = commandModel.calling_user_id,
                })
            }, _MessagePublisherSettings!.transaction_movement_topic!);

            var dto = await MapToLineDto(existingEntity);
            return new Response<PurchaseOrderLineDto>(dto);
        }
        catch (Exception ex)
        {
            await LogError(80, this.GetType().Name, nameof(EditLine), ex);
            return new Response<PurchaseOrderLineDto>(ex.Message, ResultCode.Error);
        }
    }

    public async Task<Response<PurchaseOrderHeaderDto>> Delete(PurchaseOrderHeaderDeleteCommand commandModel)
    {

        var existingEntity = await GetAsync(commandModel.id);
        if (existingEntity == null)
            return new Response<PurchaseOrderHeaderDto>("PurchaseOrderHeader not found", ResultCode.NotFound);

        try
        {
            // Soft delete
            existingEntity = CommonDataHelper<PurchaseOrderHeader>.FillDeleteFields(existingEntity, commandModel.calling_user_id);


            _Context.PurchaseOrderHeaders.Update(existingEntity);
            await _Context.SaveChangesAsync();

            var lines = await _Context.PurchaseOrderLines.Where(m => m.purchase_order_header_id == existingEntity.id && m.is_deleted == false).ToListAsync();

            foreach (var line in lines)
            {
                await this.DeleteLine(new PurchaseOrderLineDeleteCommand()
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
            return new Response<PurchaseOrderHeaderDto>(dto);
        }
        catch (Exception ex)
        {
            await LogError(80, this.GetType().Name, nameof(Delete), ex);
            return new Response<PurchaseOrderHeaderDto>(ex.Message, ResultCode.Error);
        }
    }

    public async Task<Response<PurchaseOrderLineDto>> DeleteLine(PurchaseOrderLineDeleteCommand commandModel)
    {
        var existingEntity = await GetLineAsync(commandModel.id);
        if (existingEntity == null)
            return new Response<PurchaseOrderLineDto>("PurchaseOrderLine not found", ResultCode.NotFound);

        try
        {
            // Do Delete
            existingEntity = CommonDataHelper<PurchaseOrderLine>.FillDeleteFields(existingEntity, commandModel.calling_user_id);

            _Context.PurchaseOrderLines.Update(existingEntity);
            await _Context.SaveChangesAsync();

            await PerformPriceRecalculation(existingEntity.purchase_order_header_id, commandModel.calling_user_id);

            await _MessageFactory!.GetPublisher().PublishAsync(new Models.MessageObject()
            {
                created_on = DateTime.UtcNow,
                object_type = "TransactionDeleteCommand",
                body = JsonSerializer.Serialize(new TransactionDeleteCommand()
                {
                    object_reference_id = existingEntity.purchase_order_header_id,
                    object_sub_reference_id = existingEntity.id,
                    calling_user_id = commandModel.calling_user_id,
                })
            }, _MessagePublisherSettings!.transaction_movement_topic!);

            var dto = await MapToLineDto(existingEntity);
            return new Response<PurchaseOrderLineDto>(dto);
        }
        catch (Exception ex)
        {
            await LogError(80, this.GetType().Name, nameof(DeleteLine), ex);
            return new Response<PurchaseOrderLineDto>(ex.Message, ResultCode.Error);
        }
    }

    public async Task<PagingResult<PurchaseOrderHeaderListDto>> Find(PagingSortingParameters parameters, PurchaseOrderHeaderFindCommand commandModel)
    {
        var response = new PagingResult<PurchaseOrderHeaderListDto>();

        try
        {
            var query = _Context.PurchaseOrderHeaders
                .Where(m => !m.is_deleted).Include("vendor");

            if (commandModel.vendor_id.HasValue)
            {
                query = query.Where(m => m.vendor_id == commandModel.vendor_id);
            }

            int parsed_num = 0;

            if (int.TryParse(commandModel.wildcard, out parsed_num))
            {
                query = query.Where(m => m.po_number.ToString().Contains(commandModel.wildcard));
            }
            else if (!String.IsNullOrEmpty(commandModel.wildcard))
            {
                var wild = commandModel.wildcard.ToLower();
                query = query.Where(m =>
                    m.po_type.ToLower().Contains(wild)
                    || (m.deleted_reason != null && m.deleted_reason.ToLower().Contains(wild))
                    || (m.canceled_reason != null && m.canceled_reason.ToLower().Contains(wild))
                );
            }

            var totalCount = await query.CountAsync();
            var pagedItems = await query.SortAndPageBy(parameters).ToListAsync();

            var dtos = new List<PurchaseOrderHeaderListDto>();
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

    public async Task<Response<List<PurchaseOrderHeaderListDto>>> GlobalSearch(GlobalSearchFindCommand commandModel)
    {
        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<List<PurchaseOrderHeaderListDto>>(validationResult.Exception, ResultCode.DataValidationError);

        var response = new Response<List<PurchaseOrderHeaderListDto>>();

        try
        {
            var filter = PredicateBuilder.True<PurchaseOrderHeader>();
            filter = filter.And(m => m.is_deleted == false);

            int parsed_num = 0;

            if (int.TryParse(commandModel.wildcard, out parsed_num))
            {
                filter = filter.And(m => m.po_number == parsed_num);
            }
            else
            {
                var lower = commandModel.wildcard.ToLower();
                filter = filter.And(m => m.canceled_reason != null && m.canceled_reason.ToLower().Contains(lower));
            }

            var results = _Context.PurchaseOrderHeaders.Include("vendor").Where(filter);
            var pagedItems = await results.SortAndPageBy(commandModel.parameters).ToListAsync();

            var dtos = new List<PurchaseOrderHeaderListDto>();
            foreach (var item in pagedItems)
                dtos.Add(await MapToListDto(item));


            response.Data = dtos;
        }
        catch (Exception ex)
        {
            await LogError(50, this.GetType().Name, nameof(GlobalSearch), ex);
            response.SetException(ex.Message, ResultCode.Error);
        }

        return response;
    }

    public async Task<PurchaseOrderHeaderListDto> MapToListDto(PurchaseOrderHeader databaseModel)
    {
        var dto = new PurchaseOrderHeaderListDto
        {
            id = databaseModel.id,
            is_deleted = databaseModel.is_deleted,
            created_on = databaseModel.created_on,
            created_by = databaseModel.created_by,
            updated_on = databaseModel.updated_on,
            updated_by = databaseModel.updated_by,
            deleted_on = databaseModel.deleted_on,
            deleted_by = databaseModel.deleted_by,
            vendor_id = databaseModel.vendor_id,
            po_type = databaseModel.po_type,
            revision_number = databaseModel.revision_number,
            po_number = databaseModel.po_number,
            price = databaseModel.price,
            deleted_reason = databaseModel.deleted_reason,
            canceled_reason = databaseModel.canceled_reason,
            is_complete = databaseModel.is_complete,
            is_canceled = databaseModel.is_canceled,
            completed_on = databaseModel.completed_on,
            completed_by = databaseModel.completed_by,
            canceled_on = databaseModel.canceled_on,
            canceled_by = databaseModel.canceled_by,
            guid = databaseModel.guid,
            updated_on_string = databaseModel.updated_on_string,
            updated_on_timezone = databaseModel.updated_on_timezone,
            created_on_string = databaseModel.created_on_string,
            created_on_timezone = databaseModel.created_on_timezone,
            deleted_on_string = databaseModel.deleted_on_string,
            deleted_on_timezone = databaseModel.deleted_on_timezone,
        };

        dto.vendor_name = databaseModel.vendor.vendor_name;

        return dto;
    }

    public async Task<PurchaseOrderHeaderDto> MapToDto(PurchaseOrderHeader databaseModel)
    {
        var dto = new PurchaseOrderHeaderDto
        {
            id = databaseModel.id,
            is_deleted = databaseModel.is_deleted,
            created_on = databaseModel.created_on,
            created_by = databaseModel.created_by,
            updated_on = databaseModel.updated_on,
            updated_by = databaseModel.updated_by,
            deleted_on = databaseModel.deleted_on,
            deleted_by = databaseModel.deleted_by,
            vendor_id = databaseModel.vendor_id,
            po_type = databaseModel.po_type,
            revision_number = databaseModel.revision_number,
            po_number = databaseModel.po_number,
            price = databaseModel.price,
            deleted_reason = databaseModel.deleted_reason,
            canceled_reason = databaseModel.canceled_reason,
            is_complete = databaseModel.is_complete,
            is_canceled = databaseModel.is_canceled,
            completed_on = databaseModel.completed_on,
            completed_by = databaseModel.completed_by,
            canceled_on = databaseModel.canceled_on,
            canceled_by = databaseModel.canceled_by,
            guid = databaseModel.guid,
            updated_on_string = databaseModel.updated_on_string,
            updated_on_timezone = databaseModel.updated_on_timezone,
            created_on_string = databaseModel.created_on_string,
            created_on_timezone = databaseModel.created_on_timezone,
            deleted_on_string = databaseModel.deleted_on_string,
            deleted_on_timezone = databaseModel.deleted_on_timezone,
        };

        var lines = await _Context.PurchaseOrderLines.Where(m => m.purchase_order_header_id == databaseModel.id && m.is_deleted == false).ToListAsync();

        foreach (var line in lines)
            dto.purchase_order_lines.Add(await this.MapToLineDto(line));

        if (databaseModel.vendor != null)
            dto.vendor_name = databaseModel.vendor.vendor_name;
        else
            dto.vendor_name = await _Context.Vendors.Where(m => m.id == databaseModel.vendor_id).Select(m => m.vendor_name).SingleOrDefaultAsync();

        dto.po_by = await _Context.Users.Where(m => m.external_id == databaseModel.created_by).Select(m => m.first_name + " " + m.last_name).SingleOrDefaultAsync();

        return dto;
    }

    public async Task<PurchaseOrderLineDto> MapToLineDto(PurchaseOrderLine databaseModel)
    {
        var dto = new PurchaseOrderLineDto
        {
            id = databaseModel.id,
            is_deleted = databaseModel.is_deleted,
            created_on = databaseModel.created_on,
            created_by = databaseModel.created_by,
            updated_on = databaseModel.updated_on,
            updated_by = databaseModel.updated_by,
            deleted_on = databaseModel.deleted_on,
            deleted_by = databaseModel.deleted_by,
            revision_number = databaseModel.revision_number,
            purchase_order_header_id = databaseModel.purchase_order_header_id,
            product_id = databaseModel.product_id,
            line_number = databaseModel.line_number,
            quantity = databaseModel.quantity,
            description = databaseModel.description,
            unit_price = databaseModel.unit_price,
            tax = databaseModel.tax,
            is_taxable = databaseModel.is_taxable,
            is_complete = databaseModel.is_complete,
            is_canceled = databaseModel.is_canceled,
            guid = databaseModel.guid,
            updated_on_string = databaseModel.updated_on_string,
            updated_on_timezone = databaseModel.updated_on_timezone,
            created_on_string = databaseModel.created_on_string,
            created_on_timezone = databaseModel.created_on_timezone,
            deleted_on_string = databaseModel.deleted_on_string,
            deleted_on_timezone = databaseModel.deleted_on_timezone,
        };

        if (databaseModel.product != null)
        {
            dto.product_name = databaseModel.product.product_name;
            dto.identifier1 = databaseModel.product.identifier1;
        }
        else
        {
            var product = await _Context.Products.Where(m => m.id == databaseModel.product_id).SingleOrDefaultAsync();

            if (product != null)
            {
                dto.product_name = product.product_name;
                dto.identifier1 = product.identifier1;
            }
        }


        return dto;
    }

    public PurchaseOrderHeader MapToDatabaseModel(PurchaseOrderHeaderDto dtoModel)
    {
        return new PurchaseOrderHeader
        {
            id = dtoModel.id,
            is_deleted = dtoModel.is_deleted,
            created_on = dtoModel.created_on,
            created_by = dtoModel.created_by,
            updated_on = dtoModel.updated_on,
            updated_by = dtoModel.updated_by,
            deleted_on = dtoModel.deleted_on,
            deleted_by = dtoModel.deleted_by,
            vendor_id = dtoModel.vendor_id,
            po_type = dtoModel.po_type,
            revision_number = dtoModel.revision_number,
            po_number = dtoModel.po_number,
            deleted_reason = dtoModel.deleted_reason,
            canceled_reason = dtoModel.canceled_reason,
            is_complete = dtoModel.is_complete,
            is_canceled = dtoModel.is_canceled,
            completed_on = dtoModel.completed_on,
            completed_by = dtoModel.completed_by,
            canceled_on = dtoModel.canceled_on,
            canceled_by = dtoModel.canceled_by,
            guid = dtoModel.guid,
            updated_on_string = dtoModel.updated_on_string,
            updated_on_timezone = dtoModel.updated_on_timezone,
            created_on_string = dtoModel.created_on_string,
            created_on_timezone = dtoModel.created_on_timezone,
            deleted_on_string = dtoModel.deleted_on_string,
            deleted_on_timezone = dtoModel.deleted_on_timezone,
        };
    }

    private PurchaseOrderHeader MapForCreate(PurchaseOrderHeaderCreateCommand createCommandModel)
    {
        var header = CommonDataHelper<PurchaseOrderHeader>.FillCommonFields(new PurchaseOrderHeader
        {
            vendor_id = createCommandModel.vendor_id,
            po_type = createCommandModel.po_type,
            revision_number = 1,
            is_deleted = false,
        }, createCommandModel.calling_user_id);

        return header;
    }

    public PurchaseOrderLine MapForEditLine(PurchaseOrderLineEditCommand commandModel, int purchase_order_header_id, string calling_user_id)
    {
        var line = CommonDataHelper<PurchaseOrderLine>.FillCommonFields(new PurchaseOrderLine()
        {
            purchase_order_header_id = purchase_order_header_id,
            product_id = commandModel.product_id.Value,
            line_number = commandModel.line_number,
            quantity = commandModel.quantity.Value,
            description = commandModel.description,
            unit_price = commandModel.unit_price.Value,
            tax = commandModel.tax.Value,
            is_taxable = commandModel.is_taxable.Value,
            revision_number = 1,
            is_deleted = false,
        }, commandModel.calling_user_id);

        return line;
    }

    private PurchaseOrderLine MapForCreateLine(PurchaseOrderLineCreateCommand createCommandModel, int purchase_order_header_id)
    {
        var line = CommonDataHelper<PurchaseOrderLine>.FillCommonFields(new PurchaseOrderLine()
        {
            purchase_order_header_id = purchase_order_header_id,
            product_id = createCommandModel.product_id,
            line_number = createCommandModel.line_number,
            quantity = createCommandModel.quantity,
            description = createCommandModel.description,
            unit_price = createCommandModel.unit_price,
            tax = createCommandModel.tax,
            is_taxable = createCommandModel.is_taxable,
            revision_number = 1,
            is_deleted = false,
        }, createCommandModel.calling_user_id);

        return line;
    }
    private async Task PerformPriceRecalculation(int header_id, string calling_user_id)
    {
        var header = await _Context.PurchaseOrderHeaders.SingleAsync(m => m.id == header_id);

        await PerformPriceRecalculation(header, calling_user_id);
    }

    private async Task PerformPriceRecalculation(PurchaseOrderHeader existingEntity, string calling_user_id)
    {
        var lines = await _Context.PurchaseOrderLines.Where(m => m.purchase_order_header_id == existingEntity.id).ToListAsync();

        await PerformPriceRecalculation(existingEntity, lines, calling_user_id);
    }

    private async Task PerformPriceRecalculation(PurchaseOrderHeader existingEntity, List<PurchaseOrderLine> existingLines, string calling_user_id)
    {
        var total = existingLines.Where(m => m.is_deleted == false).Sum(m => m.unit_price * m.quantity);

        existingEntity.price = total;

        existingEntity = CommonDataHelper<PurchaseOrderHeader>.FillUpdateFields(existingEntity, calling_user_id);

        _Context.Update(existingEntity);
        await _Context.SaveChangesAsync();
    }
}

