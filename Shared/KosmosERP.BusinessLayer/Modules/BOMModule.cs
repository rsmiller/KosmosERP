using KosmosERP.Models;
using KosmosERP.Module;
using KosmosERP.Database;
using KosmosERP.Database.Models;
using KosmosERP.Models.Interfaces;
using KosmosERP.BusinessLayer.Models.Module.BOM.Dto;
using KosmosERP.BusinessLayer.Models.Module.BOM.Command.Create;
using KosmosERP.BusinessLayer.Models.Module.BOM.Command.Delete;
using KosmosERP.BusinessLayer.Models.Module.BOM.Command.Edit;
using KosmosERP.BusinessLayer.Models.Module.BOM.Command.Find;
using KosmosERP.BusinessLayer.Helpers;
using KosmosERP.Models.Helpers;
using Microsoft.EntityFrameworkCore;


namespace KosmosERP.BusinessLayer.Modules;

public interface IBOMModule
        : IERPModule<BOM, BOMDto, BOMListDto, BOMCreateCommand, BOMEditCommand, BOMDeleteCommand, BOMFindCommand>, IBaseERPModule
{

}

public class BOMModule : BaseERPModule, IBOMModule
{
    private readonly IBaseERPContext _Context;

    public override Guid ModuleIdentifier => Guid.Parse("737d367d-3a2d-4b07-87ca-33baf7bb55f3");
    public override string ModuleName => "Bill of Materials";

    public BOMModule(IBaseERPContext context, ILogProviderFactory logProviderFactory) : base(context, logProviderFactory)
    {
        _Context = context;
    }

    public override void SeedPermissions()
    {
        var role = _Context.Roles.Any(m => m.name == "BOM Administrators");

        if (role == false)
        {
            _Context.Roles.Add(CommonDataHelper<Role>.FillCommonFields(new Role()
            {
                name = "BOM Administrators",
                created_by = "1",
                created_on = DateTime.UtcNow,
                updated_by = "1",
                updated_on = DateTime.UtcNow,
            }, 1));

            _Context.SaveChanges();

            base.CreateFirstRunRolePermissions();
        }
    }

    public async Task<Response<BOMDto>> Create(BOMCreateCommand commandModel)
    {
        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<BOMDto>(validationResult.Exception, ResultCode.DataValidationError);

        var base_bom = this.MapForCreate(commandModel);

        _Context.BOMs.Add(base_bom);
        await _Context.SaveChangesAsync();

        if (commandModel.child_bom != null)
            await RecursiveCreateBom(commandModel.child_bom, base_bom);

        var dto = await MapToDto(base_bom);
        return new Response<BOMDto>(dto);
    }

    private async Task RecursiveCreateBom(BOMCreateCommand createCommandModel, BOM base_bom)
    {
        var new_bom = this.MapForCreate(createCommandModel);
        
        _Context.BOMs.Add(new_bom);
        await _Context.SaveChangesAsync();

        new_bom.parent_bom_id = base_bom.id;

        _Context.BOMs.Update(base_bom);
        await _Context.SaveChangesAsync();

        if (createCommandModel.child_bom != null)
            await RecursiveCreateBom(createCommandModel.child_bom, new_bom);
    }

    public async Task<Response<BOMDto>> Delete(BOMDeleteCommand commandModel)
    {
        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<BOMDto>(validationResult.Exception, ResultCode.DataValidationError);

        var existingEntity = await GetAsync(commandModel.id);
        if (existingEntity == null)
            return new Response<BOMDto>("BOM not found", ResultCode.NotFound);

        // Soft-deletes
        existingEntity = CommonDataHelper<BOM>.FillDeleteFields(existingEntity, commandModel.calling_user_id);

        _Context.BOMs.Update(existingEntity);
        await _Context.SaveChangesAsync();

        var dto = await MapToDto(existingEntity);
        return new Response<BOMDto>(dto);
    }

    public async Task<Response<BOMDto>> Edit(BOMEditCommand commandModel)
    {
        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<BOMDto>(validationResult.Exception, ResultCode.DataValidationError);

        
        // DO actual edit
        var existingEntity = await GetAsync(commandModel.id);
        if (existingEntity == null)
            return new Response<BOMDto>("BOM not found", ResultCode.NotFound);

        if (commandModel.parent_product_id.HasValue && existingEntity.parent_product_id != commandModel.parent_product_id)
            existingEntity.parent_product_id = commandModel.parent_product_id.Value;

        if (commandModel.quantity.HasValue && existingEntity.quantity != commandModel.quantity)
            existingEntity.quantity = commandModel.quantity.Value;

        if (commandModel.product_id.HasValue && existingEntity.product_id != commandModel.product_id)
            existingEntity.product_id = commandModel.product_id.Value;

        if (commandModel.order_number.HasValue && existingEntity.order_number != commandModel.order_number)
            existingEntity.order_number = commandModel.order_number.Value;

        if (existingEntity.instructions != commandModel.instructions)
            existingEntity.instructions = commandModel.instructions;

        if (existingEntity.parent_bom_id != commandModel.parent_bom_id)
            existingEntity.parent_bom_id = commandModel.parent_bom_id;


        // Update auditing fields
        existingEntity = CommonDataHelper<BOM>.FillUpdateFields(existingEntity, commandModel.calling_user_id);


        _Context.BOMs.Update(existingEntity);
        await _Context.SaveChangesAsync();

        var dto = await MapToDto(existingEntity);
        return new Response<BOMDto>(dto);
    }

    public async Task<PagingResult<BOMListDto>> Find(PagingSortingParameters parameters, BOMFindCommand commandModel)
    {
        var response = new PagingResult<BOMListDto>();

        try
        {
            var query = _Context.BOMs.Include("product").Include("parent_product").Where(m => m.is_deleted == false);

            if (commandModel.parent_product_id.HasValue)
            {
                query = query.Where(m => m.parent_product_id == commandModel.parent_product_id);
            }
            
            if (commandModel.product_id.HasValue)
            {
                query = query.Where(m => m.product_id == commandModel.product_id);
            }


            if (!string.IsNullOrEmpty(commandModel.wildcard))
            {
                var wild = commandModel.wildcard.ToLower();
                query = query.Where(m =>
                    (m.instructions.ToLower().Contains(wild))
                );
            }

            var totalCount = await query.CountAsync();
            var pagedItems = await query.SortAndPageBy(parameters).ToListAsync();

            var dtos = new List<BOMListDto>();
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

    public BOM? Get(int object_id)
    {
        return _Context.BOMs.SingleOrDefault(m => m.id == object_id);
    }

    public async Task<BOM?> GetAsync(int object_id)
    {
        return await _Context.BOMs.SingleOrDefaultAsync(m => m.id == object_id);
    }

    public async Task<Response<BOMDto>> GetDto(int object_id)
    {
        var entity = await GetAsync(object_id);
        if (entity == null)
            return new Response<BOMDto>("BOM not found", ResultCode.NotFound);

        var dto = await MapToDto(entity);
        return new Response<BOMDto>(dto);
    }

    public async Task<Response<BOMDto>> GetDtoByGuid(string guid)
    {
        var entity = await _Context.BOMs.FirstOrDefaultAsync(c => c.guid == guid && !c.is_deleted);
        if (entity == null)
            return new Response<BOMDto>("BOM not found", ResultCode.NotFound);

        var dto = await MapToDto(entity);
        return new Response<BOMDto>(dto);
    }

    public async Task<Response<List<BOMListDto>>> GlobalSearch(GlobalSearchFindCommand commandModel)
    {
        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<List<BOMListDto>>(validationResult.Exception, ResultCode.DataValidationError);

        throw new NotImplementedException();
    }

    public BOM MapToDatabaseModel(BOMDto dtoModel)
    {
        throw new NotImplementedException();
    }

    public BOM MapForCreate(BOMCreateCommand createCommandModel)
    {
        var bom = CommonDataHelper<BOM>.FillCommonFields(new BOM
        {
            instructions = createCommandModel.instructions,
            parent_product_id = createCommandModel.parent_product_id,
            quantity = createCommandModel.quantity,
            is_deleted = false,
            order_number = createCommandModel.order_number,
            product_id = createCommandModel.product_id
        }, createCommandModel.calling_user_id);

        return bom;
    }

    public async Task<BOMDto> MapToDto(BOM databaseModel)
    {
        var parent_bom = new BOMDto
        {
            id = databaseModel.id,
            instructions = databaseModel.instructions,
            bom_id = databaseModel.parent_bom_id,
            parent_product_id = databaseModel.parent_product_id,
            quantity = databaseModel.quantity,
            guid = databaseModel.guid,
            product_id = databaseModel.product_id,
            order_number = databaseModel.order_number,
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
            deleted_on_timezone = databaseModel.deleted_on_timezone
        };


        await RecursiveGetDto(parent_bom, databaseModel.id);

        if (databaseModel.product != null)
            parent_bom.product_name = databaseModel.product.product_name;

        return parent_bom;
    }

    private async Task RecursiveGetDto(BOMDto base_dto, int parent_id)
    {
        List<BOMDto> bom_list = new List<BOMDto>();
        var child_boms = await _Context.BOMs.Where(m => m.parent_bom_id == parent_id).ToListAsync();

        foreach(var bom in child_boms)
        {
            bom_list.Add(await MapToDto(bom));
        }

        base_dto.child_boms = bom_list;
    }

    public async Task<BOMListDto> MapToListDto(BOM databaseModel)
    {
        var dto = new BOMListDto
        {
            id = databaseModel.id,
            instructions = databaseModel.instructions,
            parent_bom_id = databaseModel.parent_bom_id,
            parent_product_id = databaseModel.parent_product_id,
            quantity = databaseModel.quantity,
            product_id = databaseModel.product_id,
            order_number = databaseModel.order_number,
            guid = databaseModel.guid,
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
            deleted_on_timezone = databaseModel.deleted_on_timezone
        };

        if (databaseModel.product != null)
            dto.product_name = databaseModel.product.product_name;

        return dto;
    }
}
