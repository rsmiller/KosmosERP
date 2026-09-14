using Microsoft.EntityFrameworkCore;
using KosmosERP.Database;
using KosmosERP.Database.Models;
using KosmosERP.Module;
using KosmosERP.Models;
using KosmosERP.Models.Helpers;
using KosmosERP.Models.Interfaces;
using KosmosERP.BusinessLayer.Models.Module.JournalEntry.Dto;
using KosmosERP.BusinessLayer.Models.Module.JournalEntry.Command;
using KosmosERP.BusinessLayer.Models.Module.JournalEntry.Command.Create;
using KosmosERP.BusinessLayer.Models.Module.JournalEntry.Command.Delete;
using KosmosERP.BusinessLayer.Models.Module.JournalEntry.Command.Edit;
using KosmosERP.BusinessLayer.Models.Module.JournalEntry.Command.Find;
using KosmosERP.BusinessLayer.Helpers;

namespace KosmosERP.BusinessLayer.Modules;

public interface IJournalEntryModule : IERPModule<JournalEntryHeader, JournalEntryHeaderDto, JournalEntryHeaderListDto, JournalEntryHeaderCreateCommand, JournalEntryHeaderEditCommand, JournalEntryHeaderDeleteCommand, JournalEntryHeaderFindCommand>, IBaseERPModule
{
    Task<Response<JournalEntryLineDto>> GetLineDto(int object_id);
    Task<Response<JournalEntryLineDto>> CreateLine(JournalEntryLineCreateCommand commandModel);
    Task<Response<JournalEntryLineDto>> EditLine(JournalEntryLineEditCommand commandModel);
    Task<Response<JournalEntryLineDto>> DeleteLine(JournalEntryLineDeleteCommand commandModel);
    Task<Response<JournalEntryHeaderDto>> Post(JournalEntryPostCommand commandModel);
    Task<Response<JournalEntryHeaderDto>> Reverse(JournalEntryReverseCommand commandModel);
    Task<Response<bool>> ValidateBalance(int headerId);
    Task<PagingResult<JournalEntryHeaderListDto>> GetUnpostedEntries(PagingSortingParameters parameters);
}

public class JournalEntryModule : BaseERPModule, IJournalEntryModule
{
    public override Guid ModuleIdentifier => Guid.Parse("b2c3d4e5-f6a7-8901-bcde-f23456789012");
    public override string ModuleName => "Journal Entries";

    private IBaseERPContext _Context;
    private IFinancialTransactionModule? _FinancialTransactionModule;

    public JournalEntryModule(IBaseERPContext context, ILogProviderFactory logProviderFactory) : base(logProviderFactory)
    {
        _Context = context;
    }

    public JournalEntryModule(IBaseERPContext context, IFinancialTransactionModule financialTransactionModule, ILogProviderFactory logProviderFactory) : base(logProviderFactory)
    {
        _Context = context;
        _FinancialTransactionModule = financialTransactionModule;
    }

    public override void SeedPermissions()
    {
        var role = _Context.Roles.Any(m => m.name == "Journal Entry Administrators");

        if (role == false)
        {
            _Context.Roles.Add(CommonDataHelper<Role>.FillCommonFields(new Role()
            {
                name = "Journal Entry Administrators",
            }, 1));

            _Context.SaveChanges();
        }
    }

    public JournalEntryHeader? Get(int object_id)
    {
        return _Context.JournalEntryHeaders.SingleOrDefault(m => m.id == object_id);
    }

    public JournalEntryLine? GetLine(int object_id)
    {
        return _Context.JournalEntryLines.SingleOrDefault(m => m.id == object_id);
    }

    public async Task<JournalEntryHeader?> GetAsync(int object_id)
    {
        return await _Context.JournalEntryHeaders.SingleOrDefaultAsync(m => m.id == object_id);
    }

    public async Task<JournalEntryLine?> GetLineAsync(int object_id)
    {
        return await _Context.JournalEntryLines.SingleOrDefaultAsync(m => m.id == object_id);
    }

    public async Task<Response<JournalEntryHeaderDto>> GetDto(int object_id)
    {
        Response<JournalEntryHeaderDto> response = new Response<JournalEntryHeaderDto>();

        var result = await _Context.JournalEntryHeaders.SingleOrDefaultAsync(m => m.id == object_id);
        if (result == null)
        {
            response.SetException("Journal Entry not found", ResultCode.NotFound);
            return response;
        }

        response.Data = await this.MapToDto(result);
        return response;
    }

    public async Task<Response<JournalEntryHeaderDto>> GetDtoByGuid(string guid)
    {
        var entity = await _Context.JournalEntryHeaders.FirstOrDefaultAsync(c => c.guid == guid && !c.is_deleted);
        if (entity == null)
            return new Response<JournalEntryHeaderDto>("Journal Entry not found", ResultCode.NotFound);

        var dto = await MapToDto(entity);
        return new Response<JournalEntryHeaderDto>(dto);
    }

    public async Task<Response<JournalEntryLineDto>> GetLineDto(int object_id)
    {
        Response<JournalEntryLineDto> response = new Response<JournalEntryLineDto>();

        var result = await _Context.JournalEntryLines.SingleOrDefaultAsync(m => m.id == object_id);
        if (result == null)
        {
            response.SetException("Journal Entry Line not found", ResultCode.NotFound);
            return response;
        }

        response.Data = await this.MapToLineDto(result);
        return response;
    }

    public async Task<Response<JournalEntryHeaderDto>> Create(JournalEntryHeaderCreateCommand commandModel)
    {
        if (commandModel == null)
            return new Response<JournalEntryHeaderDto>(ResultCode.NullItemInput);

        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<JournalEntryHeaderDto>(validationResult.Exception, ResultCode.DataValidationError);

        try
        {
            var item = MapToDatabaseModel(commandModel);
            item.entry_number = await GenerateEntryNumber();

            await _Context.JournalEntryHeaders.AddAsync(item);
            await _Context.SaveChangesAsync();

            // Create lines
            foreach (var line in commandModel.journal_entry_lines)
            {
                var dbLine = MapToLineDatabaseModel(line, item.id, commandModel.calling_user_id);
                await _Context.JournalEntryLines.AddAsync(dbLine);
                await _Context.SaveChangesAsync();
            }

            var dto = await GetDto(item.id);
            return new Response<JournalEntryHeaderDto>(dto.Data!);
        }
        catch (Exception ex)
        {
            return new Response<JournalEntryHeaderDto>(ex.Message, ResultCode.Error);
        }
    }

    public async Task<Response<JournalEntryLineDto>> CreateLine(JournalEntryLineCreateCommand commandModel)
    {
        if (commandModel == null)
            return new Response<JournalEntryLineDto>(ResultCode.NullItemInput);

        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<JournalEntryLineDto>(validationResult.Exception, ResultCode.DataValidationError);

        if (!commandModel.journal_entry_header_id.HasValue)
            return new Response<JournalEntryLineDto>("Journal Entry Header is required", ResultCode.DataValidationError);

        // Check if header is already posted
        var header = await GetAsync(commandModel.journal_entry_header_id.Value);
        if (header != null && header.is_posted)
            return new Response<JournalEntryLineDto>("Cannot add lines to a posted journal entry", ResultCode.Invalid);

        try
        {
            var item = MapToLineDatabaseModel(commandModel, commandModel.journal_entry_header_id.Value, commandModel.calling_user_id);

            await _Context.JournalEntryLines.AddAsync(item);
            await _Context.SaveChangesAsync();

            var dto = await GetLineDto(item.id);
            return new Response<JournalEntryLineDto>(dto.Data!);
        }
        catch (Exception ex)
        {
            return new Response<JournalEntryLineDto>(ex.Message, ResultCode.Error);
        }
    }

    public async Task<Response<JournalEntryHeaderDto>> Edit(JournalEntryHeaderEditCommand commandModel)
    {
        if (commandModel == null)
            return new Response<JournalEntryHeaderDto>(ResultCode.NullItemInput);

        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<JournalEntryHeaderDto>(validationResult.Exception, ResultCode.DataValidationError);

        var existingEntity = await GetAsync(commandModel.id);
        if (existingEntity == null)
            return new Response<JournalEntryHeaderDto>("Journal Entry not found", ResultCode.NotFound);

        if (existingEntity.is_posted)
            return new Response<JournalEntryHeaderDto>("Cannot edit a posted journal entry", ResultCode.Invalid);

        if (commandModel.entry_date.HasValue)
            existingEntity.entry_date = commandModel.entry_date.Value;

        if (commandModel.description != null)
            existingEntity.description = commandModel.description;

        if (commandModel.reference_type.HasValue)
            existingEntity.reference_type = commandModel.reference_type.Value;

        if (commandModel.reference_id.HasValue)
            existingEntity.reference_id = commandModel.reference_id.Value;

        if (commandModel.fiscal_period != null)
            existingEntity.fiscal_period = commandModel.fiscal_period;

        existingEntity = CommonDataHelper<JournalEntryHeader>.FillUpdateFields(existingEntity, commandModel.calling_user_id);

        _Context.JournalEntryHeaders.Update(existingEntity);
        await _Context.SaveChangesAsync();

        // Edit existing lines
        foreach (var line in commandModel.journal_entry_lines)
        {
            await EditLine(line);
        }

        // Create new lines
        foreach (var line in commandModel.journal_entry_lines_create)
        {
            line.journal_entry_header_id = existingEntity.id;
            await CreateLine(line);
        }

        var dto = await MapToDto(existingEntity);
        return new Response<JournalEntryHeaderDto>(dto);
    }

    public async Task<Response<JournalEntryLineDto>> EditLine(JournalEntryLineEditCommand commandModel)
    {
        if (commandModel == null)
            return new Response<JournalEntryLineDto>(ResultCode.NullItemInput);

        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<JournalEntryLineDto>(validationResult.Exception, ResultCode.DataValidationError);

        var existingEntity = await GetLineAsync(commandModel.id);
        if (existingEntity == null)
            return new Response<JournalEntryLineDto>("Journal Entry Line not found", ResultCode.NotFound);

        // Check if parent header is posted
        var header = await GetAsync(existingEntity.journal_entry_header_id);
        if (header != null && header.is_posted)
            return new Response<JournalEntryLineDto>("Cannot edit lines of a posted journal entry", ResultCode.Invalid);

        if (commandModel.line_number.HasValue)
            existingEntity.line_number = commandModel.line_number.Value;

        if (commandModel.chart_of_account_id.HasValue)
            existingEntity.chart_of_account_id = commandModel.chart_of_account_id.Value;

        if (commandModel.debit_amount.HasValue)
            existingEntity.debit_amount = commandModel.debit_amount.Value;

        if (commandModel.credit_amount.HasValue)
            existingEntity.credit_amount = commandModel.credit_amount.Value;

        if (commandModel.description != null)
            existingEntity.description = commandModel.description;

        existingEntity = CommonDataHelper<JournalEntryLine>.FillUpdateFields(existingEntity, commandModel.calling_user_id);

        _Context.JournalEntryLines.Update(existingEntity);
        await _Context.SaveChangesAsync();

        var dto = await MapToLineDto(existingEntity);
        return new Response<JournalEntryLineDto>(dto);
    }

    public async Task<Response<JournalEntryHeaderDto>> Delete(JournalEntryHeaderDeleteCommand commandModel)
    {
        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<JournalEntryHeaderDto>(validationResult.Exception, ResultCode.DataValidationError);

        var existingEntity = await GetAsync(commandModel.id);
        if (existingEntity == null)
            return new Response<JournalEntryHeaderDto>("Journal Entry not found", ResultCode.NotFound);

        if (existingEntity.is_posted)
            return new Response<JournalEntryHeaderDto>("Cannot delete a posted journal entry. Use reverse instead.", ResultCode.Invalid);

        // Soft Delete
        existingEntity = CommonDataHelper<JournalEntryHeader>.FillDeleteFields(existingEntity, commandModel.calling_user_id);

        _Context.JournalEntryHeaders.Update(existingEntity);
        await _Context.SaveChangesAsync();

        // Delete lines
        var lines = await _Context.JournalEntryLines.Where(m => m.journal_entry_header_id == existingEntity.id).ToListAsync();
        foreach (var line in lines)
        {
            await DeleteLine(new JournalEntryLineDeleteCommand()
            {
                calling_user_id = commandModel.calling_user_id,
                id = line.id,
            });
        }

        var dto = await MapToDto(existingEntity);
        return new Response<JournalEntryHeaderDto>(dto);
    }

    public async Task<Response<JournalEntryLineDto>> DeleteLine(JournalEntryLineDeleteCommand commandModel)
    {
        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<JournalEntryLineDto>(validationResult.Exception, ResultCode.DataValidationError);

        var existingEntity = await GetLineAsync(commandModel.id);
        if (existingEntity == null)
            return new Response<JournalEntryLineDto>("Journal Entry Line not found", ResultCode.NotFound);

        // Check if parent header is posted
        var header = await GetAsync(existingEntity.journal_entry_header_id);
        if (header != null && header.is_posted)
            return new Response<JournalEntryLineDto>("Cannot delete lines of a posted journal entry", ResultCode.Invalid);

        // Soft Delete
        existingEntity = CommonDataHelper<JournalEntryLine>.FillDeleteFields(existingEntity, commandModel.calling_user_id);

        _Context.JournalEntryLines.Update(existingEntity);
        await _Context.SaveChangesAsync();

        var dto = await MapToLineDto(existingEntity);
        return new Response<JournalEntryLineDto>(dto);
    }

    public async Task<Response<bool>> ValidateBalance(int headerId)
    {
        var lines = await _Context.JournalEntryLines
            .Where(m => m.journal_entry_header_id == headerId && !m.is_deleted)
            .ToListAsync();

        var totalDebits = lines.Sum(l => l.debit_amount);
        var totalCredits = lines.Sum(l => l.credit_amount);

        var isBalanced = totalDebits == totalCredits;

        return new Response<bool>(isBalanced);
    }

    public async Task<Response<JournalEntryHeaderDto>> Post(JournalEntryPostCommand commandModel)
    {
        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<JournalEntryHeaderDto>(validationResult.Exception, ResultCode.DataValidationError);

        var existingEntity = await GetAsync(commandModel.id);
        if (existingEntity == null)
            return new Response<JournalEntryHeaderDto>("Journal Entry not found", ResultCode.NotFound);

        if (existingEntity.is_posted)
            return new Response<JournalEntryHeaderDto>("Journal Entry is already posted", ResultCode.Invalid);

        // Validate balance
        var balanceResult = await ValidateBalance(commandModel.id);
        if (!balanceResult.Data)
            return new Response<JournalEntryHeaderDto>("Journal Entry is not balanced. Total debits must equal total credits.", ResultCode.Invalid);

        // Check that entry has lines
        var hasLines = await _Context.JournalEntryLines.AnyAsync(m => m.journal_entry_header_id == commandModel.id && !m.is_deleted);
        if (!hasLines)
            return new Response<JournalEntryHeaderDto>("Journal Entry must have at least one line", ResultCode.Invalid);

        // Post the entry
        existingEntity.is_posted = true;
        existingEntity.posted_on = DateTime.UtcNow;
        existingEntity.posted_by = commandModel.calling_user_id;

        existingEntity = CommonDataHelper<JournalEntryHeader>.FillUpdateFields(existingEntity, commandModel.calling_user_id);

        _Context.JournalEntryHeaders.Update(existingEntity);
        await _Context.SaveChangesAsync();

        // Create financial transactions for each line
        if (_FinancialTransactionModule != null)
        {
            var lines = await _Context.JournalEntryLines
                .Where(m => m.journal_entry_header_id == existingEntity.id && !m.is_deleted)
                .ToListAsync();

            foreach (var line in lines)
            {
                await _FinancialTransactionModule.RecordTransaction(
                    line.chart_of_account_id,
                    existingEntity.entry_date,
                    FinancialTransactionType.JournalEntry,
                    "JournalEntry",
                    existingEntity.id,
                    existingEntity.guid,
                    line.debit_amount,
                    line.credit_amount,
                    line.description ?? existingEntity.description,
                    existingEntity.fiscal_period,
                    existingEntity.id,
                    false,
                    commandModel.calling_user_id
                );
            }
        }

        var dto = await MapToDto(existingEntity);
        return new Response<JournalEntryHeaderDto>(dto);
    }

    public async Task<Response<JournalEntryHeaderDto>> Reverse(JournalEntryReverseCommand commandModel)
    {
        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<JournalEntryHeaderDto>(validationResult.Exception, ResultCode.DataValidationError);

        var existingEntity = await GetAsync(commandModel.id);
        if (existingEntity == null)
            return new Response<JournalEntryHeaderDto>("Journal Entry not found", ResultCode.NotFound);

        if (!existingEntity.is_posted)
            return new Response<JournalEntryHeaderDto>("Can only reverse posted journal entries", ResultCode.Invalid);

        if (existingEntity.is_reversed)
            return new Response<JournalEntryHeaderDto>("Journal Entry is already reversed", ResultCode.Invalid);

        // Create reversing entry
        var reversingEntry = new JournalEntryHeader
        {
            entry_number = await GenerateEntryNumber(),
            entry_date = commandModel.reversal_date,
            description = commandModel.description ?? $"Reversal of JE #{existingEntity.entry_number}",
            reference_type = existingEntity.reference_type,
            reference_id = existingEntity.reference_id,
            is_posted = true,
            posted_on = DateTime.UtcNow,
            posted_by = commandModel.calling_user_id,
            fiscal_period = commandModel.reversal_date.ToString("yyyy-MM"),
            guid = Guid.NewGuid().ToString(),
        };

        reversingEntry = CommonDataHelper<JournalEntryHeader>.FillCommonFields(reversingEntry, commandModel.calling_user_id);

        await _Context.JournalEntryHeaders.AddAsync(reversingEntry);
        await _Context.SaveChangesAsync();

        // Create reversing lines (swap debits and credits)
        var originalLines = await _Context.JournalEntryLines
            .Where(m => m.journal_entry_header_id == existingEntity.id && !m.is_deleted)
            .ToListAsync();

        foreach (var line in originalLines)
        {
            var reversingLine = new JournalEntryLine
            {
                journal_entry_header_id = reversingEntry.id,
                line_number = line.line_number,
                chart_of_account_id = line.chart_of_account_id,
                debit_amount = line.credit_amount, // Swap
                credit_amount = line.debit_amount, // Swap
                description = $"Reversal: {line.description}",
                guid = Guid.NewGuid().ToString(),
            };

            reversingLine = CommonDataHelper<JournalEntryLine>.FillCommonFields(reversingLine, commandModel.calling_user_id);

            await _Context.JournalEntryLines.AddAsync(reversingLine);
            await _Context.SaveChangesAsync();

            // Create financial transaction for reversing line
            if (_FinancialTransactionModule != null)
            {
                await _FinancialTransactionModule.RecordTransaction(
                    reversingLine.chart_of_account_id,
                    commandModel.reversal_date,
                    FinancialTransactionType.JournalEntry,
                    "JournalEntry",
                    reversingEntry.id,
                    reversingEntry.guid,
                    reversingLine.debit_amount,
                    reversingLine.credit_amount,
                    reversingLine.description,
                    reversingEntry.fiscal_period,
                    reversingEntry.id,
                    true,
                    commandModel.calling_user_id
                );
            }
        }

        // Mark original as reversed
        existingEntity.is_reversed = true;
        existingEntity.reversed_by_entry_id = reversingEntry.id;
        existingEntity = CommonDataHelper<JournalEntryHeader>.FillUpdateFields(existingEntity, commandModel.calling_user_id);

        _Context.JournalEntryHeaders.Update(existingEntity);
        await _Context.SaveChangesAsync();

        var dto = await MapToDto(reversingEntry);
        return new Response<JournalEntryHeaderDto>(dto);
    }

    public async Task<PagingResult<JournalEntryHeaderListDto>> GetUnpostedEntries(PagingSortingParameters parameters)
    {
        var response = new PagingResult<JournalEntryHeaderListDto>();

        try
        {
            var query = _Context.JournalEntryHeaders
                .Where(m => !m.is_deleted && !m.is_posted);

            var totalCount = await query.CountAsync();
            var pagedItems = await query.SortAndPageBy(parameters).ToListAsync();

            var dtos = new List<JournalEntryHeaderListDto>();
            foreach (var item in pagedItems)
            {
                dtos.Add(await MapToListDto(item));
            }

            response.Data = dtos;
            response.TotalResultCount = totalCount;
        }
        catch (Exception ex)
        {
            await LogError(50, this.GetType().Name, nameof(GetUnpostedEntries), ex);
            response.SetException(ex.Message, ResultCode.Error);
            response.TotalResultCount = 0;
        }

        return response;
    }

    public async Task<PagingResult<JournalEntryHeaderListDto>> Find(PagingSortingParameters parameters, JournalEntryHeaderFindCommand commandModel)
    {
        var response = new PagingResult<JournalEntryHeaderListDto>();

        try
        {
            var query = _Context.JournalEntryHeaders.Where(m => !m.is_deleted);

            if (!string.IsNullOrEmpty(commandModel.wildcard))
            {
                int parsedNum;
                if (int.TryParse(commandModel.wildcard, out parsedNum))
                {
                    query = query.Where(m => m.entry_number == parsedNum);
                }
                else
                {
                    var wild = commandModel.wildcard.ToLower();
                    query = query.Where(m =>
                        (m.description != null && m.description.ToLower().Contains(wild))
                    );
                }
            }

            if (commandModel.is_posted.HasValue)
                query = query.Where(m => m.is_posted == commandModel.is_posted.Value);

            if (commandModel.is_reversed.HasValue)
                query = query.Where(m => m.is_reversed == commandModel.is_reversed.Value);

            if (commandModel.reference_type.HasValue)
                query = query.Where(m => m.reference_type == commandModel.reference_type.Value);

            if (commandModel.from_date.HasValue)
                query = query.Where(m => m.entry_date >= commandModel.from_date.Value);

            if (commandModel.to_date.HasValue)
                query = query.Where(m => m.entry_date <= commandModel.to_date.Value);

            if (!string.IsNullOrEmpty(commandModel.fiscal_period))
                query = query.Where(m => m.fiscal_period == commandModel.fiscal_period);

            var totalCount = await query.CountAsync();
            var pagedItems = await query.SortAndPageBy(parameters).ToListAsync();

            var dtos = new List<JournalEntryHeaderListDto>();
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

    public async Task<Response<List<JournalEntryHeaderListDto>>> GlobalSearch(GlobalSearchFindCommand commandModel)
    {
        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<List<JournalEntryHeaderListDto>>(validationResult.Exception, ResultCode.DataValidationError);

        var response = new Response<List<JournalEntryHeaderListDto>>();

        try
        {
            var filter = PredicateBuilder.True<JournalEntryHeader>();
            filter = filter.And(m => m.is_deleted == false);

            int parsedNum;
            if (int.TryParse(commandModel.wildcard, out parsedNum))
            {
                filter = filter.And(m => m.entry_number == parsedNum);
            }
            else
            {
                var lower = commandModel.wildcard.ToLower();
                filter = filter.And(m =>
                    (m.description != null && m.description.ToLower().Contains(lower))
                );
            }

            var results = _Context.JournalEntryHeaders.Where(filter);
            var pagedItems = await results.SortAndPageBy(commandModel.parameters).ToListAsync();

            var dtos = new List<JournalEntryHeaderListDto>();
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

    public async Task<JournalEntryHeaderListDto> MapToListDto(JournalEntryHeader databaseModel)
    {
        var lines = await _Context.JournalEntryLines
            .Where(m => m.journal_entry_header_id == databaseModel.id && !m.is_deleted)
            .ToListAsync();

        var dto = new JournalEntryHeaderListDto()
        {
            entry_number = databaseModel.entry_number,
            entry_date = databaseModel.entry_date,
            description = databaseModel.description,
            reference_type = databaseModel.reference_type,
            is_posted = databaseModel.is_posted,
            posted_on = databaseModel.posted_on,
            is_reversed = databaseModel.is_reversed,
            fiscal_period = databaseModel.fiscal_period,
            total_debits = lines.Sum(l => l.debit_amount),
            total_credits = lines.Sum(l => l.credit_amount),
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

        dto.reference_type_name = GetReferenceTypeName(databaseModel.reference_type);

        return dto;
    }

    public async Task<JournalEntryHeaderDto> MapToDto(JournalEntryHeader databaseModel)
    {
        var lines = await _Context.JournalEntryLines
            .Where(m => m.journal_entry_header_id == databaseModel.id && !m.is_deleted)
            .ToListAsync();

        var dto = new JournalEntryHeaderDto()
        {
            entry_number = databaseModel.entry_number,
            entry_date = databaseModel.entry_date,
            description = databaseModel.description,
            reference_type = databaseModel.reference_type,
            reference_id = databaseModel.reference_id,
            is_posted = databaseModel.is_posted,
            posted_on = databaseModel.posted_on,
            posted_by = databaseModel.posted_by,
            is_reversed = databaseModel.is_reversed,
            reversed_by_entry_id = databaseModel.reversed_by_entry_id,
            fiscal_period = databaseModel.fiscal_period,
            total_debits = lines.Sum(l => l.debit_amount),
            total_credits = lines.Sum(l => l.credit_amount),
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

        dto.reference_type_name = GetReferenceTypeName(databaseModel.reference_type);

        foreach (var line in lines)
            dto.journal_entry_lines.Add(await MapToLineDto(line));

        return dto;
    }

    public async Task<JournalEntryLineDto> MapToLineDto(JournalEntryLine databaseModel)
    {
        var dto = new JournalEntryLineDto()
        {
            journal_entry_header_id = databaseModel.journal_entry_header_id,
            line_number = databaseModel.line_number,
            chart_of_account_id = databaseModel.chart_of_account_id,
            debit_amount = databaseModel.debit_amount,
            credit_amount = databaseModel.credit_amount,
            description = databaseModel.description,
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

        var account = await _Context.ChartOfAccounts
            .Where(m => m.id == databaseModel.chart_of_account_id)
            .SingleOrDefaultAsync();

        if (account != null)
        {
            dto.account_number = account.account_number;
            dto.account_name = account.account_name;
        }

        return dto;
    }

    public JournalEntryHeader MapToDatabaseModel(JournalEntryHeaderCreateCommand createCommand)
    {
        return CommonDataHelper<JournalEntryHeader>.FillCommonFields(new JournalEntryHeader()
        {
            entry_date = createCommand.entry_date,
            description = createCommand.description,
            reference_type = createCommand.reference_type,
            reference_id = createCommand.reference_id,
            fiscal_period = createCommand.fiscal_period ?? createCommand.entry_date.ToString("yyyy-MM"),
            is_posted = false,
            is_reversed = false,
            guid = Guid.NewGuid().ToString(),
        }, createCommand.calling_user_id);
    }

    public JournalEntryLine MapToLineDatabaseModel(JournalEntryLineCreateCommand createCommand, int headerId, string callingUserId)
    {
        return CommonDataHelper<JournalEntryLine>.FillCommonFields(new JournalEntryLine()
        {
            journal_entry_header_id = headerId,
            line_number = createCommand.line_number,
            chart_of_account_id = createCommand.chart_of_account_id,
            debit_amount = createCommand.debit_amount,
            credit_amount = createCommand.credit_amount,
            description = createCommand.description,
            guid = Guid.NewGuid().ToString(),
            is_deleted = false
        }, callingUserId);
    }

    public JournalEntryHeader MapToDatabaseModel(JournalEntryHeaderDto dtoModel)
    {
        throw new NotImplementedException();
    }

    private async Task<int> GenerateEntryNumber()
    {
        var maxNumber = await _Context.JournalEntryHeaders.MaxAsync(m => (int?)m.entry_number) ?? 0;
        return maxNumber + 1;
    }

    private string? GetReferenceTypeName(int? referenceType)
    {
        if (!referenceType.HasValue) return null;

        return referenceType.Value switch
        {
            1 => "Manual",
            2 => "AP Invoice",
            3 => "AR Invoice",
            4 => "Credit Memo",
            5 => "Payment",
            _ => "Unknown"
        };
    }
}
