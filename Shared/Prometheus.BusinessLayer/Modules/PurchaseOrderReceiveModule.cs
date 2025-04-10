using Microsoft.EntityFrameworkCore;
using Prometheus.BusinessLayer.Interfaces;
using Prometheus.BusinessLayer.Models.Module.PurchaseOrder.Dto;
using Prometheus.BusinessLayer.Models.Module.PurchaseOrderReceive.Command.Create;
using Prometheus.BusinessLayer.Models.Module.PurchaseOrderReceive.Command.Delete;
using Prometheus.BusinessLayer.Models.Module.PurchaseOrderReceive.Command.Edit;
using Prometheus.BusinessLayer.Models.Module.PurchaseOrderReceive.Command.Find;
using Prometheus.BusinessLayer.Models.Module.PurchaseOrderReceive.Dto;
using Prometheus.BusinessLayer.Models.Module.Transaction.Command.Create;
using Prometheus.BusinessLayer.Models.Module.Transaction.Command.Delete;
using Prometheus.BusinessLayer.Models.Module.Transaction.Command.Edit;
using Prometheus.Database;
using Prometheus.Database.Models;
using Prometheus.Models;
using Prometheus.Models.Helpers;
using Prometheus.Models.Interfaces;
using Prometheus.Models.Permissions;
using Prometheus.Module;
using System.Text.Json;

namespace Prometheus.BusinessLayer.Modules;


public interface IPurchaseOrderReceiveModule : IERPModule<PurchaseOrderReceiveHeader, PurchaseOrderReceiveHeaderDto, PurchaseOrderReceiveHeaderListDto, PurchaseOrderReceiveHeaderCreateCommand, PurchaseOrderReceiveHeaderEditCommand, PurchaseOrderReceiveHeaderDeleteCommand, PurchaseOrderReceiveHeaderFindCommand>, IBaseERPModule
{
    Task<Response<PurchaseOrderReceiveLineDto>> GetLineDto(int object_id);
    Task<Response<PurchaseOrderReceiveLineDto>> CreateLine(PurchaseOrderReceiveLineCreateCommand commandModel);
    Task<Response<PurchaseOrderReceiveLineDto>> EditLine(PurchaseOrderReceiveLineEditCommand commandModel);
    Task<Response<PurchaseOrderReceiveLineDto>> DeleteLine(PurchaseOrderReceiveLineDeleteCommand commandModel);
    Task<Response<PurchaseOrderReceiveUploadDto>> CreateUpload(PurchaseOrderReceiveUploadCreateCommand commandModel);
    Task<Response<PurchaseOrderReceiveUploadDto>> DeleteUpload(PurchaseOrderReceiveUploadDeleteCommand commandModel);
}

public class PurchaseOrderReceiveModule : BaseERPModule, IPurchaseOrderReceiveModule
{
    public override Guid ModuleIdentifier => Guid.Parse("bdaa14c4-64d8-44f3-b1ad-d272c408832f");
    public override string ModuleName => "PurchaseOrderReceive";

    private IBaseERPContext _Context;
    private IMessagePublisher _MessagePublisher;

    public PurchaseOrderReceiveModule(IBaseERPContext context, IMessagePublisher messagePublisher) : base(context)
    {
        _Context = context;
        _MessagePublisher = messagePublisher;
    }

    public override void SeedPermissions()
    {
        var role = _Context.Roles.Any(m => m.name == "Purchase Order Receive Users");
        var read_permission = _Context.ModulePermissions.Any(m => m.module_id == this.ModuleIdentifier.ToString() && m.internal_permission_name == PurchaseOrderReceivePermissions.Read);
        var create_permission = _Context.ModulePermissions.Any(m => m.module_id == this.ModuleIdentifier.ToString() && m.internal_permission_name == PurchaseOrderReceivePermissions.Create);
        var edit_permission = _Context.ModulePermissions.Any(m => m.module_id == this.ModuleIdentifier.ToString() && m.internal_permission_name == PurchaseOrderReceivePermissions.Edit);
        var delete_permission = _Context.ModulePermissions.Any(m => m.module_id == this.ModuleIdentifier.ToString() && m.internal_permission_name == PurchaseOrderReceivePermissions.Delete);

        if (role == false)
        {
            _Context.Roles.Add(new Role()
            {
                name = "Purchase Order Receive Users",
                created_by = 1,
                created_on = DateTime.Now,
                updated_by = 1,
                updated_on = DateTime.Now,
            });

            _Context.SaveChanges();
        }

        var role_id = _Context.Roles.Where(m => m.name == "Purchase Order Receive Users").Select(m => m.id).Single();

        if (read_permission == false)
        {
            _Context.ModulePermissions.Add(new ModulePermission()
            {
                permission_name = "Read Purchase Order Receive",
                internal_permission_name = PurchaseOrderReceivePermissions.Read,
                module_id = this.ModuleIdentifier.ToString(),
                module_name = this.ModuleName,
                read = true,
            });

            _Context.SaveChanges();

            var read_perm_id = _Context.ModulePermissions.Where(m => m.internal_permission_name == PurchaseOrderReceivePermissions.Read).Select(m => m.id).Single();

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
                permission_name = "Create Purchase Order Receive",
                internal_permission_name = PurchaseOrderReceivePermissions.Create,
                module_id = this.ModuleIdentifier.ToString(),
                module_name = this.ModuleName,
                write = true
            });

            _Context.SaveChanges();

            var create_perm_id = _Context.ModulePermissions.Where(m => m.internal_permission_name == PurchaseOrderReceivePermissions.Create).Select(m => m.id).Single();

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
                permission_name = "Edit Purchase Order Receive",
                internal_permission_name = PurchaseOrderReceivePermissions.Edit,
                module_id = this.ModuleIdentifier.ToString(),
                module_name = this.ModuleName,
                edit = true
            });

            _Context.SaveChanges();

            var edit_perm_id = _Context.ModulePermissions.Where(m => m.internal_permission_name == PurchaseOrderReceivePermissions.Edit).Select(m => m.id).Single();

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
                permission_name = "Delete Purchase Order Receive",
                internal_permission_name = PurchaseOrderReceivePermissions.Delete,
                module_id = this.ModuleIdentifier.ToString(),
                module_name = this.ModuleName,
                delete = true
            });

            _Context.SaveChanges();

            var delete_perm_id = _Context.ModulePermissions.Where(m => m.internal_permission_name == PurchaseOrderReceivePermissions.Delete).Select(m => m.id).Single();

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

    public PurchaseOrderReceiveHeader? Get(int object_id)
    {
        return _Context.PurchaseOrderReceiveHeaders.SingleOrDefault(m => m.id == object_id);
    }

    public async Task<PurchaseOrderReceiveHeader?> GetAsync(int object_id)
    {
        return await _Context.PurchaseOrderReceiveHeaders.SingleOrDefaultAsync(m => m.id == object_id);
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

        var result = await _Context.PurchaseOrderReceiveHeaders.SingleOrDefaultAsync(m => m.id == object_id);
        if (result == null)
        {
            response.SetException("Purchase Order Receive Header not found", ResultCode.NotFound);
            return response;
        }

        response.Data = await this.MapToDto(result);

        return response;
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

        var permission_result = await base.HasPermission(commandModel.calling_user_id, commandModel.token,PurchaseOrderReceivePermissions.Create, write: true);
        if (!permission_result)
            return new Response<PurchaseOrderReceiveHeaderDto>("Invalid permission", ResultCode.InvalidPermission);

        var existingUploadEntity = await _Context.DocumentUploads.Where(m => m.id == commandModel.document_upload_id).AsNoTracking().FirstOrDefaultAsync();
        if (existingUploadEntity == null)
            return new Response<PurchaseOrderReceiveHeaderDto>("Document Upload not found", ResultCode.NotFound);


        try
        {
            var item = this.MapToDatabaseModel(commandModel, commandModel.calling_user_id);

            await _Context.PurchaseOrderReceiveHeaders.AddAsync(item);
            await _Context.SaveChangesAsync();

            // Now do lines
            foreach (var ap_line in commandModel.received_lines)
            {
                var db_line = MapToLineDatabaseModel(ap_line, item.id, commandModel.calling_user_id);

                await _Context.PurchaseOrderReceiveLines.AddAsync(db_line);
                await _Context.SaveChangesAsync();


                var purchase_line_product = await _Context.PurchaseOrderLines.Where(m => m.id == db_line.purchase_order_line_id).Select(m => m.product_id).SingleOrDefaultAsync();
                await _MessagePublisher.PublishAsync(new Models.MessageObject()
                {
                    created_on = DateTime.Now,
                    object_type = "TransactionCreateCommand",
                    body = JsonSerializer.Serialize(new TransactionCreateCommand()
                    {
                        transaction_type = TransactionType.Inbound,
                        transaction_date = DateTime.Now,
                        object_reference_id = db_line.purchase_order_receive_header_id,
                        object_sub_reference_id = db_line.id,
                        units_received = db_line.units_received,
                        product_id = purchase_line_product,
                        calling_user_id = commandModel.calling_user_id,
                    })
                }, RequiredMessageTopics.TransactionMovementTopic);
            }


            var document_upload_model = this.MapToUploadDatabaseModel(new PurchaseOrderReceiveUploadCreateCommand()
            {
                purchase_order_receive_header_id = item.id,
                document_upload_id = commandModel.document_upload_id,
                calling_user_id = commandModel.calling_user_id
            }, item.id, commandModel.calling_user_id);

            await _Context.PurchaseOrderReceiveUploads.AddAsync(document_upload_model);
            await _Context.SaveChangesAsync();


            var dto = await GetDto(item.id);

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

        var permission_result = await base.HasPermission(commandModel.calling_user_id, commandModel.token,PurchaseOrderReceivePermissions.Delete, delete: true);
        if (!permission_result)
            return new Response<PurchaseOrderReceiveHeaderDto>("Invalid permission", ResultCode.InvalidPermission);

        var existingEntity = await GetAsync(commandModel.id);
        if (existingEntity == null)
            return new Response<PurchaseOrderReceiveHeaderDto>("Purchase Order Receive Header not found", ResultCode.NotFound);

        try
        {
            existingEntity.is_deleted = true;
            existingEntity.deleted_on = DateTime.Now;
            existingEntity.deleted_by = commandModel.calling_user_id;

            _Context.PurchaseOrderReceiveHeaders.Update(existingEntity);
            await _Context.SaveChangesAsync();

            await _MessagePublisher.PublishAsync(new Models.MessageObject()
            {
                created_on = DateTime.Now,
                object_type = "TransactionDeleteCommand",
                body = JsonSerializer.Serialize(new TransactionDeleteCommand()
                {
                    object_reference_id = existingEntity.id,
                    calling_user_id = commandModel.calling_user_id,
                })
            }, RequiredMessageTopics.TransactionMovementTopic);

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

        var permission_result = await base.HasPermission(commandModel.calling_user_id, commandModel.token,PurchaseOrderReceivePermissions.Edit, edit: true);
        if (!permission_result)
            return new Response<PurchaseOrderReceiveHeaderDto>("Invalid permission", ResultCode.InvalidPermission);

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


            existingEntity.updated_on = DateTime.Now;
            existingEntity.updated_by = commandModel.calling_user_id;

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
                    await _MessagePublisher.PublishAsync(new Models.MessageObject()
                    {
                        created_on = DateTime.Now,
                        object_type = "TransactionCreateCommand",
                        body = JsonSerializer.Serialize(new TransactionCreateCommand()
                        {
                            transaction_type = TransactionType.Inbound,
                            transaction_date = DateTime.Now,
                            object_reference_id = add_line.purchase_order_receive_header_id,
                            object_sub_reference_id = add_line.id,
                            units_received = add_line.units_received,
                            product_id = purchase_line_product,
                            calling_user_id = commandModel.calling_user_id,
                        })
                    }, RequiredMessageTopics.TransactionMovementTopic);
                }
                else
                {
                    var edit_response = await this.EditLine(line);

                    if (!edit_response.Success)
                        return new Response<PurchaseOrderReceiveHeaderDto>(edit_response.Exception, ResultCode.Error);
                }
            }

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

            var totalCount = await query.CountAsync();
            var pagedItems = await query.SortAndPageBy(parameters).ToListAsync();

            var dtos = new List<PurchaseOrderReceiveHeaderListDto>();
            foreach (var item in pagedItems)
            {
                dtos.Add(await MapToListDto(item.porh));
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

        var permission_result = await base.HasPermission(commandModel.calling_user_id, commandModel.token,PurchaseOrderReceivePermissions.Create, write: true);
        if (!permission_result)
            return new Response<PurchaseOrderReceiveLineDto>("Invalid permission", ResultCode.InvalidPermission);

        if (!commandModel.purchase_order_receive_header_id.HasValue)
            return new Response<PurchaseOrderReceiveLineDto>("Purchase Order Receive Header id is a required field", ResultCode.DataValidationError);

        try
        {
            var item = this.MapToLineDatabaseModel(commandModel, commandModel.purchase_order_receive_header_id.Value, commandModel.calling_user_id);

            await _Context.PurchaseOrderReceiveLines.AddAsync(item);
            await _Context.SaveChangesAsync();

            var purchase_line_product = await _Context.PurchaseOrderLines.Where(m => m.id == item.purchase_order_line_id).Select(m => m.product_id).SingleOrDefaultAsync();
            await _MessagePublisher.PublishAsync(new Models.MessageObject()
            {
                created_on = DateTime.Now,
                object_type = "TransactionCreateCommand",
                body = JsonSerializer.Serialize(new TransactionCreateCommand()
                {
                    transaction_type = TransactionType.Inbound,
                    transaction_date = DateTime.Now,
                    object_reference_id = item.purchase_order_receive_header_id,
                    object_sub_reference_id = item.id,
                    units_received = item.units_received,
                    product_id = purchase_line_product,
                    calling_user_id = commandModel.calling_user_id,
                })
            }, RequiredMessageTopics.TransactionMovementTopic);


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

        var permission_result = await base.HasPermission(commandModel.calling_user_id, commandModel.token,PurchaseOrderReceivePermissions.Edit, edit: true);
        if (!permission_result)
            return new Response<PurchaseOrderReceiveLineDto>("Invalid permission", ResultCode.InvalidPermission);

        if (!commandModel.id.HasValue)
            return new Response<PurchaseOrderReceiveLineDto>("Order Line must have an id", ResultCode.DataValidationError);

        var existingEntity = await GetLineAsync(commandModel.id.Value);
        if (existingEntity == null)
            return new Response<PurchaseOrderReceiveLineDto>("Purchase Order Receive Line not found", ResultCode.NotFound);

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


            existingEntity.updated_on = DateTime.Now;
            existingEntity.updated_by = commandModel.calling_user_id;

            _Context.PurchaseOrderReceiveLines.Update(existingEntity);
            await _Context.SaveChangesAsync();


            // Publish this data to a message queue to be processed for transactions
            var purchase_line_product = await _Context.PurchaseOrderLines.Where(m => m.id == existingEntity.purchase_order_line_id).Select(m => m.product_id).SingleOrDefaultAsync();
            await _MessagePublisher.PublishAsync(new Models.MessageObject()
            {
                created_on = DateTime.Now,
                object_type = "TransactionEditCommand",
                body = JsonSerializer.Serialize(new TransactionEditCommand()
                {
                    object_reference_id = existingEntity.purchase_order_receive_header_id,
                    object_sub_reference_id = existingEntity.id,
                    units_received = existingEntity.units_received,
                    product_id = purchase_line_product,
                    calling_user_id = commandModel.calling_user_id,
                })
            }, RequiredMessageTopics.TransactionMovementTopic);

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

        var permission_result = await base.HasPermission(commandModel.calling_user_id, commandModel.token,PurchaseOrderReceivePermissions.Delete, delete: true);
        if (!permission_result)
            return new Response<PurchaseOrderReceiveLineDto>("Invalid permission", ResultCode.InvalidPermission);

        var existingEntity = await GetLineAsync(commandModel.id);
        if (existingEntity == null)
            return new Response<PurchaseOrderReceiveLineDto>("Purchase Order Receive Line not found", ResultCode.NotFound);
        try
        {
            existingEntity.is_deleted = true;
            existingEntity.deleted_on = DateTime.Now;
            existingEntity.deleted_by = commandModel.calling_user_id;

            _Context.PurchaseOrderReceiveLines.Update(existingEntity);
            await _Context.SaveChangesAsync();

            await _MessagePublisher.PublishAsync(new Models.MessageObject()
            {
                created_on = DateTime.Now,
                object_type = "TransactionDeleteCommand",
                body = JsonSerializer.Serialize(new TransactionDeleteCommand()
                {
                    object_reference_id = existingEntity.purchase_order_receive_header_id,
                    object_sub_reference_id = existingEntity.id,
                    calling_user_id = commandModel.calling_user_id,
                })
            }, RequiredMessageTopics.TransactionMovementTopic);

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

        var permission_result = await base.HasPermission(commandModel.calling_user_id, commandModel.token,PurchaseOrderReceivePermissions.Create, write: true);
        if (!permission_result)
            return new Response<PurchaseOrderReceiveUploadDto>("Invalid permission", ResultCode.InvalidPermission);

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

        var permission_result = await base.HasPermission(commandModel.calling_user_id, commandModel.token,PurchaseOrderReceivePermissions.Delete, delete: true);
        if (!permission_result)
            return new Response<PurchaseOrderReceiveUploadDto>("Invalid permission", ResultCode.InvalidPermission);

        var existingEntity = await GetUploadAsync(commandModel.id);
        if (existingEntity == null)
            return new Response<PurchaseOrderReceiveUploadDto>("Purchase Order Receive Upload not found", ResultCode.NotFound);

        existingEntity.is_deleted = true;
        existingEntity.deleted_on = DateTime.Now;
        existingEntity.deleted_by = commandModel.calling_user_id;

        _Context.PurchaseOrderReceiveUploads.Update(existingEntity);
        await _Context.SaveChangesAsync();

        var dto = await MapToUploadDto(existingEntity);
        return new Response<PurchaseOrderReceiveUploadDto>(dto);
    }

    public async Task<Response<List<PurchaseOrderReceiveHeaderListDto>>> GlobalSearch(PagingSortingParameters parameters, string wildcard)
    {
        throw new NotImplementedException();
    }

    public PurchaseOrderReceiveHeader MapToDatabaseModel(PurchaseOrderReceiveHeaderDto dtoModel)
    {
        throw new NotImplementedException();
    }

    public PurchaseOrderReceiveHeader MapToDatabaseModel(PurchaseOrderReceiveHeaderCreateCommand commandModel, int calling_user_id)
    {
        var now = DateTime.Now;

        return new PurchaseOrderReceiveHeader()
        {
            purchase_order_id = commandModel.purchase_order_id,
            guid = Guid.NewGuid().ToString(),
            created_by = calling_user_id,
            created_on = now,
            updated_by = calling_user_id,
            updated_on = now,
        };
    }

    public PurchaseOrderReceiveLine MapToLineDatabaseModel(PurchaseOrderReceiveLineCreateCommand commandModel, int purchase_order_receive_header_id, int calling_user_id)
    {
        var now = DateTime.Now;

        return new PurchaseOrderReceiveLine()
        {
            purchase_order_receive_header_id = purchase_order_receive_header_id,
            purchase_order_line_id = commandModel.purchase_order_line_id,
            units_received = commandModel.units_received,
            guid = Guid.NewGuid().ToString(),
            created_by = calling_user_id,
            created_on = now,
            updated_by = calling_user_id,
            updated_on = now,
        };
    }

    public PurchaseOrderReceiveLine MapToLineDatabaseModel(PurchaseOrderReceiveLineEditCommand commandModel, int purchase_order_receive_header_id, int calling_user_id)
    {
        var now = DateTime.Now;

        return new PurchaseOrderReceiveLine()
        {
            purchase_order_receive_header_id = purchase_order_receive_header_id,
            purchase_order_line_id = commandModel.purchase_order_line_id.Value,
            units_received = commandModel.units_received.Value,
            guid = Guid.NewGuid().ToString(),
            created_by = calling_user_id,
            created_on = now,
            updated_by = calling_user_id,
            updated_on = now,
        };
    }

    public PurchaseOrderReceiveUpload MapToUploadDatabaseModel(PurchaseOrderReceiveUploadCreateCommand commandModel, int purchase_order_receive_header_id, int calling_user_id)
    {
        var now = DateTime.Now;

        return new PurchaseOrderReceiveUpload()
        {
            purchase_order_receive_header_id = purchase_order_receive_header_id,
            document_upload_id = commandModel.document_upload_id,
            guid = Guid.NewGuid().ToString(),
            created_by = calling_user_id,
            created_on = now,
            updated_by = calling_user_id,
            updated_on = now,
        };
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
            is_deleted = databaseModel.is_deleted
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
            is_deleted = databaseModel.is_deleted
        };

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
            is_deleted = databaseModel.is_deleted
        };

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
            is_deleted = databaseModel.is_deleted
        };

        return dto;
    }
}
