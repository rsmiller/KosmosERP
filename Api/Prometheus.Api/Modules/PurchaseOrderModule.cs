using Prometheus.Api.Models.Module.PurchaseOrder.Command.Create;
using Prometheus.Api.Models.Module.PurchaseOrder.Command.Delete;
using Prometheus.Api.Models.Module.PurchaseOrder.Command.Edit;
using Prometheus.Api.Models.Module.PurchaseOrder.Command.Find;
using Prometheus.Api.Models.Module.PurchaseOrder.Dto;
using Prometheus.Database.Models;
using Prometheus.Database;
using Prometheus.Models.Helpers;
using Prometheus.Models.Interfaces;
using Prometheus.Models;
using Prometheus.Module;
using Microsoft.EntityFrameworkCore;

namespace Prometheus.Api.Modules;

public interface IPurchaseOrderModule : IERPModule<
    PurchaseOrderHeader,
    PurchaseOrderHeaderDto,
    PurchaseOrderHeaderListDto,
    PurchaseOrderHeaderCreateCommand,
    PurchaseOrderHeaderEditCommand,
    PurchaseOrderHeaderDeleteCommand,
    PurchaseOrderHeaderFindCommand>, IBaseERPModule
{
    // Add any custom methods for PO headers if needed
    PurchaseOrderLine? GetLine(int object_id);
    Task<PurchaseOrderLine?> GetLineAsync(int object_id);
    Task<Response<PurchaseOrderLineDto>> GetLineDto(int object_id);
    Task<Response<PurchaseOrderLineDto>> CreateLine(PurchaseOrderLineCreateCommand commandModel);
    Task<Response<PurchaseOrderLineDto>> EditLine(PurchaseOrderLineEditCommand commandModel);
    Task<Response<PurchaseOrderLineDto>> DeleteLine(PurchaseOrderLineDeleteCommand commandModel);
}

public class PurchaseOrderModule : BaseERPModule, IPurchaseOrderModule
{
    public override Guid ModuleIdentifier => Guid.Parse("78c4861d-1252-4cac-9461-0e1e0399cd83");
    public override string ModuleName => "Purchase Order";

    private readonly IBaseERPContext _Context;

    public PurchaseOrderModule(IBaseERPContext context) : base(context)
    {
        _Context = context;
    }

    public PurchaseOrderHeader? Get(int object_id)
    {
        return _Context.PurchaseOrderHeaders.SingleOrDefault(m => m.id == object_id);
    }

    public PurchaseOrderLine? GetLine(int object_id)
    {
        return _Context.PurchaseOrderLines.SingleOrDefault(m => m.id == object_id);
    }

    public async Task<PurchaseOrderHeader?> GetAsync(int object_id)
    {
        return await _Context.PurchaseOrderHeaders.SingleOrDefaultAsync(m => m.id == object_id);
    }

    public async Task<PurchaseOrderLine?> GetLineAsync(int object_id)
    {
        return await _Context.PurchaseOrderLines.SingleOrDefaultAsync(m => m.id == object_id);
    }

    public async Task<Response<PurchaseOrderHeaderDto>> GetDto(int object_id)
    {
        var entity = await GetAsync(object_id);
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

        var permission_result = await base.HasPermission(commandModel.calling_user_id, "create_purchase_order", write: true);
        if (!permission_result)
            return new Response<PurchaseOrderHeaderDto>("Invalid permission", ResultCode.InvalidPermission);


        var db_transaction = await _Context.Database.BeginTransactionAsync();

        var newPoHeader = this.MapForCreate(commandModel);

        _Context.PurchaseOrderHeaders.Add(newPoHeader);
        await _Context.SaveChangesAsync();

        foreach (var po_line in commandModel.purchase_order_lines)
        {
            var line = this.MapForCreateLine(po_line, newPoHeader.id);

            _Context.PurchaseOrderLines.Add(line);
            await _Context.SaveChangesAsync();
        }

        await db_transaction.CommitAsync();

        var dto = await MapToDto(newPoHeader);
        return new Response<PurchaseOrderHeaderDto>(dto);
    }

    public async Task<Response<PurchaseOrderLineDto>> CreateLine(PurchaseOrderLineCreateCommand commandModel)
    {
        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<PurchaseOrderLineDto>(validationResult.Exception, ResultCode.DataValidationError);

        var permission_result = await base.HasPermission(commandModel.calling_user_id, "create_purchase_order", write: true);
        if (!permission_result)
            return new Response<PurchaseOrderLineDto>("Invalid permission", ResultCode.InvalidPermission);

        var line = this.MapForCreateLine(commandModel, commandModel.purchase_order_header_id);

        _Context.PurchaseOrderLines.Add(line);
        await _Context.SaveChangesAsync();

        var dto = await MapToLineDto(line);
        return new Response<PurchaseOrderLineDto>(dto);
    }

    public async Task<Response<PurchaseOrderHeaderDto>> Edit(PurchaseOrderHeaderEditCommand commandModel)
    {
        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<PurchaseOrderHeaderDto>(validationResult.Exception, ResultCode.DataValidationError);

        var permission_result = await base.HasPermission(commandModel.calling_user_id, "edit_purchase_order", edit: true);
        if (!permission_result)
            return new Response<PurchaseOrderHeaderDto>("Invalid permission", ResultCode.InvalidPermission);


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

        existingEntity.updated_on = DateTime.Now;
        existingEntity.updated_by = commandModel.calling_user_id;

        existingEntity.revision_number = existingEntity.revision_number + 1;

        _Context.PurchaseOrderHeaders.Update(existingEntity);
        await _Context.SaveChangesAsync();

        var dto = await MapToDto(existingEntity);
        return new Response<PurchaseOrderHeaderDto>(dto);
    }

    public async Task<Response<PurchaseOrderLineDto>> EditLine(PurchaseOrderLineEditCommand commandModel)
    {
        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<PurchaseOrderLineDto>(validationResult.Exception, ResultCode.DataValidationError);

        var permission_result = await base.HasPermission(commandModel.calling_user_id, "edit_purchase_order", edit: true);
        if (!permission_result)
            return new Response<PurchaseOrderLineDto>("Invalid permission", ResultCode.InvalidPermission);


        var existingEntity = await GetLineAsync(commandModel.id);
        if (existingEntity == null)
            return new Response<PurchaseOrderLineDto>("PurchaseOrderLine not found", ResultCode.NotFound);

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

        existingEntity.updated_on = DateTime.Now;
        existingEntity.updated_by = commandModel.calling_user_id;

        existingEntity.revision_number = existingEntity.revision_number + 1;

        _Context.PurchaseOrderLines.Update(existingEntity);
        await _Context.SaveChangesAsync();

        var dto = await MapToLineDto(existingEntity);
        return new Response<PurchaseOrderLineDto>(dto);
    }

    public async Task<Response<PurchaseOrderHeaderDto>> Delete(PurchaseOrderHeaderDeleteCommand commandModel)
    {
        var existingEntity = await GetAsync(commandModel.id);
        if (existingEntity == null)
            return new Response<PurchaseOrderHeaderDto>("PurchaseOrderHeader not found", ResultCode.NotFound);

        existingEntity.is_deleted = true;
        existingEntity.deleted_on = DateTime.Now;
        existingEntity.deleted_by = commandModel.calling_user_id;

        _Context.PurchaseOrderHeaders.Update(existingEntity);
        await _Context.SaveChangesAsync();

        var dto = await MapToDto(existingEntity);
        return new Response<PurchaseOrderHeaderDto>(dto);
    }

    public async Task<Response<PurchaseOrderLineDto>> DeleteLine(PurchaseOrderLineDeleteCommand commandModel)
    {
        var existingEntity = await GetLineAsync(commandModel.id);
        if (existingEntity == null)
            return new Response<PurchaseOrderLineDto>("PurchaseOrderLine not found", ResultCode.NotFound);

        existingEntity.is_deleted = true;
        existingEntity.deleted_on = DateTime.Now;
        existingEntity.deleted_by = commandModel.calling_user_id;

        _Context.PurchaseOrderLines.Update(existingEntity);
        await _Context.SaveChangesAsync();

        var dto = await MapToLineDto(existingEntity);
        return new Response<PurchaseOrderLineDto>(dto);
    }

    public async Task<PagingResult<PurchaseOrderHeaderListDto>> Find(PagingSortingParameters parameters, PurchaseOrderHeaderFindCommand commandModel)
    {
        var response = new PagingResult<PurchaseOrderHeaderListDto>();

        try
        {
            var permission_result = await base.HasPermission(commandModel.calling_user_id, "read_purchaseorderheader", read: true);
            if (!permission_result)
            {
                response.SetException("Invalid permission", ResultCode.InvalidPermission);
                return response;
            }

            var query = _Context.PurchaseOrderHeaders
                .Where(m => !m.is_deleted);

            if (!string.IsNullOrEmpty(commandModel.wildcard))
            {
                var wild = commandModel.wildcard.ToLower();
                query = query.Where(m =>
                    m.po_type.ToLower().Contains(wild)
                    || (m.deleted_reason != null && m.deleted_reason.ToLower().Contains(wild))
                    || (m.canceled_reason != null && m.canceled_reason.ToLower().Contains(wild))
                    || m.guid.ToLower().Contains(wild)
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

    public async Task<Response<List<PurchaseOrderHeaderListDto>>> GlobalSearch(PagingSortingParameters parameters, string wildcard)
    {
        var response = new Response<List<PurchaseOrderHeaderListDto>>();
        try
        {
            var query = _Context.PurchaseOrderHeaders
                .Where(m => !m.is_deleted);

            if (!string.IsNullOrEmpty(wildcard))
            {
                var lower = wildcard.ToLower();
                query = query.Where(m =>
                    m.po_type.ToLower().Contains(lower)
                    || (m.deleted_reason != null && m.deleted_reason.ToLower().Contains(lower))
                    || (m.canceled_reason != null && m.canceled_reason.ToLower().Contains(lower))
                    || m.guid.ToLower().Contains(lower)
                );
            }

            var pagedItems = await query.SortAndPageBy(parameters).ToListAsync();

            var dtos = new List<PurchaseOrderHeaderListDto>();
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

    public async Task<PurchaseOrderHeaderListDto> MapToListDto(PurchaseOrderHeader databaseModel)
    {
        return new PurchaseOrderHeaderListDto
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
            deleted_reason = databaseModel.deleted_reason,
            canceled_reason = databaseModel.canceled_reason,
            is_complete = databaseModel.is_complete,
            is_canceled = databaseModel.is_canceled,
            completed_on = databaseModel.completed_on,
            completed_by = databaseModel.completed_by,
            canceled_on = databaseModel.canceled_on,
            canceled_by = databaseModel.canceled_by,
            guid = databaseModel.guid
        };
    }

    public async Task<PurchaseOrderHeaderDto> MapToDto(PurchaseOrderHeader databaseModel)
    {
        return new PurchaseOrderHeaderDto
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
            deleted_reason = databaseModel.deleted_reason,
            canceled_reason = databaseModel.canceled_reason,
            is_complete = databaseModel.is_complete,
            is_canceled = databaseModel.is_canceled,
            completed_on = databaseModel.completed_on,
            completed_by = databaseModel.completed_by,
            canceled_on = databaseModel.canceled_on,
            canceled_by = databaseModel.canceled_by,
            guid = databaseModel.guid
        };
    }

    public async Task<PurchaseOrderLineDto> MapToLineDto(PurchaseOrderLine databaseModel)
    {
        return new PurchaseOrderLineDto
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
            guid = databaseModel.guid
        };
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
            guid = dtoModel.guid
        };
    }

    private PurchaseOrderHeader MapForCreate(PurchaseOrderHeaderCreateCommand createCommandModel)
    {
        var now = DateTime.Now;

        var header = new PurchaseOrderHeader
        {
            vendor_id = createCommandModel.vendor_id,
            po_type = createCommandModel.po_type,
            revision_number = 1,
            is_deleted = false,
            created_on = now,
            created_by = createCommandModel.calling_user_id,
            updated_on = now,
            updated_by = createCommandModel.calling_user_id
        };

        return header;
    }

    private PurchaseOrderLine MapForCreateLine(PurchaseOrderLineCreateCommand createCommandModel, int purchase_order_header_id)
    {
        var now = DateTime.Now;

        var line = new PurchaseOrderLine()
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
            created_on = now,
            created_by = createCommandModel.calling_user_id,
            updated_on = now,
            updated_by = createCommandModel.calling_user_id
        };

        return line;
    }
}

