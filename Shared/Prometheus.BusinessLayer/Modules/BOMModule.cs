using Prometheus.Models;
using Prometheus.Module;
using Prometheus.Database;
using Prometheus.Database.Models;
using Prometheus.Models.Interfaces;
using Prometheus.Models.Permissions;
using Prometheus.BusinessLayer.Models.Module.BOM.Dto;
using Prometheus.BusinessLayer.Models.Module.BOM.Command.Create;
using Prometheus.BusinessLayer.Models.Module.BOM.Command.Delete;
using Prometheus.BusinessLayer.Models.Module.BOM.Command.Edit;
using Prometheus.BusinessLayer.Models.Module.BOM.Command.Find;


namespace Prometheus.BusinessLayer.Modules;

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
    }

    public async Task<Response<BOMDto>> Create(BOMCreateCommand commandModel)
    {
        throw new NotImplementedException();
    }

    public async Task<Response<BOMDto>> Delete(BOMDeleteCommand commandModel)
    {
        throw new NotImplementedException();
    }

    public async Task<Response<BOMDto>> Edit(BOMEditCommand commandModel)
    {
        throw new NotImplementedException();
    }

    public async Task<PagingResult<BOMListDto>> Find(PagingSortingParameters parameters, BOMFindCommand commandModel)
    {
        throw new NotImplementedException();
    }

    public BOM? Get(int object_id)
    {
        throw new NotImplementedException();
    }

    public async Task<BOM?> GetAsync(int object_id)
    {
        throw new NotImplementedException();
    }

    public async Task<Response<BOMDto>> GetDto(int object_id)
    {
        throw new NotImplementedException();
    }

    public async Task<Response<List<BOMListDto>>> GlobalSearch(PagingSortingParameters parameters, string wildcard)
    {
        throw new NotImplementedException();
    }

    public BOM MapToDatabaseModel(BOMDto dtoModel)
    {
        throw new NotImplementedException();
    }

    public async Task<BOMDto> MapToDto(BOM databaseModel)
    {
        throw new NotImplementedException();
    }

    public async Task<BOMListDto> MapToListDto(BOM databaseModel)
    {
        throw new NotImplementedException();
    }
}
