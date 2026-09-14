using Microsoft.EntityFrameworkCore;
using KosmosERP.Database;
using KosmosERP.Database.Models;
using KosmosERP.Module;
using KosmosERP.Models;
using KosmosERP.Models.Helpers;
using KosmosERP.Models.Interfaces;
using KosmosERP.BusinessLayer.Models.Module.FinancialTransaction.Dto;
using KosmosERP.BusinessLayer.Models.Module.FinancialTransaction.Command.Find;
using KosmosERP.BusinessLayer.Helpers;

namespace KosmosERP.BusinessLayer.Modules;

public interface IFinancialTransactionModule : IBaseERPModule
{
    Task<Response<FinancialTransactionDto>> GetDto(int object_id);
    Task<Response<FinancialTransactionDto>> GetDtoByGuid(string guid);
    Task<PagingResult<FinancialTransactionListDto>> Find(PagingSortingParameters parameters, FinancialTransactionFindCommand commandModel);
    Task<Response<AccountBalanceDto>> GetAccountBalance(AccountBalanceFindCommand commandModel);
    Task<PagingResult<FinancialTransactionListDto>> GetAccountLedger(PagingSortingParameters parameters, int chartOfAccountId, DateTime? fromDate, DateTime? toDate);
    Task<Response<FinancialTransactionDto>> RecordTransaction(
        int chartOfAccountId,
        DateTime transactionDate,
        int transactionType,
        string sourceModule,
        int sourceId,
        string sourceGuid,
        decimal debitAmount,
        decimal creditAmount,
        string? description,
        string? fiscalPeriod,
        int? journalEntryId,
        bool isReversal,
        string callingUserId
    );
}

public class FinancialTransactionModule : BaseERPModule, IFinancialTransactionModule
{
    public override Guid ModuleIdentifier => Guid.Parse("c3d4e5f6-a7b8-9012-cdef-345678901234");
    public override string ModuleName => "Financial Transactions";

    private IBaseERPContext _Context;

    public FinancialTransactionModule(IBaseERPContext context, ILogProviderFactory logProviderFactory) : base(logProviderFactory)
    {
        _Context = context;
    }

    public override void SeedPermissions()
    {
        var role = _Context.Roles.Any(m => m.name == "Financial Transaction Viewers");

        if (role == false)
        {
            _Context.Roles.Add(CommonDataHelper<Role>.FillCommonFields(new Role()
            {
                name = "Financial Transaction Viewers",
            }, 1));

            _Context.SaveChanges();
        }
    }

    public FinancialTransaction? Get(int object_id)
    {
        return _Context.FinancialTransactions.SingleOrDefault(m => m.id == object_id);
    }

    public async Task<FinancialTransaction?> GetAsync(int object_id)
    {
        return await _Context.FinancialTransactions.SingleOrDefaultAsync(m => m.id == object_id);
    }

    public async Task<Response<FinancialTransactionDto>> GetDto(int object_id)
    {
        Response<FinancialTransactionDto> response = new Response<FinancialTransactionDto>();

        var result = await _Context.FinancialTransactions.SingleOrDefaultAsync(m => m.id == object_id);
        if (result == null)
        {
            response.SetException("Financial Transaction not found", ResultCode.NotFound);
            return response;
        }

        response.Data = await this.MapToDto(result);
        return response;
    }

    public async Task<Response<FinancialTransactionDto>> GetDtoByGuid(string guid)
    {
        var entity = await _Context.FinancialTransactions.FirstOrDefaultAsync(c => c.guid == guid && !c.is_deleted);
        if (entity == null)
            return new Response<FinancialTransactionDto>("Financial Transaction not found", ResultCode.NotFound);

        var dto = await MapToDto(entity);
        return new Response<FinancialTransactionDto>(dto);
    }

    public async Task<Response<AccountBalanceDto>> GetAccountBalance(AccountBalanceFindCommand commandModel)
    {
        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<AccountBalanceDto>(validationResult.Exception, ResultCode.DataValidationError);

        var response = new Response<AccountBalanceDto>();

        try
        {
            var account = await _Context.ChartOfAccounts
                .Where(m => m.id == commandModel.chart_of_account_id && !m.is_deleted)
                .SingleOrDefaultAsync();

            if (account == null)
                return new Response<AccountBalanceDto>("Chart of Account not found", ResultCode.NotFound);

            var query = _Context.FinancialTransactions
                .Where(m => m.chart_of_account_id == commandModel.chart_of_account_id && !m.is_deleted);

            if (commandModel.as_of_date.HasValue)
                query = query.Where(m => m.transaction_date <= commandModel.as_of_date.Value);

            var transactions = await query.ToListAsync();

            var totalDebits = transactions.Sum(t => t.debit_amount);
            var totalCredits = transactions.Sum(t => t.credit_amount);

            // Calculate balance based on normal balance type
            decimal balance;
            if (account.normal_balance == NormalBalance.Debit)
            {
                // Debit normal: Assets, Expenses
                balance = totalDebits - totalCredits;
            }
            else
            {
                // Credit normal: Liabilities, Equity, Revenue
                balance = totalCredits - totalDebits;
            }

            response.Data = new AccountBalanceDto()
            {
                chart_of_account_id = account.id,
                account_number = account.account_number,
                account_name = account.account_name,
                account_type = account.account_type,
                balance = balance,
                total_debits = totalDebits,
                total_credits = totalCredits,
                as_of_date = commandModel.as_of_date
            };
        }
        catch (Exception ex)
        {
            await LogError(50, this.GetType().Name, nameof(GetAccountBalance), ex);
            response.SetException(ex.Message, ResultCode.Error);
        }

        return response;
    }

    public async Task<PagingResult<FinancialTransactionListDto>> GetAccountLedger(PagingSortingParameters parameters, int chartOfAccountId, DateTime? fromDate, DateTime? toDate)
    {
        var response = new PagingResult<FinancialTransactionListDto>();

        try
        {
            var query = _Context.FinancialTransactions
                .Where(m => m.chart_of_account_id == chartOfAccountId && !m.is_deleted);

            if (fromDate.HasValue)
                query = query.Where(m => m.transaction_date >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(m => m.transaction_date <= toDate.Value);

            var totalCount = await query.CountAsync();
            var pagedItems = await query.OrderBy(m => m.transaction_date).ThenBy(m => m.id).SortAndPageBy(parameters).ToListAsync();

            var dtos = new List<FinancialTransactionListDto>();
            foreach (var item in pagedItems)
            {
                dtos.Add(await MapToListDto(item));
            }

            response.Data = dtos;
            response.TotalResultCount = totalCount;
        }
        catch (Exception ex)
        {
            await LogError(50, this.GetType().Name, nameof(GetAccountLedger), ex);
            response.SetException(ex.Message, ResultCode.Error);
            response.TotalResultCount = 0;
        }

        return response;
    }

    public async Task<PagingResult<FinancialTransactionListDto>> Find(PagingSortingParameters parameters, FinancialTransactionFindCommand commandModel)
    {
        var response = new PagingResult<FinancialTransactionListDto>();

        try
        {
            var query = _Context.FinancialTransactions.Where(m => !m.is_deleted);

            if (!string.IsNullOrEmpty(commandModel.wildcard))
            {
                var wild = commandModel.wildcard.ToLower();
                query = query.Where(m =>
                    (m.description != null && m.description.ToLower().Contains(wild))
                    || m.source_module.ToLower().Contains(wild)
                );
            }

            if (commandModel.chart_of_account_id.HasValue)
                query = query.Where(m => m.chart_of_account_id == commandModel.chart_of_account_id.Value);

            if (commandModel.transaction_type.HasValue)
                query = query.Where(m => m.transaction_type == commandModel.transaction_type.Value);

            if (!string.IsNullOrEmpty(commandModel.source_module))
                query = query.Where(m => m.source_module == commandModel.source_module);

            if (commandModel.from_date.HasValue)
                query = query.Where(m => m.transaction_date >= commandModel.from_date.Value);

            if (commandModel.to_date.HasValue)
                query = query.Where(m => m.transaction_date <= commandModel.to_date.Value);

            if (!string.IsNullOrEmpty(commandModel.fiscal_period))
                query = query.Where(m => m.fiscal_period == commandModel.fiscal_period);

            if (commandModel.is_reversal.HasValue)
                query = query.Where(m => m.is_reversal == commandModel.is_reversal.Value);

            var totalCount = await query.CountAsync();
            var pagedItems = await query.SortAndPageBy(parameters).ToListAsync();

            var dtos = new List<FinancialTransactionListDto>();
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

    public async Task<Response<FinancialTransactionDto>> RecordTransaction(
        int chartOfAccountId,
        DateTime transactionDate,
        int transactionType,
        string sourceModule,
        int sourceId,
        string sourceGuid,
        decimal debitAmount,
        decimal creditAmount,
        string? description,
        string? fiscalPeriod,
        int? journalEntryId,
        bool isReversal,
        string callingUserId)
    {
        try
        {
            // Calculate running balance for this account
            var previousBalance = await _Context.FinancialTransactions
                .Where(m => m.chart_of_account_id == chartOfAccountId && !m.is_deleted)
                .OrderByDescending(m => m.transaction_date)
                .ThenByDescending(m => m.id)
                .Select(m => m.running_balance)
                .FirstOrDefaultAsync();

            var account = await _Context.ChartOfAccounts
                .Where(m => m.id == chartOfAccountId)
                .SingleOrDefaultAsync();

            decimal runningBalance;
            if (account != null && account.normal_balance == NormalBalance.Debit)
            {
                runningBalance = previousBalance + debitAmount - creditAmount;
            }
            else
            {
                runningBalance = previousBalance + creditAmount - debitAmount;
            }

            var transaction = CommonDataHelper<FinancialTransaction>.FillCommonFields(new FinancialTransaction()
            {
                transaction_date = transactionDate,
                transaction_type = transactionType,
                source_module = sourceModule,
                source_id = sourceId,
                source_guid = sourceGuid,
                chart_of_account_id = chartOfAccountId,
                debit_amount = debitAmount,
                credit_amount = creditAmount,
                running_balance = runningBalance,
                description = description,
                fiscal_period = fiscalPeriod ?? transactionDate.ToString("yyyy-MM"),
                journal_entry_id = journalEntryId,
                is_reversal = isReversal,
                guid = Guid.NewGuid().ToString(),
            }, callingUserId);

            await _Context.FinancialTransactions.AddAsync(transaction);
            await _Context.SaveChangesAsync();

            var dto = await MapToDto(transaction);
            return new Response<FinancialTransactionDto>(dto);
        }
        catch (Exception ex)
        {
            return new Response<FinancialTransactionDto>(ex.Message, ResultCode.Error);
        }
    }

    public async Task<FinancialTransactionListDto> MapToListDto(FinancialTransaction databaseModel)
    {
        var dto = new FinancialTransactionListDto()
        {
            transaction_date = databaseModel.transaction_date,
            transaction_type = databaseModel.transaction_type,
            source_module = databaseModel.source_module,
            source_id = databaseModel.source_id,
            chart_of_account_id = databaseModel.chart_of_account_id,
            debit_amount = databaseModel.debit_amount,
            credit_amount = databaseModel.credit_amount,
            running_balance = databaseModel.running_balance,
            description = databaseModel.description,
            fiscal_period = databaseModel.fiscal_period,
            is_reversal = databaseModel.is_reversal,
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

        dto.transaction_type_name = GetTransactionTypeName(databaseModel.transaction_type);

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

    public async Task<FinancialTransactionDto> MapToDto(FinancialTransaction databaseModel)
    {
        var dto = new FinancialTransactionDto()
        {
            transaction_date = databaseModel.transaction_date,
            transaction_type = databaseModel.transaction_type,
            source_module = databaseModel.source_module,
            source_id = databaseModel.source_id,
            source_guid = databaseModel.source_guid,
            chart_of_account_id = databaseModel.chart_of_account_id,
            debit_amount = databaseModel.debit_amount,
            credit_amount = databaseModel.credit_amount,
            running_balance = databaseModel.running_balance,
            description = databaseModel.description,
            fiscal_period = databaseModel.fiscal_period,
            journal_entry_id = databaseModel.journal_entry_id,
            is_reversal = databaseModel.is_reversal,
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

        dto.transaction_type_name = GetTransactionTypeName(databaseModel.transaction_type);

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

    private string GetTransactionTypeName(int transactionType)
    {
        return transactionType switch
        {
            1 => "Journal Entry",
            2 => "AP Post",
            3 => "AR Post",
            4 => "Payment Received",
            5 => "Payment Sent",
            6 => "Credit Memo Applied",
            _ => "Unknown"
        };
    }
}
