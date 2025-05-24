using KosmosERP.Database;
using KosmosERP.Models.Helpers;
using KosmosERP.Models.Interfaces;
using KosmosERP.Models;
using KosmosERP.Module;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using KosmosERP.Models.Permissions;
using KosmosERP.BusinessLayer.Models.Module.Transaction.Command.Create;
using KosmosERP.BusinessLayer.Interfaces;
using KosmosERP.BusinessLayer.Helpers;
using KosmosERP.BusinessLayer.Models.Module.Transaction.Command.Edit;
using KosmosERP.BusinessLayer.Models.Module.Transaction.Command.Delete;
using KosmosERP.BusinessLayer.Models.Module.PurchaseOrder.Command.Create;
using KosmosERP.BusinessLayer.Models.Module.PurchaseOrder.Command.Delete;
using KosmosERP.BusinessLayer.Models.Module.PurchaseOrder.Command.Edit;
using KosmosERP.BusinessLayer.Models.Module.PurchaseOrder.Command.Find;
using KosmosERP.BusinessLayer.Models.Module.PurchaseOrder.Dto;
using KosmosERP.Database.Models;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;
using Mysqlx.Crud;


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
    Task<Response<PurchaseOrderLineDto>> CreateLine(PurchaseOrderLineCreateCommand commandModel);
    Task<Response<PurchaseOrderLineDto>> EditLine(PurchaseOrderLineEditCommand commandModel);
    Task<Response<PurchaseOrderLineDto>> DeleteLine(PurchaseOrderLineDeleteCommand commandModel);
}

public class PurchaseOrderModule : BaseERPModule, IPurchaseOrderModule
{
    public override Guid ModuleIdentifier => Guid.Parse("78c4861d-1252-4cac-9461-0e1e0399cd83");
    public override string ModuleName => "Purchase Order";

    private readonly IBaseERPContext _Context;
    private IMessagePublisher _MessagePublisher;

    public PurchaseOrderModule(IBaseERPContext context, IMessagePublisher messagePublisher) : base(context)
    {
        _Context = context;
        _MessagePublisher = messagePublisher;
    }

    public override void SeedPermissions()
    {
        var role = _Context.Roles.Any(m => m.name == "PO Users");
        var read_permission = _Context.ModulePermissions.Any(m => m.module_id == this.ModuleIdentifier.ToString() && m.internal_permission_name == PurchaseOrderPermissions.Read);
        var create_permission = _Context.ModulePermissions.Any(m => m.module_id == this.ModuleIdentifier.ToString() && m.internal_permission_name == PurchaseOrderPermissions.Create);
        var edit_permission = _Context.ModulePermissions.Any(m => m.module_id == this.ModuleIdentifier.ToString() && m.internal_permission_name == PurchaseOrderPermissions.Edit);
        var delete_permission = _Context.ModulePermissions.Any(m => m.module_id == this.ModuleIdentifier.ToString() && m.internal_permission_name == PurchaseOrderPermissions.Delete);

        if (role == false)
        {
            _Context.Roles.Add(CommonDataHelper<Role>.FillCommonFields(new Role()
            {
                name = "PO Users",
            }, 1));

            _Context.SaveChanges();
        }

        var role_id = _Context.Roles.Where(m => m.name == "PO Users").Select(m => m.id).Single();

        if (read_permission == false)
        {
            _Context.ModulePermissions.Add(new ModulePermission()
            {
                permission_name = "Read Purchase Orders",
                internal_permission_name = PurchaseOrderPermissions.Read,
                module_id = this.ModuleIdentifier.ToString(),
                module_name = this.ModuleName,
                read = true,
            });

            _Context.SaveChanges();

            var read_perm_id = _Context.ModulePermissions.Where(m => m.internal_permission_name == PurchaseOrderPermissions.Read).Select(m => m.id).Single();

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
                permission_name = "Create Purchase Orders",
                internal_permission_name = PurchaseOrderPermissions.Create,
                module_id = this.ModuleIdentifier.ToString(),
                module_name = this.ModuleName,
                write = true
            });

            _Context.SaveChanges();

            var create_perm_id = _Context.ModulePermissions.Where(m => m.internal_permission_name == PurchaseOrderPermissions.Create).Select(m => m.id).Single();

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
                permission_name = "Edit Purchase Orders",
                internal_permission_name = PurchaseOrderPermissions.Edit,
                module_id = this.ModuleIdentifier.ToString(),
                module_name = this.ModuleName,
                edit = true
            });

            _Context.SaveChanges();

            var edit_perm_id = _Context.ModulePermissions.Where(m => m.internal_permission_name == PurchaseOrderPermissions.Edit).Select(m => m.id).Single();

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
                permission_name = "Delete Purchase Orders",
                internal_permission_name = PurchaseOrderPermissions.Delete,
                module_id = this.ModuleIdentifier.ToString(),
                module_name = this.ModuleName,
                delete = true
            });

            _Context.SaveChanges();

            var delete_perm_id = _Context.ModulePermissions.Where(m => m.internal_permission_name == PurchaseOrderPermissions.Delete).Select(m => m.id).Single();

            _Context.RolePermissions.Add(CommonDataHelper<RolePermission>.FillCommonFields(new RolePermission()
            {
                role_id = role_id,
                module_permission_id = delete_perm_id,
            }, 1));

            _Context.SaveChanges();
        }


        var material_po_type = _Context.KeyValueStores.Where(m => m.module_id == this.ModuleIdentifier.ToString() && m.key == "po_type_materials").SingleOrDefault();
        var office_po_type = _Context.KeyValueStores.Where(m => m.module_id == this.ModuleIdentifier.ToString() && m.key == "po_type_office_suplies").SingleOrDefault();

        if (material_po_type == null)
        {
            _Context.KeyValueStores.Add(CommonDataHelper<KeyValueStore>.FillCommonFields(new KeyValueStore()
            {
                key = "po_type_materials",
                value = "Build Material",
            }, 1));

            _Context.SaveChanges();
        }

        if (office_po_type == null)
        {
            _Context.KeyValueStores.Add(CommonDataHelper<KeyValueStore>.FillCommonFields(new KeyValueStore()
            {
                key = "po_type_office_suplies",
                value = "Office Use",
            }, 1));

            _Context.SaveChanges();
        }
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

        var permission_result = await base.HasPermission(commandModel.calling_user_id, commandModel.token,PurchaseOrderPermissions.Create, write: true);
        if (!permission_result)
            return new Response<PurchaseOrderHeaderDto>("Invalid permission", ResultCode.InvalidPermission);

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
                await _MessagePublisher.PublishAsync(new Models.MessageObject()
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
                }, RequiredMessageTopics.TransactionMovementTopic);
            }


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

        var permission_result = await base.HasPermission(commandModel.calling_user_id, commandModel.token,PurchaseOrderPermissions.Create, write: true);
        if (!permission_result)
            return new Response<PurchaseOrderLineDto>("Invalid permission", ResultCode.InvalidPermission);

        try
        {
            var line = this.MapForCreateLine(commandModel, commandModel.purchase_order_header_id);

            _Context.PurchaseOrderLines.Add(line);
            await _Context.SaveChangesAsync();

            // Publish this data to a message queue to be processed for transactions
            await _MessagePublisher.PublishAsync(new Models.MessageObject()
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
            }, RequiredMessageTopics.TransactionMovementTopic);


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

        var permission_result = await base.HasPermission(commandModel.calling_user_id, commandModel.token,PurchaseOrderPermissions.Edit, edit: true);
        if (!permission_result)
            return new Response<PurchaseOrderHeaderDto>("Invalid permission", ResultCode.InvalidPermission);

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
                    await _MessagePublisher.PublishAsync(new Models.MessageObject()
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
                    }, RequiredMessageTopics.TransactionMovementTopic);
                }
                else
                {
                    var edit_response = await this.EditLine(line);

                    if (!edit_response.Success)
                        return new Response<PurchaseOrderHeaderDto>(edit_response.Exception, ResultCode.Error);
                }
            }

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

        var permission_result = await base.HasPermission(commandModel.calling_user_id, commandModel.token,PurchaseOrderPermissions.Edit, edit: true);
        if (!permission_result)
            return new Response<PurchaseOrderLineDto>("Invalid permission", ResultCode.InvalidPermission);

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

            // Publish this data to a message queue to be processed for transactions
            await _MessagePublisher.PublishAsync(new Models.MessageObject()
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
            }, RequiredMessageTopics.TransactionMovementTopic);

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

            await _MessagePublisher.PublishAsync(new Models.MessageObject()
            {
                created_on = DateTime.UtcNow,
                object_type = "TransactionDeleteCommand",
                body = JsonSerializer.Serialize(new TransactionDeleteCommand()
                {
                    object_reference_id = existingEntity.purchase_order_header_id,
                    object_sub_reference_id = existingEntity.id,
                    calling_user_id = commandModel.calling_user_id,
                })
            }, RequiredMessageTopics.TransactionMovementTopic);

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
            var permission_result = await base.HasPermission(commandModel.calling_user_id, commandModel.token,PurchaseOrderPermissions.Read, read: true);
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

        foreach(var line in lines)
            dto.purchase_order_lines.Add(await this.MapToLineDto(line));


        dto.vendor_name = databaseModel.vendor.vendor_name;

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

        dto.product_name = databaseModel.product.product_name;

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

    public PurchaseOrderLine MapForEditLine(PurchaseOrderLineEditCommand commandModel, int purchase_order_header_id, int calling_user_id)
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
}

