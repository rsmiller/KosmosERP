using KosmosERP.Models;
using KosmosERP.Module;
using KosmosERP.Database;
using KosmosERP.Database.Models;
using KosmosERP.Models.Interfaces;
using KosmosERP.BusinessLayer.Models.Module.Subscription.Dto;
using KosmosERP.BusinessLayer.Models.Module.Subscription.Command.Create;
using KosmosERP.BusinessLayer.Models.Module.Subscription.Command.Delete;
using KosmosERP.BusinessLayer.Models.Module.Subscription.Command.Edit;
using KosmosERP.BusinessLayer.Models.Module.Subscription.Command.Find;
using KosmosERP.BusinessLayer.Helpers;
using KosmosERP.Models.Helpers;
using Microsoft.EntityFrameworkCore;


namespace KosmosERP.BusinessLayer.Modules;

public interface ISubscriptionModule
        : IERPModule<Subscription, SubscriptionDto, SubscriptionListDto, SubscriptionCreateCommand, SubscriptionEditCommand, SubscriptionDeleteCommand, SubscriptionFindCommand>, IBaseERPModule
{

}

public class SubscriptionModule : BaseERPModule, ISubscriptionModule
{
    private readonly IBaseERPContext _Context;

    public override Guid ModuleIdentifier => Guid.Parse("218d357d-5a1d-5b09-91ca-11baf6bb23f7");
    public override string ModuleName => "Billing Subscription";

    private IMemoryCacheService<Customer> _CustomerMemoryService;
    private IMemoryCacheService<Product> _ProductMemoryService;

    private ICustomerModule _CustomerModule;
    private IOrderModule _OrderModule;

    public SubscriptionModule(IBaseERPContext context,
                                        ICustomerModule customer_module,
                                        IOrderModule order_module,
                                        IMemoryCacheService<Customer> customerMemService,
                                        IMemoryCacheService<Product> productMemService, 
                                        ILogProviderFactory logProviderFactory) : base(context, logProviderFactory)
    {
        _Context = context;
        _CustomerModule = customer_module;
        _OrderModule = order_module;

        _CustomerMemoryService = customerMemService;
        _ProductMemoryService = productMemService;
    }

    public override void SeedPermissions()
    {
        var role = _Context.Roles.Any(m => m.name == "Billing Subscription Administrators");

        if (role == false)
        {
            _Context.Roles.Add(CommonDataHelper<Role>.FillCommonFields(new Role()
            {
                name = "Billing Subscription Administrators",
            }, 1));

            _Context.SaveChanges();

            base.CreateFirstRunRolePermissions();
        }


        var existing_permissions = _Context.ModulePermissions.Any(m => m.module_id == this.ModuleIdentifier.ToString());
        if (!existing_permissions)
        {
            _Context.ModulePermissions.AddRange(new[]
            {
                CommonDataHelper<ModulePermission>.FillCommonFields(new ModulePermission()
                {
                    module_id = this.ModuleIdentifier.ToString(),
                    module_name = this.ModuleName,
                    permission_name = "Read Billing Subscriptions",
                    internal_permission_name = "read_billing_subscription",
                    read = true,
                    write = false,
                    edit = false,
                    delete = false,
                    is_active = true
                }, 1),
                CommonDataHelper<ModulePermission>.FillCommonFields(new ModulePermission()
                {
                    module_id = this.ModuleIdentifier.ToString(),
                    module_name = this.ModuleName,
                    permission_name = "Create Billing Subscriptions",
                    internal_permission_name = "create_billing_subscription",
                    read = false,
                    write = true,
                    edit = false,
                    delete = false,
                    is_active = true
                }, 1),
                CommonDataHelper<ModulePermission>.FillCommonFields(new ModulePermission()
                {
                    module_id = this.ModuleIdentifier.ToString(),
                    module_name = this.ModuleName,
                    permission_name = "Edit Billing Subscriptions",
                    internal_permission_name = "edit_billing_subscription",
                    read = false,
                    write = false,
                    edit = true,
                    delete = false,
                    is_active = true
                }, 1),
                CommonDataHelper<ModulePermission>.FillCommonFields(new ModulePermission()
                {
                    module_id = this.ModuleIdentifier.ToString(),
                    module_name = this.ModuleName,
                    permission_name = "Delete Billing Subscriptions",
                    internal_permission_name = "delete_billing_subscription",
                    read = false,
                    write = false,
                    edit = false,
                    delete = true,
                    is_active = true
                }, 1)
            });

            _Context.SaveChanges();
        }
    }

    public Subscription? Get(int object_id)
    {
        return _Context.Subscriptions.SingleOrDefault(m => m.id == object_id);
    }

    public async Task<Subscription?> GetAsync(int object_id)
    {
        return await _Context.Subscriptions.SingleOrDefaultAsync(m => m.id == object_id);
    }

    public async Task<Response<SubscriptionDto>> GetDto(int object_id)
    {
        Response<SubscriptionDto> response = new Response<SubscriptionDto>();

        var result = await _Context.Subscriptions.SingleOrDefaultAsync(m => m.id == object_id);
        if (result == null)
        {
            response.SetException("Billing Subscription not found", ResultCode.NotFound);
            return response;
        }

        response.Data = await this.MapToDto(result);
        return response;
    }

    public async Task<Response<SubscriptionDto>> GetDtoByGuid(string guid)
    {
        Response<SubscriptionDto> response = new Response<SubscriptionDto>();

        var result = await _Context.Subscriptions.SingleOrDefaultAsync(m => m.guid == guid);
        if (result == null)
        {
            response.SetException("Billing Subscription not found", ResultCode.NotFound);
            return response;
        }

        response.Data = await this.MapToDto(result);
        return response;
    }

    public async Task<Response<SubscriptionDto>> Create(SubscriptionCreateCommand commandModel)
    {
        if (commandModel == null)
            return new Response<SubscriptionDto>(ResultCode.NullItemInput);

        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<SubscriptionDto>(validationResult.Exception, ResultCode.DataValidationError);

        var existingCustomer = await _Context.Customers.SingleOrDefaultAsync(m => m.id == commandModel.customer_id);
        if (existingCustomer == null)
            return new Response<SubscriptionDto>("Customer not found", ResultCode.NotFound);

        try
        {
            var item = this.MapToDatabaseModel(commandModel, commandModel.calling_user_id);
            
            item.subscription_number = await this.ManuallyGenerateASubscriptionNumber();


            await _Context.Subscriptions.AddAsync(item);
            await _Context.SaveChangesAsync();

            if(commandModel.start_date <= DateOnly.FromDateTime(DateTime.Now))
            {
                var entry = this.MapToEntityDatabaseModel(item, commandModel.calling_user_id);

                await _Context.SubscriptionEntries.AddAsync(entry);
                await _Context.SaveChangesAsync();
            }

            var dto = await GetDto(item.id);

            return new Response<SubscriptionDto>(dto.Data);
        }
        catch (Exception ex)
        {
            await LogError(80, this.GetType().Name, nameof(Create), ex);
            return new Response<SubscriptionDto>(ex.Message, ResultCode.Error);
        }
    }

    public async Task<Response<SubscriptionDto>> Edit(SubscriptionEditCommand commandModel)
    {
        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<SubscriptionDto>(validationResult.Exception, ResultCode.DataValidationError);
        
        var existingEntity = await GetAsync(commandModel.id);
        if (existingEntity == null)
            return new Response<SubscriptionDto>("Billing Subscription not found", ResultCode.NotFound);

        if (commandModel.quantity.HasValue && existingEntity.quantity != commandModel.quantity)
            existingEntity.quantity = commandModel.quantity.Value;

        if (commandModel.price.HasValue && existingEntity.price != commandModel.price)
            existingEntity.price = commandModel.price.Value;
            
        if (commandModel.tax.HasValue && existingEntity.tax != commandModel.tax)
            existingEntity.tax = commandModel.tax.Value;

        // Must be first
        if (commandModel.start_date.HasValue && existingEntity.start_date != commandModel.start_date)
            existingEntity.start_date = commandModel.start_date.Value;

        // Then this
        if (commandModel.cycle_days.HasValue && existingEntity.cycle_days != commandModel.cycle_days)
        {
            existingEntity.cycle_days = commandModel.cycle_days.Value;
            existingEntity.next_date = existingEntity.start_date.AddDays(commandModel.cycle_days.Value);
        }

        if (commandModel.end_date.HasValue && existingEntity.end_date != commandModel.end_date)
            existingEntity.end_date = commandModel.end_date.Value;
            

        existingEntity = CommonDataHelper<Subscription>.FillUpdateFields(existingEntity, commandModel.calling_user_id);


        _Context.Subscriptions.Update(existingEntity);
        await _Context.SaveChangesAsync();


        var dto = await MapToDto(existingEntity);
        return new Response<SubscriptionDto>(dto);
    }

    public async Task<Response<SubscriptionDto>> Delete(SubscriptionDeleteCommand commandModel)
    {
        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<SubscriptionDto>(validationResult.Exception, ResultCode.DataValidationError);

        var existingEntity = await GetAsync(commandModel.id);
        if (existingEntity == null)
            return new Response<SubscriptionDto>("Billing Subscription not found", ResultCode.NotFound);


        // Delete Record
        existingEntity = CommonDataHelper<Subscription>.FillDeleteFields(existingEntity, commandModel.calling_user_id);


        _Context.Subscriptions.Update(existingEntity);
        await _Context.SaveChangesAsync();

        var dto = await MapToDto(existingEntity);
        return new Response<SubscriptionDto>(dto);
    }

    public async Task<PagingResult<SubscriptionListDto>> Find(PagingSortingParameters parameters, SubscriptionFindCommand commandModel)
    {
        var response = new PagingResult<SubscriptionListDto>();

        try
        {
            var filter = PredicateBuilder.True<Subscription>();
            filter = filter.And(m => m.is_deleted == false);

            if (commandModel.customer_id.HasValue)
            {
                filter = filter.And(m => m.customer_id == commandModel.customer_id);
            }

            int sub_number = 0;

            if (!string.IsNullOrEmpty(commandModel.wildcard))
            {
                if(int.TryParse(commandModel.wildcard, out sub_number))
                {
                    var wild = commandModel.wildcard.ToLower();
                    filter = filter.Or(m => m.subscription_number == sub_number);
                }
            }


            var totalCount = await _Context.Subscriptions.CountAsync(filter);
            var results = _Context.Subscriptions.Where(filter);
            var pagedItems = await results.SortAndPageBy(parameters).ToListAsync();

            var dtos = new List<SubscriptionListDto>();
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

    public async Task<Response<List<SubscriptionListDto>>> GlobalSearch(GlobalSearchFindCommand commandModel)
    {
        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<List<SubscriptionListDto>>(validationResult.Exception, ResultCode.DataValidationError);


        var response = new Response<List<SubscriptionListDto>>();

        try
        {
            var filter = PredicateBuilder.True<Subscription>();
            filter = filter.And(m => m.is_deleted == false);

            int parsed_num = 0;

            if (int.TryParse(commandModel.wildcard, out parsed_num))
            {
                filter = filter.And(m => m.subscription_number == parsed_num);
            }
            else
            {
                var lower = commandModel.wildcard.ToLower();
                filter = filter.And(m => m.guid.ToLower().Contains(lower));
            }

            var results = _Context.Subscriptions.Where(filter);
            var pagedItems = await results.SortAndPageBy(commandModel.parameters).ToListAsync();

            var dtos = new List<SubscriptionListDto>();
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

    public async Task<SubscriptionListDto> MapToListDto(Subscription databaseModel)
    {
        var dto = new SubscriptionListDto()
        {
            id = databaseModel.id,
            customer_id = databaseModel.customer_id,
            order_header_id = databaseModel.order_header_id,
            subscription_number = databaseModel.subscription_number,
            quantity = databaseModel.quantity,
            price = databaseModel.price,
            tax = databaseModel.tax,
            cycle_days = databaseModel.cycle_days,
            start_date = databaseModel.start_date,
            next_date = databaseModel.next_date,
            end_date = databaseModel.end_date,
            guid = databaseModel.guid,
            is_deleted = databaseModel.is_deleted,
            created_on = databaseModel.created_on,
            created_by = databaseModel.created_by,
            updated_on = databaseModel.updated_on,
            updated_by = databaseModel.updated_by,
            deleted_on = databaseModel.deleted_on,
            deleted_by = databaseModel.deleted_by,
            updated_on_string = databaseModel.updated_on_string,
            updated_on_timezone = databaseModel.updated_on_timezone,
            created_on_string = databaseModel.created_on_string,
            created_on_timezone = databaseModel.created_on_timezone,
            deleted_on_string = databaseModel.deleted_on_string,
            deleted_on_timezone = databaseModel.deleted_on_timezone,
        };

        var customer_val = await _CustomerMemoryService.GetDatabaseValue(databaseModel.customer_id);

        if (customer_val != null)
            dto.customer_name = customer_val.customer_name;

        return dto;
    }

    public async Task<SubscriptionDto> MapToDto(Subscription databaseModel)
    {
        var dto = new SubscriptionDto()
        {
            id = databaseModel.id,
            customer_id = databaseModel.customer_id,
            order_header_id = databaseModel.order_header_id,
            subscription_number = databaseModel.subscription_number,
            quantity = databaseModel.quantity,
            price = databaseModel.price,
            tax = databaseModel.tax,
            cycle_days = databaseModel.cycle_days,
            start_date = databaseModel.start_date,
            next_date = databaseModel.next_date,
            end_date = databaseModel.end_date,
            guid = databaseModel.guid,
            is_deleted = databaseModel.is_deleted,
            created_on = databaseModel.created_on,
            created_by = databaseModel.created_by,
            updated_on = databaseModel.updated_on,
            updated_by = databaseModel.updated_by,
            deleted_on = databaseModel.deleted_on,
            deleted_by = databaseModel.deleted_by,
            updated_on_string = databaseModel.updated_on_string,
            updated_on_timezone = databaseModel.updated_on_timezone,
            created_on_string = databaseModel.created_on_string,
            created_on_timezone = databaseModel.created_on_timezone,
            deleted_on_string = databaseModel.deleted_on_string,
            deleted_on_timezone = databaseModel.deleted_on_timezone,
        };

        var customer = await _CustomerModule.GetDto(databaseModel.customer_id);
        var order = await _OrderModule.GetDto(databaseModel.order_header_id);

        if (customer.Success && customer.Data != null)
            dto.customer = customer.Data;

        if (order.Success && order.Data != null)
            dto.order = order.Data;
            
        return dto;
    }

    public Subscription MapToDatabaseModel(SubscriptionDto dtoModel)
    {
        throw new NotImplementedException();
    }

    public Subscription MapToDatabaseModel(SubscriptionCreateCommand commandModel, string calling_user_id)
    {
        var next_date = commandModel.start_date.AddDays(commandModel.cycle_days);

        return CommonDataHelper<Subscription>.FillCommonFields(new Subscription()
        {
            customer_id = commandModel.customer_id,
            order_header_id = commandModel.order_header_id,
            quantity = commandModel.quantity,
            price = commandModel.price,
            tax = commandModel.tax,
            cycle_days = commandModel.cycle_days,
            start_date = commandModel.start_date,
            next_date = next_date,
            end_date = commandModel.end_date,
            guid = Guid.NewGuid().ToString(),
        }, calling_user_id);
    }

    public SubscriptionEntry MapToEntityDatabaseModel(Subscription subscription, string calling_user_id)
    {
        return CommonDataHelper<SubscriptionEntry>.FillCommonFields(new SubscriptionEntry()
        {
            billing_subscription_id = subscription.id,
        }, calling_user_id);
    }

    private async Task<int> ManuallyGenerateASubscriptionNumber()
    {
        var total_records = await _Context.Subscriptions.CountAsync();
        int start = DatabaseStartNumbers.Subscriptions;

        return (total_records + start + 1);
    }
}