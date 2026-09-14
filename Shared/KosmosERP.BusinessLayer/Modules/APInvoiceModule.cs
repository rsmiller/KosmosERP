using Microsoft.EntityFrameworkCore;
using KosmosERP.Database;
using KosmosERP.Database.Models;
using KosmosERP.Module;
using KosmosERP.Models;
using KosmosERP.Models.Helpers;
using KosmosERP.Models.Interfaces;
using KosmosERP.BusinessLayer.Models.Module.APInvoice.Dto;
using KosmosERP.BusinessLayer.Models.Module.APInvoice.Command.Create;
using KosmosERP.BusinessLayer.Models.Module.APInvoice.Command.Delete;
using KosmosERP.BusinessLayer.Models.Module.APInvoice.Command.Edit;
using KosmosERP.BusinessLayer.Models.Module.APInvoice.Command.Find;
using KosmosERP.BusinessLayer.Models.Module.APInvoice.Command;
using KosmosERP.BusinessLayer.Helpers;

namespace KosmosERP.BusinessLayer.Modules;

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
    public override string ModuleName => "AP Invoices";

    private IBaseERPContext _Context;

    private IPurchaseOrderModule _POModule;
    private IOrderModule _OrderModule;
    private IARInvoiceModule _ARModule;
    private IPurchaseOrderReceiveModule _POReceiveModule;
    private IDocumentUploadModule _DocumentUploadModule;
    private IFinancialTransactionModule? _FinancialTransactionModule;
    private IChartOfAccountModule? _ChartOfAccountModule;

    public APInvoiceModule(IBaseERPContext context, ILogProviderFactory logProviderFactory) : base(logProviderFactory)
    {
        _Context = context;
    }

    public APInvoiceModule(IBaseERPContext context,
                                IPurchaseOrderModule po_module,
                                IOrderModule order_module,
                                IARInvoiceModule ar_module,
                                IPurchaseOrderReceiveModule po_receive_module,
                                IDocumentUploadModule document_module,
                                IFinancialTransactionModule financial_transaction_module,
                                IChartOfAccountModule chart_of_account_module,
                                ILogProviderFactory logProviderFactory) : base(logProviderFactory)
    {
        _Context = context;
        _POModule = po_module;
        _OrderModule = order_module;
        _ARModule = ar_module;
        _POReceiveModule = po_receive_module;
        _DocumentUploadModule = document_module;
        _FinancialTransactionModule = financial_transaction_module;
        _ChartOfAccountModule = chart_of_account_module;
    }

    public override void SeedPermissions()
    {
        var role = _Context.Roles.Any(m => m.name == "AP Invoice Administrators");

        if (role == false)
        {
            _Context.Roles.Add(CommonDataHelper<Role>.FillCommonFields(new Role()
            {
                name = "AP Invoice Administrators",
                created_by = "1",
                created_on = DateTime.UtcNow,
                updated_by = "1",
                updated_on = DateTime.UtcNow,
            }, 1));

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

    public async Task<Response<APInvoiceHeaderDto>> GetDtoByGuid(string guid)
    {
        var entity = await _Context.APInvoiceHeaders.FirstOrDefaultAsync(c => c.guid == guid && !c.is_deleted);
        if (entity == null)
            return new Response<APInvoiceHeaderDto>("APInvoiceHeader not found", ResultCode.NotFound);

        var dto = await MapToDto(entity);
        return new Response<APInvoiceHeaderDto>(dto);
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

            // Post to General Ledger
            await PostToGeneralLedger(item, commandModel.calling_user_id);

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


        existingEntity = CommonDataHelper<APInvoiceHeader>.FillUpdateFields(existingEntity, commandModel.calling_user_id);


        _Context.APInvoiceHeaders.Update(existingEntity);
        await _Context.SaveChangesAsync();


        // Process them lines bro
        foreach (var line in commandModel.ap_create_invoice_lines)
        {
            await _Context.APInvoiceLines.AddAsync(this.MapToLineDatabaseModel(line, existingEntity.id, commandModel.calling_user_id));
            await _Context.SaveChangesAsync();
        }

        foreach (var line in commandModel.ap_edit_invoice_lines)
        {
            await this.EditLine(line);
        }


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

        var existingEntity = await GetLineAsync(commandModel.id);
        if (existingEntity == null)
            return new Response<APInvoiceLineDto>("AP Invoice Line not found", ResultCode.NotFound);


        if (existingEntity.line_number != commandModel.line_number && commandModel.line_number.HasValue)
            existingEntity.line_number = commandModel.line_number.Value;

        if (existingEntity.line_total != commandModel.line_total)
            existingEntity.line_total = commandModel.line_total.Value;

        if (existingEntity.qty_invoiced != commandModel.qty_invoiced && commandModel.qty_invoiced.HasValue)
            existingEntity.qty_invoiced = commandModel.qty_invoiced.Value;

        if (!String.IsNullOrEmpty(commandModel.gl_account) && existingEntity.gl_account != commandModel.gl_account)
            existingEntity.gl_account = commandModel.gl_account;

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


        existingEntity = CommonDataHelper<APInvoiceLine>.FillUpdateFields(existingEntity, commandModel.calling_user_id);


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

        var existingEntity = await GetAsync(commandModel.id);
        if (existingEntity == null)
            return new Response<APInvoiceHeaderDto>("AP Invoice Header not found", ResultCode.NotFound);

        // Soft Delete
        existingEntity = CommonDataHelper<APInvoiceHeader>.FillDeleteFields(existingEntity, commandModel.calling_user_id);

        _Context.APInvoiceHeaders.Update(existingEntity);
        await _Context.SaveChangesAsync();

        // Delete lines
        var lines = await _Context.APInvoiceLines.Where(m => m.ap_invoice_header_id == existingEntity.id).ToListAsync();
        foreach(var line in lines)
        {
            await this.DeleteLine(new APInvoiceLineDeleteCommand()
            {
                calling_user_id = commandModel.calling_user_id,
                id = line.id,
            });
        }


        var dto = await MapToDto(existingEntity);
        return new Response<APInvoiceHeaderDto>(dto);
    }

    public async Task<Response<APInvoiceLineDto>> DeleteLine(APInvoiceLineDeleteCommand commandModel)
    {
        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<APInvoiceLineDto>(validationResult.Exception, ResultCode.DataValidationError);

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


            existingEntity = CommonDataHelper<APInvoiceHeader>.FillUpdateFields(existingEntity, associationCommand.calling_user_id);


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

            existingEntity = CommonDataHelper<APInvoiceLine>.FillUpdateFields(existingEntity, associationCommand.calling_user_id);


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
            var existingEntity = await GetAsync(associationCommand.ap_invoice_object_id);
            if (existingEntity == null)
                return new Response<APInvoiceHeaderDto>("AP Invoice Header not found", ResultCode.NotFound);
        
            var associatedPO = await _Context.PurchaseOrderReceiveHeaders.Where(m => m.id == associationCommand.ap_invoice_object_id).SingleOrDefaultAsync();
            if (associatedPO == null)
                return new Response<APInvoiceHeaderDto>("PO Invoice not found", ResultCode.NotFound);


            existingEntity.purchase_order_receive_id = associationCommand.ap_invoice_object_id;
            existingEntity.invoice_received_date = associatedPO.created_on;

            existingEntity = CommonDataHelper<APInvoiceHeader>.FillUpdateFields(existingEntity, associationCommand.calling_user_id);


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
                query = query.Where(m => m.invoice_number.ToLower().Contains(wild)
                    || m.memo.ToLower().Contains(wild)
                );
            }

            decimal parsed_num = 0;

            if (decimal.TryParse(commandModel.wildcard, out parsed_num))
            {
                var wild = commandModel.wildcard.ToLower();
                query = query.Where(m =>m.invoice_total == parsed_num);
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

    public async Task<Response<List<APInvoiceHeaderListDto>>> GlobalSearch(GlobalSearchFindCommand commandModel)
    {
        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<List<APInvoiceHeaderListDto>>(validationResult.Exception, ResultCode.DataValidationError);
        
        throw new NotImplementedException();
    }

    public async Task<Response<List<APInvoiceAssociationDto>>> GetAssociations(APInvoiceAssociationsFindCommand commandModel)
    {
        Response<List<APInvoiceAssociationDto>> response = new Response<List<APInvoiceAssociationDto>>();
        response.Data = new List<APInvoiceAssociationDto>();

        try
        {
            var existingEntity = await GetAsync(commandModel.ap_invoice_object_id);
            if (existingEntity == null)
                return new Response<List<APInvoiceAssociationDto>>("AP Invoice Header not found", ResultCode.NotFound);

            var ap_invoice_lines = await _Context.APInvoiceLines.Where(m => m.ap_invoice_header_id == existingEntity.id).ToListAsync();

            if (existingEntity.association_is_purchase_order == true)
            {
                var purchase_order_header = await _Context.PurchaseOrderHeaders.Where(m => m.id == existingEntity.association_object_id).SingleOrDefaultAsync();
                if (purchase_order_header == null)
                    return new Response<List<APInvoiceAssociationDto>>("Purchase Order Header not found", ResultCode.NotFound);

                var purchase_order_lines = await _Context.PurchaseOrderLines.Where(m => m.purchase_order_header_id == existingEntity.association_object_id).ToListAsync();

                foreach (var ap_invoice_line in ap_invoice_lines)
                {
                    if(ap_invoice_line.association_object_line_id.HasValue)
                    {
                        var found_line = purchase_order_lines.FirstOrDefault(m => m.id == ap_invoice_line.association_object_line_id);
                        if (found_line != null)
                        {
                            response.Data.Add(new APInvoiceAssociationDto()
                            {
                                id = ap_invoice_line.id,
                                is_purchase_order = true,
                                additional_data = new Dictionary<string, string>()
                                {
                                    {"quantity", found_line.quantity.ToString() },
                                    {"product_id", found_line.product_id.ToString() },
                                    {"unit_price", found_line.unit_price.ToString() }
                                }
                            });
                        }
                    }
                }
            }
            else if (existingEntity.association_is_sales_order == true)
            {
                var sales_order_header = await _Context.OrderHeaders.Where(m => m.id == existingEntity.association_object_id).SingleOrDefaultAsync();
                if (sales_order_header == null)
                    return new Response<List<APInvoiceAssociationDto>>("Sales Order Header not found", ResultCode.NotFound);

                var sales_order_lines = await _Context.OrderLines.Where(m => m.order_header_id == existingEntity.association_object_id).ToListAsync();

                foreach (var ap_invoice_line in ap_invoice_lines)
                {
                    if (ap_invoice_line.association_object_line_id.HasValue)
                    {
                        var found_line = sales_order_lines.FirstOrDefault(m => m.id == ap_invoice_line.association_object_line_id);
                        if (found_line != null)
                        {
                            response.Data.Add(new APInvoiceAssociationDto()
                            {
                                id = ap_invoice_line.id,
                                is_sales_order = true,
                                additional_data = new Dictionary<string, string>()
                                {
                                    {"quantity", found_line.quantity.ToString() },
                                    {"product_id", found_line.product_id.ToString() },
                                    {"unit_price", found_line.unit_price.ToString() }
                                }
                            });
                        }
                    }
                }
            }
            else if (existingEntity.association_is_ar_invoice == true)
            {
                var ar_invoice_header = await _Context.ARInvoiceHeaders.Where(m => m.id == existingEntity.association_object_id).SingleOrDefaultAsync();
                if (ar_invoice_header == null)
                    return new Response<List<APInvoiceAssociationDto>>("AR Invoice Header not found", ResultCode.NotFound);

                var ar_invoice_lines = await _Context.ARInvoiceLines.Where(m => m.ar_invoice_header_id == existingEntity.association_object_id).ToListAsync();

                foreach (var ap_invoice_line in ap_invoice_lines)
                {
                    if (ap_invoice_line.association_object_line_id.HasValue)
                    {
                        var found_line = ar_invoice_lines.FirstOrDefault(m => m.id == ap_invoice_line.association_object_line_id);
                        if (found_line != null)
                        {
                            response.Data.Add(new APInvoiceAssociationDto()
                            {
                                id = ap_invoice_line.id,
                                is_ar_invoice = true,
                                additional_data = new Dictionary<string, string>()
                                {
                                    {"quantity", found_line.invoice_qty.ToString() },
                                    {"order_quantity", found_line.order_qty.ToString() },
                                    {"product_id", found_line.product_id.ToString() },
                                    {"unit_price", found_line.order_line.unit_price.ToString() }
                                }
                            });
                        }
                    }
                }
            }
            
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
            created_by = databaseModel.created_by,
            updated_on = databaseModel.updated_on,
            updated_by = databaseModel.updated_by,
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
            created_by = databaseModel.created_by,
            updated_by = databaseModel.updated_by,
            created_on = databaseModel.created_on,
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

        dto.vendor_name = await _Context.Vendors.Where(m => m.id == databaseModel.vendor_id).Select(m => m.vendor_name).SingleOrDefaultAsync();

        var lines = await _Context.APInvoiceLines
            .Where(m => m.ap_invoice_header_id == databaseModel.id && !m.is_deleted)
            .ToListAsync();

        foreach(var ap_line in lines)
            dto.ap_invoice_lines.Add(await MapToLineDto(ap_line));

        if (databaseModel.association_object_id.HasValue && databaseModel.association_is_purchase_order)
        {
            dto.association_number = await _Context.PurchaseOrderHeaders.Where(m => m.id == dto.association_object_id && !m.is_deleted).Select(m => m.po_number).SingleOrDefaultAsync();

            var purchase_order_lines = await _Context.PurchaseOrderLines.Where(m => m.purchase_order_header_id == dto.association_object_id && !m.is_deleted).ToListAsync();

            foreach (var po_line in purchase_order_lines)
                dto.po_lines.Add(await _POModule.MapToLineDto(po_line));


            var receive_lines = await (from ph in _Context.PurchaseOrderReceiveHeaders
                                       join pl in _Context.PurchaseOrderReceiveLines on ph.id equals pl.purchase_order_receive_header_id
                                       where ph.purchase_order_id == databaseModel.association_object_id
                                       select pl).ToListAsync();

            foreach (var rec_lines in receive_lines)
                dto.receive_lines.Add(await _POReceiveModule.MapToLineDto(rec_lines));


            dto.first_po_receive_date = receive_lines.OrderBy(m => m.created_on).Select(m => (DateTime?)m.created_on).FirstOrDefault();

            
            var rec_documents = await _Context.DocumentUploads.Where(m => m.document_object_id == databaseModel.association_object_id).ToListAsync();
            foreach (var doc in rec_documents)
                dto.documents.Add(await _DocumentUploadModule.MapToDto(doc));
        }

        if (databaseModel.association_object_id.HasValue && databaseModel.association_is_sales_order)
        {
            dto.association_number = await _Context.OrderHeaders.Where(m => m.id == dto.association_object_id).Select(m => m.order_number).SingleOrDefaultAsync();

            var order_lines = await _Context.OrderLines.Where(m => m.order_header_id == dto.association_object_id && !m.is_deleted).ToListAsync();

            foreach (var order_line in order_lines)
                dto.order_lines.Add(await _OrderModule.MapToLineDto(order_line));
                
        }

        if (databaseModel.association_object_id.HasValue && databaseModel.association_is_ar_invoice)
        {
            dto.association_number = await _Context.ARInvoiceHeaders.Where(m => m.id == dto.association_object_id).Select(m => m.invoice_number).SingleOrDefaultAsync();

            var ar_lines = await _Context.ARInvoiceLines.Where(m => m.ar_invoice_header_id == dto.association_object_id && !m.is_deleted).ToListAsync();

            foreach (var ar_line in ar_lines)
                dto.ar_lines.Add(await _ARModule.MapToLineDto(ar_line));
        }


        var documents = await _Context.DocumentUploads.Where(m => m.document_object_id == databaseModel.id).ToListAsync();
        foreach (var doc in documents)
            dto.documents.Add(await _DocumentUploadModule.MapToDto(doc));

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
            gl_account = databaseModel.gl_account,
            description = databaseModel.description,
            association_object_id = databaseModel.association_object_id,
            association_object_line_id = databaseModel.association_object_line_id,
            association_is_purchase_order = databaseModel.association_is_purchase_order,
            association_is_sales_order = databaseModel.association_is_sales_order,
            association_is_ar_invoice = databaseModel.association_is_ar_invoice,
            guid = databaseModel.guid,
            id = databaseModel.id,
            created_by = databaseModel.created_by,
            updated_by = databaseModel.updated_by,
            created_on = databaseModel.created_on,
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

        dto.units_ordered = 0;

        dto.units_received = await _Context.PurchaseOrderReceiveLines.Where(m => m.purchase_order_line_id == dto.id).SumAsync(m => m.units_received);

        if (databaseModel.association_object_id.HasValue && databaseModel.association_is_purchase_order)
        {
            dto.units_ordered = await _Context.PurchaseOrderLines.Where(m => m.id == dto.association_object_line_id).SumAsync(m => m.quantity);
        }

        if (databaseModel.association_object_id.HasValue && databaseModel.association_is_sales_order)
        {
            dto.units_ordered = await _Context.OrderLines.Where(m => m.id == dto.association_object_line_id).SumAsync(m => m.quantity);
        }

        if (databaseModel.association_object_id.HasValue && databaseModel.association_is_ar_invoice)
        {
            dto.units_ordered = await _Context.ARInvoiceLines.Where(m => m.id == dto.association_object_line_id).SumAsync(m => m.invoice_qty);
        }

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
            gl_account = databaseModel.gl_account,
            description = databaseModel.description,
            association_object_id = databaseModel.association_object_id,
            association_object_line_id = databaseModel.association_object_line_id,
            guid = databaseModel.guid,
            id = databaseModel.id,
            created_by = databaseModel.created_by,
            updated_by = databaseModel.updated_by,
            created_on = databaseModel.created_on,
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

        dto.units_ordered = 0;
        dto.units_received = 0;

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
        }, createCommand.calling_user_id);
    }

    public APInvoiceLine MapToLineDatabaseModel(APInvoiceLineCreateCommand createCommand, int ap_invoice_header_id, string calling_user_id)
    {
        return CommonDataHelper<APInvoiceLine>.FillCommonFields(new APInvoiceLine()
        {
            ap_invoice_header_id = ap_invoice_header_id,
            line_number = createCommand.line_number,
            line_total = createCommand.line_total,
            qty_invoiced = createCommand.qty_invoiced,
            gl_account = createCommand.gl_account,
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

    /// <summary>
    /// Posts AP Invoice to General Ledger
    /// Debit: Purchases/Expense account (increases expense)
    /// Credit: Accounts Payable (increases liability)
    /// </summary>
    private async Task PostToGeneralLedger(APInvoiceHeader invoice, string callingUserId)
    {
        if (_FinancialTransactionModule == null || _ChartOfAccountModule == null)
        {
            await LogTrace("GL", 
                $"GL posting skipped for AP Invoice {invoice.id} - FinancialTransactionModule or ChartOfAccountModule not available");
            return;
        }

        try
        {
            // Get GL account numbers from KeyValueStore
            var purchasesAccountKV = await _Context.KeyValueStores
                .Where(m => m.module_id == KeyValueIds.GLAccounts && m.key == "gl_account_purchases")
                .SingleOrDefaultAsync();
            
            var accountsPayableKV = await _Context.KeyValueStores
                .Where(m => m.module_id == KeyValueIds.GLAccounts && m.key == "gl_account_accounts_payable")
                .SingleOrDefaultAsync();

            if (purchasesAccountKV == null || accountsPayableKV == null)
            {
                await LogTrace("GL", 
                    $"GL posting skipped for AP Invoice {invoice.id} - GL account settings not found in KeyValueStore");
                return;
            }

            // Look up actual Chart of Account records by account number
            var purchasesAccount = await _ChartOfAccountModule.GetDtoByAccountNumber(purchasesAccountKV.value ?? "5010");
            var accountsPayable = await _ChartOfAccountModule.GetDtoByAccountNumber(accountsPayableKV.value ?? "2010");

            if (!purchasesAccount.Success || purchasesAccount.Data == null || 
                !accountsPayable.Success || accountsPayable.Data == null)
            {
                await LogTrace("GL", 
                    $"GL posting skipped for AP Invoice {invoice.id} - Chart of Account records not found");
                return;
            }

            var transactionDate = invoice.invoice_date;
            var fiscalPeriod = transactionDate.ToString("yyyy-MM");
            var description = $"AP Invoice #{invoice.invoice_number} - Vendor ID: {invoice.vendor_id}";

            // Debit Purchases/Expense (increases expense)
            await _FinancialTransactionModule.RecordTransaction(
                purchasesAccount.Data.id,
                transactionDate,
                FinancialTransactionType.APPost,
                "APInvoice",
                invoice.id,
                invoice.guid,
                invoice.invoice_total,  // Debit amount
                0,                       // Credit amount
                description,
                fiscalPeriod,
                null,
                false,
                callingUserId
            );

            // Credit Accounts Payable (increases liability)
            await _FinancialTransactionModule.RecordTransaction(
                accountsPayable.Data.id,
                transactionDate,
                FinancialTransactionType.APPost,
                "APInvoice",
                invoice.id,
                invoice.guid,
                0,                       // Debit amount
                invoice.invoice_total,   // Credit amount
                description,
                fiscalPeriod,
                null,
                false,
                callingUserId
            );

            // Update invoice posting status
            invoice.is_posted = true;
            invoice.posted_on = DateTime.UtcNow;
            invoice.posted_by = callingUserId;
            await _Context.SaveChangesAsync();

            await LogTrace("GL", 
                $"Successfully posted AP Invoice {invoice.id} to GL");
        }
        catch (Exception ex)
        {
            await LogError(50, this.GetType().Name, nameof(PostToGeneralLedger), ex);
            // Don't throw - invoice was created successfully, GL posting can be retried
        }
    }
}
