using Prometheus.Models;
using Prometheus.Module;
using Prometheus.Database;
using Prometheus.Models.Helpers;
using Prometheus.Database.Models;
using Prometheus.Models.Interfaces;
using Prometheus.Models.Permissions;
using Microsoft.EntityFrameworkCore;
using Prometheus.BusinessLayer.Helpers;
using Prometheus.BusinessLayer.Models.Module.ProductionOrder.Dto;
using Prometheus.BusinessLayer.Models.Module.ProductionOrder.Command.Create;
using Prometheus.BusinessLayer.Models.Module.ProductionOrder.Command.Delete;
using Prometheus.BusinessLayer.Models.Module.ProductionOrder.Command.Edit;
using Prometheus.BusinessLayer.Models.Module.ProductionOrder.Command.Find;


namespace Prometheus.BusinessLayer.Modules;

public interface IProductionOrderModule : IERPModule<ProductionOrderHeader, ProductionOrderHeaderDto, ProductionOrderHeaderListDto, ProductionOrderHeaderCreateCommand, ProductionOrderHeaderEditCommand, ProductionOrderHeaderDeleteCommand, ProductionOrderHeaderFindCommand>, IBaseERPModule
{
    Task<Response<ProductionOrderLineDto>> GetLineDto(int object_id);
    Task<Response<ProductionOrderLineDto>> CreateLine(ProductionOrderLineCreateCommand commandModel);
    Task<Response<ProductionOrderLineDto>> EditLine(ProductionOrderLineEditCommand commandModel);
    Task<Response<ProductionOrderLineDto>> DeleteLine(ProductionOrderLineDeleteCommand commandModel);
}

public class ProductionOrderModule : BaseERPModule, IProductionOrderModule
{
	public override Guid ModuleIdentifier => Guid.Parse("97dd4b13-ff15-47ff-955d-5e957644cffd");
	public override string ModuleName => "ProductionOrder";

	private IBaseERPContext _Context;

	public ProductionOrderModule(IBaseERPContext context) : base(context)
    {
		_Context = context;
	}

	public override void SeedPermissions()
	{
        var role = _Context.Roles.Any(m => m.name == "Production Order Users");
        var read_permission = _Context.ModulePermissions.Any(m => m.module_id == this.ModuleIdentifier.ToString() && m.internal_permission_name == ProductionOrderPermissions.Read);
        var create_permission = _Context.ModulePermissions.Any(m => m.module_id == this.ModuleIdentifier.ToString() && m.internal_permission_name == ProductionOrderPermissions.Create);
        var edit_permission = _Context.ModulePermissions.Any(m => m.module_id == this.ModuleIdentifier.ToString() && m.internal_permission_name == ProductionOrderPermissions.Edit);
        var delete_permission = _Context.ModulePermissions.Any(m => m.module_id == this.ModuleIdentifier.ToString() && m.internal_permission_name == ProductionOrderPermissions.Delete);

        if (role == false)
        {
            _Context.Roles.Add(new Role()
            {
                name = "Production Order Users",
                created_by = 1,
                created_on = DateTime.UtcNow,
                updated_by = 1,
                updated_on = DateTime.UtcNow,
            });

            _Context.SaveChanges();
        }

        var role_id = _Context.Roles.Where(m => m.name == "Production Order Users").Select(m => m.id).Single();

        if (read_permission == false)
        {
            _Context.ModulePermissions.Add(new ModulePermission()
            {
                permission_name = "Read Production Order",
                internal_permission_name = ProductionOrderPermissions.Read,
                module_id = this.ModuleIdentifier.ToString(),
                module_name = this.ModuleName,
                read = true,
            });

            _Context.SaveChanges();

            var read_perm_id = _Context.ModulePermissions.Where(m => m.internal_permission_name == ProductionOrderPermissions.Read).Select(m => m.id).Single();

            _Context.RolePermissions.Add(new RolePermission()
            {
                role_id = role_id,
                module_permission_id = read_perm_id,
                created_by = 1,
                created_on = DateTime.UtcNow,
                updated_by = 1,
                updated_on = DateTime.UtcNow,
            });

            _Context.SaveChanges();
        }

        if (create_permission == false)
        {
            _Context.ModulePermissions.Add(new ModulePermission()
            {
                permission_name = "Create Production Order",
                internal_permission_name = ProductionOrderPermissions.Create,
                module_id = this.ModuleIdentifier.ToString(),
                module_name = this.ModuleName,
                write = true
            });

            _Context.SaveChanges();

            var create_perm_id = _Context.ModulePermissions.Where(m => m.internal_permission_name == ProductionOrderPermissions.Create).Select(m => m.id).Single();

            _Context.RolePermissions.Add(new RolePermission()
            {
                role_id = role_id,
                module_permission_id = create_perm_id,
                created_by = 1,
                created_on = DateTime.UtcNow,
                updated_by = 1,
                updated_on = DateTime.UtcNow,
            });

            _Context.SaveChanges();
        }

        if (edit_permission == false)
        {
            _Context.ModulePermissions.Add(new ModulePermission()
            {
                permission_name = "Edit Production Order",
                internal_permission_name = ProductionOrderPermissions.Edit,
                module_id = this.ModuleIdentifier.ToString(),
                module_name = this.ModuleName,
                edit = true
            });

            _Context.SaveChanges();

            var edit_perm_id = _Context.ModulePermissions.Where(m => m.internal_permission_name == ProductionOrderPermissions.Edit).Select(m => m.id).Single();

            _Context.RolePermissions.Add(new RolePermission()
            {
                role_id = role_id,
                module_permission_id = edit_perm_id,
                created_by = 1,
                created_on = DateTime.UtcNow,
                updated_by = 1,
                updated_on = DateTime.UtcNow,
            });

            _Context.SaveChanges();
        }

        if (delete_permission == false)
        {
            _Context.ModulePermissions.Add(new ModulePermission()
            {
                permission_name = "Delete Production Order",
                internal_permission_name = ProductionOrderPermissions.Delete,
                module_id = this.ModuleIdentifier.ToString(),
                module_name = this.ModuleName,
                delete = true
            });

            _Context.SaveChanges();

            var delete_perm_id = _Context.ModulePermissions.Where(m => m.internal_permission_name == ProductionOrderPermissions.Delete).Select(m => m.id).Single();

            _Context.RolePermissions.Add(new RolePermission()
            {
                role_id = role_id,
                module_permission_id = delete_perm_id,
                created_by = 1,
                created_on = DateTime.UtcNow,
                updated_by = 1,
                updated_on = DateTime.UtcNow,
            });

            _Context.SaveChanges();
        }
    }

    public ProductionOrderHeader? Get(int object_id)
    {
        return _Context.ProductionOrderHeaders.SingleOrDefault(m => m.id == object_id);
    }

    public ProductionOrderLine? GetLine(int object_id)
    {
        return _Context.ProductionOrderLines.SingleOrDefault(m => m.id == object_id);
    }

    public async Task<ProductionOrderHeader?> GetAsync(int object_id)
    {
        return await _Context.ProductionOrderHeaders.SingleOrDefaultAsync(m => m.id == object_id);
    }

    public async Task<ProductionOrderLine?> GetLineAsync(int object_id)
    {
        return await _Context.ProductionOrderLines.SingleOrDefaultAsync(m => m.id == object_id);
    }

    public async Task<Response<ProductionOrderHeaderDto>> GetDto(int object_id)
    {
        Response<ProductionOrderHeaderDto> response = new Response<ProductionOrderHeaderDto>();

        var result = await _Context.ProductionOrderHeaders.SingleOrDefaultAsync(m => m.id == object_id);
        if (result == null)
        {
            response.SetException("AP Invoice Header not found", ResultCode.NotFound);
            return response;
        }

        response.Data = await this.MapToDto(result);
        return response;
    }

    public async Task<Response<ProductionOrderLineDto>> GetLineDto(int object_id)
    {
        Response<ProductionOrderLineDto> response = new Response<ProductionOrderLineDto>();

        var result = await _Context.ProductionOrderLines.SingleOrDefaultAsync(m => m.id == object_id);
        if (result == null)
        {
            response.SetException("AP Invoice Line not found", ResultCode.NotFound);
            return response;
        }

        response.Data = await this.MapToLineDto(result);
        return response;
    }

    public async Task<Response<ProductionOrderHeaderDto>> Create(ProductionOrderHeaderCreateCommand commandModel)
    {
        if (commandModel == null)
            return new Response<ProductionOrderHeaderDto>(ResultCode.NullItemInput);

        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<ProductionOrderHeaderDto>(validationResult.Exception, ResultCode.DataValidationError);

        var permission_result = await base.HasPermission(commandModel.calling_user_id, commandModel.token, ProductionOrderPermissions.Create, write: true);
        if (!permission_result)
            return new Response<ProductionOrderHeaderDto>("Invalid permission", ResultCode.InvalidPermission);

        try
        {
            var item = MapToDatabaseModel(commandModel);

            await _Context.ProductionOrderHeaders.AddAsync(item);
            await _Context.SaveChangesAsync();

            // Now do lines
            foreach (var ap_line in commandModel.production_order_lines)
            {
                var db_line = await MapToLineDatabaseModel(ap_line, item.id, commandModel.calling_user_id);

                await _Context.ProductionOrderLines.AddAsync(db_line);
                await _Context.SaveChangesAsync();
            }


            var dto = await GetDto(item.id);

            return new Response<ProductionOrderHeaderDto>(dto.Data);
        }
        catch (Exception ex)
        {
            return new Response<ProductionOrderHeaderDto>(ex.Message, ResultCode.Error);
        }
    }

    public async Task<Response<ProductionOrderLineDto>> CreateLine(ProductionOrderLineCreateCommand commandModel)
    {
        if (commandModel == null)
            return new Response<ProductionOrderLineDto>(ResultCode.NullItemInput);

        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<ProductionOrderLineDto>(validationResult.Exception, ResultCode.DataValidationError);

        var permission_result = await base.HasPermission(commandModel.calling_user_id, commandModel.token, ProductionOrderPermissions.Create, write: true);
        if (!permission_result)
            return new Response<ProductionOrderLineDto>("Invalid permission", ResultCode.InvalidPermission);

        if (!commandModel.production_order_header_id.HasValue)
            return new Response<ProductionOrderLineDto>("Production Order Header os a required field", ResultCode.DataValidationError);

        try
        {
            var item = await MapToLineDatabaseModel(commandModel, commandModel.production_order_header_id.Value, commandModel.calling_user_id);

            await _Context.ProductionOrderLines.AddAsync(item);
            await _Context.SaveChangesAsync();

            var dto = await GetLineDto(item.id);

            return new Response<ProductionOrderLineDto>(dto.Data);
        }
        catch (Exception ex)
        {
            return new Response<ProductionOrderLineDto>(ex.Message, ResultCode.Error);
        }
    }

    public async Task<Response<ProductionOrderHeaderDto>> Edit(ProductionOrderHeaderEditCommand commandModel)
    {
        if (commandModel == null)
            return new Response<ProductionOrderHeaderDto>(ResultCode.NullItemInput);

        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<ProductionOrderHeaderDto>(validationResult.Exception, ResultCode.DataValidationError);

        var permission_result = await base.HasPermission(commandModel.calling_user_id, commandModel.token, ProductionOrderPermissions.Edit, edit: true);
        if (!permission_result)
            return new Response<ProductionOrderHeaderDto>("Invalid permission", ResultCode.InvalidPermission);

        var existingEntity = await GetAsync(commandModel.id);
        if (existingEntity == null)
            return new Response<ProductionOrderHeaderDto>("Production Order Header not found", ResultCode.NotFound);


        if (existingEntity.status_id != commandModel.status_id && commandModel.status_id.HasValue)
            existingEntity.status_id = commandModel.status_id.Value;

        if (existingEntity.priority_id != commandModel.priority_id && commandModel.priority_id.HasValue)
            existingEntity.priority_id = commandModel.priority_id.Value;

        if (commandModel.planned_start_date.HasValue && existingEntity.planned_start_date != commandModel.planned_start_date)
            existingEntity.planned_start_date = commandModel.planned_start_date.Value;

        if (commandModel.planned_complete_date.HasValue && existingEntity.planned_complete_date != commandModel.planned_complete_date)
            existingEntity.planned_complete_date = commandModel.planned_complete_date.Value;

        if (commandModel.actual_completed_on.HasValue && existingEntity.actual_completed_on != commandModel.actual_completed_on)
            existingEntity.actual_completed_on = commandModel.actual_completed_on.Value;

        if (commandModel.is_complete.HasValue && existingEntity.is_complete != commandModel.is_complete)
            existingEntity.is_complete = commandModel.is_complete.Value;


        existingEntity.updated_on = DateTime.UtcNow;
        existingEntity.updated_by = commandModel.calling_user_id;

        _Context.ProductionOrderHeaders.Update(existingEntity);
        await _Context.SaveChangesAsync();

        var dto = await MapToDto(existingEntity);
        return new Response<ProductionOrderHeaderDto>(dto);
    }

    public async Task<Response<ProductionOrderLineDto>> EditLine(ProductionOrderLineEditCommand commandModel)
    {
        if (commandModel == null)
            return new Response<ProductionOrderLineDto>(ResultCode.NullItemInput);

        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<ProductionOrderLineDto>(validationResult.Exception, ResultCode.DataValidationError);

        var permission_result = await base.HasPermission(commandModel.calling_user_id, commandModel.token, ProductionOrderPermissions.Edit, edit: true);
        if (!permission_result)
            return new Response<ProductionOrderLineDto>("Invalid permission", ResultCode.InvalidPermission);

        var existingEntity = await GetLineAsync(commandModel.id);
        if (existingEntity == null)
            return new Response<ProductionOrderLineDto>("Production Order Line not found", ResultCode.NotFound);


        if (existingEntity.line_number != commandModel.quantity && commandModel.line_number.HasValue)
            existingEntity.line_number = commandModel.line_number.Value;

        if (existingEntity.quantity != commandModel.quantity && commandModel.quantity.HasValue)
            existingEntity.quantity = commandModel.quantity.Value;

        if (existingEntity.status_id != commandModel.status_id && commandModel.status_id.HasValue)
            existingEntity.status_id = commandModel.status_id.Value;

        if (commandModel.started_on.HasValue && existingEntity.started_on != commandModel.started_on)
            existingEntity.started_on = commandModel.started_on;

        if (commandModel.completed_on.HasValue && existingEntity.completed_on != commandModel.completed_on)
            existingEntity.completed_on = commandModel.completed_on;

        if (commandModel.is_complete.HasValue && existingEntity.is_complete != commandModel.is_complete)
            existingEntity.is_complete = commandModel.is_complete.Value;


        existingEntity.updated_on = DateTime.UtcNow;
        existingEntity.updated_by = commandModel.calling_user_id;

        _Context.ProductionOrderLines.Update(existingEntity);
        await _Context.SaveChangesAsync();

        var dto = await MapToLineDto(existingEntity);
        return new Response<ProductionOrderLineDto>(dto);
    }

    public async Task<Response<ProductionOrderHeaderDto>> Delete(ProductionOrderHeaderDeleteCommand commandModel)
    {
        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<ProductionOrderHeaderDto>(validationResult.Exception, ResultCode.DataValidationError);

        var permission_result = await base.HasPermission(commandModel.calling_user_id, commandModel.token, ProductionOrderPermissions.Delete, delete: true);
        if (!permission_result)
            return new Response<ProductionOrderHeaderDto>("Invalid permission", ResultCode.InvalidPermission);

        var existingEntity = await GetAsync(commandModel.id);
        if (existingEntity == null)
            return new Response<ProductionOrderHeaderDto>("AP Invoice Header not found", ResultCode.NotFound);

        // Soft Delete
        existingEntity = CommonDataHelper<ProductionOrderHeader>.FillDeleteFields(existingEntity, commandModel.calling_user_id);

        _Context.ProductionOrderHeaders.Update(existingEntity);
        await _Context.SaveChangesAsync();

        var dto = await MapToDto(existingEntity);
        return new Response<ProductionOrderHeaderDto>(dto);
    }

    public async Task<Response<ProductionOrderLineDto>> DeleteLine(ProductionOrderLineDeleteCommand commandModel)
    {
        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<ProductionOrderLineDto>(validationResult.Exception, ResultCode.DataValidationError);

        var permission_result = await base.HasPermission(commandModel.calling_user_id, commandModel.token, ProductionOrderPermissions.Delete, delete: true);
        if (!permission_result)
            return new Response<ProductionOrderLineDto>("Invalid permission", ResultCode.InvalidPermission);

        var existingEntity = await GetLineAsync(commandModel.id);
        if (existingEntity == null)
            return new Response<ProductionOrderLineDto>("AP Invoice Line not found", ResultCode.NotFound);

        // Soft Delete
        existingEntity = CommonDataHelper<ProductionOrderLine>.FillDeleteFields(existingEntity, commandModel.calling_user_id);

        _Context.ProductionOrderLines.Update(existingEntity);
        await _Context.SaveChangesAsync();

        var dto = await MapToLineDto(existingEntity);
        return new Response<ProductionOrderLineDto>(dto);
    }

    public async Task<PagingResult<ProductionOrderHeaderListDto>> Find(PagingSortingParameters parameters, ProductionOrderHeaderFindCommand commandModel)
    {
        var response = new PagingResult<ProductionOrderHeaderListDto>();

        try
        {
            var query = _Context.ProductionOrderHeaders
                .Where(m => !m.is_deleted);

            decimal parsed_num = 0;

            if (decimal.TryParse(commandModel.wildcard, out parsed_num))
            {
                var wild = commandModel.wildcard.ToLower();
                query = query.Where(m =>
                    m.order_header.order_number == parsed_num
                );
            }

            var totalCount = await query.CountAsync();
            var pagedItems = await query.SortAndPageBy(parameters).ToListAsync();

            var dtos = new List<ProductionOrderHeaderListDto>();
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

    public async Task<Response<List<ProductionOrderHeaderListDto>>> GlobalSearch(PagingSortingParameters parameters, string wildcard)
    {
        throw new NotImplementedException();
    }

    public async Task<ProductionOrderHeaderListDto> MapToListDto(ProductionOrderHeader databaseModel)
    {
        var dto = new ProductionOrderHeaderListDto()
        {
            order_header_id = databaseModel.order_header_id,
            status_id = databaseModel.status_id,
            priority_id = databaseModel.priority_id,
            planned_start_date = databaseModel.planned_start_date,
            planned_complete_date = databaseModel.planned_complete_date,
            is_complete = databaseModel.is_complete,
            guid = databaseModel.guid,
            id = databaseModel.id,
            created_on = databaseModel.created_on,
            updated_on = databaseModel.updated_on,
            is_deleted = databaseModel.is_deleted,
            created_on_string = databaseModel.created_on_string,
            created_on_timezone = databaseModel.created_on_timezone,
            updated_on_string = databaseModel.updated_on_string,
            updated_on_timezone = databaseModel.updated_on_timezone,
        };

        return dto;
    }

    public async Task<ProductionOrderHeaderDto> MapToDto(ProductionOrderHeader databaseModel)
    {
        var dto = new ProductionOrderHeaderDto()
        {
            order_header_id = databaseModel.order_header_id,
            status_id = databaseModel.status_id,
            priority_id = databaseModel.priority_id,
            planned_start_date = databaseModel.planned_start_date,
            planned_complete_date = databaseModel.planned_complete_date,
            actual_completed_on = databaseModel.actual_completed_on,
            is_complete = databaseModel.is_complete,
            guid = databaseModel.guid,
            id = databaseModel.id,
            created_on = databaseModel.created_on,
            updated_on = databaseModel.updated_on,
            is_deleted = databaseModel.is_deleted,
            created_on_string = databaseModel.created_on_string,
            created_on_timezone = databaseModel.created_on_timezone,
            updated_on_string = databaseModel.updated_on_string,
            updated_on_timezone = databaseModel.updated_on_timezone,
        };

        var production_order_lines = await _Context.ProductionOrderLines
            .Where(m => m.production_order_header_id == databaseModel.id && !m.is_deleted)
            .ToListAsync();

        foreach (var order_line in production_order_lines)
            dto.production_order_lines.Add(await MapToLineDto(order_line));


        return dto;
    }

    public async Task<ProductionOrderLineDto> MapToLineDto(ProductionOrderLine databaseModel)
    {
        var dto = new ProductionOrderLineDto()
        {
            production_order_header_id = databaseModel.production_order_header_id,
            quantity = databaseModel.quantity,
            started_on = databaseModel.started_on,
            order_line_id = databaseModel.order_line_id,
            completed_on = databaseModel.completed_on,
            status_id = databaseModel.status_id,
            is_complete = databaseModel.is_complete,
            guid = databaseModel.guid,
            id = databaseModel.id,
            created_on = databaseModel.created_on,
            updated_on = databaseModel.updated_on,
            is_deleted = databaseModel.is_deleted,
            created_on_string = databaseModel.created_on_string,
            created_on_timezone = databaseModel.created_on_timezone,
            updated_on_string = databaseModel.updated_on_string,
            updated_on_timezone = databaseModel.updated_on_timezone,
        };

        using(var order_module = new OrderModule(_Context, null))
        {
            var dto_response = await order_module.GetLineDto(databaseModel.order_line_id);
            if (dto_response.Success)
                dto.order_line = dto_response.Data;

        }

        return dto;
    }

    private async Task<List<PurchaseOrderReceiveLine>> GetPurchaseOrderReceiveData(int association_object_id)
    {
        List<PurchaseOrderReceiveLine> response = new List<PurchaseOrderReceiveLine>();

        var purchase_order_header = await _Context.PurchaseOrderHeaders
            .Where(m => m.id == association_object_id)
            .SingleOrDefaultAsync();

        if (purchase_order_header != null)
        {

        }


        return response;
    }

    public async Task<ProductionOrderLineListDto> MapToLineListDto(ProductionOrderLine databaseModel)
    {
        var dto = new ProductionOrderLineListDto()
        {
            production_order_header_id = databaseModel.production_order_header_id,
            quantity = databaseModel.quantity,
            started_on = databaseModel.started_on,
            order_line_id = databaseModel.order_line_id,
            completed_on = databaseModel.completed_on,
            status_id = databaseModel.status_id,
            is_complete = databaseModel.is_complete,
            guid = databaseModel.guid,
            id = databaseModel.id,
            created_on = databaseModel.created_on,
            updated_on = databaseModel.updated_on,
            is_deleted = databaseModel.is_deleted,
            created_on_string = databaseModel.created_on_string,
            created_on_timezone = databaseModel.created_on_timezone,
            updated_on_string = databaseModel.updated_on_string,
            updated_on_timezone = databaseModel.updated_on_timezone,
        };

        return dto;
    }

    public ProductionOrderHeader MapToDatabaseModel(ProductionOrderHeaderCreateCommand createCommand)
    {
        return CommonDataHelper<ProductionOrderHeader>.FillCommonFields(new ProductionOrderHeader()
        {
            order_header_id = createCommand.order_header_id,
            status_id = createCommand.status_id,
            priority_id = createCommand.priority_id,
            planned_start_date = createCommand.planned_start_date,
            planned_complete_date = createCommand.planned_complete_date,
            guid = Guid.NewGuid().ToString(),
            created_on = DateTime.UtcNow,
            updated_on = DateTime.UtcNow,
            is_deleted = false
        }, createCommand.calling_user_id);
    }

    public async Task<ProductionOrderLine> MapToLineDatabaseModel(ProductionOrderLineCreateCommand createCommand, int production_order_header_id, int calling_user_id)
    {
        var line = CommonDataHelper<ProductionOrderLine>.FillCommonFields(new ProductionOrderLine()
        {
            production_order_header_id = production_order_header_id,
            order_line_id = createCommand.order_line_id,
            quantity = createCommand.quantity,
            started_on = createCommand.started_on,
            completed_on = createCommand.completed_on,
            status_id = createCommand.status_id,
            guid = Guid.NewGuid().ToString(),
            is_deleted = false,
        }, calling_user_id);


        var order_line = await _Context.OrderLines.Where(m => m.id == createCommand.order_line_id).SingleOrDefaultAsync();

        if (order_line == null)
            throw new Exception("Order line not found");

        return line;
    }

    public ProductionOrderHeader MapToDatabaseModel(ProductionOrderHeaderDto dtoModel)
    {
        throw new NotImplementedException();
    }
}