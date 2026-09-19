using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using KosmosERP.Database;
using KosmosERP.Database.Models;
using KosmosERP.Models;
using KosmosERP.Models.Helpers;
using KosmosERP.Module;
using KosmosERP.Models.Interfaces;
using KosmosERP.BusinessLayer.Helpers;
using KosmosERP.BusinessLayer.Models.Module.PurchaseOrderReceive.Command.Create;
using KosmosERP.BusinessLayer.Models.Module.PurchaseOrderReceive.Command.Delete;
using KosmosERP.BusinessLayer.Models.Module.PurchaseOrderReceive.Command.Edit;
using KosmosERP.BusinessLayer.Models.Module.PurchaseOrderReceive.Command.Find;
using KosmosERP.BusinessLayer.Models.Module.PurchaseOrderReceive.Dto;
using KosmosERP.BusinessLayer.Models.Module.Transaction.Command.Create;
using KosmosERP.BusinessLayer.Models.Module.Transaction.Command.Delete;
using KosmosERP.BusinessLayer.Models.Module.Transaction.Command.Edit;

namespace KosmosERP.BusinessLayer.Modules;


public interface IPurchaseOrderReceiveModule : IERPModule<PurchaseOrderReceiveHeader, PurchaseOrderReceiveHeaderDto, PurchaseOrderReceiveHeaderListDto, PurchaseOrderReceiveHeaderCreateCommand, PurchaseOrderReceiveHeaderEditCommand, PurchaseOrderReceiveHeaderDeleteCommand, PurchaseOrderReceiveHeaderFindCommand>, IBaseERPModule
{
    Task<Response<PurchaseOrderReceiveLineDto>> GetLineDto(int object_id);
    Task<Response<PurchaseOrderReceiveLineDto>> CreateLine(PurchaseOrderReceiveLineCreateCommand commandModel);
    Task<Response<PurchaseOrderReceiveLineDto>> EditLine(PurchaseOrderReceiveLineEditCommand commandModel);
    Task<Response<PurchaseOrderReceiveLineDto>> DeleteLine(PurchaseOrderReceiveLineDeleteCommand commandModel);
    Task<Response<PurchaseOrderReceiveUploadDto>> CreateUpload(PurchaseOrderReceiveUploadCreateCommand commandModel);
    Task<Response<PurchaseOrderReceiveUploadDto>> DeleteUpload(PurchaseOrderReceiveUploadDeleteCommand commandModel);
    Task<Response<PurchaseOrderReceiveHeaderDto>> GetDtoByPOId(int purchase_order_id);

    Task<PurchaseOrderReceiveLineDto> MapToLineDto(PurchaseOrderReceiveLine databaseModel);
}

public class PurchaseOrderReceiveModule : BaseERPModule, IPurchaseOrderReceiveModule
{
    public override Guid ModuleIdentifier => Guid.Parse("bdaa14c4-64d8-44f3-b1ad-d272c408832f");
    public override string ModuleName => "Purchase Order Receive";

    private IBaseERPContext _Context;
    private IMessageFactory? _MessageFactory;
    private IDocumentUploadModule? _DocumentUploadModule;
    private IMessagePublisherSettings? _MessagePublisherSettings;

    public PurchaseOrderReceiveModule(IBaseERPContext context, ILogProviderFactory logProviderFactory) : base(context, logProviderFactory)
    {
        _Context = context;
    }

    public PurchaseOrderReceiveModule(IBaseERPContext context, 
                                        IMessageFactory messageFactory, 
                                        IDocumentUploadModule documentUploadModule, 
                                        IMessagePublisherSettings messageSettings, 
                                        ILogProviderFactory logProviderFactory) : base(context, logProviderFactory)
    {
        _Context = context;
        _MessageFactory = messageFactory;
        _DocumentUploadModule = documentUploadModule;
        _MessagePublisherSettings = messageSettings;
    }

    public override void SeedPermissions()
    {
        var role = _Context.Roles.Any(m => m.name == "Purchase Order Receive Administrators");

        if (role == false)
        {
            _Context.Roles.Add(CommonDataHelper<Role>.FillCommonFields(new Role()
            {
                name = "Purchase Order Receive Administrators",
            }, 1));

            _Context.SaveChanges();

            base.CreateFirstRunRolePermissions();
        }
    }

    public PurchaseOrderReceiveHeader? Get(int object_id)
    {
        return _Context.PurchaseOrderReceiveHeaders.Include("purchase_order").SingleOrDefault(m => m.id == object_id);
    }

    public async Task<PurchaseOrderReceiveHeader?> GetAsync(int object_id)
    {
        return await _Context.PurchaseOrderReceiveHeaders.Include("purchase_order").SingleOrDefaultAsync(m => m.id == object_id);
    }

    public async Task<PurchaseOrderReceiveLine?> GetLineAsync(int object_id)
    {
        return await _Context.PurchaseOrderReceiveLines.SingleOrDefaultAsync(m => m.id == object_id);
    }

    public async Task<PurchaseOrderReceiveUpload?> GetUploadAsync(int object_id)
    {
        return await _Context.PurchaseOrderReceiveUploads.SingleOrDefaultAsync(m => m.id == object_id);
    }

    
    public async Task<Response<PurchaseOrderReceiveHeaderDto>> GetDto(int object_id)
    {
        Response<PurchaseOrderReceiveHeaderDto> response = new Response<PurchaseOrderReceiveHeaderDto>();

        var result = await _Context.PurchaseOrderReceiveHeaders.Include("purchase_order").SingleOrDefaultAsync(m => m.id == object_id);
        if (result == null)
        {
            response.SetException("Purchase Order Receive Header not found", ResultCode.NotFound);
            return response;
        }

        response.Data = await this.MapToDto(result);

        return response;
    }

    public async Task<Response<PurchaseOrderReceiveHeaderDto>> GetDtoByGuid(string guid)
    {
        var entity = await _Context.PurchaseOrderReceiveHeaders.Include("purchase_order").FirstOrDefaultAsync(c => c.guid == guid && !c.is_deleted);
        if (entity == null)
            return new Response<PurchaseOrderReceiveHeaderDto>("PurchaseOrderReceiveHeader not found", ResultCode.NotFound);

        var dto = await MapToDto(entity);
        return new Response<PurchaseOrderReceiveHeaderDto>(dto);
    }

    public async Task<Response<PurchaseOrderReceiveHeaderDto>> GetDtoByPOId(int purchase_order_id)
    {
        var entity = await _Context.PurchaseOrderReceiveHeaders.Include("purchase_order").FirstOrDefaultAsync(c => c.purchase_order_id == purchase_order_id && !c.is_deleted);
        if (entity == null)
            return new Response<PurchaseOrderReceiveHeaderDto>("PurchaseOrderReceiveHeader not found", ResultCode.NotFound);

        var dto = await MapToDto(entity);
        return new Response<PurchaseOrderReceiveHeaderDto>(dto);
    }
    
    public async Task<Response<PurchaseOrderReceiveLineDto>> GetLineDto(int object_id)
    {
        Response<PurchaseOrderReceiveLineDto> response = new Response<PurchaseOrderReceiveLineDto>();

        var result = await _Context.PurchaseOrderReceiveLines.SingleOrDefaultAsync(m => m.id == object_id);
        if (result == null)
        {
            response.SetException("Purchase Order Receive Line not found", ResultCode.NotFound);
            return response;
        }

        response.Data = await this.MapToLineDto(result);

        return response;
    }

    public async Task<Response<PurchaseOrderReceiveUploadDto>> GetUploadDto(int object_id)
    {
        Response<PurchaseOrderReceiveUploadDto> response = new Response<PurchaseOrderReceiveUploadDto>();

        var result = await _Context.PurchaseOrderReceiveUploads.SingleOrDefaultAsync(m => m.id == object_id);
        if (result == null)
        {
            response.SetException("Purchase Order Receive Upload not found", ResultCode.NotFound);
            return response;
        }

        response.Data = await this.MapToUploadDto(result);

        return response;
    }
    

    public async Task<Response<PurchaseOrderReceiveHeaderDto>> Create(PurchaseOrderReceiveHeaderCreateCommand commandModel)
    {
        if (commandModel == null)
            return new Response<PurchaseOrderReceiveHeaderDto>(ResultCode.NullItemInput);

        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<PurchaseOrderReceiveHeaderDto>(validationResult.Exception, ResultCode.DataValidationError);

        var existingUploadEntity = await _Context.DocumentUploads.Where(m => m.id == commandModel.document_upload_id).AsNoTracking().FirstOrDefaultAsync();
        if (existingUploadEntity == null)
            return new Response<PurchaseOrderReceiveHeaderDto>("Document Upload not found", ResultCode.NotFound);

        var existingPurchaseOrderEntity = await _Context.PurchaseOrderHeaders.Where(m => m.id == commandModel.purchase_order_id).AsNoTracking().FirstOrDefaultAsync();
        if (existingPurchaseOrderEntity == null)
            return new Response<PurchaseOrderReceiveHeaderDto>("Purchase Order not found", ResultCode.NotFound);

        try
        {
            PurchaseOrderReceiveHeader header;

            var exsitingEntity = await _Context.PurchaseOrderReceiveHeaders.FirstOrDefaultAsync(m => m.purchase_order_id == commandModel.purchase_order_id);

            if (exsitingEntity == null)
            {
                var purchase_order_line_sum = await _Context.PurchaseOrderLines
                                                .Where(m => m.purchase_order_header_id == existingPurchaseOrderEntity.id)
                                                .SumAsync(m => m.quantity);
                var total_receieved = commandModel.received_lines.Sum(m => m.units_received);


                var item = this.MapToDatabaseModel(commandModel, commandModel.calling_user_id);

                item.units_ordered = purchase_order_line_sum;
                item.units_received = total_receieved;

                if(total_receieved >= purchase_order_line_sum)
                {
                    item.is_complete = true;
                }

                await _Context.PurchaseOrderReceiveHeaders.AddAsync(item);
                await _Context.SaveChangesAsync();

                header = item;
            }
            else
            {
                header = exsitingEntity;

                var total_receieved = commandModel.received_lines.Sum(m => m.units_received);
                header.units_received += total_receieved;
                
                if (header.units_received >= header.units_ordered)
                {
                    header.is_complete = true;
                }

                header = CommonDataHelper<PurchaseOrderReceiveHeader>.FillUpdateFields(header, commandModel.calling_user_id);

                _Context.PurchaseOrderReceiveHeaders.Update(header);
                await _Context.SaveChangesAsync();
            }



            // Now do lines
            foreach (var ap_line in commandModel.received_lines)
            {
                var db_line = MapToLineDatabaseModel(ap_line, header.id, commandModel.calling_user_id);

                await _Context.PurchaseOrderReceiveLines.AddAsync(db_line);
                await _Context.SaveChangesAsync();


                var purchase_line_product = await _Context.PurchaseOrderLines.Where(m => m.id == db_line.purchase_order_line_id).Select(m => m.product_id).SingleOrDefaultAsync();
                await _MessageFactory!.GetPublisher().PublishAsync(new Models.MessageObject()
                {
                    created_on = DateTime.UtcNow,
                    object_type = "TransactionCreateCommand",
                    body = JsonSerializer.Serialize(new TransactionCreateCommand()
                    {
                        transaction_type = TransactionType.Inbound,
                        transaction_date = DateTime.UtcNow,
                        object_reference_id = db_line.purchase_order_receive_header_id,
                        object_sub_reference_id = db_line.id,
                        units_received = db_line.units_received,
                        product_id = purchase_line_product,
                        calling_user_id = commandModel.calling_user_id,
                    })
                }, _MessagePublisherSettings!.transaction_movement_topic!);
            }


            var document_upload_model = this.MapToUploadDatabaseModel(new PurchaseOrderReceiveUploadCreateCommand()
            {
                purchase_order_receive_header_id = header.id,
                document_upload_id = commandModel.document_upload_id,
                calling_user_id = commandModel.calling_user_id
            }, header.id, commandModel.calling_user_id);

            await _Context.PurchaseOrderReceiveUploads.AddAsync(document_upload_model);
            await _Context.SaveChangesAsync();


            var dto = await GetDto(header.id);

            return new Response<PurchaseOrderReceiveHeaderDto>(dto.Data);
        }
        catch (Exception ex)
        {
            return new Response<PurchaseOrderReceiveHeaderDto>(ex.Message, ResultCode.Error);
        }
    }

    public async Task<Response<PurchaseOrderReceiveHeaderDto>> Delete(PurchaseOrderReceiveHeaderDeleteCommand commandModel)
    {
        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<PurchaseOrderReceiveHeaderDto>(validationResult.Exception, ResultCode.DataValidationError);

        var existingEntity = await GetAsync(commandModel.id);
        if (existingEntity == null)
            return new Response<PurchaseOrderReceiveHeaderDto>("Purchase Order Receive Header not found", ResultCode.NotFound);

        try
        {
            // Delete
            existingEntity = CommonDataHelper<PurchaseOrderReceiveHeader>.FillDeleteFields(existingEntity, commandModel.calling_user_id);


            _Context.PurchaseOrderReceiveHeaders.Update(existingEntity);
            await _Context.SaveChangesAsync();

            var lines = await _Context.PurchaseOrderReceiveLines.Where(m => m.purchase_order_line_id == existingEntity.id).ToListAsync();
            foreach(var line in lines)
            {
                await this.DeleteLine(new PurchaseOrderReceiveLineDeleteCommand()
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
            return new Response<PurchaseOrderReceiveHeaderDto>(dto);
        }
        catch (Exception ex)
        {
            await LogError(80, this.GetType().Name, nameof(Delete), ex);
            return new Response<PurchaseOrderReceiveHeaderDto>(ex.Message, ResultCode.Error);
        }
    }

    public async Task<Response<PurchaseOrderReceiveHeaderDto>> Edit(PurchaseOrderReceiveHeaderEditCommand commandModel)
    {
        if (commandModel == null)
            return new Response<PurchaseOrderReceiveHeaderDto>(ResultCode.NullItemInput);

        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<PurchaseOrderReceiveHeaderDto>(validationResult.Exception, ResultCode.DataValidationError);

        var existingEntity = await GetAsync(commandModel.id);
        if (existingEntity == null)
            return new Response<PurchaseOrderReceiveHeaderDto>("Purchase Order Receive Header not found", ResultCode.NotFound);


        // Check for validation issues with the lines
        foreach (var line in commandModel.received_lines)
        {
            var line_validation = ModelValidationHelper.ValidateModel(line);
            if (!line_validation.Success)
                return new Response<PurchaseOrderReceiveHeaderDto>(line_validation.Exception, ResultCode.DataValidationError);

            var new_edited_lines = commandModel.received_lines.Where(m => !m.id.HasValue).ToList();
            foreach (var new_edit in new_edited_lines)
            {
                // These fields must be set to be considered a new line
                if (!new_edit.purchase_order_line_id.HasValue
                    && !new_edit.units_received.HasValue)
                {
                    return new Response<PurchaseOrderReceiveHeaderDto>("Required field not set on new line", ResultCode.DataValidationError);
                }
            }

        }

        try
        {

            if (commandModel.purchase_order_id.HasValue && existingEntity.purchase_order_id != commandModel.purchase_order_id)
                existingEntity.purchase_order_id = commandModel.purchase_order_id.Value;
            if (commandModel.is_complete.HasValue && existingEntity.is_complete != commandModel.is_complete)
                existingEntity.is_complete = commandModel.is_complete.Value;
            if (commandModel.is_canceled.HasValue && existingEntity.is_canceled != commandModel.is_canceled)
                existingEntity.is_canceled = commandModel.is_canceled.Value;
            if (existingEntity.canceled_reason != commandModel.canceled_reason)
                existingEntity.canceled_reason = commandModel.canceled_reason;


            existingEntity = CommonDataHelper<PurchaseOrderReceiveHeader>.FillUpdateFields(existingEntity, commandModel.calling_user_id);


            _Context.PurchaseOrderReceiveHeaders.Update(existingEntity);
            await _Context.SaveChangesAsync();

            // Create or update lines
            foreach (var line in commandModel.received_lines)
            {
                // Edit lines
                if (!line.id.HasValue)
                {
                    var add_line = this.MapToLineDatabaseModel(line, existingEntity.id, commandModel.calling_user_id);
                    await _Context.PurchaseOrderReceiveLines.AddAsync(add_line);
                    await _Context.SaveChangesAsync();

                    // Publish this data to a message queue to be processed for transactions
                    var purchase_line_product = await _Context.PurchaseOrderLines.Where(m => m.id == add_line.purchase_order_line_id).Select(m => m.product_id).SingleOrDefaultAsync();

                    await _MessageFactory!.GetPublisher().PublishAsync(new Models.MessageObject()
                    {
                        created_on = DateTime.UtcNow,
                        object_type = "TransactionCreateCommand",
                        body = JsonSerializer.Serialize(new TransactionCreateCommand()
                        {
                            transaction_type = TransactionType.Inbound,
                            transaction_date = DateTime.UtcNow,
                            object_reference_id = add_line.purchase_order_receive_header_id,
                            object_sub_reference_id = add_line.id,
                            units_received = add_line.units_received,
                            product_id = purchase_line_product,
                            calling_user_id = commandModel.calling_user_id,
                        })
                    }, _MessagePublisherSettings!.transaction_movement_topic!);
                }
                else
                {
                    var edit_response = await this.EditLine(line);

                    if (!edit_response.Success)
                        return new Response<PurchaseOrderReceiveHeaderDto>(edit_response.Exception, ResultCode.Error);
                }
            }


            // After everything lets update totals
            var purchase_order_line_sum = await _Context.PurchaseOrderLines
                        .Where(m => m.purchase_order_header_id == existingEntity.purchase_order_id)
                        .SumAsync(m => m.quantity);
            var total_receieved = await _Context.PurchaseOrderReceiveLines
                                    .Where(m => m.purchase_order_receive_header_id == existingEntity.id && m.is_deleted == false)
                                    .SumAsync(m => m.units_received);

            existingEntity.units_ordered = purchase_order_line_sum;
            existingEntity.units_received = total_receieved;

            _Context.PurchaseOrderReceiveHeaders.Update(existingEntity);
            await _Context.SaveChangesAsync();

            var dto = await MapToDto(existingEntity);

            return new Response<PurchaseOrderReceiveHeaderDto>(dto);
        }
        catch (Exception ex)
        {
            await LogError(80, this.GetType().Name, nameof(Edit), ex);
            return new Response<PurchaseOrderReceiveHeaderDto>(ex.Message, ResultCode.Error);
        }
    }

    public async Task<PagingResult<PurchaseOrderReceiveHeaderListDto>> Find(PagingSortingParameters parameters, PurchaseOrderReceiveHeaderFindCommand commandModel)
    {
        var response = new PagingResult<PurchaseOrderReceiveHeaderListDto>();

        try
        {
            var query = _Context.PurchaseOrderReceiveHeaders.Join(_Context.PurchaseOrderHeaders, porh => porh.purchase_order_id, po => po.id,
                                                            (porh, po) => new { po, porh })
                                                            .Where(m => !m.porh.is_deleted && !m.po.is_deleted);

            decimal parsed_num = 0;

            if (decimal.TryParse(commandModel.wildcard, out parsed_num))
            {
                var wild = commandModel.wildcard.ToLower();
                query = query.Where(m => m.po.po_number == parsed_num);
            }

            var totalCount = await query.Select(m => m.porh).CountAsync();
            var pagedItems = await query.Select(m => m.porh).Include("purchase_order").SortAndPageBy(parameters).ToListAsync();

            var dtos = new List<PurchaseOrderReceiveHeaderListDto>();
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

    public async Task<Response<PurchaseOrderReceiveLineDto>> CreateLine(PurchaseOrderReceiveLineCreateCommand commandModel)
    {
        if (commandModel == null)
            return new Response<PurchaseOrderReceiveLineDto>(ResultCode.NullItemInput);

        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<PurchaseOrderReceiveLineDto>(validationResult.Exception, ResultCode.DataValidationError);

        if (!commandModel.purchase_order_receive_header_id.HasValue)
            return new Response<PurchaseOrderReceiveLineDto>("Purchase Order Receive Header id is a required field", ResultCode.DataValidationError);

        var existingHeaderEntity = await GetAsync(commandModel.purchase_order_receive_header_id.Value);
        if (existingHeaderEntity == null)
            return new Response<PurchaseOrderReceiveLineDto>("Purchase Order Receive Header not found", ResultCode.NotFound);

        var existingLineEntity = await _Context.PurchaseOrderLines.Where(m => m.id == commandModel.purchase_order_line_id).SingleOrDefaultAsync();
        if (existingLineEntity == null)
            return new Response<PurchaseOrderReceiveLineDto>("Purchase Order Line not found", ResultCode.NotFound);


        try
        {
            var item = this.MapToLineDatabaseModel(commandModel, commandModel.purchase_order_receive_header_id.Value, commandModel.calling_user_id);

            await _Context.PurchaseOrderReceiveLines.AddAsync(item);
            await _Context.SaveChangesAsync();

            // After everything lets update totals
            var purchase_order_line_sum = await _Context.PurchaseOrderLines
                        .Where(m => m.purchase_order_header_id == existingLineEntity.purchase_order_header_id)
                        .SumAsync(m => m.quantity);
            var total_receieved = await _Context.PurchaseOrderReceiveLines
                                    .Where(m => m.purchase_order_receive_header_id == existingHeaderEntity.id && m.is_deleted == false)
                                    .SumAsync(m => m.units_received);

            existingHeaderEntity = CommonDataHelper<PurchaseOrderReceiveHeader>.FillUpdateFields(existingHeaderEntity, commandModel.calling_user_id);

            existingHeaderEntity.units_ordered = purchase_order_line_sum;
            existingHeaderEntity.units_received = total_receieved;

            _Context.PurchaseOrderReceiveHeaders.Update(existingHeaderEntity);
            await _Context.SaveChangesAsync();

            var purchase_line_product = await _Context.PurchaseOrderLines.Where(m => m.id == item.purchase_order_line_id).Select(m => m.product_id).SingleOrDefaultAsync();

            await _MessageFactory!.GetPublisher().PublishAsync(new Models.MessageObject()
            {
                created_on = DateTime.UtcNow,
                object_type = "TransactionCreateCommand",
                body = JsonSerializer.Serialize(new TransactionCreateCommand()
                {
                    transaction_type = TransactionType.Inbound,
                    transaction_date = DateTime.UtcNow,
                    object_reference_id = item.purchase_order_receive_header_id,
                    object_sub_reference_id = item.id,
                    units_received = item.units_received,
                    product_id = purchase_line_product,
                    calling_user_id = commandModel.calling_user_id,
                })
            }, _MessagePublisherSettings!.transaction_movement_topic!);


            var dto = await GetLineDto(item.id);

            return new Response<PurchaseOrderReceiveLineDto>(dto.Data);
        }
        catch (Exception ex)
        {
            await LogError(80, this.GetType().Name, nameof(CreateLine), ex);
            return new Response<PurchaseOrderReceiveLineDto>(ex.Message, ResultCode.Error);
        }
    }

    public async Task<Response<PurchaseOrderReceiveLineDto>> EditLine(PurchaseOrderReceiveLineEditCommand commandModel)
    {
        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<PurchaseOrderReceiveLineDto>(validationResult.Exception, ResultCode.DataValidationError);

        if (!commandModel.id.HasValue)
            return new Response<PurchaseOrderReceiveLineDto>("Order Line must have an id", ResultCode.DataValidationError);

        var existingEntity = await GetLineAsync(commandModel.id.Value);
        if (existingEntity == null)
            return new Response<PurchaseOrderReceiveLineDto>("Purchase Order Receive Line not found", ResultCode.NotFound);

        var existingHeaderEntity = await GetAsync(existingEntity.purchase_order_receive_header_id);
        if (existingHeaderEntity == null)
            return new Response<PurchaseOrderReceiveLineDto>("Purchase Order Receive Header not found", ResultCode.NotFound);

        try
        {

            if (commandModel.purchase_order_line_id.HasValue && existingEntity.purchase_order_line_id != commandModel.purchase_order_line_id)
                existingEntity.purchase_order_line_id = commandModel.purchase_order_line_id.Value;
            if (commandModel.units_received.HasValue && existingEntity.units_received != commandModel.units_received)
                existingEntity.units_received = commandModel.units_received.Value;
            if (commandModel.is_complete.HasValue && existingEntity.is_complete != commandModel.is_complete)
                existingEntity.is_complete = commandModel.is_complete.Value;
            if (commandModel.is_canceled.HasValue && existingEntity.is_canceled != commandModel.is_canceled)
                existingEntity.is_canceled = commandModel.is_canceled.Value;
            if (existingEntity.canceled_reason != commandModel.canceled_reason)
                existingEntity.canceled_reason = commandModel.canceled_reason;


            existingEntity = CommonDataHelper<PurchaseOrderReceiveLine>.FillUpdateFields(existingEntity, commandModel.calling_user_id);


            _Context.PurchaseOrderReceiveLines.Update(existingEntity);
            await _Context.SaveChangesAsync();

            // After everything lets update totals
            var purchase_order_line_sum = await _Context.PurchaseOrderLines
                        .Where(m => m.purchase_order_header_id == existingHeaderEntity.purchase_order_id)
                        .SumAsync(m => m.quantity);
            var total_receieved = await _Context.PurchaseOrderReceiveLines
                                    .Where(m => m.purchase_order_receive_header_id == existingHeaderEntity.id && m.is_deleted == false)
                                    .SumAsync(m => m.units_received);

            existingHeaderEntity = CommonDataHelper<PurchaseOrderReceiveHeader>.FillUpdateFields(existingHeaderEntity, commandModel.calling_user_id);

            existingHeaderEntity.units_ordered = purchase_order_line_sum;
            existingHeaderEntity.units_received = total_receieved;

            _Context.PurchaseOrderReceiveHeaders.Update(existingHeaderEntity);
            await _Context.SaveChangesAsync();

            // Publish this data to a message queue to be processed for transactions
            var purchase_line_product = await _Context.PurchaseOrderLines.Where(m => m.id == existingEntity.purchase_order_line_id).Select(m => m.product_id).SingleOrDefaultAsync();

            await _MessageFactory!.GetPublisher().PublishAsync(new Models.MessageObject()
            {
                created_on = DateTime.UtcNow,
                object_type = "TransactionEditCommand",
                body = JsonSerializer.Serialize(new TransactionEditCommand()
                {
                    object_reference_id = existingEntity.purchase_order_receive_header_id,
                    object_sub_reference_id = existingEntity.id,
                    units_received = existingEntity.units_received,
                    product_id = purchase_line_product,
                    calling_user_id = commandModel.calling_user_id,
                })
            }, _MessagePublisherSettings!.transaction_movement_topic!);

            var dto = await MapToLineDto(existingEntity);

            return new Response<PurchaseOrderReceiveLineDto>(dto);
        }
        catch (Exception ex)
        {
            await LogError(80, this.GetType().Name, nameof(EditLine), ex);
            return new Response<PurchaseOrderReceiveLineDto>(ex.Message, ResultCode.Error);
        }
    }

    public async Task<Response<PurchaseOrderReceiveLineDto>> DeleteLine(PurchaseOrderReceiveLineDeleteCommand commandModel)
    {
        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<PurchaseOrderReceiveLineDto>(validationResult.Exception, ResultCode.DataValidationError);

        var existingEntity = await GetLineAsync(commandModel.id);
        if (existingEntity == null)
            return new Response<PurchaseOrderReceiveLineDto>("Purchase Order Receive Line not found", ResultCode.NotFound);

        var existingHeaderEntity = await GetAsync(existingEntity.purchase_order_receive_header_id);
        if (existingHeaderEntity == null)
            return new Response<PurchaseOrderReceiveLineDto>("Purchase Order Receive Header not found", ResultCode.NotFound);


        try
        {
            existingEntity = CommonDataHelper<PurchaseOrderReceiveLine>.FillDeleteFields(existingEntity, commandModel.calling_user_id);

            _Context.PurchaseOrderReceiveLines.Update(existingEntity);
            await _Context.SaveChangesAsync();

            // After everything lets update totals
            var purchase_order_line_sum = await _Context.PurchaseOrderLines
                        .Where(m => m.purchase_order_header_id == existingHeaderEntity.purchase_order_id)
                        .SumAsync(m => m.quantity);
            var total_receieved = await _Context.PurchaseOrderReceiveLines
                                    .Where(m => m.purchase_order_receive_header_id == existingHeaderEntity.id && m.is_deleted == false)
                                    .SumAsync(m => m.units_received);

            existingHeaderEntity = CommonDataHelper<PurchaseOrderReceiveHeader>.FillUpdateFields(existingHeaderEntity, commandModel.calling_user_id);

            existingHeaderEntity.units_ordered = purchase_order_line_sum;
            existingHeaderEntity.units_received = total_receieved;

            _Context.PurchaseOrderReceiveHeaders.Update(existingHeaderEntity);
            await _Context.SaveChangesAsync();

            await _MessageFactory!.GetPublisher().PublishAsync(new Models.MessageObject()
            {
                created_on = DateTime.UtcNow,
                object_type = "TransactionDeleteCommand",
                body = JsonSerializer.Serialize(new TransactionDeleteCommand()
                {
                    object_reference_id = existingEntity.purchase_order_receive_header_id,
                    object_sub_reference_id = existingEntity.id,
                    calling_user_id = commandModel.calling_user_id,
                })
            }, _MessagePublisherSettings!.transaction_movement_topic!);

            var dto = await MapToLineDto(existingEntity);
            return new Response<PurchaseOrderReceiveLineDto>(dto);
        }
        catch (Exception ex)
        {
            await LogError(80, this.GetType().Name, nameof(DeleteLine), ex);
            return new Response<PurchaseOrderReceiveLineDto>(ex.Message, ResultCode.Error);
        }
    }

    public async Task<Response<PurchaseOrderReceiveUploadDto>> CreateUpload(PurchaseOrderReceiveUploadCreateCommand commandModel)
    {
        if (commandModel == null)
            return new Response<PurchaseOrderReceiveUploadDto>(ResultCode.NullItemInput);

        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<PurchaseOrderReceiveUploadDto>(validationResult.Exception, ResultCode.DataValidationError);

        var existingHeaderEntity = await GetAsync(commandModel.purchase_order_receive_header_id);
        if (existingHeaderEntity == null)
            return new Response<PurchaseOrderReceiveUploadDto>("Purchase Order Receive Header not found", ResultCode.NotFound);

        var existingUploadEntity = await _Context.DocumentUploads.Where(m => m.id == commandModel.document_upload_id).AsNoTracking().FirstOrDefaultAsync();
        if (existingUploadEntity == null)
            return new Response<PurchaseOrderReceiveUploadDto>("Document Upload not found", ResultCode.NotFound);


        try
        {
            var item = this.MapToUploadDatabaseModel(commandModel, commandModel.purchase_order_receive_header_id, commandModel.calling_user_id);

            await _Context.PurchaseOrderReceiveUploads.AddAsync(item);
            await _Context.SaveChangesAsync();

            var dto = await GetUploadDto(item.id);

            return new Response<PurchaseOrderReceiveUploadDto>(dto.Data);
        }
        catch (Exception ex)
        {
            return new Response<PurchaseOrderReceiveUploadDto>(ex.Message, ResultCode.Error);
        }
    }

    public async Task<Response<PurchaseOrderReceiveUploadDto>> DeleteUpload(PurchaseOrderReceiveUploadDeleteCommand commandModel)
    {
        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<PurchaseOrderReceiveUploadDto>(validationResult.Exception, ResultCode.DataValidationError);

        var existingEntity = await GetUploadAsync(commandModel.id);
        if (existingEntity == null)
            return new Response<PurchaseOrderReceiveUploadDto>("Purchase Order Receive Upload not found", ResultCode.NotFound);


        // Soft delete
        existingEntity = CommonDataHelper<PurchaseOrderReceiveUpload>.FillDeleteFields(existingEntity, commandModel.calling_user_id);


        _Context.PurchaseOrderReceiveUploads.Update(existingEntity);
        await _Context.SaveChangesAsync();

        var dto = await MapToUploadDto(existingEntity);
        return new Response<PurchaseOrderReceiveUploadDto>(dto);
    }

    public async Task<Response<List<PurchaseOrderReceiveHeaderListDto>>> GlobalSearch(GlobalSearchFindCommand commandModel)
    {
        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<List<PurchaseOrderReceiveHeaderListDto>>(validationResult.Exception, ResultCode.DataValidationError);


        throw new NotImplementedException();
    }

    public PurchaseOrderReceiveHeader MapToDatabaseModel(PurchaseOrderReceiveHeaderDto dtoModel)
    {
        throw new NotImplementedException();
    }

    public PurchaseOrderReceiveHeader MapToDatabaseModel(PurchaseOrderReceiveHeaderCreateCommand commandModel, string calling_user_id)
    {
        var now = DateTime.UtcNow;

        return CommonDataHelper<PurchaseOrderReceiveHeader>.FillCommonFields(new PurchaseOrderReceiveHeader()
        {
            purchase_order_id = commandModel.purchase_order_id,
            guid = Guid.NewGuid().ToString(),
        }, calling_user_id);
    }

    public PurchaseOrderReceiveLine MapToLineDatabaseModel(PurchaseOrderReceiveLineCreateCommand commandModel, int purchase_order_receive_header_id, string calling_user_id)
    {
        return CommonDataHelper<PurchaseOrderReceiveLine>.FillCommonFields(new PurchaseOrderReceiveLine()
        {
            purchase_order_receive_header_id = purchase_order_receive_header_id,
            purchase_order_line_id = commandModel.purchase_order_line_id,
            units_received = commandModel.units_received,
            guid = Guid.NewGuid().ToString(),
        }, calling_user_id);
    }

    public PurchaseOrderReceiveLine MapToLineDatabaseModel(PurchaseOrderReceiveLineEditCommand commandModel, int purchase_order_receive_header_id, string calling_user_id)
    {
        return CommonDataHelper<PurchaseOrderReceiveLine>.FillCommonFields(new PurchaseOrderReceiveLine()
        {
            purchase_order_receive_header_id = purchase_order_receive_header_id,
            purchase_order_line_id = commandModel.purchase_order_line_id.Value,
            units_received = commandModel.units_received.Value,
            guid = Guid.NewGuid().ToString(),
        }, calling_user_id);
    }

    public PurchaseOrderReceiveUpload MapToUploadDatabaseModel(PurchaseOrderReceiveUploadCreateCommand commandModel, int purchase_order_receive_header_id, string calling_user_id)
    {
        return CommonDataHelper<PurchaseOrderReceiveUpload>.FillCommonFields(new PurchaseOrderReceiveUpload()
        {
            purchase_order_receive_header_id = purchase_order_receive_header_id,
            document_upload_id = commandModel.document_upload_id,
            guid = Guid.NewGuid().ToString(),
        }, calling_user_id);
    }

    public async Task<PurchaseOrderReceiveHeaderDto> MapToDto(PurchaseOrderReceiveHeader databaseModel)
    {
        var dto = new PurchaseOrderReceiveHeaderDto()
        {
            id = databaseModel.id,
            purchase_order_id = databaseModel.purchase_order_id,
            units_ordered = databaseModel.units_ordered,
            units_received = databaseModel.units_received,
            is_complete = databaseModel.is_complete,
            is_canceled = databaseModel.is_canceled,
            canceled_reason = databaseModel.canceled_reason,
            completed_on = databaseModel.completed_on,
            canceled_on = databaseModel.canceled_on,
            canceled_by = databaseModel.canceled_by,
            guid = databaseModel.guid,
            created_by = databaseModel.created_by,
            created_on = databaseModel.created_on,
            updated_by = databaseModel.updated_by,
            updated_on = databaseModel.updated_on,
            deleted_by = databaseModel.deleted_by,
            deleted_on = databaseModel.deleted_on,
            is_deleted = databaseModel.is_deleted,
            created_on_string = databaseModel.created_on_string,
            created_on_timezone = databaseModel.created_on_timezone,
            updated_on_string = databaseModel.updated_on_string,
            updated_on_timezone = databaseModel.updated_on_timezone,
            deleted_on_string = databaseModel.deleted_on_string,
            deleted_on_timezone = databaseModel.deleted_on_timezone,
        };

        var uploads = await _Context.PurchaseOrderReceiveUploads
                                .Where(m => m.purchase_order_receive_header_id == databaseModel.id 
                                            && m.is_deleted == false).ToListAsync();

        var lines = await _Context.PurchaseOrderReceiveLines
                                .Where(m => m.purchase_order_receive_header_id == databaseModel.id
                                            && m.is_deleted == false).ToListAsync();

        foreach(var line in lines)
            dto.received_lines.Add(await MapToLineDto(line));

        foreach (var upload in uploads)
            dto.received_uploads.Add(await MapToUploadDto(upload));


        dto.po_number = databaseModel.purchase_order.po_number;
        dto.po_by = await _Context.Users.Where(m => m.external_id == databaseModel.purchase_order.created_by).Select(m => m.first_name + " " + m.last_name).SingleOrDefaultAsync();


        return dto;
    }

    public async Task<PurchaseOrderReceiveHeaderListDto> MapToListDto(PurchaseOrderReceiveHeader databaseModel)
    {
        var dto = new PurchaseOrderReceiveHeaderListDto()
        {
            id = databaseModel.id,
            purchase_order_id = databaseModel.purchase_order_id,
            units_ordered = databaseModel.units_ordered,
            units_received = databaseModel.units_received,
            is_complete = databaseModel.is_complete,
            is_canceled = databaseModel.is_canceled,
            canceled_reason = databaseModel.canceled_reason,
            completed_on = databaseModel.completed_on,
            canceled_on = databaseModel.canceled_on,
            canceled_by = databaseModel.canceled_by,
            guid = databaseModel.guid,
            created_by = databaseModel.created_by,
            created_on = databaseModel.created_on,
            updated_by = databaseModel.updated_by,
            updated_on = databaseModel.updated_on,
            deleted_by = databaseModel.deleted_by,
            deleted_on = databaseModel.deleted_on,
            is_deleted = databaseModel.is_deleted,
            created_on_string = databaseModel.created_on_string,
            created_on_timezone = databaseModel.created_on_timezone,
            updated_on_string = databaseModel.updated_on_string,
            updated_on_timezone = databaseModel.updated_on_timezone,
            deleted_on_string = databaseModel.deleted_on_string,
            deleted_on_timezone = databaseModel.deleted_on_timezone,
        };

        dto.po_number = databaseModel.purchase_order.po_number;
        dto.po_by = await _Context.Users.Where(m => m.external_id == databaseModel.purchase_order.created_by).Select(m => m.first_name + " " + m.last_name).SingleOrDefaultAsync();

        return dto;
    }

    public async Task<PurchaseOrderReceiveLineDto> MapToLineDto(PurchaseOrderReceiveLine databaseModel)
    {
        var dto = new PurchaseOrderReceiveLineDto()
        {
            id = databaseModel.id,
            purchase_order_receive_header_id = databaseModel.purchase_order_receive_header_id,
            purchase_order_line_id = databaseModel.purchase_order_line_id,
            units_ordered = databaseModel.units_ordered,
            units_received = databaseModel.units_received,
            is_complete = databaseModel.is_complete,
            is_canceled = databaseModel.is_canceled,
            canceled_reason = databaseModel.canceled_reason,
            completed_on = databaseModel.completed_on,
            canceled_on = databaseModel.canceled_on,
            canceled_by = databaseModel.canceled_by,
            guid = databaseModel.guid,
            deleted_by = databaseModel.deleted_by,
            deleted_on = databaseModel.deleted_on,
            is_deleted = databaseModel.is_deleted,
            created_by = databaseModel.created_by,
            created_on = databaseModel.created_on,
            updated_by = databaseModel.updated_by,
            updated_on = databaseModel.updated_on,
            created_on_string = databaseModel.created_on_string,
            created_on_timezone = databaseModel.created_on_timezone,
            updated_on_string = databaseModel.updated_on_string,
            updated_on_timezone = databaseModel.updated_on_timezone,
            deleted_on_string = databaseModel.deleted_on_string,
            deleted_on_timezone = databaseModel.deleted_on_timezone,
        };

        var product_and_po_info = await (from po in _Context.PurchaseOrderLines
                            join p in _Context.Products on po.product_id equals p.id
                            where po.id == databaseModel.purchase_order_line_id
                            select new {p, po}).FirstOrDefaultAsync();

        if (product_and_po_info != null)
        {
            dto.product_name = product_and_po_info.p.product_name;
            dto.line_number = product_and_po_info.po.line_number;
        }
        
        return dto;
    }

    public async Task<PurchaseOrderReceiveUploadDto> MapToUploadDto(PurchaseOrderReceiveUpload databaseModel)
    {
        var dto = new PurchaseOrderReceiveUploadDto()
        {
            created_by = databaseModel.created_by,
            created_on = databaseModel.created_on,
            guid = databaseModel.guid,
            id = databaseModel.id,
            purchase_order_receive_header_id = databaseModel.purchase_order_receive_header_id,
            updated_by = databaseModel.updated_by,
            updated_on = databaseModel.updated_on,
            deleted_by = databaseModel.deleted_by,
            deleted_on = databaseModel.deleted_on,
            is_deleted = databaseModel.is_deleted,
            created_on_string = databaseModel.created_on_string,
            created_on_timezone = databaseModel.created_on_timezone,
            updated_on_string = databaseModel.updated_on_string,
            updated_on_timezone = databaseModel.updated_on_timezone,
            deleted_on_string = databaseModel.deleted_on_string,
            deleted_on_timezone = databaseModel.deleted_on_timezone,
            document_upload_id = databaseModel.document_upload_id,
        };

        var associated_document_upload = await _DocumentUploadModule!.GetAsync(databaseModel.document_upload_id);

        if (associated_document_upload != null)
        {
            dto.document_upload = await _DocumentUploadModule!.MapToDto(associated_document_upload);
        }

        return dto;
    }
}
