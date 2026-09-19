using KosmosERP.Database.Models;
using KosmosERP.Database;
using KosmosERP.Models.Helpers;
using KosmosERP.Models.Interfaces;
using KosmosERP.Models;
using KosmosERP.Module;
using Microsoft.EntityFrameworkCore;
using KosmosERP.BusinessLayer.Helpers;
using KosmosERP.BusinessLayer.Models.Module.Customer.Dto;
using KosmosERP.BusinessLayer.Models.Module.Customer.Command.Create;
using KosmosERP.BusinessLayer.Models.Module.Customer.Command.Edit;
using KosmosERP.BusinessLayer.Models.Module.Customer.Command.Delete;
using KosmosERP.BusinessLayer.Models.Module.Customer.Command.Find;

namespace KosmosERP.BusinessLayer.Modules;

public interface ICustomerModule : IERPModule<
        Customer,
        CustomerDto,
        CustomerListDto,
        CustomerCreateCommand,
        CustomerEditCommand,
        CustomerDeleteCommand,
        CustomerFindCommand>, IBaseERPModule
{
    Task<List<KeyValueStore>> GetPaymentTerms();
    Task<List<KeyValueStore>> GetShippingMethods();
    Task<List<KeyValueStore>> GetPayMethods();
}

public class CustomerModule : BaseERPModule, ICustomerModule
{
    public override Guid ModuleIdentifier => Guid.Parse("09b8ce60-c202-4601-87ab-07edb01a06ed");
    public override string ModuleName => "Customers";

    private readonly IBaseERPContext _Context;
    private IMemoryCacheService<KeyValueStore> _KVMemoryService;
    private IMemoryCacheService<Customer> _CustomerMemoryService;

    public CustomerModule(IBaseERPContext context, ILogProviderFactory logProviderFactory) : base(context, logProviderFactory)
    {

    }

    public CustomerModule(IBaseERPContext context, IMemoryCacheService<KeyValueStore> kvMService,
                            IMemoryCacheService<Customer> customerMemService, 
                            ILogProviderFactory logProviderFactory) : base(context, logProviderFactory)
    {
        _Context = context;
        _KVMemoryService = kvMService;
        _CustomerMemoryService = customerMemService;
    }

    public override void SeedPermissions()
    {
        var role = _Context.Roles.Any(m => m.name == "Customer Administrators");

        if (role == false)
        {
            _Context.Roles.Add(CommonDataHelper<Role>.FillCommonFields(new Role()
            {
                name = "Customer Administrators",
            }, 1));

            _Context.SaveChanges();

            base.CreateFirstRunRolePermissions();
        }
    }

    public Customer? Get(int object_id)
    {
        return _Context.Customers
            .SingleOrDefault(m => m.id == object_id);
    }

    public async Task<Customer?> GetAsync(int object_id)
    {
        return await _Context.Customers
            .SingleOrDefaultAsync(m => m.id == object_id);
    }

    public async Task<Response<CustomerDto>> GetDto(int object_id)
    {
        var entity = await GetAsync(object_id);
        if (entity == null)
            return new Response<CustomerDto>("Customer not found", ResultCode.NotFound);

        var dto = await MapToDto(entity);
        return new Response<CustomerDto>(dto);
    }

    public async Task<Response<CustomerDto>> GetDtoByGuid(string guid)
    {
        var entity = await _Context.Customers.FirstOrDefaultAsync(c => c.guid == guid && !c.is_deleted);
        if (entity == null)
            return new Response<CustomerDto>("Customer not found", ResultCode.NotFound);

        var dto = await MapToDto(entity);
        return new Response<CustomerDto>(dto);
    }

    public async Task<Response<CustomerDto>> Create(CustomerCreateCommand commandModel)
    {
        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<CustomerDto>(validationResult.Exception, ResultCode.DataValidationError);

        var newCustomer = this.MapForCreate(commandModel);
        newCustomer.customer_number = await this.ManuallyGenerateACustomerNumber();

        _Context.Customers.Add(newCustomer);
        await _Context.SaveChangesAsync();

        // Update Memory Cache
        _CustomerMemoryService.UpdateDatabaseValue(newCustomer.id, newCustomer);

        var dto = await MapToDto(newCustomer);
        return new Response<CustomerDto>(dto);
    }

    public async Task<Response<CustomerDto>> Edit(CustomerEditCommand commandModel)
    {
        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<CustomerDto>(validationResult.Exception, ResultCode.DataValidationError);

        var existingEntity = await GetAsync(commandModel.id);
        if (existingEntity == null)
            return new Response<CustomerDto>("Customer not found", ResultCode.NotFound);

        if (existingEntity.customer_name != commandModel.customer_name)
            existingEntity.customer_name = commandModel.customer_name;

        if (existingEntity.customer_description != commandModel.customer_description)
            existingEntity.customer_description = commandModel.customer_description;

        if (existingEntity.phone != commandModel.phone)
            existingEntity.phone = commandModel.phone;

        if (existingEntity.fax != commandModel.fax)
            existingEntity.fax = commandModel.fax;

        if (existingEntity.general_email != commandModel.general_email)
            existingEntity.general_email = commandModel.general_email;

        if (existingEntity.accounting_email != commandModel.accounting_email)
            existingEntity.accounting_email = commandModel.accounting_email;

        if (existingEntity.website != commandModel.website)
            existingEntity.website = commandModel.website;

        if (existingEntity.category != commandModel.category)
            existingEntity.category = commandModel.category;

        if (existingEntity.external_id != commandModel.external_id)
            existingEntity.external_id = commandModel.external_id;

        if (commandModel.tax_rate.HasValue && existingEntity.tax_rate != commandModel.tax_rate)
            existingEntity.tax_rate = commandModel.tax_rate.Value;

        if (commandModel.is_taxable.HasValue && existingEntity.is_taxable != commandModel.is_taxable)
            existingEntity.is_taxable = commandModel.is_taxable.Value;


        if (!String.IsNullOrEmpty(commandModel.payment_terms) && existingEntity.payment_terms != commandModel.payment_terms)
            existingEntity.payment_terms = commandModel.payment_terms;
            

        existingEntity = CommonDataHelper<Customer>.FillUpdateFields(existingEntity, commandModel.calling_user_id);


        _Context.Customers.Update(existingEntity);
        await _Context.SaveChangesAsync();


        // Update Memory Cache
        _CustomerMemoryService.UpdateDatabaseValue(existingEntity.id, existingEntity);


        var dto = await MapToDto(existingEntity);
        return new Response<CustomerDto>(dto);
    }

    public async Task<Response<CustomerDto>> Delete(CustomerDeleteCommand commandModel)
    {
        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<CustomerDto>(validationResult.Exception, ResultCode.DataValidationError);

        var existingEntity = await GetAsync(commandModel.id);
        if (existingEntity == null)
            return new Response<CustomerDto>("Customer not found", ResultCode.NotFound);

        // DO Delete
        existingEntity = CommonDataHelper<Customer>.FillDeleteFields(existingEntity, commandModel.calling_user_id);

        _Context.Customers.Update(existingEntity);
        await _Context.SaveChangesAsync();

        var dto = await MapToDto(existingEntity);
        return new Response<CustomerDto>(dto);
    }

    public async Task<PagingResult<CustomerListDto>> Find(PagingSortingParameters parameters, CustomerFindCommand commandModel)
    {
        var response = new PagingResult<CustomerListDto>();
        try
        {

            var query = _Context.Customers
                .Where(m => !m.is_deleted);

            
            if (!string.IsNullOrEmpty(commandModel.wildcard))
            {
                var wild = commandModel.wildcard.ToLower();
                query = query.Where(m =>
                    m.customer_name.ToLower().Contains(wild)
                    || (m.customer_description != null && m.customer_description.ToLower().Contains(wild))
                    || m.phone.ToLower().Contains(wild)
                    || (m.fax != null && m.fax.ToLower().Contains(wild))
                    || (m.general_email != null && m.general_email.ToLower().Contains(wild))
                    || (m.website != null && m.website.ToLower().Contains(wild))
                    || m.category.ToLower().Contains(wild)
                    || m.guid.ToLower().Contains(wild)
                );
            }

            var totalCount = await query.CountAsync();
            var pagedItems = await query.SortAndPageBy(parameters).ToListAsync();

            var dtos = new List<CustomerListDto>();
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

    public async Task<Response<List<CustomerListDto>>> GlobalSearch(GlobalSearchFindCommand commandModel)
    {
        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<List<CustomerListDto>>(validationResult.Exception, ResultCode.DataValidationError);

        var response = new Response<List<CustomerListDto>>();

        try
        {
            var query = _Context.Customers
                .Where(m => !m.is_deleted);

            if (!string.IsNullOrEmpty(commandModel.wildcard))
            {
                var lower = commandModel.wildcard.ToLower();
                query = query.Where(m =>
                    m.customer_name.ToLower().Contains(lower)
                    || (m.customer_description != null && m.customer_description.ToLower().Contains(lower))
                    || m.phone.ToLower().Contains(lower)
                    || (m.fax != null && m.fax.ToLower().Contains(lower))
                    || (m.general_email != null && m.general_email.ToLower().Contains(lower))
                    || (m.website != null && m.website.ToLower().Contains(lower))
                    || m.category.ToLower().Contains(lower)
                    || m.guid.ToLower().Contains(lower)
                );
            }

            var pagedItems = await query.SortAndPageBy(commandModel.parameters).ToListAsync();

            var dtos = new List<CustomerListDto>();
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

    public async Task<List<KeyValueStore>> GetPaymentTerms()
    {
        return await _Context.KeyValueStores.Where(m => m.module_id == KeyValueIds.PaymentTerms && m.is_deleted == false).ToListAsync();
    }

    public async Task<List<KeyValueStore>> GetShippingMethods()
    {
        return await _Context.KeyValueStores.Where(m => m.module_id == KeyValueIds.ShippingMethods && m.is_deleted == false).ToListAsync();
    }

    public async Task<List<KeyValueStore>> GetPayMethods()
    {
        return await _Context.KeyValueStores.Where(m => m.module_id == KeyValueIds.PayMethods && m.is_deleted == false).ToListAsync();
    }

    public async Task<CustomerListDto> MapToListDto(Customer databaseModel)
    {
        var dto = new CustomerListDto
        {
            id = databaseModel.id,
            is_deleted = databaseModel.is_deleted,
            created_on = databaseModel.created_on,
            created_by = databaseModel.created_by,
            updated_on = databaseModel.updated_on,
            updated_by = databaseModel.updated_by,
            deleted_on = databaseModel.deleted_on,
            deleted_by = databaseModel.deleted_by,
            created_on_string = databaseModel.created_on_string,
            created_on_timezone = databaseModel.created_on_timezone,
            updated_on_string = databaseModel.updated_on_string,
            updated_on_timezone = databaseModel.updated_on_timezone,
            deleted_on_string = databaseModel.deleted_on_string,
            deleted_on_timezone = databaseModel.deleted_on_timezone,
            customer_number = databaseModel.customer_number,
            customer_name = databaseModel.customer_name,
            customer_description = databaseModel.customer_description,
            phone = databaseModel.phone,
            fax = databaseModel.fax,
            general_email = databaseModel.general_email,
            accounting_email = databaseModel.accounting_email,
            website = databaseModel.website,
            category = databaseModel.category,
            guid = databaseModel.guid,
            is_taxable = databaseModel.is_taxable,
            tax_rate = databaseModel.tax_rate,
            payment_terms = databaseModel.payment_terms,
        };

        var payment_terms_val = await _KVMemoryService.GetKeyValue(databaseModel.payment_terms);

        if (payment_terms_val != null)
            dto.payment_terms_name = payment_terms_val.value;

        return dto;
    }

    public async Task<CustomerDto> MapToDto(Customer databaseModel)
    {
        var dto = new CustomerDto
        {
            id = databaseModel.id,
            is_deleted = databaseModel.is_deleted,
            created_on = databaseModel.created_on,
            created_by = databaseModel.created_by,
            updated_on = databaseModel.updated_on,
            updated_by = databaseModel.updated_by,
            deleted_on = databaseModel.deleted_on,
            deleted_by = databaseModel.deleted_by,
            created_on_string = databaseModel.created_on_string,
            created_on_timezone = databaseModel.created_on_timezone,
            updated_on_string = databaseModel.updated_on_string,
            updated_on_timezone = databaseModel.updated_on_timezone,
            deleted_on_string = databaseModel.deleted_on_string,
            deleted_on_timezone = databaseModel.deleted_on_timezone,
            customer_number = databaseModel.customer_number,
            customer_name = databaseModel.customer_name,
            customer_description = databaseModel.customer_description,
            phone = databaseModel.phone,
            fax = databaseModel.fax,
            general_email = databaseModel.general_email,
            accounting_email = databaseModel.accounting_email,
            website = databaseModel.website,
            category = databaseModel.category,
            guid = databaseModel.guid,
            is_taxable = databaseModel.is_taxable,
            tax_rate = databaseModel.tax_rate,
            payment_terms = databaseModel.payment_terms,
            external_id = databaseModel.external_id
        };

        var payment_terms_val = await _KVMemoryService.GetKeyValue(databaseModel.payment_terms);
        
        if (payment_terms_val != null)
            dto.payment_terms_name = payment_terms_val.value;

        return dto;
    }

    public Customer MapToDatabaseModel(CustomerDto dtoModel)
    {
        return new Customer
        {
            id = dtoModel.id,
            is_deleted = dtoModel.is_deleted,
            created_on = dtoModel.created_on,
            created_by = dtoModel.created_by,
            updated_on = dtoModel.updated_on,
            updated_by = dtoModel.updated_by,
            deleted_on = dtoModel.deleted_on,
            deleted_by = dtoModel.deleted_by,
            deleted_on_string = dtoModel.deleted_on_string,
            deleted_on_timezone = dtoModel.deleted_on_timezone,
            updated_on_string = dtoModel.updated_on_string,
            updated_on_timezone = dtoModel.updated_on_timezone,
            customer_number = dtoModel.customer_number,
            customer_name = dtoModel.customer_name,
            customer_description = dtoModel.customer_description,
            phone = dtoModel.phone,
            fax = dtoModel.fax,
            general_email = dtoModel.general_email,
            accounting_email = dtoModel.accounting_email,
            website = dtoModel.website,
            category = dtoModel.category,
            guid = dtoModel.guid,
            is_taxable = dtoModel.is_taxable,
            tax_rate = dtoModel.tax_rate,
            payment_terms = dtoModel.payment_terms,
            external_id = dtoModel.external_id
        };
    }

    private Customer MapForCreate(CustomerCreateCommand createCommandModel)
    {
        var customer = CommonDataHelper<Customer>.FillCommonFields(new Customer
        {
            customer_name = createCommandModel.customer_name,
            customer_description = createCommandModel.customer_description,
            phone = createCommandModel.phone,
            fax = createCommandModel.fax,
            general_email = createCommandModel.general_email,
            accounting_email = createCommandModel.accounting_email,
            website = createCommandModel.website,
            category = createCommandModel.category,
            is_deleted = false,
            is_taxable = createCommandModel.is_taxable,
            tax_rate = createCommandModel.tax_rate,
            payment_terms = createCommandModel.payment_terms,
            external_id = createCommandModel.external_id
        }, createCommandModel.calling_user_id);

        return customer;
    }


    private async Task<int> ManuallyGenerateACustomerNumber()
    {
        var total_records = await _Context.Customers.CountAsync();
        int start = DatabaseStartNumbers.Customers;

        return (total_records + start + 1);
    }
}
