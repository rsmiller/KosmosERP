using Microsoft.EntityFrameworkCore;
using KosmosERP.Database;
using KosmosERP.Database.Models;
using KosmosERP.Models;
using KosmosERP.Models.Helpers;
using KosmosERP.Models.Interfaces;
using KosmosERP.Module;
using KosmosERP.Models.Permissions;
using KosmosERP.BusinessLayer.Models.Module.Product.Command.Create;
using KosmosERP.BusinessLayer.Models.Module.Product.Command.Delete;
using KosmosERP.BusinessLayer.Models.Module.Product.Command.Edit;
using KosmosERP.BusinessLayer.Models.Module.Product.Command.Find;
using KosmosERP.BusinessLayer.Models.Module.Product.Dto;
using KosmosERP.BusinessLayer.Helpers;

namespace KosmosERP.BusinessLayer.Modules;

public interface IProductModule : IERPModule<Database.Models.Product, ProductDto, ProductListDto, ProductCreateCommand, ProductEditCommand, ProductDeleteCommand, ProductFindCommand>, IBaseERPModule
{
    Task<Response<ProductAttributeDto>> CreateAttribute(ProductAttributeCreateCommand commandModel);
    Task<Response<ProductAttributeDto>> EditAttribute(ProductAttributeEditCommand commandModel);
    Task<Response<ProductAttributeDto>> DeleteAttribute(ProductAttributeDeleteCommand commandModel);

    Task<List<KeyValueStore>> GetProductCategories();
}

public class ProductModule : BaseERPModule, IProductModule
{

    public override Guid ModuleIdentifier => Guid.Parse("b8b0d255-3901-4007-b9c7-b0678f89c955");
    public override string ModuleName => "Products";

    private IBaseERPContext _Context;

    public ProductModule(IBaseERPContext context) : base(context)
    {
        _Context = context;
    }

    public override void SeedPermissions()
    {
        var role = _Context.Roles.Any(m => m.name == "Product Users");
        var read_permission = _Context.ModulePermissions.Any(m => m.module_id == this.ModuleIdentifier.ToString() && m.internal_permission_name == ProductPermissions.Read);
        var create_permission = _Context.ModulePermissions.Any(m => m.module_id == this.ModuleIdentifier.ToString() && m.internal_permission_name == ProductPermissions.Create);
        var edit_permission = _Context.ModulePermissions.Any(m => m.module_id == this.ModuleIdentifier.ToString() && m.internal_permission_name == ProductPermissions.Edit);
        var delete_permission = _Context.ModulePermissions.Any(m => m.module_id == this.ModuleIdentifier.ToString() && m.internal_permission_name == ProductPermissions.Delete);

        if (role == false)
        {
            _Context.Roles.Add(CommonDataHelper<Role>.FillCommonFields(new Role()
            {
                name = "Product Users",
            }, 1));

            _Context.SaveChanges();
        }

        var role_id = _Context.Roles.Where(m => m.name == "Product Users").Select(m => m.id).Single();

        if (read_permission == false)
        {
            _Context.ModulePermissions.Add(new ModulePermission()
            {
                permission_name = "Read Product",
                internal_permission_name = ProductPermissions.Read,
                module_id = this.ModuleIdentifier.ToString(),
                module_name = this.ModuleName,
                read = true,
            });

            _Context.SaveChanges();

            var read_perm_id = _Context.ModulePermissions.Where(m => m.internal_permission_name == ProductPermissions.Read).Select(m => m.id).Single();

            _Context.RolePermissions.Add(CommonDataHelper<RolePermission>.FillCommonFields(new RolePermission()
            {
                role_id = role_id,
                module_permission_id = read_perm_id,
            }, 1));

            _Context.SaveChanges();
        }

        if (create_permission == false)
        {
            _Context.ModulePermissions.Add(new ModulePermission()
            {
                permission_name = "Create Product",
                internal_permission_name = ProductPermissions.Create,
                module_id = this.ModuleIdentifier.ToString(),
                module_name = this.ModuleName,
                write = true
            });

            _Context.SaveChanges();

            var create_perm_id = _Context.ModulePermissions.Where(m => m.internal_permission_name == ProductPermissions.Create).Select(m => m.id).Single();

            _Context.RolePermissions.Add(CommonDataHelper<RolePermission>.FillCommonFields(new RolePermission()
            {
                role_id = role_id,
                module_permission_id = create_perm_id,
            }, 1));

            _Context.SaveChanges();
        }

        if (edit_permission == false)
        {
            _Context.ModulePermissions.Add(new ModulePermission()
            {
                permission_name = "Edit Product",
                internal_permission_name = ProductPermissions.Edit,
                module_id = this.ModuleIdentifier.ToString(),
                module_name = this.ModuleName,
                edit = true
            });

            _Context.SaveChanges();

            var edit_perm_id = _Context.ModulePermissions.Where(m => m.internal_permission_name == ProductPermissions.Edit).Select(m => m.id).Single();

            _Context.RolePermissions.Add(CommonDataHelper<RolePermission>.FillCommonFields(new RolePermission()
            {
                role_id = role_id,
                module_permission_id = edit_perm_id,
            }, 1));

            _Context.SaveChanges();
        }

        if (delete_permission == false)
        {
            _Context.ModulePermissions.Add(new ModulePermission()
            {
                permission_name = "Delete Product",
                internal_permission_name = ProductPermissions.Delete,
                module_id = this.ModuleIdentifier.ToString(),
                module_name = this.ModuleName,
                delete = true
            });

            _Context.SaveChanges();

            var delete_perm_id = _Context.ModulePermissions.Where(m => m.internal_permission_name == ProductPermissions.Delete).Select(m => m.id).Single();

            _Context.RolePermissions.Add(CommonDataHelper<RolePermission>.FillCommonFields(new RolePermission()
            {
                role_id = role_id,
                module_permission_id = delete_perm_id,
            }, 1));

            _Context.SaveChanges();
        }
    }

    public Database.Models.Product? Get(int object_id)
    {
        return _Context.Products.SingleOrDefault(m => m.id == object_id);
    }

    public async Task<Database.Models.Product?> GetAsync(int object_id)
    {
        return await _Context.Products.SingleOrDefaultAsync(m => m.id == object_id);
    }

    public async Task<Response<ProductDto>> GetDto(int object_id)
    {
        Response<ProductDto> response = new Response<ProductDto>();

        var result = await _Context.Products.SingleOrDefaultAsync(m => m.id == object_id);
        if (result == null)
        {
            response.SetException("Product not found", ResultCode.NotFound);
            return response;
        }

        response.Data = await this.MapToDto(result);
        return response;
    }

    public async Task<ProductAttribute?> GetAttributeAsync(int object_id)
    {
        return await _Context.ProductAttributes.SingleOrDefaultAsync(m => m.id == object_id);
    }

    public async Task<Response<ProductDto>> Create(ProductCreateCommand commandModel)
    {
        if (commandModel == null)
            return new Response<ProductDto>(ResultCode.NullItemInput);

        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<ProductDto>(validationResult.Exception, ResultCode.DataValidationError);

        var permission_result = await base.HasPermission(commandModel.calling_user_id, commandModel.token,ProductPermissions.Create, write: true);
        if (!permission_result)
            return new Response<ProductDto>("Invalid permission", ResultCode.InvalidPermission);

        try
        {
            var alreadyExists = ProductExists(commandModel);
            if (alreadyExists == true)
                return new Response<ProductDto>(ResultCode.AlreadyExists);


            var item = MapToDatabaseModel(commandModel);

            await _Context.Products.AddAsync(item);
            await _Context.SaveChangesAsync();


            // Do attributes if available
            foreach(var attribute in commandModel.product_attributes)
            {
                attribute.product_id = item.id;
                attribute.calling_user_id = commandModel.calling_user_id;
                attribute.token = commandModel.token;

                var attibute_response = await this.CreateAttribute(attribute);

                if (!attibute_response.Success)
                    return new Response<ProductDto>(attibute_response.Exception, ResultCode.Error);
            }


            var dto = await GetDto(item.id);

            return new Response<ProductDto>(dto.Data);
        }
        catch (Exception ex)
        {
            return new Response<ProductDto>(ex.Message, ResultCode.Error);
        }
    }

    public async Task<Response<ProductDto>> Edit(ProductEditCommand commandModel)
    {
        if (commandModel == null)
            return new Response<ProductDto>(ResultCode.NullItemInput);

        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<ProductDto>(validationResult.Exception, ResultCode.DataValidationError);

        var permission_result = await base.HasPermission(commandModel.calling_user_id, commandModel.token, ProductPermissions.Edit, edit: true);
        if (!permission_result)
            return new Response<ProductDto>("Invalid permission", ResultCode.InvalidPermission);


        var existingEntity = await GetAsync(commandModel.id);
        if (existingEntity == null)
            return new Response<ProductDto>("Product not found", ResultCode.NotFound);

        try
        {
            if (!String.IsNullOrEmpty(commandModel.identifier1) && existingEntity.identifier1 != commandModel.identifier1)
                existingEntity.identifier1 = commandModel.identifier1;
            if (existingEntity.identifier2 != commandModel.identifier2)
                existingEntity.identifier2 = commandModel.identifier2;
            if (existingEntity.identifier3 != commandModel.identifier3)
                existingEntity.identifier3 = commandModel.identifier3;

            if (!String.IsNullOrEmpty(commandModel.product_name) && existingEntity.product_name != commandModel.product_name)
                existingEntity.product_name = commandModel.product_name;
            if (!String.IsNullOrEmpty(commandModel.product_class) && existingEntity.product_class != commandModel.product_class)
                existingEntity.product_class = commandModel.product_class;
            if (!String.IsNullOrEmpty(commandModel.category) && existingEntity.category != commandModel.category)
                existingEntity.category = commandModel.category;

            if (!String.IsNullOrEmpty(commandModel.internal_description) && existingEntity.internal_description != commandModel.internal_description)
                existingEntity.internal_description = commandModel.internal_description;
            if (existingEntity.external_description != commandModel.external_description)
                existingEntity.external_description = commandModel.external_description;
            if (existingEntity.rfid_id != commandModel.rfid_id)
                existingEntity.rfid_id = commandModel.rfid_id;

            if (commandModel.list_price.HasValue && existingEntity.list_price != commandModel.list_price)
                existingEntity.list_price = commandModel.list_price.Value;
            if (commandModel.sales_price.HasValue && existingEntity.sales_price != commandModel.sales_price)
                existingEntity.sales_price = commandModel.sales_price.Value;
            if (commandModel.unit_cost.HasValue && existingEntity.unit_cost != commandModel.unit_cost)
                existingEntity.unit_cost = commandModel.unit_cost.Value;
            if (commandModel.our_cost.HasValue && existingEntity.our_cost != commandModel.our_cost)
                existingEntity.our_cost = commandModel.our_cost.Value;

            if (commandModel.is_labor.HasValue && existingEntity.is_labor != commandModel.is_labor)
                existingEntity.is_labor = commandModel.is_labor.Value;
            if (commandModel.is_material.HasValue && existingEntity.is_material != commandModel.is_material)
                existingEntity.is_material = commandModel.is_material.Value;
            if (commandModel.is_taxable.HasValue && existingEntity.is_taxable != commandModel.is_taxable)
                existingEntity.is_taxable = commandModel.is_taxable.Value;
            if (commandModel.is_rental_item.HasValue && existingEntity.is_rental_item != commandModel.is_rental_item)
                existingEntity.is_rental_item = commandModel.is_rental_item.Value;
            if (commandModel.is_retired.HasValue && existingEntity.is_retired != commandModel.is_retired)
                existingEntity.is_retired = commandModel.is_retired.Value;
            if (commandModel.is_shippable.HasValue && existingEntity.is_shippable != commandModel.is_shippable)
                existingEntity.is_shippable = commandModel.is_shippable.Value;
            if (commandModel.is_stock.HasValue && existingEntity.is_stock != commandModel.is_stock)
                existingEntity.is_stock = commandModel.is_stock.Value;


            existingEntity = CommonDataHelper<Product>.FillUpdateFields(existingEntity, commandModel.calling_user_id);

            _Context.Products.Update(existingEntity);
            await _Context.SaveChangesAsync();

            var dto = await MapToDto(existingEntity);

            return new Response<ProductDto>(dto);
        }
        catch (Exception ex)
        {
            await LogError(80, this.GetType().Name, nameof(Edit), ex);
            return new Response<ProductDto>(ex.Message, ResultCode.Error);
        }
    }

    public async Task<Response<ProductDto>> Delete(ProductDeleteCommand commandModel)
    {
        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<ProductDto>(validationResult.Exception, ResultCode.DataValidationError);

        var permission_result = await base.HasPermission(commandModel.calling_user_id, commandModel.token,ProductPermissions.Delete, delete: true);
        if (!permission_result)
            return new Response<ProductDto>("Invalid permission", ResultCode.InvalidPermission);


        var existingEntity = await GetAsync(commandModel.id);
        if (existingEntity == null)
            return new Response<ProductDto>("Product not found", ResultCode.NotFound);

        // Do delete
        existingEntity = CommonDataHelper<Product>.FillDeleteFields(existingEntity, commandModel.calling_user_id);

        _Context.Products.Update(existingEntity);
        await _Context.SaveChangesAsync();

        var dto = await MapToDto(existingEntity);
        return new Response<ProductDto>(dto);
    }

    public async Task<Response<ProductAttributeDto>> CreateAttribute(ProductAttributeCreateCommand commandModel)
    {
        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<ProductAttributeDto>(validationResult.Exception, ResultCode.DataValidationError);

        var permission_result = await base.HasPermission(commandModel.calling_user_id, commandModel.token, ProductPermissions.Create, write: true);
        if (!permission_result)
            return new Response<ProductAttributeDto>("Invalid permission", ResultCode.InvalidPermission);

        try
        {
            var item = this.MapToAttributeDatabaseModel(commandModel, commandModel.calling_user_id);

            await _Context.ProductAttributes.AddAsync(item);
            await _Context.SaveChangesAsync();

            var dto = MapToProductAttributeDto(item);

            return new Response<ProductAttributeDto>(dto);
        }
        catch (Exception ex)
        {
            return new Response<ProductAttributeDto>(ex.Message, ResultCode.Error);
        }
    }

    public async Task<Response<ProductAttributeDto>> EditAttribute(ProductAttributeEditCommand commandModel)
    {
        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<ProductAttributeDto>(validationResult.Exception, ResultCode.DataValidationError);

        var permission_result = await base.HasPermission(commandModel.calling_user_id, commandModel.token, ProductPermissions.Edit, edit: true);
        if (!permission_result)
            return new Response<ProductAttributeDto>("Invalid permission", ResultCode.InvalidPermission);

        var existingEntity = await GetAttributeAsync(commandModel.id);
        if (existingEntity == null)
            return new Response<ProductAttributeDto>("Attribute not found", ResultCode.NotFound);


        if (commandModel.product_id.HasValue && existingEntity.product_id != commandModel.product_id)
            existingEntity.product_id = commandModel.product_id.Value;
        if (existingEntity.attribute_name != commandModel.attribute_name)
            existingEntity.attribute_name = commandModel.attribute_name;
        if (existingEntity.attribute_value != commandModel.attribute_value)
            existingEntity.attribute_value = commandModel.attribute_value;
        if (existingEntity.attribute_value2 != commandModel.attribute_value2)
            existingEntity.attribute_value2 = commandModel.attribute_value2;
        if (existingEntity.attribute_value3 != commandModel.attribute_value3)
            existingEntity.attribute_value3 = commandModel.attribute_value3;


        _Context.ProductAttributes.Update(existingEntity);
        await _Context.SaveChangesAsync();

        var dto = MapToProductAttributeDto(existingEntity);

        return new Response<ProductAttributeDto>(dto);
    }

    public async Task<Response<ProductAttributeDto>> DeleteAttribute(ProductAttributeDeleteCommand commandModel)
    {
        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<ProductAttributeDto>(validationResult.Exception, ResultCode.DataValidationError);

        var permission_result = await base.HasPermission(commandModel.calling_user_id, commandModel.token, ProductPermissions.Delete, delete: true);
        if (!permission_result)
            return new Response<ProductAttributeDto>("Invalid permission", ResultCode.InvalidPermission);

        var existingEntity = await GetAttributeAsync(commandModel.id);
        if (existingEntity == null)
            return new Response<ProductAttributeDto>("Attribute not found", ResultCode.NotFound);


        // Destroy
        existingEntity = CommonDataHelper<ProductAttribute>.FillDeleteFields(existingEntity, commandModel.calling_user_id);

        _Context.ProductAttributes.Update(existingEntity);
        await _Context.SaveChangesAsync();

        var dto = MapToProductAttributeDto(existingEntity);

        return new Response<ProductAttributeDto>(dto);
    }


    public async Task<PagingResult<ProductListDto>> Find(PagingSortingParameters parameters, ProductFindCommand commandModel)
    {
        var response = new PagingResult<ProductListDto>();

        try
        {
            var query = _Context.Products.Where(m => !m.is_deleted);

            if (!string.IsNullOrEmpty(commandModel.wildcard))
            {
                var wild = commandModel.wildcard.ToLower();
                query = query.Where(m => m.product_name.ToLower().Contains(wild)
                                        || m.identifier1.ToLower().Contains(wild)
                                        || m.identifier2.ToLower().Contains(wild)
                                        || m.identifier3.ToLower().Contains(wild));
            }

            var totalCount = await query.CountAsync();
            var pagedItems = await query.SortAndPageBy(parameters).ToListAsync();

            var dtos = new List<ProductListDto>();
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

    public async Task<Response<List<ProductListDto>>> GlobalSearch(PagingSortingParameters parameters, string wildcard)
    {
        throw new NotImplementedException();
    }

    public async Task<List<KeyValueStore>> GetProductCategories()
    {
        return await _Context.KeyValueStores.Where(m => m.module_id == KeyValueIds.ProductCategories && m.is_deleted == false).ToListAsync();
    }

    public async Task<ProductListDto> MapToListDto(Database.Models.Product databaseModel)
    {
        var dto = new ProductListDto()
        {
            sales_price = databaseModel.sales_price,
            unit_cost = databaseModel.unit_cost,
            list_price = databaseModel.list_price,
            product_class = databaseModel.product_class,
            product_name = databaseModel.product_name,
            is_sales_item = databaseModel.is_sales_item,
            category = databaseModel.category,
            guid = databaseModel.guid,
            id = databaseModel.id,
            internal_description = databaseModel.internal_description,
            identifier1 = databaseModel.identifier1,
            identifier2 = databaseModel.identifier2,
            identifier3 = databaseModel.identifier3,
            vendor_id = databaseModel.vendor_id,
            created_on = databaseModel.created_on,
            created_by = databaseModel.created_by,
            updated_by = databaseModel.updated_by,
            updated_on = databaseModel.updated_on,
            deleted_by = databaseModel.deleted_by,
            deleted_on = databaseModel.deleted_on,
            external_description = databaseModel.external_description,
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

    public async Task<ProductDto> MapToDto(Database.Models.Product databaseModel)
    {
        var dto = new ProductDto()
        {
            sales_price = databaseModel.sales_price,
            unit_cost = databaseModel.unit_cost,
            list_price = databaseModel.list_price,
            product_class = databaseModel.product_class,
            product_name = databaseModel.product_name,
            is_sales_item = databaseModel.is_sales_item,
            category = databaseModel.category,
            guid = databaseModel.guid,
            id = databaseModel.id,
            internal_description = databaseModel.internal_description,
            identifier1 = databaseModel.identifier1,
            identifier2 = databaseModel.identifier2,
            identifier3 = databaseModel.identifier3,
            is_labor = databaseModel.is_labor,
            is_material = databaseModel.is_material,
            is_rental_item = databaseModel.is_rental_item,
            is_retired = databaseModel.is_retired,
            is_shippable = databaseModel.is_shippable,
            is_stock = databaseModel.is_stock,
            is_taxable = databaseModel.is_taxable,
            vendor_id = databaseModel.vendor_id,
            rfid_id = databaseModel.rfid_id,
            required_min_order = databaseModel.required_min_order,
            required_reorder_level = databaseModel.required_reorder_level,
            required_stock_level = databaseModel.required_stock_level,
            our_cost = databaseModel.our_cost,
            retired_on = databaseModel.retired_on,
            created_on = databaseModel.created_on,
            updated_on = databaseModel.updated_on,
            created_by = databaseModel.created_by,
            updated_by = databaseModel.updated_by,
            deleted_on = databaseModel.deleted_on,
            deleted_by = databaseModel.deleted_by,
            external_description = databaseModel.external_description,
            created_on_string = databaseModel.created_on_string,
            created_on_timezone = databaseModel.created_on_timezone,
            updated_on_string = databaseModel.updated_on_string,
            updated_on_timezone = databaseModel.updated_on_timezone,
            deleted_on_string = databaseModel.deleted_on_string,
            deleted_on_timezone = databaseModel.deleted_on_timezone,
        };

        var attributes = await _Context.ProductAttributes.Where(m => m.product_id == databaseModel.id && m.is_deleted == false).ToListAsync();
        foreach (var attribute in attributes)
            dto.product_attributes.Add(MapToProductAttributeDto(attribute));

        dto.vendor_name = await _Context.Vendors.Where(m => m.id == databaseModel.vendor_id).Select(m => m.vendor_name).SingleOrDefaultAsync();

        return dto;
    }

    public Database.Models.Product MapToDatabaseModel(ProductDto dtoModel)
    {
        return new Database.Models.Product()
        {
            product_name = dtoModel.product_name,
            sales_price = dtoModel.sales_price,
            unit_cost = dtoModel.unit_cost,
            list_price = dtoModel.list_price,
            product_class = dtoModel.product_class,
            is_sales_item = dtoModel.is_sales_item,
            category = dtoModel.category,
            guid = dtoModel.guid,
            id = dtoModel.id,
            internal_description = dtoModel.internal_description,
            identifier1 = dtoModel.identifier1,
            identifier2 = dtoModel.identifier2,
            identifier3 = dtoModel.identifier3,
            is_labor = dtoModel.is_labor,
            is_material = dtoModel.is_material,
            is_rental_item = dtoModel.is_rental_item,
            is_retired = dtoModel.is_retired,
            is_shippable = dtoModel.is_shippable,
            is_stock = dtoModel.is_stock,
            is_taxable = dtoModel.is_taxable,
            vendor_id = dtoModel.vendor_id,
            rfid_id = dtoModel.rfid_id,
            required_min_order = dtoModel.required_min_order,
            required_reorder_level = dtoModel.required_reorder_level,
            required_stock_level = dtoModel.required_stock_level,
            our_cost = dtoModel.our_cost,
            retired_on = dtoModel.retired_on,
            created_on = dtoModel.created_on,
            updated_on = dtoModel.updated_on,
            external_description = dtoModel.external_description
        };
    }

    public Database.Models.Product MapToDatabaseModel(ProductCreateCommand createCommand)
    {
        return CommonDataHelper<Database.Models.Product>.FillCommonFields(new Database.Models.Product()
        {
            product_name = createCommand.product_name,
            sales_price = createCommand.sales_price,
            unit_cost = createCommand.unit_cost,
            list_price = createCommand.list_price,
            product_class = createCommand.product_class,
            is_sales_item = createCommand.is_sales_item,
            category = createCommand.category,
            guid = createCommand.guid,
            internal_description = createCommand.internal_description,
            identifier1 = createCommand.identifier1,
            identifier2 = createCommand.identifier2,
            identifier3 = createCommand.identifier3,
            is_labor = createCommand.is_labor,
            is_material = createCommand.is_material,
            is_rental_item = createCommand.is_rental_item,
            is_retired = createCommand.is_retired,
            is_shippable = createCommand.is_shippable,
            is_stock = createCommand.is_stock,
            is_taxable = createCommand.is_taxable,
            vendor_id = createCommand.vendor_id,
            rfid_id = createCommand.rfid_id,
            required_min_order = createCommand.required_min_order,
            required_reorder_level = createCommand.required_reorder_level,
            required_stock_level = createCommand.required_stock_level,
            our_cost = createCommand.our_cost,
            external_description = createCommand.external_description,
        }, createCommand.calling_user_id);
    }

    public ProductAttribute MapToAttributeDatabaseModel(ProductAttributeCreateCommand createCommand, int calling_user_id)
    {
        return CommonDataHelper<ProductAttribute>.FillCommonFields(new ProductAttribute()
        {
            product_id = createCommand.product_id.Value,
            attribute_name = createCommand.attribute_name,
            attribute_value = createCommand.attribute_value,
            attribute_value2 = createCommand.attribute_value2,
            attribute_value3 = createCommand.attribute_value3,
        }, calling_user_id);
    }

    public ProductAttributeDto MapToProductAttributeDto(ProductAttribute databaseModel)
    {
        return new ProductAttributeDto()
        {
            attribute_name = databaseModel.attribute_name,
            id = databaseModel.id,
            attribute_value = databaseModel.attribute_value,
            attribute_value2 = databaseModel.attribute_value2,
            attribute_value3 = databaseModel.attribute_value3,
            product_id = databaseModel.product_id,
            created_on_string = databaseModel.created_on_string,
            created_on_timezone = databaseModel.created_on_timezone,
            updated_on_string = databaseModel.updated_on_string,
            updated_on_timezone = databaseModel.updated_on_timezone,
            deleted_by = databaseModel.deleted_by,
            deleted_on = databaseModel.deleted_on,
            updated_by = databaseModel.updated_by,
            updated_on = databaseModel.updated_on,
            created_by = databaseModel.created_by,
            created_on = databaseModel.created_on,
            deleted_on_string = databaseModel.deleted_on_string,
            deleted_on_timezone = databaseModel.deleted_on_timezone,
        };
    }

    private bool ProductExists(ProductCreateCommand createCommand)
    {
        return false;
    }
}
