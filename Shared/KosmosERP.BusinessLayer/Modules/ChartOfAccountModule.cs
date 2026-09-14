using Microsoft.EntityFrameworkCore;
using KosmosERP.Database;
using KosmosERP.Database.Models;
using KosmosERP.Module;
using KosmosERP.Models;
using KosmosERP.Models.Helpers;
using KosmosERP.Models.Interfaces;
using KosmosERP.BusinessLayer.Models.Module.ChartOfAccount.Dto;
using KosmosERP.BusinessLayer.Models.Module.ChartOfAccount.Command.Create;
using KosmosERP.BusinessLayer.Models.Module.ChartOfAccount.Command.Delete;
using KosmosERP.BusinessLayer.Models.Module.ChartOfAccount.Command.Edit;
using KosmosERP.BusinessLayer.Models.Module.ChartOfAccount.Command.Find;
using KosmosERP.BusinessLayer.Helpers;

namespace KosmosERP.BusinessLayer.Modules;

public interface IChartOfAccountModule : IERPModule<ChartOfAccount, ChartOfAccountDto, ChartOfAccountListDto, ChartOfAccountCreateCommand, ChartOfAccountEditCommand, ChartOfAccountDeleteCommand, ChartOfAccountFindCommand>, IBaseERPModule
{
    Task<Response<ChartOfAccountDto>> GetDtoByAccountNumber(string accountNumber);
    Task<Response<List<ChartOfAccountDto>>> GetChildAccounts(int parentAccountId);
    Task<Response<List<ChartOfAccountListDto>>> GetAccountsByType(int accountType);
}

public class ChartOfAccountModule : BaseERPModule, IChartOfAccountModule
{
    public override Guid ModuleIdentifier => Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890");
    public override string ModuleName => "Chart of Accounts";

    private IBaseERPContext _Context;

    public ChartOfAccountModule(IBaseERPContext context, ILogProviderFactory logProviderFactory) : base(logProviderFactory)
    {
        _Context = context;
    }

    public override void SeedPermissions()
    {
        var role = _Context.Roles.Any(m => m.name == "Chart of Account Administrators");

        if (role == false)
        {
            _Context.Roles.Add(CommonDataHelper<Role>.FillCommonFields(new Role()
            {
                name = "Chart of Account Administrators",
            }, 1));

            _Context.SaveChanges();
        }
    }

    public ChartOfAccount? Get(int object_id)
    {
        return _Context.ChartOfAccounts.SingleOrDefault(m => m.id == object_id);
    }

    public async Task<ChartOfAccount?> GetAsync(int object_id)
    {
        return await _Context.ChartOfAccounts.SingleOrDefaultAsync(m => m.id == object_id);
    }

    public async Task<Response<ChartOfAccountDto>> GetDto(int object_id)
    {
        Response<ChartOfAccountDto> response = new Response<ChartOfAccountDto>();

        var result = await _Context.ChartOfAccounts.SingleOrDefaultAsync(m => m.id == object_id);
        if (result == null)
        {
            response.SetException("Chart of Account not found", ResultCode.NotFound);
            return response;
        }

        response.Data = await this.MapToDto(result);
        return response;
    }

    public async Task<Response<ChartOfAccountDto>> GetDtoByGuid(string guid)
    {
        var entity = await _Context.ChartOfAccounts.FirstOrDefaultAsync(c => c.guid == guid && !c.is_deleted);
        if (entity == null)
            return new Response<ChartOfAccountDto>("Chart of Account not found", ResultCode.NotFound);

        var dto = await MapToDto(entity);
        return new Response<ChartOfAccountDto>(dto);
    }

    public async Task<Response<ChartOfAccountDto>> GetDtoByAccountNumber(string accountNumber)
    {
        var entity = await _Context.ChartOfAccounts.FirstOrDefaultAsync(c => c.account_number == accountNumber && !c.is_deleted);
        if (entity == null)
            return new Response<ChartOfAccountDto>("Chart of Account not found", ResultCode.NotFound);

        var dto = await MapToDto(entity);
        return new Response<ChartOfAccountDto>(dto);
    }

    public async Task<Response<List<ChartOfAccountDto>>> GetChildAccounts(int parentAccountId)
    {
        var response = new Response<List<ChartOfAccountDto>>();

        try
        {
            var children = await _Context.ChartOfAccounts
                .Where(m => m.parent_account_id == parentAccountId && !m.is_deleted)
                .ToListAsync();

            var dtos = new List<ChartOfAccountDto>();
            foreach (var item in children)
            {
                dtos.Add(await MapToDto(item));
            }

            response.Data = dtos;
        }
        catch (Exception ex)
        {
            await LogError(50, this.GetType().Name, nameof(GetChildAccounts), ex);
            response.SetException(ex.Message, ResultCode.Error);
        }

        return response;
    }

    public async Task<Response<List<ChartOfAccountListDto>>> GetAccountsByType(int accountType)
    {
        var response = new Response<List<ChartOfAccountListDto>>();

        try
        {
            var accounts = await _Context.ChartOfAccounts
                .Where(m => m.account_type == accountType && !m.is_deleted && m.is_active)
                .OrderBy(m => m.account_number)
                .ToListAsync();

            var dtos = new List<ChartOfAccountListDto>();
            foreach (var item in accounts)
            {
                dtos.Add(await MapToListDto(item));
            }

            response.Data = dtos;
        }
        catch (Exception ex)
        {
            await LogError(50, this.GetType().Name, nameof(GetAccountsByType), ex);
            response.SetException(ex.Message, ResultCode.Error);
        }

        return response;
    }

    public async Task<Response<ChartOfAccountDto>> Create(ChartOfAccountCreateCommand commandModel)
    {
        if (commandModel == null)
            return new Response<ChartOfAccountDto>(ResultCode.NullItemInput);

        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<ChartOfAccountDto>(validationResult.Exception, ResultCode.DataValidationError);

        try
        {
            var alreadyExists = await _Context.ChartOfAccounts.AnyAsync(m => m.account_number == commandModel.account_number && !m.is_deleted);
            if (alreadyExists)
                return new Response<ChartOfAccountDto>("Account number already exists", ResultCode.AlreadyExists);

            var item = MapToDatabaseModel(commandModel);

            await _Context.ChartOfAccounts.AddAsync(item);
            await _Context.SaveChangesAsync();

            var dto = await GetDto(item.id);
            return new Response<ChartOfAccountDto>(dto.Data!);
        }
        catch (Exception ex)
        {
            return new Response<ChartOfAccountDto>(ex.Message, ResultCode.Error);
        }
    }

    public async Task<Response<ChartOfAccountDto>> Edit(ChartOfAccountEditCommand commandModel)
    {
        if (commandModel == null)
            return new Response<ChartOfAccountDto>(ResultCode.NullItemInput);

        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<ChartOfAccountDto>(validationResult.Exception, ResultCode.DataValidationError);

        var existingEntity = await GetAsync(commandModel.id);
        if (existingEntity == null)
            return new Response<ChartOfAccountDto>("Chart of Account not found", ResultCode.NotFound);

        if (!string.IsNullOrEmpty(commandModel.account_number) && existingEntity.account_number != commandModel.account_number)
        {
            var duplicate = await _Context.ChartOfAccounts.AnyAsync(m => m.account_number == commandModel.account_number && m.id != commandModel.id && !m.is_deleted);
            if (duplicate)
                return new Response<ChartOfAccountDto>("Account number already exists", ResultCode.AlreadyExists);
            existingEntity.account_number = commandModel.account_number;
        }

        if (!string.IsNullOrEmpty(commandModel.account_name))
            existingEntity.account_name = commandModel.account_name;

        if (commandModel.account_type.HasValue)
            existingEntity.account_type = commandModel.account_type.Value;

        if (commandModel.parent_account_id.HasValue)
            existingEntity.parent_account_id = commandModel.parent_account_id.Value;

        if (commandModel.is_active.HasValue)
            existingEntity.is_active = commandModel.is_active.Value;

        if (commandModel.normal_balance.HasValue)
            existingEntity.normal_balance = commandModel.normal_balance.Value;

        if (commandModel.description != null)
            existingEntity.description = commandModel.description;

        existingEntity = CommonDataHelper<ChartOfAccount>.FillUpdateFields(existingEntity, commandModel.calling_user_id);

        _Context.ChartOfAccounts.Update(existingEntity);
        await _Context.SaveChangesAsync();

        var dto = await MapToDto(existingEntity);
        return new Response<ChartOfAccountDto>(dto);
    }

    public async Task<Response<ChartOfAccountDto>> Delete(ChartOfAccountDeleteCommand commandModel)
    {
        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<ChartOfAccountDto>(validationResult.Exception, ResultCode.DataValidationError);

        var existingEntity = await GetAsync(commandModel.id);
        if (existingEntity == null)
            return new Response<ChartOfAccountDto>("Chart of Account not found", ResultCode.NotFound);

        // Check if account has children
        var hasChildren = await _Context.ChartOfAccounts.AnyAsync(m => m.parent_account_id == commandModel.id && !m.is_deleted);
        if (hasChildren)
            return new Response<ChartOfAccountDto>("Cannot delete account with child accounts", ResultCode.Invalid);

        // Check if account has transactions
        var hasTransactions = await _Context.FinancialTransactions.AnyAsync(m => m.chart_of_account_id == commandModel.id);
        if (hasTransactions)
            return new Response<ChartOfAccountDto>("Cannot delete account with financial transactions", ResultCode.Invalid);

        // Soft Delete
        existingEntity = CommonDataHelper<ChartOfAccount>.FillDeleteFields(existingEntity, commandModel.calling_user_id);

        _Context.ChartOfAccounts.Update(existingEntity);
        await _Context.SaveChangesAsync();

        var dto = await MapToDto(existingEntity);
        return new Response<ChartOfAccountDto>(dto);
    }

    public async Task<PagingResult<ChartOfAccountListDto>> Find(PagingSortingParameters parameters, ChartOfAccountFindCommand commandModel)
    {
        var response = new PagingResult<ChartOfAccountListDto>();

        try
        {
            var query = _Context.ChartOfAccounts.Where(m => !m.is_deleted);

            if (!string.IsNullOrEmpty(commandModel.wildcard))
            {
                var wild = commandModel.wildcard.ToLower();
                query = query.Where(m =>
                    m.account_number.ToLower().Contains(wild)
                    || m.account_name.ToLower().Contains(wild)
                    || (m.description != null && m.description.ToLower().Contains(wild))
                );
            }

            if (commandModel.account_type.HasValue)
                query = query.Where(m => m.account_type == commandModel.account_type.Value);

            if (commandModel.is_active.HasValue)
                query = query.Where(m => m.is_active == commandModel.is_active.Value);

            if (commandModel.parent_account_id.HasValue)
                query = query.Where(m => m.parent_account_id == commandModel.parent_account_id.Value);

            var totalCount = await query.CountAsync();
            var pagedItems = await query.SortAndPageBy(parameters).ToListAsync();

            var dtos = new List<ChartOfAccountListDto>();
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

    public async Task<Response<List<ChartOfAccountListDto>>> GlobalSearch(GlobalSearchFindCommand commandModel)
    {
        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<List<ChartOfAccountListDto>>(validationResult.Exception, ResultCode.DataValidationError);

        var response = new Response<List<ChartOfAccountListDto>>();

        try
        {
            var filter = PredicateBuilder.True<ChartOfAccount>();
            filter = filter.And(m => m.is_deleted == false);

            var lower = commandModel.wildcard.ToLower();
            filter = filter.And(m =>
                m.account_number.ToLower().Contains(lower)
                || m.account_name.ToLower().Contains(lower)
            );

            var results = _Context.ChartOfAccounts.Where(filter);
            var pagedItems = await results.SortAndPageBy(commandModel.parameters).ToListAsync();

            var dtos = new List<ChartOfAccountListDto>();
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

    public async Task<ChartOfAccountListDto> MapToListDto(ChartOfAccount databaseModel)
    {
        var dto = new ChartOfAccountListDto()
        {
            account_number = databaseModel.account_number,
            account_name = databaseModel.account_name,
            account_type = databaseModel.account_type,
            parent_account_id = databaseModel.parent_account_id,
            is_active = databaseModel.is_active,
            normal_balance = databaseModel.normal_balance,
            description = databaseModel.description,
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

        dto.account_type_name = GetAccountTypeName(databaseModel.account_type);

        if (databaseModel.parent_account_id.HasValue)
        {
            dto.parent_account_number = await _Context.ChartOfAccounts
                .Where(m => m.id == databaseModel.parent_account_id.Value)
                .Select(m => m.account_number)
                .SingleOrDefaultAsync();
        }

        return dto;
    }

    public async Task<ChartOfAccountDto> MapToDto(ChartOfAccount databaseModel)
    {
        var dto = new ChartOfAccountDto()
        {
            account_number = databaseModel.account_number,
            account_name = databaseModel.account_name,
            account_type = databaseModel.account_type,
            parent_account_id = databaseModel.parent_account_id,
            is_active = databaseModel.is_active,
            normal_balance = databaseModel.normal_balance,
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

        dto.account_type_name = GetAccountTypeName(databaseModel.account_type);

        if (databaseModel.parent_account_id.HasValue)
        {
            var parent = await _Context.ChartOfAccounts
                .Where(m => m.id == databaseModel.parent_account_id.Value)
                .SingleOrDefaultAsync();

            if (parent != null)
            {
                dto.parent_account_number = parent.account_number;
                dto.parent_account_name = parent.account_name;
            }
        }

        // Load child accounts
        var children = await _Context.ChartOfAccounts
            .Where(m => m.parent_account_id == databaseModel.id && !m.is_deleted)
            .ToListAsync();

        foreach (var child in children)
        {
            dto.child_accounts.Add(await MapToDto(child));
        }

        return dto;
    }

    public ChartOfAccount MapToDatabaseModel(ChartOfAccountCreateCommand createCommand)
    {
        return CommonDataHelper<ChartOfAccount>.FillCommonFields(new ChartOfAccount()
        {
            account_number = createCommand.account_number,
            account_name = createCommand.account_name,
            account_type = createCommand.account_type,
            parent_account_id = createCommand.parent_account_id,
            is_active = createCommand.is_active,
            normal_balance = createCommand.normal_balance,
            description = createCommand.description,
            guid = Guid.NewGuid().ToString(),
        }, createCommand.calling_user_id);
    }

    public ChartOfAccount MapToDatabaseModel(ChartOfAccountDto dtoModel)
    {
        throw new NotImplementedException();
    }

    private string GetAccountTypeName(int accountType)
    {
        return accountType switch
        {
            1 => "Asset",
            2 => "Liability",
            3 => "Equity",
            4 => "Revenue",
            5 => "Expense",
            _ => "Unknown"
        };
    }
}
