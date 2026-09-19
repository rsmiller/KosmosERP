using Microsoft.EntityFrameworkCore;
using KosmosERP.Database;
using KosmosERP.Database.Models;
using KosmosERP.Module;
using KosmosERP.Models;
using KosmosERP.Models.Helpers;
using KosmosERP.Models.Interfaces;
using KosmosERP.BusinessLayer.Models.Module.CreditMemo.Dto;
using KosmosERP.BusinessLayer.Models.Module.CreditMemo.Command.Create;
using KosmosERP.BusinessLayer.Models.Module.CreditMemo.Command.Delete;
using KosmosERP.BusinessLayer.Models.Module.CreditMemo.Command.Edit;
using KosmosERP.BusinessLayer.Models.Module.CreditMemo.Command.Find;
using KosmosERP.BusinessLayer.Helpers;

namespace KosmosERP.BusinessLayer.Modules;

public interface ICreditMemoModule : IERPModule<CreditMemoHeader, CreditMemoHeaderDto, CreditMemoHeaderListDto, CreditMemoHeaderCreateCommand, CreditMemoHeaderEditCommand, CreditMemoHeaderDeleteCommand, CreditMemoHeaderFindCommand>, IBaseERPModule
{
    Task<Response<CreditMemoLineDto>> GetLineDto(int object_id);
    Task<Response<CreditMemoLineDto>> CreateLine(CreditMemoLineCreateCommand commandModel);
    Task<Response<CreditMemoLineDto>> EditLine(CreditMemoLineEditCommand commandModel);
    Task<Response<CreditMemoLineDto>> DeleteLine(CreditMemoLineDeleteCommand commandModel);
}

public class CreditMemoModule : BaseERPModule, ICreditMemoModule
{
    public override Guid ModuleIdentifier => Guid.Parse("30ccc6b9-d81c-457b-a6df-065adf577316");
    public override string ModuleName => "Credit Memos";

    private IBaseERPContext _Context;

    public CreditMemoModule(IBaseERPContext context, ILogProviderFactory logProviderFactory) : base(context, logProviderFactory)
    {
        _Context = context;
    }

    public override void SeedPermissions()
    {
        var role = _Context.Roles.Any(m => m.name == "Credit Memo Administrators");

        if (role == false)
        {
            _Context.Roles.Add(CommonDataHelper<Role>.FillCommonFields(new Role()
            {
                name = "Credit Memo Administrators",
            }, 1));

            _Context.SaveChanges();

            base.CreateFirstRunRolePermissions();
        }
    }

    public CreditMemoHeader? Get(int object_id)
    {
        return _Context.CreditMemoHeaders.SingleOrDefault(m => m.id == object_id);
    }

    public CreditMemoLine? GetLine(int object_id)
    {
        return _Context.CreditMemoLines.SingleOrDefault(m => m.id == object_id);
    }

    public async Task<CreditMemoHeader?> GetAsync(int object_id)
    {
        return await _Context.CreditMemoHeaders.SingleOrDefaultAsync(m => m.id == object_id);
    }

    public async Task<CreditMemoLine?> GetLineAsync(int object_id)
    {
        return await _Context.CreditMemoLines.SingleOrDefaultAsync(m => m.id == object_id);
    }

    public async Task<Response<CreditMemoHeaderDto>> GetDto(int object_id)
    {
        Response<CreditMemoHeaderDto> response = new Response<CreditMemoHeaderDto>();

        var result = await _Context.CreditMemoHeaders.SingleOrDefaultAsync(m => m.id == object_id);
        if (result == null)
        {
            response.SetException("Credit Memo Header not found", ResultCode.NotFound);
            return response;
        }

        response.Data = await this.MapToDto(result);
        return response;
    }

    public async Task<Response<CreditMemoHeaderDto>> GetDtoByGuid(string guid)
    {
        var entity = await _Context.CreditMemoHeaders.FirstOrDefaultAsync(c => c.guid == guid && !c.is_deleted);
        if (entity == null)
            return new Response<CreditMemoHeaderDto>("CreditMemoHeader not found", ResultCode.NotFound);

        var dto = await MapToDto(entity);
        return new Response<CreditMemoHeaderDto>(dto);
    }
    
    public async Task<Response<CreditMemoLineDto>> GetLineDto(int object_id)
    {
        Response<CreditMemoLineDto> response = new Response<CreditMemoLineDto>();

        var result = await _Context.CreditMemoLines.SingleOrDefaultAsync(m => m.id == object_id);
        if (result == null)
        {
            response.SetException("Credit Memo Line not found", ResultCode.NotFound);
            return response;
        }

        response.Data = await this.MapToLineDto(result);
        return response;
    }

    public async Task<Response<CreditMemoHeaderDto>> Create(CreditMemoHeaderCreateCommand commandModel)
    {
        if (commandModel == null)
            return new Response<CreditMemoHeaderDto>(ResultCode.NullItemInput);

        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<CreditMemoHeaderDto>(validationResult.Exception, ResultCode.DataValidationError);
        try
        {
            var alreadyExists = CreditMemoHeaderExists(commandModel);
            if (alreadyExists == true)
                return new Response<CreditMemoHeaderDto>(ResultCode.AlreadyExists);

            var item = MapToDatabaseModel(commandModel);

            item.credit_memo_number = await this.ManuallyGenerateAnCreditMemoNumber();

            item.credit_memo_total = commandModel.credit_memo_lines.Sum(m => m.line_total);

            await _Context.CreditMemoHeaders.AddAsync(item);
            await _Context.SaveChangesAsync();

            // Now do lines
            foreach(var credit_line in commandModel.credit_memo_lines)
            {
                var db_line = MapToLineDatabaseModel(credit_line, item.id, commandModel.calling_user_id);

                await _Context.CreditMemoLines.AddAsync(db_line);
                await _Context.SaveChangesAsync();
            }

            var dto = await GetDto(item.id);

            return new Response<CreditMemoHeaderDto>(dto.Data);
        }
        catch (Exception ex)
        {
            return new Response<CreditMemoHeaderDto>(ex.Message, ResultCode.Error);
        }
    }

    public async Task<Response<CreditMemoLineDto>> CreateLine(CreditMemoLineCreateCommand commandModel)
    {
        if (commandModel == null)
            return new Response<CreditMemoLineDto>(ResultCode.NullItemInput);

        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<CreditMemoLineDto>(validationResult.Exception, ResultCode.DataValidationError);

        if(!commandModel.credit_memo_header_id.HasValue)
            return new Response<CreditMemoLineDto>("Credit Memo Header is a required field", ResultCode.DataValidationError);

        try
        {
            var item = MapToLineDatabaseModel(commandModel, commandModel.credit_memo_header_id.Value, commandModel.calling_user_id);

            await _Context.CreditMemoLines.AddAsync(item);
            await _Context.SaveChangesAsync();

            var dto = await GetLineDto(item.id);

            return new Response<CreditMemoLineDto>(dto.Data);
        }
        catch (Exception ex)
        {
            return new Response<CreditMemoLineDto>(ex.Message, ResultCode.Error);
        }
    }

    public async Task<Response<CreditMemoHeaderDto>> Edit(CreditMemoHeaderEditCommand commandModel)
    {
        if (commandModel == null)
            return new Response<CreditMemoHeaderDto>(ResultCode.NullItemInput);

        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<CreditMemoHeaderDto>(validationResult.Exception, ResultCode.DataValidationError);

        var existingEntity = await GetAsync(commandModel.id);
        if (existingEntity == null)
            return new Response<CreditMemoHeaderDto>("Credit Memo Header not found", ResultCode.NotFound);

        if (existingEntity.customer_id != commandModel.customer_id && commandModel.customer_id.HasValue)
            existingEntity.customer_id = commandModel.customer_id.Value;

        if (existingEntity.credit_memo_date != commandModel.credit_memo_date && commandModel.credit_memo_date.HasValue)
            existingEntity.credit_memo_date = commandModel.credit_memo_date.Value;

        if (existingEntity.credit_memo_due_date != commandModel.credit_memo_due_date && commandModel.credit_memo_due_date.HasValue)
            existingEntity.credit_memo_due_date = commandModel.credit_memo_due_date.Value;

        if (existingEntity.credit_memo_total != commandModel.credit_memo_total && commandModel.credit_memo_total.HasValue)
            existingEntity.credit_memo_total = commandModel.credit_memo_total.Value;

        if (existingEntity.memo != commandModel.memo)
            existingEntity.memo = commandModel.memo;

        if (commandModel.ar_invoice_header_id.HasValue)
            existingEntity.ar_invoice_header_id = commandModel.ar_invoice_header_id.Value;

        if (commandModel.order_header_id.HasValue)
            existingEntity.order_header_id = commandModel.order_header_id.Value;

        if (commandModel.credit_reason != null)
            existingEntity.credit_reason = commandModel.credit_reason;

        if (commandModel.is_approved.HasValue)
            existingEntity.is_approved = commandModel.is_approved.Value;

        if (commandModel.is_applied.HasValue)
            existingEntity.is_applied = commandModel.is_applied.Value;

        existingEntity = CommonDataHelper<CreditMemoHeader>.FillUpdateFields(existingEntity, commandModel.calling_user_id);

        _Context.CreditMemoHeaders.Update(existingEntity);
        await _Context.SaveChangesAsync();

        // Process the lines
        foreach (var line in commandModel.credit_memo_lines)
        {
            if(line.id.HasValue)
            {
                await this.EditLine(line);
            }
            else
            {
                await this.CreateLine(new CreditMemoLineCreateCommand()
                {
                    calling_user_id = commandModel.calling_user_id,
                    credit_memo_header_id = existingEntity.id,
                    line_number = line.line_number.Value,
                    line_total = line.line_total.Value,
                    qty_credited = line.qty_credited.Value,
                    gl_account_id = line.gl_account_id,
                    description = line.description,
                    product_id = line.product_id,
                    ar_invoice_line_id = line.ar_invoice_line_id,
                    order_line_id = line.order_line_id,
                });
            }
        }

        var dto = await MapToDto(existingEntity);
        return new Response<CreditMemoHeaderDto>(dto);
    }

    public async Task<Response<CreditMemoLineDto>> EditLine(CreditMemoLineEditCommand commandModel)
    {
        if (commandModel == null)
            return new Response<CreditMemoLineDto>(ResultCode.NullItemInput);

        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<CreditMemoLineDto>(validationResult.Exception, ResultCode.DataValidationError);

        var existingEntity = await GetLineAsync(commandModel.id.Value);
        if (existingEntity == null)
            return new Response<CreditMemoLineDto>("Credit Memo Line not found", ResultCode.NotFound);

        if (existingEntity.line_number != commandModel.line_number && commandModel.line_number.HasValue)
            existingEntity.line_number = commandModel.line_number.Value;

        if (existingEntity.line_total != commandModel.line_total)
            existingEntity.line_total = commandModel.line_total.Value;

        if (existingEntity.qty_credited != commandModel.qty_credited && commandModel.qty_credited.HasValue)
            existingEntity.qty_credited = commandModel.qty_credited.Value;

        if (!String.IsNullOrEmpty(commandModel.gl_account_id) && existingEntity.gl_account_id != commandModel.gl_account_id)
            existingEntity.gl_account_id = commandModel.gl_account_id;

        if (existingEntity.description != commandModel.description)
            existingEntity.description = commandModel.description;

        if (commandModel.product_id.HasValue)
            existingEntity.product_id = commandModel.product_id.Value;

        if (commandModel.ar_invoice_line_id.HasValue)
            existingEntity.ar_invoice_line_id = commandModel.ar_invoice_line_id.Value;

        if (commandModel.order_line_id.HasValue)
            existingEntity.order_line_id = commandModel.order_line_id.Value;

        existingEntity = CommonDataHelper<CreditMemoLine>.FillUpdateFields(existingEntity, commandModel.calling_user_id);

        _Context.CreditMemoLines.Update(existingEntity);
        await _Context.SaveChangesAsync();

        var dto = await MapToLineDto(existingEntity);
        return new Response<CreditMemoLineDto>(dto);
    }

    public async Task<Response<CreditMemoHeaderDto>> Delete(CreditMemoHeaderDeleteCommand commandModel)
    {
        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<CreditMemoHeaderDto>(validationResult.Exception, ResultCode.DataValidationError);

        var existingEntity = await GetAsync(commandModel.id);
        if (existingEntity == null)
            return new Response<CreditMemoHeaderDto>("Credit Memo Header not found", ResultCode.NotFound);

        // Soft Delete
        existingEntity = CommonDataHelper<CreditMemoHeader>.FillDeleteFields(existingEntity, commandModel.calling_user_id);

        _Context.CreditMemoHeaders.Update(existingEntity);
        await _Context.SaveChangesAsync();

        // Delete lines
        var lines = await _Context.CreditMemoLines.Where(m => m.credit_memo_header_id == existingEntity.id).ToListAsync();
        foreach(var line in lines)
        {
            await this.DeleteLine(new CreditMemoLineDeleteCommand()
            {
                calling_user_id = commandModel.calling_user_id,
                id = line.id,
            });
        }

        var dto = await MapToDto(existingEntity);
        return new Response<CreditMemoHeaderDto>(dto);
    }

    public async Task<Response<CreditMemoLineDto>> DeleteLine(CreditMemoLineDeleteCommand commandModel)
    {
        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<CreditMemoLineDto>(validationResult.Exception, ResultCode.DataValidationError);

        var existingEntity = await GetLineAsync(commandModel.id);
        if (existingEntity == null)
            return new Response<CreditMemoLineDto>("Credit Memo Line not found", ResultCode.NotFound);

        // Soft Delete
        existingEntity = CommonDataHelper<CreditMemoLine>.FillDeleteFields(existingEntity, commandModel.calling_user_id);

        _Context.CreditMemoLines.Update(existingEntity);
        await _Context.SaveChangesAsync();

        var dto = await MapToLineDto(existingEntity);
        return new Response<CreditMemoLineDto>(dto);
    }

    public async Task<PagingResult<CreditMemoHeaderListDto>> Find(PagingSortingParameters parameters, CreditMemoHeaderFindCommand commandModel)
    {
        var response = new PagingResult<CreditMemoHeaderListDto>();

        try
        {
            var query = _Context.CreditMemoHeaders
                .Where(m => !m.is_deleted);

            decimal parsed_num = 0;

            if (decimal.TryParse(commandModel.wildcard, out parsed_num))
            {
                var wild = commandModel.wildcard.ToLower();
                query = query.Where(m =>
                    m.credit_memo_total == parsed_num || m.credit_memo_number == parsed_num
                );
            }
            else if (!string.IsNullOrEmpty(commandModel.wildcard))
            {
                var wild = commandModel.wildcard.ToLower();
                query = query.Where(m =>
                    m.memo.ToLower().Contains(wild)
                    || m.credit_reason.ToLower().Contains(wild)
                );
            }

            var totalCount = await query.CountAsync();
            var pagedItems = await query.SortAndPageBy(parameters).ToListAsync();

            var dtos = new List<CreditMemoHeaderListDto>();
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

    public async Task<Response<List<CreditMemoHeaderListDto>>> GlobalSearch(GlobalSearchFindCommand commandModel)
    {
        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<List<CreditMemoHeaderListDto>>(validationResult.Exception, ResultCode.DataValidationError);


        var response = new Response<List<CreditMemoHeaderListDto>>();

        try
        {
            var filter = PredicateBuilder.True<CreditMemoHeader>();
            filter = filter.And(m => m.is_deleted == false);

            int parsed_num = 0;

            if (int.TryParse(commandModel.wildcard, out parsed_num))
            {
                filter = filter.And(m => m.credit_memo_number == parsed_num);
            }
            else
            {
                var lower = commandModel.wildcard.ToLower();
                filter = filter.And(m =>
                    m.credit_reason.ToLower().Contains(lower)
                    || m.memo.ToLower().Contains(lower)
                );
            }

            var results = _Context.CreditMemoHeaders.Where(filter);
            var pagedItems = await results.SortAndPageBy(commandModel.parameters).ToListAsync();

            var dtos = new List<CreditMemoHeaderListDto>();
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

    public async Task<CreditMemoHeaderListDto> MapToListDto(CreditMemoHeader databaseModel)
    {
        var dto = new CreditMemoHeaderListDto()
        {
            customer_id = databaseModel.customer_id,
            credit_memo_number = databaseModel.credit_memo_number,
            credit_memo_date = databaseModel.credit_memo_date,
            credit_memo_due_date = databaseModel.credit_memo_due_date,
            credit_memo_total = databaseModel.credit_memo_total,
            ar_invoice_header_id = databaseModel.ar_invoice_header_id,
            order_header_id = databaseModel.order_header_id,
            memo = databaseModel.memo,
            credit_reason = databaseModel.credit_reason,
            is_approved = databaseModel.is_approved,
            is_applied = databaseModel.is_applied,
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

        dto.customer_name = await _Context.Customers.Where(m => m.id == databaseModel.customer_id).Select(m => m.customer_name).SingleOrDefaultAsync();

        return dto;
    }

    public async Task<CreditMemoHeaderDto> MapToDto(CreditMemoHeader databaseModel)
    {
        var dto = new CreditMemoHeaderDto()
        {
            customer_id = databaseModel.customer_id,
            credit_memo_number = databaseModel.credit_memo_number,
            credit_memo_date = databaseModel.credit_memo_date,
            credit_memo_due_date = databaseModel.credit_memo_due_date,
            credit_memo_total = databaseModel.credit_memo_total,
            memo = databaseModel.memo,
            ar_invoice_header_id = databaseModel.ar_invoice_header_id,
            order_header_id = databaseModel.order_header_id,
            credit_reason = databaseModel.credit_reason,
            is_approved = databaseModel.is_approved,
            is_applied = databaseModel.is_applied,
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

        dto.customer_name = await _Context.Customers.Where(m => m.id == databaseModel.customer_id).Select(m => m.customer_name).SingleOrDefaultAsync();

        if (databaseModel.ar_invoice_header_id.HasValue)
        {
            dto.ar_invoice_number = await _Context.ARInvoiceHeaders.Where(m => m.id == databaseModel.ar_invoice_header_id.Value).Select(m => m.invoice_number.ToString()).SingleOrDefaultAsync();
        }

        if (databaseModel.order_header_id.HasValue)
        {
            dto.order_number = await _Context.OrderHeaders.Where(m => m.id == databaseModel.order_header_id.Value).Select(m => m.order_number.ToString()).SingleOrDefaultAsync();
        }

        var lines = await _Context.CreditMemoLines
            .Where(m => m.credit_memo_header_id == databaseModel.id && !m.is_deleted)
            .ToListAsync();

        foreach(var credit_line in lines)
            dto.credit_memo_lines.Add(await MapToLineDto(credit_line));

        return dto;
    }

    public async Task<CreditMemoLineDto> MapToLineDto(CreditMemoLine databaseModel)
    {
        var dto = new CreditMemoLineDto()
        {
            credit_memo_header_id = databaseModel.credit_memo_header_id,
            line_number = databaseModel.line_number,
            line_total = databaseModel.line_total,
            qty_credited = databaseModel.qty_credited,
            gl_account_id = databaseModel.gl_account_id,
            description = databaseModel.description,
            product_id = databaseModel.product_id,
            ar_invoice_line_id = databaseModel.ar_invoice_line_id,
            order_line_id = databaseModel.order_line_id,
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

        if (databaseModel.product_id.HasValue)
        {
            var product = await _Context.Products.Where(m => m.id == databaseModel.product_id.Value).SingleOrDefaultAsync();
            if (product != null)
            {
                dto.product_name = product.product_name;
                dto.product_sku = product.identifier1;
            }
        }

        return dto;
    }

    public CreditMemoHeader MapToDatabaseModel(CreditMemoHeaderCreateCommand createCommand)
    {
        return CommonDataHelper<CreditMemoHeader>.FillCommonFields(new CreditMemoHeader()
        {
            customer_id = createCommand.customer_id,
            credit_memo_date = createCommand.credit_memo_date,
            credit_memo_due_date = createCommand.credit_memo_due_date,
            credit_memo_total = createCommand.credit_memo_total,
            memo = createCommand.memo,
            ar_invoice_header_id = createCommand.ar_invoice_header_id,
            order_header_id = createCommand.order_header_id,
            credit_reason = createCommand.credit_reason,
            is_approved = createCommand.is_approved,
            is_applied = createCommand.is_applied,
            guid = Guid.NewGuid().ToString(),
        }, createCommand.calling_user_id);
    }

    public CreditMemoLine MapToLineDatabaseModel(CreditMemoLineCreateCommand createCommand, int credit_memo_header_id, string calling_user_id)
    {
        return CommonDataHelper<CreditMemoLine>.FillCommonFields(new CreditMemoLine()
        {
            credit_memo_header_id = credit_memo_header_id,
            line_number = createCommand.line_number,
            line_total = createCommand.line_total,
            qty_credited = createCommand.qty_credited,
            gl_account_id = createCommand.gl_account_id,
            description = createCommand.description,
            product_id = createCommand.product_id,
            ar_invoice_line_id = createCommand.ar_invoice_line_id,
            order_line_id = createCommand.order_line_id,
            guid = Guid.NewGuid().ToString(),
            is_deleted = false
        }, calling_user_id);
    }

    private bool CreditMemoHeaderExists(CreditMemoHeaderCreateCommand createCommand)
    {
       return false;
    }

    public CreditMemoHeader MapToDatabaseModel(CreditMemoHeaderDto dtoModel)
    {
        throw new NotImplementedException();
    }

    private async Task<int> ManuallyGenerateAnCreditMemoNumber()
    {
        var total_records = await _Context.OrderHeaders.CountAsync();
        int start = DatabaseStartNumbers.CreditMemos;

        return (total_records + start + 1);
    }
} 