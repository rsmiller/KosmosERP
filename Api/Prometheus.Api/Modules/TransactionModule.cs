using Prometheus.Api.Models.Module.Transaction.Command.Create;
using Prometheus.Api.Models.Module.Transaction.Command.Delete;
using Prometheus.Api.Models.Module.Transaction.Command.Edit;
using Prometheus.Api.Models.Module.Transaction.Command.Find;
using Prometheus.Api.Models.Module.Transaction.Dto;
using Prometheus.Database;
using Prometheus.Models.Helpers;
using Prometheus.Models.Interfaces;
using Prometheus.Models;
using Prometheus.Module;
using Prometheus.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Prometheus.Api.Modules;

public interface ITransactionModule : IERPModule<
    Transaction,
    TransactionDto,
    TransactionListDto,
    TransactionCreateCommand,
    TransactionEditCommand,
    TransactionDeleteCommand,
    TransactionFindCommand>, IBaseERPModule
{
    // Add any Transaction-specific methods here if needed
}

public class TransactionModule : BaseERPModule, ITransactionModule
{
    public override Guid ModuleIdentifier => Guid.Parse("416786e0-47b3-440a-90da-b7036d72b1f7");
    public override string ModuleName => "Transactions";

    private readonly IBaseERPContext _Context;

    public TransactionModule(IBaseERPContext context) : base(context)
    {
        _Context = context;
    }

    public override void SeedPermissions()
    {
        var read_permission = _Context.ModulePermissions.Any(m => m.module_id == this.ModuleIdentifier.ToString() && m.internal_permission_name == "read_transaction");
        var create_permission = _Context.ModulePermissions.Any(m => m.module_id == this.ModuleIdentifier.ToString() && m.internal_permission_name == "create_transaction");
        var edit_permission = _Context.ModulePermissions.Any(m => m.module_id == this.ModuleIdentifier.ToString() && m.internal_permission_name == "edit_transaction");
        var delete_permission = _Context.ModulePermissions.Any(m => m.module_id == this.ModuleIdentifier.ToString() && m.internal_permission_name == "delete_transaction");

        if (read_permission == false)
        {
            _Context.ModulePermissions.Add(new ModulePermission()
            {
                permission_name = "Read Transaction",
                internal_permission_name = "read_transaction",
                module_id = this.ModuleIdentifier.ToString(),
                module_name = this.ModuleName,
                read = true,
            });

            _Context.SaveChanges();
        }

        if (create_permission == false)
        {
            _Context.ModulePermissions.Add(new ModulePermission()
            {
                permission_name = "Create Transaction",
                internal_permission_name = "create_transaction",
                module_id = this.ModuleIdentifier.ToString(),
                module_name = this.ModuleName,
                write = true
            });

            _Context.SaveChanges();
        }

        if (edit_permission == false)
        {
            _Context.ModulePermissions.Add(new ModulePermission()
            {
                permission_name = "Edit Transaction",
                internal_permission_name = "edit_transaction",
                module_id = this.ModuleIdentifier.ToString(),
                module_name = this.ModuleName,
                edit = true
            });

            _Context.SaveChanges();
        }

        if (delete_permission == false)
        {
            _Context.ModulePermissions.Add(new ModulePermission()
            {
                permission_name = "Delete Transaction",
                internal_permission_name = "Delete_transaction",
                module_id = this.ModuleIdentifier.ToString(),
                module_name = this.ModuleName,
                delete = true
            });

            _Context.SaveChanges();
        }
    }

    public Transaction? Get(int object_id)
    {
        // Using SingleOrDefault instead of Find
        return _Context.Transactions
            .SingleOrDefault(m => m.id == object_id);
    }

    public async Task<Transaction?> GetAsync(int object_id)
    {
        return await _Context.Transactions
            .SingleOrDefaultAsync(m => m.id == object_id);
    }

    public async Task<Response<TransactionDto>> GetDto(int object_id)
    {
        var entity = await GetAsync(object_id);
        if (entity == null)
            return new Response<TransactionDto>("Transaction not found", ResultCode.NotFound);

        var dto = await MapToDto(entity);
        return new Response<TransactionDto>(dto);
    }

    public async Task<Response<TransactionDto>> Create(TransactionCreateCommand commandModel)
    {
        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<TransactionDto>(validationResult.Exception, ResultCode.DataValidationError);

        var permission_result = await base.HasPermission(commandModel.calling_user_id, "create_transaction", write: true);
        if (!permission_result)
            return new Response<TransactionDto>("Invalid permission", ResultCode.InvalidPermission);


        var newTransaction = this.MapForCreate(commandModel);

        _Context.Transactions.Add(newTransaction);
        await _Context.SaveChangesAsync();

        var dto = await MapToDto(newTransaction);
        return new Response<TransactionDto>(dto);
    }

    public async Task<Response<TransactionDto>> Edit(TransactionEditCommand commandModel)
    {
        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<TransactionDto>(validationResult.Exception, ResultCode.DataValidationError);

        var permission_result = await base.HasPermission(commandModel.calling_user_id, "edit_transaction", edit: true);
        if (!permission_result)
            return new Response<TransactionDto>("Invalid permission", ResultCode.InvalidPermission);

        var existingEntity = await GetAsync(commandModel.id);
        if (existingEntity == null)
            return new Response<TransactionDto>("Transaction not found", ResultCode.NotFound);

        if (commandModel.product_id.HasValue && existingEntity.product_id != commandModel.product_id)
            existingEntity.product_id = commandModel.product_id.Value;

        if (commandModel.transaction_type.HasValue && existingEntity.transaction_type != commandModel.transaction_type)
            existingEntity.transaction_type = commandModel.transaction_type.Value;

        if (commandModel.transaction_date.HasValue && existingEntity.transaction_date != commandModel.transaction_date)
            existingEntity.transaction_date = commandModel.transaction_date.Value;

        if (commandModel.object_reference_id.HasValue && existingEntity.object_reference_id != commandModel.object_reference_id)
            existingEntity.object_reference_id = commandModel.object_reference_id.Value;

        if (existingEntity.object_sub_reference_id != commandModel.object_sub_reference_id)
            existingEntity.object_sub_reference_id = commandModel.object_sub_reference_id;

        if (commandModel.units_sold.HasValue && existingEntity.units_sold != commandModel.units_sold)
            existingEntity.units_sold = commandModel.units_sold.Value;

        if (commandModel.units_shipped.HasValue && existingEntity.units_shipped != commandModel.units_shipped)
            existingEntity.units_shipped = commandModel.units_shipped.Value;

        if (commandModel.units_purchased.HasValue && existingEntity.units_purchased != commandModel.units_purchased)
            existingEntity.units_purchased = commandModel.units_purchased.Value;

        if (commandModel.units_received.HasValue && existingEntity.units_received != commandModel.units_received)
            existingEntity.units_received = commandModel.units_received.Value;

        if (commandModel.purchased_unit_cost.HasValue && existingEntity.purchased_unit_cost != commandModel.purchased_unit_cost)
            existingEntity.purchased_unit_cost = commandModel.purchased_unit_cost.Value;

        if (commandModel.sold_unit_price.HasValue && existingEntity.sold_unit_price != commandModel.sold_unit_price)
            existingEntity.sold_unit_price = commandModel.sold_unit_price.Value;

        // Update auditing fields
        existingEntity.updated_on = DateTime.Now;
        existingEntity.updated_by = commandModel.calling_user_id;

        _Context.Transactions.Update(existingEntity);
        await _Context.SaveChangesAsync();

        var dto = await MapToDto(existingEntity);
        return new Response<TransactionDto>(dto);
    }

    public async Task<Response<TransactionDto>> Delete(TransactionDeleteCommand commandModel)
    {
        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<TransactionDto>(validationResult.Exception, ResultCode.DataValidationError);

        var permission_result = await base.HasPermission(commandModel.calling_user_id, "delete_transaction", delete: true);
        if (!permission_result)
            return new Response<TransactionDto>("Invalid permission", ResultCode.InvalidPermission);

        var existingEntity = await GetAsync(commandModel.id);
        if (existingEntity == null)
            return new Response<TransactionDto>("Transaction not found", ResultCode.NotFound);

        existingEntity.is_deleted = true;
        existingEntity.deleted_on = DateTime.Now;
        existingEntity.deleted_by = commandModel.calling_user_id;

        _Context.Transactions.Update(existingEntity);
        await _Context.SaveChangesAsync();

        var dto = await MapToDto(existingEntity);
        return new Response<TransactionDto>(dto);
    }

    public async Task<PagingResult<TransactionListDto>> Find(PagingSortingParameters parameters, TransactionFindCommand commandModel)
    {
        var response = new PagingResult<TransactionListDto>();

        try
        {
            // Example permission check
            var permission_result = await base.HasPermission(commandModel.calling_user_id, "read_transaction", read: true);
            if (!permission_result)
            {
                response.SetException("Invalid permission", ResultCode.InvalidPermission);
                return response;
            }

            var query = _Context.Transactions.Where(m => m.is_deleted == false);

            if (!string.IsNullOrEmpty(commandModel.wildcard))
            {
                var wild = commandModel.wildcard.ToLower();
                query = query.Where(m => m.guid.ToLower().Contains(wild));
            }

            if (commandModel.product_id.HasValue)
                query = query.Where(m => m.product_id == commandModel.product_id.Value);

            if (commandModel.object_reference_id.HasValue)
                query = query.Where(m => m.object_reference_id == commandModel.object_reference_id.Value);

            var totalCount = await query.CountAsync();
            var pagedItems = await query.SortAndPageBy(parameters).ToListAsync();

            var dtos = new List<TransactionListDto>();
            foreach (var item in pagedItems)
                dtos.Add(await MapToListDto(item));

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

    public async Task<Response<List<TransactionListDto>>> GlobalSearch(PagingSortingParameters parameters, string wildcard)
    {
        var response = new Response<List<TransactionListDto>>();
        try
        {
            var query = _Context.Transactions.Where(m => m.is_deleted == false);

            if (!string.IsNullOrEmpty(wildcard))
            {
                var lower = wildcard.ToLower();
                query = query.Where(m => m.guid.ToLower().Contains(lower));
            }

            var pagedItems = await query.SortAndPageBy(parameters).ToListAsync();

            var dtos = new List<TransactionListDto>();
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

    // -------------------------------------------------------------------
    // MAP METHODS
    // -------------------------------------------------------------------

    public async Task<TransactionListDto> MapToListDto(Transaction databaseModel)
    {
        // For one-to-one mapping, same as MapToDto
        return new TransactionListDto
        {
            id = databaseModel.id,
            is_deleted = databaseModel.is_deleted,
            created_on = databaseModel.created_on,
            created_by = databaseModel.created_by,
            updated_on = databaseModel.updated_on,
            updated_by = databaseModel.updated_by,
            deleted_on = databaseModel.deleted_on,
            deleted_by = databaseModel.deleted_by,

            product_id = databaseModel.product_id,
            transaction_type = databaseModel.transaction_type,
            transaction_date = databaseModel.transaction_date,
            object_reference_id = databaseModel.object_reference_id,
            object_sub_reference_id = databaseModel.object_sub_reference_id,
            units_sold = databaseModel.units_sold,
            units_shipped = databaseModel.units_shipped,
            units_purchased = databaseModel.units_purchased,
            units_received = databaseModel.units_received,
            purchased_unit_cost = databaseModel.purchased_unit_cost,
            sold_unit_price = databaseModel.sold_unit_price,
            guid = databaseModel.guid
        };
    }

    public async Task<TransactionDto> MapToDto(Transaction databaseModel)
    {
        return new TransactionDto
        {
            id = databaseModel.id,
            is_deleted = databaseModel.is_deleted,
            created_on = databaseModel.created_on,
            created_by = databaseModel.created_by,
            updated_on = databaseModel.updated_on,
            updated_by = databaseModel.updated_by,
            deleted_on = databaseModel.deleted_on,
            deleted_by = databaseModel.deleted_by,

            product_id = databaseModel.product_id,
            transaction_type = databaseModel.transaction_type,
            transaction_date = databaseModel.transaction_date,
            object_reference_id = databaseModel.object_reference_id,
            object_sub_reference_id = databaseModel.object_sub_reference_id,
            units_sold = databaseModel.units_sold,
            units_shipped = databaseModel.units_shipped,
            units_purchased = databaseModel.units_purchased,
            units_received = databaseModel.units_received,
            purchased_unit_cost = databaseModel.purchased_unit_cost,
            sold_unit_price = databaseModel.sold_unit_price,
            guid = databaseModel.guid
        };
    }

    public Transaction MapToDatabaseModel(TransactionDto dtoModel)
    {
        return new Transaction
        {
            id = dtoModel.id,
            is_deleted = dtoModel.is_deleted,
            created_on = dtoModel.created_on,
            created_by = dtoModel.created_by,
            updated_on = dtoModel.updated_on,
            updated_by = dtoModel.updated_by,
            deleted_on = dtoModel.deleted_on,
            deleted_by = dtoModel.deleted_by,

            product_id = dtoModel.product_id,
            transaction_type = dtoModel.transaction_type,
            transaction_date = dtoModel.transaction_date,
            object_reference_id = dtoModel.object_reference_id,
            object_sub_reference_id = dtoModel.object_sub_reference_id,
            units_sold = dtoModel.units_sold,
            units_shipped = dtoModel.units_shipped,
            units_purchased = dtoModel.units_purchased,
            units_received = dtoModel.units_received,
            purchased_unit_cost = dtoModel.purchased_unit_cost,
            sold_unit_price = dtoModel.sold_unit_price,
            guid = dtoModel.guid
        };
    }

    private Transaction MapForCreate(TransactionCreateCommand createCommandModel)
    {
        var now = DateTime.Now;

        var transaction = new Transaction
        {
            product_id = createCommandModel.product_id,
            transaction_type = createCommandModel.transaction_type,
            transaction_date = createCommandModel.transaction_date,
            object_reference_id = createCommandModel.object_reference_id,
            object_sub_reference_id = createCommandModel.object_sub_reference_id,
            units_sold = createCommandModel.units_sold,
            units_shipped = createCommandModel.units_shipped,
            units_purchased = createCommandModel.units_purchased,
            units_received = createCommandModel.units_received,
            purchased_unit_cost = createCommandModel.purchased_unit_cost,
            sold_unit_price = createCommandModel.sold_unit_price,
            created_on = now,
            created_by = createCommandModel.calling_user_id,
            updated_on = now,
            updated_by = createCommandModel.calling_user_id
        };

        return transaction;
    }
}
