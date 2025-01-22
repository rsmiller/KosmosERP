using Prometheus.Database.Models;
using Prometheus.Database;
using Prometheus.Models.Helpers;
using Prometheus.Models.Interfaces;
using Prometheus.Models;
using Prometheus.Module;
using Prometheus.Api.Models.Module.Customer.Command.Create;
using Prometheus.Api.Models.Module.Customer.Command.Edit;
using Prometheus.Api.Models.Module.Customer.Command.Delete;
using Prometheus.Api.Models.Module.Customer.Dto;
using Prometheus.Api.Models.Module.Customer.Command.Find;
using Microsoft.EntityFrameworkCore;

namespace Prometheus.Api.Modules;

public interface ICustomerModule : IERPModule<
Customer,
CustomerDto,
CustomerListDto,
CustomerCreateCommand,
CustomerEditCommand,
CustomerDeleteCommand,
CustomerFindCommand>, IBaseERPModule
{
    // Add any customer-specific methods if needed
}

public class CustomerModule : BaseERPModule, ICustomerModule
{
    public override Guid ModuleIdentifier => Guid.Parse("09b8ce60-c202-4601-87ab-07edb01a06ed");
    public override string ModuleName => "Customers";

    private readonly IBaseERPContext _Context;

    public CustomerModule(IBaseERPContext context) : base(context)
    {
        _Context = context;
    }

    public override void SeedPermissions()
    {
        var role = _Context.Roles.Any(m => m.name == "Customer Users");
        var read_permission = _Context.ModulePermissions.Any(m => m.module_id == this.ModuleIdentifier.ToString() && m.internal_permission_name == "read_customer");
        var create_permission = _Context.ModulePermissions.Any(m => m.module_id == this.ModuleIdentifier.ToString() && m.internal_permission_name == "create_customer");
        var edit_permission = _Context.ModulePermissions.Any(m => m.module_id == this.ModuleIdentifier.ToString() && m.internal_permission_name == "edit_customer");
        var delete_permission = _Context.ModulePermissions.Any(m => m.module_id == this.ModuleIdentifier.ToString() && m.internal_permission_name == "delete_customer");

        if (role == false)
        {
            _Context.Roles.Add(new Role()
            {
                name = "Customer Users",
                created_by = 1,
                created_on = DateTime.Now,
                updated_by = 1,
                updated_on = DateTime.Now,
            });

            _Context.SaveChanges();
        }

        var role_id = _Context.Roles.Where(m => m.name == "Customer Users").Select(m => m.id).Single();

        if (read_permission == false)
        {
            _Context.ModulePermissions.Add(new ModulePermission()
            {
                permission_name = "Read Customer",
                internal_permission_name = "read_customer",
                module_id = this.ModuleIdentifier.ToString(),
                module_name = this.ModuleName,
                read = true,
            });

            _Context.SaveChanges();

            var read_perm_id = _Context.ModulePermissions.Where(m => m.internal_permission_name == "read_customer").Select(m => m.id).Single();

            _Context.RolePermissions.Add(new RolePermission()
            {
                role_id = role_id,
                module_permission_id = read_perm_id,
                created_by = 1,
                created_on = DateTime.Now,
                updated_by = 1,
                updated_on = DateTime.Now,
            });

            _Context.SaveChanges();
        }

        if (create_permission == false)
        {
            _Context.ModulePermissions.Add(new ModulePermission()
            {
                permission_name = "Create Customer",
                internal_permission_name = "create_customer",
                module_id = this.ModuleIdentifier.ToString(),
                module_name = this.ModuleName,
                write = true
            });

            _Context.SaveChanges();

            var create_perm_id = _Context.ModulePermissions.Where(m => m.internal_permission_name == "create_customer").Select(m => m.id).Single();

            _Context.RolePermissions.Add(new RolePermission()
            {
                role_id = role_id,
                module_permission_id = create_perm_id,
                created_by = 1,
                created_on = DateTime.Now,
                updated_by = 1,
                updated_on = DateTime.Now,
            });

            _Context.SaveChanges();
        }

        if (edit_permission == false)
        {
            _Context.ModulePermissions.Add(new ModulePermission()
            {
                permission_name = "Edit Customer",
                internal_permission_name = "edit_customer",
                module_id = this.ModuleIdentifier.ToString(),
                module_name = this.ModuleName,
                edit = true
            });

            _Context.SaveChanges();

            var edit_perm_id = _Context.ModulePermissions.Where(m => m.internal_permission_name == "edit_customer").Select(m => m.id).Single();

            _Context.RolePermissions.Add(new RolePermission()
            {
                role_id = role_id,
                module_permission_id = edit_perm_id,
                created_by = 1,
                created_on = DateTime.Now,
                updated_by = 1,
                updated_on = DateTime.Now,
            });

            _Context.SaveChanges();
        }

        if (delete_permission == false)
        {
            _Context.ModulePermissions.Add(new ModulePermission()
            {
                permission_name = "Delete Customer",
                internal_permission_name = "delete_customer",
                module_id = this.ModuleIdentifier.ToString(),
                module_name = this.ModuleName,
                delete = true
            });

            _Context.SaveChanges();

            var delete_perm_id = _Context.ModulePermissions.Where(m => m.internal_permission_name == "delete_customer").Select(m => m.id).Single();

            _Context.RolePermissions.Add(new RolePermission()
            {
                role_id = role_id,
                module_permission_id = delete_perm_id,
                created_by = 1,
                created_on = DateTime.Now,
                updated_by = 1,
                updated_on = DateTime.Now,
            });

            _Context.SaveChanges();
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

    public async Task<Response<CustomerDto>> Create(CustomerCreateCommand commandModel)
    {
        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<CustomerDto>(validationResult.Exception, ResultCode.DataValidationError);

        var permission_result = await base.HasPermission(commandModel.calling_user_id, "create_customer", write: true);
        if (!permission_result)
            return new Response<CustomerDto>("Invalid permission", ResultCode.InvalidPermission);

        var newCustomer = this.MapForCreate(commandModel);

        _Context.Customers.Add(newCustomer);
        await _Context.SaveChangesAsync();

        var dto = await MapToDto(newCustomer);
        return new Response<CustomerDto>(dto);
    }

    public async Task<Response<CustomerDto>> Edit(CustomerEditCommand commandModel)
    {
        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<CustomerDto>(validationResult.Exception, ResultCode.DataValidationError);

        var permission_result = await base.HasPermission(commandModel.calling_user_id, "edit_customer", edit: true);
        if (!permission_result)
            return new Response<CustomerDto>("Invalid permission", ResultCode.InvalidPermission);

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

        if (existingEntity.website != commandModel.website)
            existingEntity.website = commandModel.website;

        if (existingEntity.category != commandModel.category)
            existingEntity.category = commandModel.category;


        existingEntity.updated_on = DateTime.Now;
        existingEntity.updated_by = commandModel.calling_user_id;

        _Context.Customers.Update(existingEntity);
        await _Context.SaveChangesAsync();

        var dto = await MapToDto(existingEntity);
        return new Response<CustomerDto>(dto);
    }

    public async Task<Response<CustomerDto>> Delete(CustomerDeleteCommand commandModel)
    {
        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<CustomerDto>(validationResult.Exception, ResultCode.DataValidationError);

        var permission_result = await base.HasPermission(commandModel.calling_user_id, "delete_customer", delete: true);
        if (!permission_result)
            return new Response<CustomerDto>("Invalid permission", ResultCode.InvalidPermission);

        var existingEntity = await GetAsync(commandModel.id);
        if (existingEntity == null)
            return new Response<CustomerDto>("Customer not found", ResultCode.NotFound);

        existingEntity.is_deleted = true;
        existingEntity.deleted_on = DateTime.Now;
        existingEntity.deleted_by = commandModel.calling_user_id;

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
            var permission_result = await base.HasPermission(commandModel.calling_user_id, "read_customer", read: true);
            if (!permission_result)
            {
                response.SetException("Invalid permission", ResultCode.InvalidPermission);
                return response;
            }

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

    public async Task<Response<List<CustomerListDto>>> GlobalSearch(PagingSortingParameters parameters, string wildcard)
    {
        var response = new Response<List<CustomerListDto>>();
        try
        {
            var query = _Context.Customers
                .Where(m => !m.is_deleted);

            if (!string.IsNullOrEmpty(wildcard))
            {
                var lower = wildcard.ToLower();
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

            var pagedItems = await query.SortAndPageBy(parameters).ToListAsync();

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

    public async Task<CustomerListDto> MapToListDto(Customer databaseModel)
    {
        return new CustomerListDto
        {
            id = databaseModel.id,
            is_deleted = databaseModel.is_deleted,
            created_on = databaseModel.created_on,
            created_by = databaseModel.created_by,
            updated_on = databaseModel.updated_on,
            updated_by = databaseModel.updated_by,
            deleted_on = databaseModel.deleted_on,
            deleted_by = databaseModel.deleted_by,
            customer_number = databaseModel.customer_number,
            customer_name = databaseModel.customer_name,
            customer_description = databaseModel.customer_description,
            phone = databaseModel.phone,
            fax = databaseModel.fax,
            general_email = databaseModel.general_email,
            website = databaseModel.website,
            category = databaseModel.category,
            guid = databaseModel.guid
        };
    }

    public async Task<CustomerDto> MapToDto(Customer databaseModel)
    {
        return new CustomerDto
        {
            // Base fields
            id = databaseModel.id,
            is_deleted = databaseModel.is_deleted,
            created_on = databaseModel.created_on,
            created_by = databaseModel.created_by,
            updated_on = databaseModel.updated_on,
            updated_by = databaseModel.updated_by,
            deleted_on = databaseModel.deleted_on,
            deleted_by = databaseModel.deleted_by,

            // Customer fields
            customer_number = databaseModel.customer_number,
            customer_name = databaseModel.customer_name,
            customer_description = databaseModel.customer_description,
            phone = databaseModel.phone,
            fax = databaseModel.fax,
            general_email = databaseModel.general_email,
            website = databaseModel.website,
            category = databaseModel.category,
            guid = databaseModel.guid
        };
    }

    public Customer MapToDatabaseModel(CustomerDto dtoModel)
    {
        return new Customer
        {
            // Base fields
            id = dtoModel.id,
            is_deleted = dtoModel.is_deleted,
            created_on = dtoModel.created_on,
            created_by = dtoModel.created_by,
            updated_on = dtoModel.updated_on,
            updated_by = dtoModel.updated_by,
            deleted_on = dtoModel.deleted_on,
            deleted_by = dtoModel.deleted_by,

            // Customer fields
            customer_number = dtoModel.customer_number,
            customer_name = dtoModel.customer_name,
            customer_description = dtoModel.customer_description,
            phone = dtoModel.phone,
            fax = dtoModel.fax,
            general_email = dtoModel.general_email,
            website = dtoModel.website,
            category = dtoModel.category,
            guid = dtoModel.guid
        };
    }

    private Customer MapForCreate(CustomerCreateCommand createCommandModel)
    {
        var now = DateTime.Now;

        var customer = new Customer
        {
            customer_name = createCommandModel.customer_name,
            customer_description = createCommandModel.customer_description,
            phone = createCommandModel.phone,
            fax = createCommandModel.fax,
            general_email = createCommandModel.general_email,
            website = createCommandModel.website,
            category = createCommandModel.category,
            is_deleted = false,
            created_on = now,
            created_by = createCommandModel.calling_user_id,
            updated_on = now,
            updated_by = createCommandModel.calling_user_id
        };

        return customer;
    }
}
