using Microsoft.EntityFrameworkCore;
using Prometheus.Database;
using Prometheus.Database.Models;
using Prometheus.Module;
using Prometheus.Models;
using Prometheus.Models.Helpers;
using Prometheus.Models.Interfaces;
using Prometheus.BusinessLayer.Models.Module.APInvoiceHeader.Dto;
using Prometheus.BusinessLayer.Models.Module.APInvoiceHeader.Command.Create;
using Prometheus.BusinessLayer.Models.Module.APInvoiceHeader.Command.Delete;
using Prometheus.BusinessLayer.Models.Module.APInvoiceHeader.Command.Edit;
using Prometheus.BusinessLayer.Models.Module.APInvoiceHeader.Command.Find;
using Prometheus.BusinessLayer.Models.Module.APInvoiceLine.Command.Create;
using Prometheus.BusinessLayer.Models.Module.APInvoiceLine.Dto;
using Prometheus.BusinessLayer.Models.Module.APInvoiceLine.Command.Delete;
using Prometheus.BusinessLayer.Models.Module.APInvoice.Command;
using Prometheus.BusinessLayer.Models.Module.APInvoiceLine.Command.Edit;
using Prometheus.BusinessLayer.Models.Module.APInvoice.Command.Find;
using Prometheus.BusinessLayer.Models.Module.APInvoice.Dto;
using Prometheus.Models.Permissions;
using Prometheus.BusinessLayer.Helpers;

namespace Prometheus.Api.Modules;

public interface IAPInvoiceModule : IERPModule<APInvoiceHeader, APInvoiceHeaderDto, APInvoiceHeaderListDto, APInvoiceHeaderCreateCommand, APInvoiceHeaderEditCommand, APInvoiceHeaderDeleteCommand, APInvoiceHeaderFindCommand>, IBaseERPModule
{
    Task<Response<APInvoiceLineDto>> GetLineDto(int object_id);
    Task<Response<APInvoiceLineDto>> CreateLine(APInvoiceLineCreateCommand commandModel);
    Task<Response<APInvoiceLineDto>> EditLine(APInvoiceLineEditCommand commandModel);
    Task<Response<APInvoiceLineDto>> DeleteLine(APInvoiceLineDeleteCommand commandModel);
    Task<Response<APInvoiceHeaderDto>> AssociateHeaderObject(APInvoiceAssoicationCommand commandModel);
    Task<Response<APInvoiceLineDto>> AssociateLineObject(APInvoiceAssoicationCommand commandModel);
    Task<Response<APInvoiceHeaderDto>> AssociateReceivedPO (APInvoiceAssociatePOCommand commandModel);
    Task<Response<List<APInvoiceAssociationDto>>> GetAssociations(APInvoiceAssociationsFindCommand commandModel);
}

public class APInvoiceModule : BaseERPModule, IAPInvoiceModule
{
    public override Guid ModuleIdentifier => Guid.Parse("ecf469ca-0a18-459b-954d-47de0bf23cf6");
    public override string ModuleName => "APInvoiceHeaders";

    private IBaseERPContext _Context;

    public APInvoiceModule(IBaseERPContext context) : base(context)
    {
        _Context = context;
    }

    public override void SeedPermissions()
    {
        var role = _Context.Roles.Any(m => m.name == "AP Invoice Users");
        var read_permission = _Context.ModulePermissions.Any(m => m.module_id == this.ModuleIdentifier.ToString() && m.internal_permission_name == APInvoicePermissions.Read);
        var create_permission = _Context.ModulePermissions.Any(m => m.module_id == this.ModuleIdentifier.ToString() && m.internal_permission_name == APInvoicePermissions.Create);
        var edit_permission = _Context.ModulePermissions.Any(m => m.module_id == this.ModuleIdentifier.ToString() && m.internal_permission_name == APInvoicePermissions.Edit);
        var delete_permission = _Context.ModulePermissions.Any(m => m.module_id == this.ModuleIdentifier.ToString() && m.internal_permission_name == APInvoicePermissions.Delete);

        if (role == false)
        {
            _Context.Roles.Add(new Role()
            {
                name = "AP Invoice Users",
                created_by = 1,
                created_on = DateTime.UtcNow,
                updated_by = 1,
                updated_on = DateTime.UtcNow,
            });

            _Context.SaveChanges();
        }

        var role_id = _Context.Roles.Where(m => m.name == "AP Invoice Users").Select(m => m.id).Single();

        if (read_permission == false)
        {
            _Context.ModulePermissions.Add(new ModulePermission()
            {
                permission_name = "Read AP Invoice",
                internal_permission_name = APInvoicePermissions.Read,
                module_id = this.ModuleIdentifier.ToString(),
                module_name = this.ModuleName,
                read = true,
            });

            _Context.SaveChanges();

            var read_perm_id = _Context.ModulePermissions.Where(m => m.internal_permission_name == APInvoicePermissions.Read).Select(m => m.id).Single();

            _Context.RolePermissions.Add(new RolePermission()
            {
                role_id = role_id,
                module_permission_id = read_perm_id,
                created_by = 1,
                created_on = DateTime.UtcNow,
                updated_by = 1,
                updated_on = DateTime.UtcNow,
            });

            _Context.SaveChanges();
        }

        if (create_permission == false)
        {
            _Context.ModulePermissions.Add(new ModulePermission()
            {
                permission_name = "Create AP Invoice",
                internal_permission_name = APInvoicePermissions.Create,
                module_id = this.ModuleIdentifier.ToString(),
                module_name = this.ModuleName,
                write = true
            });

            _Context.SaveChanges();

            var create_perm_id = _Context.ModulePermissions.Where(m => m.internal_permission_name == APInvoicePermissions.Create).Select(m => m.id).Single();

            _Context.RolePermissions.Add(new RolePermission()
            {
                role_id = role_id,
                module_permission_id = create_perm_id,
                created_by = 1,
                created_on = DateTime.UtcNow,
                updated_by = 1,
                updated_on = DateTime.UtcNow,
            });

            _Context.SaveChanges();
        }

        if (edit_permission == false)
        {
            _Context.ModulePermissions.Add(new ModulePermission()
            {
                permission_name = "Edit AP Invoice",
                internal_permission_name = APInvoicePermissions.Edit,
                module_id = this.ModuleIdentifier.ToString(),
                module_name = this.ModuleName,
                edit = true
            });

            _Context.SaveChanges();

            var edit_perm_id = _Context.ModulePermissions.Where(m => m.internal_permission_name == APInvoicePermissions.Edit).Select(m => m.id).Single();

            _Context.RolePermissions.Add(new RolePermission()
            {
                role_id = role_id,
                module_permission_id = edit_perm_id,
                created_by = 1,
                created_on = DateTime.UtcNow,
                updated_by = 1,
                updated_on = DateTime.UtcNow,
            });

            _Context.SaveChanges();
        }

        if (delete_permission == false)
        {
            _Context.ModulePermissions.Add(new ModulePermission()
            {
                permission_name = "Delete AP Invoice",
                internal_permission_name = APInvoicePermissions.Delete,
                module_id = this.ModuleIdentifier.ToString(),
                module_name = this.ModuleName,
                delete = true
            });

            _Context.SaveChanges();

            var delete_perm_id = _Context.ModulePermissions.Where(m => m.internal_permission_name == APInvoicePermissions.Delete).Select(m => m.id).Single();

            _Context.RolePermissions.Add(new RolePermission()
            {
                role_id = role_id,
                module_permission_id = delete_perm_id,
                created_by = 1,
                created_on = DateTime.UtcNow,
                updated_by = 1,
                updated_on = DateTime.UtcNow,
            });

            _Context.SaveChanges();
        }
    }

    public APInvoiceHeader? Get(int object_id)
    {
        return _Context.APInvoiceHeaders.SingleOrDefault(m => m.id == object_id);
    }

    public APInvoiceLine? GetLine(int object_id)
    {
        return _Context.APInvoiceLines.SingleOrDefault(m => m.id == object_id);
    }

    public async Task<APInvoiceHeader?> GetAsync(int object_id)
    {
        return await _Context.APInvoiceHeaders.SingleOrDefaultAsync(m => m.id == object_id);
    }

    public async Task<APInvoiceLine?> GetLineAsync(int object_id)
    {
        return await _Context.APInvoiceLines.SingleOrDefaultAsync(m => m.id == object_id);
    }

    public async Task<Response<APInvoiceHeaderDto>> GetDto(int object_id)
    {
        Response<APInvoiceHeaderDto> response = new Response<APInvoiceHeaderDto>();

        var result = await _Context.APInvoiceHeaders.SingleOrDefaultAsync(m => m.id == object_id);
        if (result == null)
        {
            response.SetException("AP Invoice Header not found", ResultCode.NotFound);
            return response;
        }

        response.Data = await this.MapToDto(result);
        return response;
    }

    public async Task<Response<APInvoiceLineDto>> GetLineDto(int object_id)
    {
        Response<APInvoiceLineDto> response = new Response<APInvoiceLineDto>();

        var result = await _Context.APInvoiceLines.SingleOrDefaultAsync(m => m.id == object_id);
        if (result == null)
        {
            response.SetException("AP Invoice Line not found", ResultCode.NotFound);
            return response;
        }

        response.Data = await this.MapToLineDto(result);
        return response;
    }

    public async Task<Response<APInvoiceHeaderDto>> Create(APInvoiceHeaderCreateCommand commandModel)
    {
        if (commandModel == null)
            return new Response<APInvoiceHeaderDto>(ResultCode.NullItemInput);

        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<APInvoiceHeaderDto>(validationResult.Exception, ResultCode.DataValidationError);

        var permission_result = await base.HasPermission(commandModel.calling_user_id, commandModel.token,APInvoicePermissions.Create, write: true);
        if (!permission_result)
            return new Response<APInvoiceHeaderDto>("Invalid permission", ResultCode.InvalidPermission);

        try
        {
            var alreadyExists = APInvoiceHeaderExists(commandModel);
            if (alreadyExists == true)
                return new Response<APInvoiceHeaderDto>(ResultCode.AlreadyExists);

            var item = MapToDatabaseModel(commandModel);

            await _Context.APInvoiceHeaders.AddAsync(item);
            await _Context.SaveChangesAsync();

            // Now do lines
            foreach(var ap_line in commandModel.ap_invoice_lines)
            {
                var db_line = MapToLineDatabaseModel(ap_line, item.id, commandModel.calling_user_id);

                await _Context.APInvoiceLines.AddAsync(db_line);
                await _Context.SaveChangesAsync();
            }


            var dto = await GetDto(item.id);

            return new Response<APInvoiceHeaderDto>(dto.Data);
        }
        catch (Exception ex)
        {
            return new Response<APInvoiceHeaderDto>(ex.Message, ResultCode.Error);
        }
    }

    public async Task<Response<APInvoiceLineDto>> CreateLine(APInvoiceLineCreateCommand commandModel)
    {
        if (commandModel == null)
            return new Response<APInvoiceLineDto>(ResultCode.NullItemInput);

        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<APInvoiceLineDto>(validationResult.Exception, ResultCode.DataValidationError);

        var permission_result = await base.HasPermission(commandModel.calling_user_id, commandModel.token,APInvoicePermissions.Create, write: true);
        if (!permission_result)
            return new Response<APInvoiceLineDto>("Invalid permission", ResultCode.InvalidPermission);

        if(!commandModel.ap_invoice_header_id.HasValue)
            return new Response<APInvoiceLineDto>("AP Invoice Header os a required field", ResultCode.DataValidationError);

        try
        {
            var item = MapToLineDatabaseModel(commandModel, commandModel.ap_invoice_header_id.Value, commandModel.calling_user_id);

            await _Context.APInvoiceLines.AddAsync(item);
            await _Context.SaveChangesAsync();

            var dto = await GetLineDto(item.id);

            return new Response<APInvoiceLineDto>(dto.Data);
        }
        catch (Exception ex)
        {
            return new Response<APInvoiceLineDto>(ex.Message, ResultCode.Error);
        }
    }

    public async Task<Response<APInvoiceHeaderDto>> Edit(APInvoiceHeaderEditCommand commandModel)
    {
        if (commandModel == null)
            return new Response<APInvoiceHeaderDto>(ResultCode.NullItemInput);

        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<APInvoiceHeaderDto>(validationResult.Exception, ResultCode.DataValidationError);

        var permission_result = await base.HasPermission(commandModel.calling_user_id, commandModel.token,APInvoicePermissions.Edit, edit: true);
        if (!permission_result)
            return new Response<APInvoiceHeaderDto>("Invalid permission", ResultCode.InvalidPermission);

        var existingEntity = await GetAsync(commandModel.id);
        if (existingEntity == null)
            return new Response<APInvoiceHeaderDto>("AP Invoice Header not found", ResultCode.NotFound);


        if (existingEntity.vendor_id != commandModel.vendor_id && commandModel.vendor_id.HasValue)
            existingEntity.vendor_id = commandModel.vendor_id.Value;

        if (existingEntity.invoice_number != commandModel.invoice_number)
            existingEntity.invoice_number = commandModel.invoice_number;

        if (existingEntity.invoice_date != commandModel.invoice_date && commandModel.invoice_date.HasValue)
            existingEntity.invoice_date = commandModel.invoice_date.Value;

        if (existingEntity.invoice_due_date != commandModel.invoice_due_date && commandModel.invoice_due_date.HasValue)
            existingEntity.invoice_due_date = commandModel.invoice_due_date.Value;

        if (existingEntity.invoice_received_date != commandModel.invoice_received_date && commandModel.invoice_received_date.HasValue)
            existingEntity.invoice_received_date = commandModel.invoice_received_date.Value;

        if (existingEntity.invoice_total != commandModel.invoice_total && commandModel.invoice_total.HasValue)
            existingEntity.invoice_total = commandModel.invoice_total.Value;

        if (existingEntity.memo != commandModel.memo)
            existingEntity.memo = commandModel.memo;

        if (commandModel.purchase_order_receive_id.HasValue)
            existingEntity.purchase_order_receive_id = commandModel.purchase_order_receive_id.Value;

        if (commandModel.association_object_id.HasValue)
            existingEntity.association_object_id = commandModel.association_object_id.Value;

        if (commandModel.association_is_purchase_order.HasValue)
            existingEntity.association_is_purchase_order = commandModel.association_is_purchase_order.Value;
        
        if (commandModel.association_is_sales_order.HasValue)
            existingEntity.association_is_sales_order = commandModel.association_is_sales_order.Value;

        if (commandModel.association_is_ar_invoice.HasValue)
            existingEntity.association_is_ar_invoice = commandModel.association_is_ar_invoice.Value;

        if (commandModel.packing_list_is_required.HasValue)
            existingEntity.packing_list_is_required = commandModel.packing_list_is_required.Value;

        if (commandModel.is_paid.HasValue)
            existingEntity.is_paid = commandModel.is_paid.Value;


        existingEntity.updated_on = DateTime.UtcNow;
        existingEntity.updated_by = commandModel.calling_user_id;

        _Context.APInvoiceHeaders.Update(existingEntity);
        await _Context.SaveChangesAsync();

        var dto = await MapToDto(existingEntity);
        return new Response<APInvoiceHeaderDto>(dto);
    }

    public async Task<Response<APInvoiceLineDto>> EditLine(APInvoiceLineEditCommand commandModel)
    {
        if (commandModel == null)
            return new Response<APInvoiceLineDto>(ResultCode.NullItemInput);

        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<APInvoiceLineDto>(validationResult.Exception, ResultCode.DataValidationError);

        var permission_result = await base.HasPermission(commandModel.calling_user_id, commandModel.token,APInvoicePermissions.Edit, edit: true);
        if (!permission_result)
            return new Response<APInvoiceLineDto>("Invalid permission", ResultCode.InvalidPermission);

        var existingEntity = await GetLineAsync(commandModel.id);
        if (existingEntity == null)
            return new Response<APInvoiceLineDto>("AP Invoice Line not found", ResultCode.NotFound);


        if (existingEntity.line_number != commandModel.line_number && commandModel.line_number.HasValue)
            existingEntity.line_number = commandModel.line_number.Value;

        if (existingEntity.line_total != commandModel.line_total)
            existingEntity.line_total = commandModel.line_total.Value;

        if (existingEntity.qty_invoiced != commandModel.qty_invoiced && commandModel.qty_invoiced.HasValue)
            existingEntity.qty_invoiced = commandModel.qty_invoiced.Value;

        if (existingEntity.gl_account_id != commandModel.gl_account_id && commandModel.gl_account_id.HasValue)
            existingEntity.gl_account_id = commandModel.gl_account_id.Value;

        if (existingEntity.description != commandModel.description)
            existingEntity.description = commandModel.description;

        if (commandModel.association_object_id.HasValue)
            existingEntity.association_object_id = commandModel.association_object_id.Value;

        if (commandModel.association_is_purchase_order.HasValue)
            existingEntity.association_is_purchase_order = commandModel.association_is_purchase_order.Value;

        if (commandModel.association_is_sales_order.HasValue)
            existingEntity.association_is_sales_order = commandModel.association_is_sales_order.Value;

        if (commandModel.association_is_ar_invoice.HasValue)
            existingEntity.association_is_ar_invoice = commandModel.association_is_ar_invoice.Value;


        existingEntity.updated_on = DateTime.UtcNow;
        existingEntity.updated_by = commandModel.calling_user_id;

        _Context.APInvoiceLines.Update(existingEntity);
        await _Context.SaveChangesAsync();

        var dto = await MapToLineDto(existingEntity);
        return new Response<APInvoiceLineDto>(dto);
    }

    public async Task<Response<APInvoiceHeaderDto>> Delete(APInvoiceHeaderDeleteCommand commandModel)
    {
        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<APInvoiceHeaderDto>(validationResult.Exception, ResultCode.DataValidationError);

        var permission_result = await base.HasPermission(commandModel.calling_user_id, commandModel.token,APInvoicePermissions.Delete, delete: true);
        if (!permission_result)
            return new Response<APInvoiceHeaderDto>("Invalid permission", ResultCode.InvalidPermission);

        var existingEntity = await GetAsync(commandModel.id);
        if (existingEntity == null)
            return new Response<APInvoiceHeaderDto>("AP Invoice Header not found", ResultCode.NotFound);

        // Soft Delete
        existingEntity = CommonDataHelper<APInvoiceHeader>.FillDeleteFields(existingEntity, commandModel.calling_user_id);

        _Context.APInvoiceHeaders.Update(existingEntity);
        await _Context.SaveChangesAsync();

        var dto = await MapToDto(existingEntity);
        return new Response<APInvoiceHeaderDto>(dto);
    }

    public async Task<Response<APInvoiceLineDto>> DeleteLine(APInvoiceLineDeleteCommand commandModel)
    {
        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<APInvoiceLineDto>(validationResult.Exception, ResultCode.DataValidationError);

        var permission_result = await base.HasPermission(commandModel.calling_user_id, commandModel.token,APInvoicePermissions.Delete, delete: true);
        if (!permission_result)
            return new Response<APInvoiceLineDto>("Invalid permission", ResultCode.InvalidPermission);

        var existingEntity = await GetLineAsync(commandModel.id);
        if (existingEntity == null)
            return new Response<APInvoiceLineDto>("AP Invoice Line not found", ResultCode.NotFound);

        // Soft Delete
        existingEntity = CommonDataHelper<APInvoiceLine>.FillDeleteFields(existingEntity, commandModel.calling_user_id);

        _Context.APInvoiceLines.Update(existingEntity);
        await _Context.SaveChangesAsync();

        var dto = await MapToLineDto(existingEntity);
        return new Response<APInvoiceLineDto>(dto);
    }

    public async Task<Response<APInvoiceHeaderDto>> AssociateHeaderObject(APInvoiceAssoicationCommand associationCommand)
    {
        var validationResult = ModelValidationHelper.ValidateModel(associationCommand);
        if (!validationResult.Success)
            return new Response<APInvoiceHeaderDto>(validationResult.Exception, ResultCode.DataValidationError);

        Response<APInvoiceHeaderDto> response = new Response<APInvoiceHeaderDto>();

        try
        {
            var permission_result = await base.HasPermission(associationCommand.calling_user_id, associationCommand.token, APInvoicePermissions.Edit, edit: true);
            if (!permission_result)
                return new Response<APInvoiceHeaderDto>("Invalid permission", ResultCode.InvalidPermission);

            var existingEntity = await GetAsync(associationCommand.ap_invoice_object_id);
            if (existingEntity == null)
                return new Response<APInvoiceHeaderDto>("AP Invoice Header not found", ResultCode.NotFound);

            if (associationCommand.association_is_purchase_order == false
                && associationCommand.association_is_sales_order == false
                && associationCommand.association_is_ar_invoice == false)
            {
                return new Response<APInvoiceHeaderDto>("Type of Association was not set", ResultCode.Invalid);
            }

            if (associationCommand.association_is_purchase_order == true)
            {
                var existingAssociation = await _Context.PurchaseOrderHeaders.Where(m => m.id == associationCommand.association_object_id).SingleOrDefaultAsync();
                if (existingAssociation == null)
                    return new Response<APInvoiceHeaderDto>("Associated Purchase Order not found", ResultCode.NotFound);

            }
            else if (associationCommand.association_is_sales_order == true)
            {
                var existingAssociation = await _Context.OrderHeaders.Where(m => m.id == associationCommand.association_object_id).SingleOrDefaultAsync();
                if (existingAssociation == null)
                    return new Response<APInvoiceHeaderDto>("Associated Order not found", ResultCode.NotFound);
            }
            else if (associationCommand.association_is_ar_invoice == true)
            {
                var existingAssociation = await _Context.ARInvoiceHeaders.Where(m => m.id == associationCommand.association_object_id).SingleOrDefaultAsync();
                if (existingAssociation == null)
                    return new Response<APInvoiceHeaderDto>("Associated Order not found", ResultCode.NotFound);
            }

            existingEntity.association_object_id = associationCommand.association_object_id;
            existingEntity.association_is_purchase_order = false;
            existingEntity.association_is_sales_order = false;
            existingEntity.association_is_ar_invoice = false;

            if (associationCommand.association_is_purchase_order == true)
                existingEntity.association_is_purchase_order = true;
            else if (associationCommand.association_is_sales_order == true)
                existingEntity.association_is_sales_order = true;
            else if (associationCommand.association_is_ar_invoice == true)
                existingEntity.association_is_ar_invoice = true;

            existingEntity.updated_by = associationCommand.calling_user_id;
            existingEntity.updated_on = DateTime.UtcNow;

            _Context.APInvoiceHeaders.Update(existingEntity);
            await _Context.SaveChangesAsync();

            var dto = await this.MapToDto(existingEntity);

            return new Response<APInvoiceHeaderDto>(dto);
        }
        catch (Exception ex)
        {
            await LogError(50, this.GetType().Name, nameof(AssociateHeaderObject), ex);
            response.SetException(ex.Message, ResultCode.Error);
        }

        return response;
    }


    public async Task<Response<APInvoiceLineDto>> AssociateLineObject(APInvoiceAssoicationCommand associationCommand)
    {
        var validationResult = ModelValidationHelper.ValidateModel(associationCommand);
        if (!validationResult.Success)
            return new Response<APInvoiceLineDto>(validationResult.Exception, ResultCode.DataValidationError);

        Response<APInvoiceLineDto> response = new Response<APInvoiceLineDto>();

        try
        {
            var permission_result = await base.HasPermission(associationCommand.calling_user_id, associationCommand.token, APInvoicePermissions.Edit, edit: true);
            if (!permission_result)
                return new Response<APInvoiceLineDto>("Invalid permission", ResultCode.InvalidPermission);

            var existingEntity = await GetLineAsync(associationCommand.ap_invoice_object_id);
            if (existingEntity == null)
                return new Response<APInvoiceLineDto>("AP Invoice Line not found", ResultCode.NotFound);

            if (associationCommand.association_is_purchase_order == false
                && associationCommand.association_is_sales_order == false
                && associationCommand.association_is_ar_invoice == false)
            {
                return new Response<APInvoiceLineDto>("Type of Association was not set", ResultCode.Invalid);
            }

            if (associationCommand.association_is_purchase_order == true)
            {
                var existingAssociation = await _Context.PurchaseOrderHeaders.Where(m => m.id == associationCommand.association_object_id).SingleOrDefaultAsync();
                if (existingAssociation == null)
                    return new Response<APInvoiceLineDto>("Associated Purchase Order not found", ResultCode.NotFound);

            }
            else if (associationCommand.association_is_sales_order == true)
            {
                var existingAssociation = await _Context.OrderHeaders.Where(m => m.id == associationCommand.association_object_id).SingleOrDefaultAsync();
                if (existingAssociation == null)
                    return new Response<APInvoiceLineDto>("Associated Order not found", ResultCode.NotFound);
            }
            else if (associationCommand.association_is_ar_invoice == true)
            {
                var existingAssociation = await _Context.ARInvoiceHeaders.Where(m => m.id == associationCommand.association_object_id).SingleOrDefaultAsync();
                if (existingAssociation == null)
                    return new Response<APInvoiceLineDto>("Associated Order not found", ResultCode.NotFound);
            }

            existingEntity.association_object_id = associationCommand.association_object_id;
            existingEntity.association_is_purchase_order = false;
            existingEntity.association_is_sales_order = false;
            existingEntity.association_is_ar_invoice = false;

            if (associationCommand.association_is_purchase_order == true)
                existingEntity.association_is_purchase_order = true;
            else if (associationCommand.association_is_sales_order == true)
                existingEntity.association_is_sales_order = true;
            else if (associationCommand.association_is_ar_invoice == true)
                existingEntity.association_is_ar_invoice = true;

            existingEntity.updated_by = associationCommand.calling_user_id;
            existingEntity.updated_on = DateTime.UtcNow;

            _Context.APInvoiceLines.Update(existingEntity);
            await _Context.SaveChangesAsync();

            var dto = await this.MapToLineDto(existingEntity);

            return new Response<APInvoiceLineDto>(dto);
        }
        catch (Exception ex)
        {
            await LogError(50, this.GetType().Name, nameof(AssociateLineObject), ex);
            response.SetException(ex.Message, ResultCode.Error);
        }

        return response;
    }

    public async Task<Response<APInvoiceHeaderDto>> AssociateReceivedPO(APInvoiceAssociatePOCommand associationCommand)
    {
        var validationResult = ModelValidationHelper.ValidateModel(associationCommand);
        if (!validationResult.Success)
            return new Response<APInvoiceHeaderDto>(validationResult.Exception, ResultCode.DataValidationError);

        Response<APInvoiceHeaderDto> response = new Response<APInvoiceHeaderDto>();

        try
        {
            var permission_result = await base.HasPermission(associationCommand.calling_user_id, associationCommand.token, APInvoicePermissions.Edit, edit: true);
            if (!permission_result)
                return new Response<APInvoiceHeaderDto>("Invalid permission", ResultCode.InvalidPermission);

            var existingEntity = await GetAsync(associationCommand.ap_invoice_object_id);
            if (existingEntity == null)
                return new Response<APInvoiceHeaderDto>("AP Invoice Header not found", ResultCode.NotFound);
        
            var associatedPO = await _Context.PurchaseOrderReceiveHeaders.Where(m => m.id == associationCommand.ap_invoice_object_id).SingleOrDefaultAsync();
            if (associatedPO == null)
                return new Response<APInvoiceHeaderDto>("PO Invoice not found", ResultCode.NotFound);

            existingEntity.purchase_order_receive_id = associationCommand.ap_invoice_object_id;
            existingEntity.updated_by = associationCommand.calling_user_id;
            existingEntity.updated_on = DateTime.UtcNow;

            _Context.APInvoiceHeaders.Update(existingEntity);
            await _Context.SaveChangesAsync();

            var dto = await this.MapToDto(existingEntity);

            return new Response<APInvoiceHeaderDto>(dto);
        }
        catch (Exception ex)
        {
            await LogError(50, this.GetType().Name, nameof(AssociateReceivedPO), ex);
            response.SetException(ex.Message, ResultCode.Error);
        }

        return response;
    }

    public async Task<PagingResult<APInvoiceHeaderListDto>> Find(PagingSortingParameters parameters, APInvoiceHeaderFindCommand commandModel)
    {
        var response = new PagingResult<APInvoiceHeaderListDto>();

        try
        {
            var query = _Context.APInvoiceHeaders
                .Where(m => !m.is_deleted);

            if (!string.IsNullOrEmpty(commandModel.wildcard))
            {
                var wild = commandModel.wildcard.ToLower();
                query = query.Where(m =>
                    m.invoice_number.ToLower().Contains(wild)
                    || m.memo.ToLower().Contains(wild)
                );
            }

            decimal parsed_num = 0;

            if (decimal.TryParse(commandModel.wildcard, out parsed_num))
            {
                var wild = commandModel.wildcard.ToLower();
                query = query.Where(m =>
                    m.invoice_total == parsed_num
                );
            }

            var totalCount = await query.CountAsync();
            var pagedItems = await query.SortAndPageBy(parameters).ToListAsync();

            var dtos = new List<APInvoiceHeaderListDto>();
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

    public async Task<Response<List<APInvoiceHeaderListDto>>> GlobalSearch(PagingSortingParameters parameters, string wildcard)
    {
        throw new NotImplementedException();
    }

    public async Task<Response<List<APInvoiceAssociationDto>>> GetAssociations(APInvoiceAssociationsFindCommand commandModel)
    {
        Response<List<APInvoiceAssociationDto>> response = new Response<List<APInvoiceAssociationDto>>();
        response.Data = new List<APInvoiceAssociationDto>();

        try
        {

        }
        catch (Exception ex)
        {
            await LogError(50, this.GetType().Name, nameof(GetAssociations), ex);
            return new Response<List<APInvoiceAssociationDto>>(ex.Message, ResultCode.Error);
        }

        return response;
    }

    public async Task<APInvoiceHeaderListDto> MapToListDto(APInvoiceHeader databaseModel)
    {
        var dto = new APInvoiceHeaderListDto()
        {
            vendor_id = databaseModel.vendor_id,
            invoice_number = databaseModel.invoice_number,
            invoice_date = databaseModel.invoice_date,
            invoice_due_date = databaseModel.invoice_due_date,
            invoice_received_date = databaseModel.invoice_received_date,
            invoice_total = databaseModel.invoice_total,
            memo = databaseModel.memo,
            packing_list_is_required = databaseModel.packing_list_is_required,
            is_paid = databaseModel.is_paid,
            guid = databaseModel.guid,
            id = databaseModel.id,
            created_on = databaseModel.created_on,
            updated_on = databaseModel.updated_on,
            is_deleted = databaseModel.is_deleted,
            created_on_string = databaseModel.created_on_string,
            created_on_timezone = databaseModel.created_on_timezone,
            updated_on_string = databaseModel.updated_on_string,
            updated_on_timezone = databaseModel.updated_on_timezone,
        };

        dto.vendor_name = await _Context.Vendors.Where(m => m.id == databaseModel.vendor_id).Select(m => m.vendor_name).SingleOrDefaultAsync();

        return dto;
    }

    public async Task<APInvoiceHeaderDto> MapToDto(APInvoiceHeader databaseModel)
    {
        var dto = new APInvoiceHeaderDto()
        {
            vendor_id = databaseModel.vendor_id,
            invoice_number = databaseModel.invoice_number,
            invoice_date = databaseModel.invoice_date,
            invoice_due_date = databaseModel.invoice_due_date,
            invoice_received_date = databaseModel.invoice_received_date,
            invoice_total = databaseModel.invoice_total,
            memo = databaseModel.memo,
            purchase_order_receive_id = databaseModel.purchase_order_receive_id,
            packing_list_is_required = databaseModel.packing_list_is_required,
            association_object_id = databaseModel.association_object_id,
            association_is_purchase_order = databaseModel.association_is_purchase_order,
            association_is_sales_order = databaseModel.association_is_sales_order,
            association_is_ar_invoice = databaseModel.association_is_ar_invoice,
            is_paid = databaseModel.is_paid,
            guid = databaseModel.guid,
            id = databaseModel.id,
            created_on = databaseModel.created_on,
            updated_on = databaseModel.updated_on,
            is_deleted = databaseModel.is_deleted,
            created_on_string = databaseModel.created_on_string,
            created_on_timezone = databaseModel.created_on_timezone,
            updated_on_string = databaseModel.updated_on_string,
            updated_on_timezone = databaseModel.updated_on_timezone,
        };

        dto.vendor_name = await _Context.Vendors.Where(m => m.id == databaseModel.vendor_id).Select(m => m.vendor_name).SingleOrDefaultAsync();

        var lines = await _Context.APInvoiceLines
            .Where(m => m.ap_invoice_header_id == databaseModel.id && !m.is_deleted)
            .ToListAsync();

        foreach(var ap_line in lines)
            dto.ap_invoice_lines.Add(await MapToLineDto(ap_line));


        return dto;
    }

    public async Task<APInvoiceLineDto> MapToLineDto(APInvoiceLine databaseModel)
    {
        var dto = new APInvoiceLineDto()
        {
            ap_invoice_header_id = databaseModel.ap_invoice_header_id,
            line_number = databaseModel.line_number,
            line_total = databaseModel.line_total,
            qty_invoiced = databaseModel.qty_invoiced,
            gl_account_id = databaseModel.gl_account_id,
            description = databaseModel.description,
            association_object_id = databaseModel.association_object_id,
            association_object_line_id = databaseModel.association_object_line_id,
            association_is_purchase_order = databaseModel.association_is_purchase_order,
            association_is_sales_order = databaseModel.association_is_sales_order,
            association_is_ar_invoice = databaseModel.association_is_ar_invoice,
            guid = databaseModel.guid,
            id = databaseModel.id,
            created_on = databaseModel.created_on,
            updated_on = databaseModel.updated_on,
            is_deleted = databaseModel.is_deleted,
            created_on_string = databaseModel.created_on_string,
            created_on_timezone = databaseModel.created_on_timezone,
            updated_on_string = databaseModel.updated_on_string,
            updated_on_timezone = databaseModel.updated_on_timezone,
        };

        if(databaseModel.association_object_id.HasValue && databaseModel.association_is_purchase_order)
        {
            var receive_lines = (from por in _Context.PurchaseOrderReceiveHeaders
                                 join pol in _Context.PurchaseOrderReceiveLines on por.id equals pol.purchase_order_receive_header_id
                                 where por.purchase_order_id == databaseModel.association_object_id
                                 select new { por, pol }).ToList();

            foreach (var line in receive_lines)
            {

            }
        }

        
            //TODO; Need assocaite table structure for receive to po matching

        return dto;
    }

    private async Task<List<PurchaseOrderReceiveLine>> GetPurchaseOrderReceiveData(int association_object_id)
    {
        List<PurchaseOrderReceiveLine> response = new List<PurchaseOrderReceiveLine>();

        var purchase_order_header = await _Context.PurchaseOrderHeaders
            .Where(m => m.id == association_object_id)
            .SingleOrDefaultAsync();

        if(purchase_order_header != null)
        {
            
        }
        

        return response;
    }

    public async Task<APInvoiceLineListDto> MapToLineListDto(APInvoiceLine databaseModel)
    {
        var dto = new APInvoiceLineListDto()
        {
            ap_invoice_header_id = databaseModel.ap_invoice_header_id,
            line_number = databaseModel.line_number,
            line_total = databaseModel.line_total,
            qty_invoiced = databaseModel.qty_invoiced,
            gl_account_id = databaseModel.gl_account_id,
            description = databaseModel.description,
            association_object_id = databaseModel.association_object_id,
            association_object_line_id = databaseModel.association_object_line_id,
            guid = databaseModel.guid,
            id = databaseModel.id,
            created_on = databaseModel.created_on,
            updated_on = databaseModel.updated_on,
            is_deleted = databaseModel.is_deleted,
            created_on_string = databaseModel.created_on_string,
            created_on_timezone = databaseModel.created_on_timezone,
            updated_on_string = databaseModel.updated_on_string,
            updated_on_timezone = databaseModel.updated_on_timezone,
        };

        return dto;
    }

    public APInvoiceHeader MapToDatabaseModel(APInvoiceHeaderCreateCommand createCommand)
    {
        return CommonDataHelper<APInvoiceHeader>.FillCommonFields(new APInvoiceHeader()
        {
            vendor_id = createCommand.vendor_id,
            invoice_number = createCommand.invoice_number,
            invoice_date = createCommand.invoice_date,
            invoice_due_date = createCommand.invoice_due_date,
            invoice_received_date = createCommand.invoice_received_date,
            invoice_total = createCommand.invoice_total,
            memo = createCommand.memo,
            purchase_order_receive_id = createCommand.purchase_order_receive_id,
            packing_list_is_required = createCommand.packing_list_is_required,
            association_object_id = createCommand.association_object_id,
            association_is_purchase_order = createCommand.association_is_purchase_order,
            association_is_sales_order = createCommand.association_is_sales_order,
            association_is_ar_invoice = createCommand.association_is_ar_invoice,
            is_paid = createCommand.is_paid,
            guid = Guid.NewGuid().ToString(),
            created_on = DateTime.UtcNow,
            updated_on = DateTime.UtcNow,
            is_deleted = false
        }, createCommand.calling_user_id);
    }

    public APInvoiceLine MapToLineDatabaseModel(APInvoiceLineCreateCommand createCommand, int ap_invoice_header_id, int calling_user_id)
    {
        return CommonDataHelper<APInvoiceLine>.FillCommonFields(new APInvoiceLine()
        {
            ap_invoice_header_id = ap_invoice_header_id,
            line_number = createCommand.line_number,
            line_total = createCommand.line_total,
            qty_invoiced = createCommand.qty_invoiced,
            gl_account_id = createCommand.gl_account_id,
            description = createCommand.description,
            association_object_id = createCommand.association_object_id,
            association_object_line_id = createCommand.association_object_line_id,
            association_is_purchase_order = createCommand.association_is_purchase_order,
            association_is_sales_order = createCommand.association_is_sales_order,
            association_is_ar_invoice = createCommand.association_is_ar_invoice,
            guid = Guid.NewGuid().ToString(),
            is_deleted = false
        }, calling_user_id);
    }

    private bool APInvoiceHeaderExists(APInvoiceHeaderCreateCommand createCommand)
    {
        return _Context.APInvoiceHeaders.Any(m => m.invoice_number == createCommand.invoice_number && m.vendor_id == createCommand.vendor_id);
    }

    private bool APInvoiceLineExists(APInvoiceLineCreateCommand createCommand)
    {
        return _Context.APInvoiceLines.Any(m => m.line_number == createCommand.line_number);
    }

    public APInvoiceHeader MapToDatabaseModel(APInvoiceHeaderDto dtoModel)
    {
        throw new NotImplementedException();
    }
}
