using KosmosERP.Models;
using KosmosERP.Module;
using KosmosERP.Database;
using KosmosERP.Database.Models;
using KosmosERP.Models.Interfaces;
using KosmosERP.Models.Permissions;
using KosmosERP.BusinessLayer.Models.Module.BOM.Dto;
using KosmosERP.BusinessLayer.Models.Module.BOM.Command.Create;
using KosmosERP.BusinessLayer.Models.Module.BOM.Command.Delete;
using KosmosERP.BusinessLayer.Models.Module.BOM.Command.Edit;
using KosmosERP.BusinessLayer.Models.Module.BOM.Command.Find;
using KosmosERP.BusinessLayer.Helpers;
using KosmosERP.Models.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;


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

    public BOMModule(IBaseERPContext context) : base(context)
    {
        _Context = context;
    }

    public override void SeedPermissions()
    {
        var role = _Context.Roles.Any(m => m.name == "BOM Users");
        var read_permission = _Context.ModulePermissions.Any(m => m.module_id == this.ModuleIdentifier.ToString() && m.internal_permission_name == BOMPermissions.Read);
        var create_permission = _Context.ModulePermissions.Any(m => m.module_id == this.ModuleIdentifier.ToString() && m.internal_permission_name == BOMPermissions.Create);
        var edit_permission = _Context.ModulePermissions.Any(m => m.module_id == this.ModuleIdentifier.ToString() && m.internal_permission_name == BOMPermissions.Edit);
        var delete_permission = _Context.ModulePermissions.Any(m => m.module_id == this.ModuleIdentifier.ToString() && m.internal_permission_name == BOMPermissions.Delete);


        if (role == false)
        {
            _Context.Roles.Add(CommonDataHelper<Role>.FillCommonFields(new Role()
            {
                name = "BOM Users",
                created_by = 1,
                created_on = DateTime.UtcNow,
                updated_by = 1,
                updated_on = DateTime.UtcNow,
            }, 1));

            _Context.SaveChanges();
        }

        var role_id = _Context.Roles.Where(m => m.name == "BOM Users").Select(m => m.id).Single();

        if (read_permission == false)
        {
            _Context.ModulePermissions.Add(new ModulePermission()
            {
                permission_name = "Read BOM",
                internal_permission_name = BOMPermissions.Read,
                module_id = this.ModuleIdentifier.ToString(),
                module_name = this.ModuleName,
                read = true,
            });

            _Context.SaveChanges();

            var read_perm_id = _Context.ModulePermissions.Where(m => m.internal_permission_name == BOMPermissions.Read).Select(m => m.id).Single();

            _Context.RolePermissions.Add(CommonDataHelper<RolePermission>.FillCommonFields(new RolePermission()
            {
                role_id = role_id,
                module_permission_id = read_perm_id,
                created_by = 1,
                created_on = DateTime.UtcNow,
                updated_by = 1,
                updated_on = DateTime.UtcNow,
            }, 1));

            _Context.SaveChanges();
        }

        if (create_permission == false)
        {
            _Context.ModulePermissions.Add(new ModulePermission()
            {
                permission_name = "Create BOM",
                internal_permission_name = BOMPermissions.Create,
                module_id = this.ModuleIdentifier.ToString(),
                module_name = this.ModuleName,
                write = true
            });

            _Context.SaveChanges();

            var create_perm_id = _Context.ModulePermissions.Where(m => m.internal_permission_name == BOMPermissions.Create).Select(m => m.id).Single();

            _Context.RolePermissions.Add(CommonDataHelper<RolePermission>.FillCommonFields(new RolePermission()
            {
                role_id = role_id,
                module_permission_id = create_perm_id,
                created_by = 1,
                created_on = DateTime.UtcNow,
                updated_by = 1,
                updated_on = DateTime.UtcNow,
            }, 1));

            _Context.SaveChanges();
        }

        if (edit_permission == false)
        {
            _Context.ModulePermissions.Add(new ModulePermission()
            {
                permission_name = "Edit BOM",
                internal_permission_name = BOMPermissions.Edit,
                module_id = this.ModuleIdentifier.ToString(),
                module_name = this.ModuleName,
                edit = true
            });

            _Context.SaveChanges();

            var edit_perm_id = _Context.ModulePermissions.Where(m => m.internal_permission_name == BOMPermissions.Edit).Select(m => m.id).Single();

            _Context.RolePermissions.Add(CommonDataHelper<RolePermission>.FillCommonFields(new RolePermission()
            {
                role_id = role_id,
                module_permission_id = edit_perm_id,
                created_by = 1,
                created_on = DateTime.UtcNow,
                updated_by = 1,
                updated_on = DateTime.UtcNow,
            }, 1));

            _Context.SaveChanges();
        }

        if (delete_permission == false)
        {
            _Context.ModulePermissions.Add(new ModulePermission()
            {
                permission_name = "Delete BOM",
                internal_permission_name = BOMPermissions.Delete,
                module_id = this.ModuleIdentifier.ToString(),
                module_name = this.ModuleName,
                delete = true
            });

            _Context.SaveChanges();

            var delete_perm_id = _Context.ModulePermissions.Where(m => m.internal_permission_name == BOMPermissions.Delete).Select(m => m.id).Single();

            _Context.RolePermissions.Add(CommonDataHelper<RolePermission>.FillCommonFields(new RolePermission()
            {
                role_id = role_id,
                module_permission_id = delete_perm_id,
                created_by = 1,
                created_on = DateTime.UtcNow,
                updated_by = 1,
                updated_on = DateTime.UtcNow,
            }, 1));

            _Context.SaveChanges();
        }
    }

    public async Task<Response<BOMDto>> Create(BOMCreateCommand commandModel)
    {
        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<BOMDto>(validationResult.Exception, ResultCode.DataValidationError);

        var permission_result = await base.HasPermission(commandModel.calling_user_id, commandModel.token, BOMPermissions.Create, write: true);
        if (!permission_result)
            return new Response<BOMDto>("Invalid permission", ResultCode.InvalidPermission);

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

        var permission_result = await base.HasPermission(commandModel.calling_user_id, commandModel.token, BOMPermissions.Delete, delete: true);
        if (!permission_result)
            return new Response<BOMDto>("Invalid permission", ResultCode.InvalidPermission);

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

        var permission_result = await base.HasPermission(commandModel.calling_user_id, commandModel.token, BOMPermissions.Edit, edit: true);
        if (!permission_result)
            return new Response<BOMDto>("Invalid permission", ResultCode.InvalidPermission);

        var existingEntity = await GetAsync(commandModel.id);
        if (existingEntity == null)
            return new Response<BOMDto>("BOM not found", ResultCode.NotFound);

        if (commandModel.parent_product_id.HasValue && existingEntity.parent_product_id != commandModel.parent_product_id)
            existingEntity.parent_product_id = commandModel.parent_product_id.Value;

        if (commandModel.quantity.HasValue && existingEntity.quantity != commandModel.quantity)
            existingEntity.quantity = commandModel.quantity.Value;

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
            // Example permission check
            var permission_result = await base.HasPermission(commandModel.calling_user_id, commandModel.token, BOMPermissions.Read, read: true);
            if (!permission_result)
            {
                response.SetException("Invalid permission", ResultCode.InvalidPermission);
                return response;
            }

            var query = _Context.BOMs
                .Where(m => m.is_deleted == false);

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

    public async Task<Response<List<BOMListDto>>> GlobalSearch(PagingSortingParameters parameters, string wildcard)
    {
        throw new NotImplementedException();
    }

    public BOM MapToDatabaseModel(BOMDto dtoModel)
    {
        throw new NotImplementedException();
    }

    public BOM MapForCreate(BOMCreateCommand createCommandModel)
    {
        var address = CommonDataHelper<BOM>.FillCommonFields(new BOM
        {
            instructions = createCommandModel.instructions,
            parent_product_id = createCommandModel.parent_product_id,
            quantity = createCommandModel.quantity,
            is_deleted = false,
        }, createCommandModel.calling_user_id);

        return address;
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
        return new BOMListDto
        {
            id = databaseModel.id,
            instructions = databaseModel.instructions,
            parent_bom_id = databaseModel.parent_bom_id,
            parent_product_id = databaseModel.parent_product_id,
            quantity = databaseModel.quantity,
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
    }
}
