using KosmosERP.Models;
using KosmosERP.Module;
using KosmosERP.Database;
using KosmosERP.Models.Helpers;
using KosmosERP.Database.Models;
using KosmosERP.Models.Interfaces;
using Microsoft.EntityFrameworkCore;
using KosmosERP.BusinessLayer.Helpers;
using KosmosERP.BusinessLayer.Models.Module.ProductionOrder.Dto;
using KosmosERP.BusinessLayer.Models.Module.ProductionOrder.Command.Create;
using KosmosERP.BusinessLayer.Models.Module.ProductionOrder.Command.Delete;
using KosmosERP.BusinessLayer.Models.Module.ProductionOrder.Command.Edit;
using KosmosERP.BusinessLayer.Models.Module.ProductionOrder.Command.Find;


namespace KosmosERP.BusinessLayer.Modules;

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
	public override string ModuleName => "Production Orders";

	private IBaseERPContext _Context;
    private IMemoryCacheService<KeyValueStore> _KVMemoryService;
    private IMemoryCacheService<Customer> _CustomerMemoryService;
    private IMessagePublisherSettings _MessagePublisherSettings;
    private IPaymentProviderFactory _PaymentFactory;
    private ILogProviderFactory _LogProviderFactory;

    public ProductionOrderModule(IBaseERPContext context, ILogProviderFactory logProviderFactory) : base(context, logProviderFactory)
    {
        _Context = context;
        _LogProviderFactory = logProviderFactory;
    }

    public ProductionOrderModule(IBaseERPContext context, IMemoryCacheService<KeyValueStore> kvMService,
                                    IMessagePublisherSettings messageSettings,
                                    IMemoryCacheService<Customer> customerMemService,
                                    IPaymentProviderFactory providerFactory, 
                                    ILogProviderFactory logProviderFactory) : base(context, logProviderFactory)
    {
        _Context = context;
        _KVMemoryService = kvMService;
        _CustomerMemoryService = customerMemService;
        _MessagePublisherSettings = messageSettings;
        _PaymentFactory = providerFactory;
        _LogProviderFactory = logProviderFactory;
    }

	public override void SeedPermissions()
	{
        var role = _Context.Roles.Any(m => m.name == "Production Order Administrators");

        if (role == false)
        {
            _Context.Roles.Add(CommonDataHelper<Role>.FillCommonFields(new Role()
            {
                name = "Production Order Administrators",
            }, 1));

            _Context.SaveChanges();

            base.CreateFirstRunRolePermissions();
        }


        var submitted_status = _Context.KeyValueStores.Where(m => m.module_id == this.ModuleIdentifier.ToString()
                                    && m.key == "production_order_status_submitted").SingleOrDefault();
        var pulled_status = _Context.KeyValueStores.Where(m => m.module_id == this.ModuleIdentifier.ToString()
                                    && m.key == "production_order_status_pulled").SingleOrDefault();
        var pulled_wip = _Context.KeyValueStores.Where(m => m.module_id == this.ModuleIdentifier.ToString()
                                    && m.key == "production_order_status_wip").SingleOrDefault();
        var pulled_qc = _Context.KeyValueStores.Where(m => m.module_id == this.ModuleIdentifier.ToString()
                                    && m.key == "production_order_status_qc").SingleOrDefault();
        var pulled_complete = _Context.KeyValueStores.Where(m => m.module_id == this.ModuleIdentifier.ToString()
                                    && m.key == "production_order_status_complete").SingleOrDefault();

        var ready_to_ship = _Context.KeyValueStores.Where(m => m.module_id == this.ModuleIdentifier.ToString()
                                    && m.key == "production_order_status_ready_to_ship").SingleOrDefault();

        if (submitted_status == null)
        {
            _Context.KeyValueStores.Add(CommonDataHelper<KeyValueStore>.FillCommonFields(new KeyValueStore()
            {
                key = "production_order_status_submitted",
                value = "Submitted",
                module_id = this.ModuleIdentifier.ToString(),
            }, 1));

            _Context.SaveChanges();
        }

        if (pulled_status == null)
        {
            _Context.KeyValueStores.Add(CommonDataHelper<KeyValueStore>.FillCommonFields(new KeyValueStore()
            {
                key = "production_order_status_pulled",
                value = "Parts Pulled",
                module_id = this.ModuleIdentifier.ToString(),
            }, 1));

            _Context.SaveChanges();
        }

        if (pulled_wip == null)
        {
            _Context.KeyValueStores.Add(CommonDataHelper<KeyValueStore>.FillCommonFields(new KeyValueStore()
            {
                key = "production_order_status_wip",
                value = "Work In Progress",
                module_id = this.ModuleIdentifier.ToString(),
            }, 1));

            _Context.SaveChanges();
        }

        if (pulled_qc == null)
        {
            _Context.KeyValueStores.Add(CommonDataHelper<KeyValueStore>.FillCommonFields(new KeyValueStore()
            {
                key = "production_order_status_qc",
                value = "Quality Check",
                module_id = this.ModuleIdentifier.ToString(),
            }, 1));

            _Context.SaveChanges();
        }

        if (pulled_complete == null)
        {
            _Context.KeyValueStores.Add(CommonDataHelper<KeyValueStore>.FillCommonFields(new KeyValueStore()
            {
                key = "production_order_status_complete",
                value = "Complete",
                module_id = this.ModuleIdentifier.ToString(),
            }, 1));

            _Context.SaveChanges();
        }

        if (ready_to_ship == null)
        {
            _Context.KeyValueStores.Add(CommonDataHelper<KeyValueStore>.FillCommonFields(new KeyValueStore()
            {
                key = "production_order_status_ready_to_ship",
                value = "Ready To Ship",
                module_id = this.ModuleIdentifier.ToString(),
            }, 1));

            _Context.SaveChanges();
        }
    }

    public ProductionOrderHeader? Get(int object_id)
    {
        return _Context.ProductionOrderHeaders.Include("order_header").SingleOrDefault(m => m.id == object_id);
    }

    public ProductionOrderLine? GetLine(int object_id)
    {
        return _Context.ProductionOrderLines.SingleOrDefault(m => m.id == object_id);
    }

    public async Task<ProductionOrderHeader?> GetAsync(int object_id)
    {
        return await _Context.ProductionOrderHeaders.Include("order_header").SingleOrDefaultAsync(m => m.id == object_id);
    }

    public async Task<ProductionOrderLine?> GetLineAsync(int object_id)
    {
        return await _Context.ProductionOrderLines.SingleOrDefaultAsync(m => m.id == object_id);
    }

    public async Task<Response<ProductionOrderHeaderDto>> GetDto(int object_id)
    {
        Response<ProductionOrderHeaderDto> response = new Response<ProductionOrderHeaderDto>();

        var result = await _Context.ProductionOrderHeaders.Include("order_header").SingleOrDefaultAsync(m => m.id == object_id);
        if (result == null)
        {
            response.SetException("AP Invoice Header not found", ResultCode.NotFound);
            return response;
        }

        response.Data = await this.MapToDto(result);
        return response;
    }

    public async Task<Response<ProductionOrderHeaderDto>> GetDtoByGuid(string guid)
    {
        var entity = await _Context.ProductionOrderHeaders.Include("order_header").FirstOrDefaultAsync(c => c.guid == guid && !c.is_deleted);
        if (entity == null)
            return new Response<ProductionOrderHeaderDto>("ProductionOrderHeader not found", ResultCode.NotFound);

        var dto = await MapToDto(entity);
        return new Response<ProductionOrderHeaderDto>(dto);
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

        var existingEntity = await GetAsync(commandModel.id);
        if (existingEntity == null)
            return new Response<ProductionOrderHeaderDto>("Production Order Header not found", ResultCode.NotFound);


        if (!String.IsNullOrEmpty(existingEntity.status) && existingEntity.status != commandModel.status)
            existingEntity.status = commandModel.status;

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


        existingEntity = CommonDataHelper<ProductionOrderHeader>.FillUpdateFields(existingEntity, commandModel.calling_user_id);


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

        var existingEntity = await GetLineAsync(commandModel.id);
        if (existingEntity == null)
            return new Response<ProductionOrderLineDto>("Production Order Line not found", ResultCode.NotFound);


        if (existingEntity.line_number != commandModel.quantity && commandModel.line_number.HasValue)
            existingEntity.line_number = commandModel.line_number.Value;

        if (existingEntity.quantity != commandModel.quantity && commandModel.quantity.HasValue)
            existingEntity.quantity = commandModel.quantity.Value;

        if (!String.IsNullOrEmpty(existingEntity.status) && existingEntity.status != commandModel.status)
            existingEntity.status = commandModel.status;

        if (commandModel.started_on.HasValue && existingEntity.started_on != commandModel.started_on)
            existingEntity.started_on = commandModel.started_on;

        if (commandModel.completed_on.HasValue && existingEntity.completed_on != commandModel.completed_on)
            existingEntity.completed_on = commandModel.completed_on;

        if (commandModel.is_complete.HasValue && existingEntity.is_complete != commandModel.is_complete)
            existingEntity.is_complete = commandModel.is_complete.Value;


        existingEntity = CommonDataHelper<ProductionOrderLine>.FillUpdateFields(existingEntity, commandModel.calling_user_id);


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

        var existingEntity = await GetAsync(commandModel.id);
        if (existingEntity == null)
            return new Response<ProductionOrderHeaderDto>("AP Invoice Header not found", ResultCode.NotFound);

        // Soft Delete
        existingEntity = CommonDataHelper<ProductionOrderHeader>.FillDeleteFields(existingEntity, commandModel.calling_user_id);

        _Context.ProductionOrderHeaders.Update(existingEntity);
        await _Context.SaveChangesAsync();

        var production_order_lines = await _Context.ProductionOrderLines
            .Where(m => m.production_order_header_id == existingEntity.id && !m.is_deleted)
            .ToListAsync();

        foreach(var line in  production_order_lines)
        {
            await this.DeleteLine(new ProductionOrderLineDeleteCommand()
            {
                calling_user_id = commandModel.calling_user_id,
                id = line.id,
            });
        }

        var dto = await MapToDto(existingEntity);
        return new Response<ProductionOrderHeaderDto>(dto);
    }

    public async Task<Response<ProductionOrderLineDto>> DeleteLine(ProductionOrderLineDeleteCommand commandModel)
    {
        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<ProductionOrderLineDto>(validationResult.Exception, ResultCode.DataValidationError);

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
            var query = _Context.ProductionOrderHeaders.Include("order_header")
                .Where(m => !m.is_deleted).Include("order_header");

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

    public async Task<Response<List<ProductionOrderHeaderListDto>>> GlobalSearch(GlobalSearchFindCommand commandModel)
    {
        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<List<ProductionOrderHeaderListDto>>(validationResult.Exception, ResultCode.DataValidationError);


        throw new NotImplementedException();
    }

    public async Task<ProductionOrderHeaderListDto> MapToListDto(ProductionOrderHeader databaseModel)
    {
        var dto = new ProductionOrderHeaderListDto()
        {
            order_header_id = databaseModel.order_header_id,
            status = databaseModel.status,
            priority_id = databaseModel.priority_id,
            planned_start_date = databaseModel.planned_start_date,
            planned_complete_date = databaseModel.planned_complete_date,
            is_complete = databaseModel.is_complete,
            guid = databaseModel.guid,
            id = databaseModel.id,
            production_lead_minutes = databaseModel.production_lead_minutes,
            created_on = databaseModel.created_on,
            updated_on = databaseModel.updated_on,
            created_by = databaseModel.created_by,
            updated_by = databaseModel.updated_by,
            deleted_on = databaseModel.deleted_on,
            deleted_by = databaseModel.deleted_by,
            is_deleted = databaseModel.is_deleted,
            created_on_string = databaseModel.created_on_string,
            created_on_timezone = databaseModel.created_on_timezone,
            updated_on_string = databaseModel.updated_on_string,
            updated_on_timezone = databaseModel.updated_on_timezone,
            deleted_on_string = databaseModel.deleted_on_string,
            deleted_on_timezone = databaseModel.deleted_on_timezone,
        };

        var status_name_val = await _KVMemoryService.GetKeyValue(databaseModel.status);

        if (status_name_val != null)
            dto.status_name = status_name_val.value;

        dto.order_number = databaseModel.order_header.order_number;

        return dto;
    }

    public async Task<ProductionOrderHeaderDto> MapToDto(ProductionOrderHeader databaseModel)
    {
        var dto = new ProductionOrderHeaderDto()
        {
            order_header_id = databaseModel.order_header_id,
            status = databaseModel.status,
            priority_id = databaseModel.priority_id,
            planned_start_date = databaseModel.planned_start_date,
            planned_complete_date = databaseModel.planned_complete_date,
            actual_completed_on = databaseModel.actual_completed_on,
            is_complete = databaseModel.is_complete,
            guid = databaseModel.guid,
            id = databaseModel.id,
            production_lead_minutes = databaseModel.production_lead_minutes,
            created_on = databaseModel.created_on,
            updated_on = databaseModel.updated_on,
            deleted_by = databaseModel.deleted_by,
            deleted_on = databaseModel.deleted_on,
            created_by = databaseModel.created_by,
            updated_by = databaseModel.updated_by,
            is_deleted = databaseModel.is_deleted,
            created_on_string = databaseModel.created_on_string,
            created_on_timezone = databaseModel.created_on_timezone,
            updated_on_string = databaseModel.updated_on_string,
            updated_on_timezone = databaseModel.updated_on_timezone,
            deleted_on_string = databaseModel.deleted_on_string,
            deleted_on_timezone = databaseModel.deleted_on_timezone,
        };

        var production_order_lines = await _Context.ProductionOrderLines
            .Where(m => m.production_order_header_id == databaseModel.id && !m.is_deleted)
            .ToListAsync();

        foreach (var order_line in production_order_lines)
            dto.production_order_lines.Add(await MapToLineDto(order_line));


        var status_name_val = await _KVMemoryService.GetKeyValue(databaseModel.status);

        if (status_name_val != null)
            dto.status_name = status_name_val.value;

        var order_response = await OrderModule.StaticMapToDto(_Context, databaseModel.order_header);
        dto.order_header = order_response;
        
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
            status = databaseModel.status,
            is_complete = databaseModel.is_complete,
            guid = databaseModel.guid,
            id = databaseModel.id,
            production_lead_minutes = databaseModel.production_lead_minutes,
            created_on = databaseModel.created_on,
            updated_on = databaseModel.updated_on,
            deleted_on = databaseModel.deleted_on,
            deleted_by = databaseModel.deleted_by,
            updated_by = databaseModel.updated_by,
            created_by = databaseModel.created_by,
            is_deleted = databaseModel.is_deleted,
            created_on_string = databaseModel.created_on_string,
            created_on_timezone = databaseModel.created_on_timezone,
            updated_on_string = databaseModel.updated_on_string,
            updated_on_timezone = databaseModel.updated_on_timezone,
            deleted_on_string = databaseModel.deleted_on_string,
            deleted_on_timezone = databaseModel.deleted_on_timezone,
        };

        using(var order_module = new OrderModule(_Context, null, _MessagePublisherSettings, _KVMemoryService, _CustomerMemoryService, this, _PaymentFactory, _LogProviderFactory, null))
        {
            var dto_response = await order_module.GetLineDto(databaseModel.order_line_id);
            if (dto_response.Success)
                dto.order_line = dto_response.Data;

        }
        
        using(var bom_module = new BOMModule(_Context, _LogProviderFactory))
        {
            var boms_response = await bom_module.Find(new PagingSortingParameters(1, 1000, "order_number-desc"), new Models.Module.BOM.Command.Find.BOMFindCommand()
            {
                parent_product_id = dto.order_line?.product_id,
            });

            if(boms_response.Success)
            {
                var boms = boms_response.Data;

                foreach(var bom in boms)
                {
                    dto.boms.Add(bom);
                }
            }
        }

        var status_name_val = await _KVMemoryService.GetKeyValue(databaseModel.status);

        if (status_name_val != null)
            dto.status_name = status_name_val.value;

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
            status = databaseModel.status,
            is_complete = databaseModel.is_complete,
            guid = databaseModel.guid,
            id = databaseModel.id,
            production_lead_minutes = databaseModel.production_lead_minutes,
            created_on = databaseModel.created_on,
            updated_on = databaseModel.updated_on,
            deleted_by = databaseModel.deleted_by,
            deleted_on = databaseModel.deleted_on,
            created_by = databaseModel.created_by,
            updated_by = databaseModel.updated_by,
            is_deleted = databaseModel.is_deleted,
            created_on_string = databaseModel.created_on_string,
            created_on_timezone = databaseModel.created_on_timezone,
            updated_on_string = databaseModel.updated_on_string,
            updated_on_timezone = databaseModel.updated_on_timezone,
            deleted_on_string = databaseModel.deleted_on_string,
            deleted_on_timezone = databaseModel.deleted_on_timezone,
        };


        var status_name_val = await _KVMemoryService.GetKeyValue(databaseModel.status);

        if (status_name_val != null)
            dto.status_name = status_name_val.value;
        return dto;
    }

    public ProductionOrderHeader MapToDatabaseModel(ProductionOrderHeaderCreateCommand createCommand)
    {
        return CommonDataHelper<ProductionOrderHeader>.FillCommonFields(new ProductionOrderHeader()
        {
            order_header_id = createCommand.order_header_id,
            status = createCommand.status,
            priority_id = createCommand.priority_id,
            planned_start_date = createCommand.planned_start_date,
            planned_complete_date = createCommand.planned_complete_date,
            guid = Guid.NewGuid().ToString(),
            created_on = DateTime.UtcNow,
            updated_on = DateTime.UtcNow,
            is_deleted = false
        }, createCommand.calling_user_id);
    }

    public async Task<ProductionOrderLine> MapToLineDatabaseModel(ProductionOrderLineCreateCommand createCommand, int production_order_header_id, string calling_user_id)
    {
        var line = CommonDataHelper<ProductionOrderLine>.FillCommonFields(new ProductionOrderLine()
        {
            production_order_header_id = production_order_header_id,
            order_line_id = createCommand.order_line_id,
            quantity = createCommand.quantity,
            started_on = createCommand.started_on,
            completed_on = createCommand.completed_on,
            status = createCommand.status,
            production_lead_minutes = createCommand.production_lead_minutes,
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