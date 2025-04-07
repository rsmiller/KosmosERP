using Prometheus.BusinessLayer.Models.Module.APInvoiceLine.Dto;
using Prometheus.BusinessLayer.Models.Module.PurchaseOrderReceive.Command.Create;
using Prometheus.BusinessLayer.Models.Module.PurchaseOrderReceive.Command.Delete;
using Prometheus.BusinessLayer.Models.Module.PurchaseOrderReceive.Command.Edit;
using Prometheus.BusinessLayer.Models.Module.PurchaseOrderReceive.Command.Find;
using Prometheus.BusinessLayer.Models.Module.PurchaseOrderReceive.Dto;
using Prometheus.Database;
using Prometheus.Database.Models;
using Prometheus.Models;
using Prometheus.Models.Interfaces;
using Prometheus.Module;

namespace Prometheus.BusinessLayer.Modules;


public interface IPurchaseOrderReceiveModule : IERPModule<PurchaseOrderReceiveHeader, PurchaseOrderReceiveHeaderDto, PurchaseOrderReceiveHeaderListDto, PurchaseOrderReceiveHeaderCreateCommand, PurchaseOrderReceiveHeaderEditCommand, PurchaseOrderReceiveHeaderDeleteCommand, PurchaseOrderReceiveHeaderFindCommand>, IBaseERPModule
{
    Task<Response<PurchaseOrderReceiveLineDto>> GetLineDto(int object_id);
    Task<Response<APInvoiceLineDto>> CreateLine(PurchaseOrderReceiveLineCreateCommand commandModel);
    Task<Response<APInvoiceLineDto>> EditLine(PurchaseOrderReceiveLineEditCommand commandModel);
    Task<Response<APInvoiceLineDto>> DeleteLine(PurchaseOrderReceiveLineDeleteCommand commandModel);
}

public class PurchaseOrderReceiveModule : BaseERPModule, IPurchaseOrderReceiveModule
{
    public override Guid ModuleIdentifier => Guid.Parse("bdaa14c4-64d8-44f3-b1ad-d272c408832f");
    public override string ModuleName => "PurchaseOrderReceive";

    private IBaseERPContext _Context;

    public PurchaseOrderReceiveModule(IBaseERPContext context) : base(context)
    {
        _Context = context;
    }

    public override void SeedPermissions()
    {
    }

    public PurchaseOrderReceiveHeader? Get(int object_id)
    {
        throw new NotImplementedException();
    }

    public async Task<PurchaseOrderReceiveHeader?> GetAsync(int object_id)
    {
        throw new NotImplementedException();
    }

    public async Task<Response<PurchaseOrderReceiveHeaderDto>> GetDto(int object_id)
    {
        throw new NotImplementedException();
    }
    public async Task<Response<PurchaseOrderReceiveLineDto>> GetLineDto(int object_id)
    {
        throw new NotImplementedException();
    }

    public async Task<Response<PurchaseOrderReceiveHeaderDto>> Create(PurchaseOrderReceiveHeaderCreateCommand commandModel)
    {
        throw new NotImplementedException();
    }

    public async Task<Response<PurchaseOrderReceiveHeaderDto>> Delete(PurchaseOrderReceiveHeaderDeleteCommand commandModel)
    {
        throw new NotImplementedException();
    }

    public async Task<Response<PurchaseOrderReceiveHeaderDto>> Edit(PurchaseOrderReceiveHeaderEditCommand commandModel)
    {
        throw new NotImplementedException();
    }

    public async Task<PagingResult<PurchaseOrderReceiveHeaderListDto>> Find(PagingSortingParameters parameters, PurchaseOrderReceiveHeaderFindCommand commandModel)
    {
        throw new NotImplementedException();
    }

    public async Task<Response<APInvoiceLineDto>> CreateLine(PurchaseOrderReceiveLineCreateCommand commandModel)
    {
        throw new NotImplementedException();
    }

    public async Task<Response<APInvoiceLineDto>> EditLine(PurchaseOrderReceiveLineEditCommand commandModel)
    {
            throw new NotImplementedException();
    }

    public async Task<Response<APInvoiceLineDto>> DeleteLine(PurchaseOrderReceiveLineDeleteCommand commandModel)
    {
        throw new NotImplementedException();
    }

    public async Task<Response<List<PurchaseOrderReceiveHeaderListDto>>> GlobalSearch(PagingSortingParameters parameters, string wildcard)
    {
        throw new NotImplementedException();
    }

    public PurchaseOrderReceiveHeader MapToDatabaseModel(PurchaseOrderReceiveHeaderDto dtoModel)
    {
        throw new NotImplementedException();
    }

    public async Task<PurchaseOrderReceiveHeaderDto> MapToDto(PurchaseOrderReceiveHeader databaseModel)
    {
        throw new NotImplementedException();
    }

    public async Task<PurchaseOrderReceiveHeaderListDto> MapToListDto(PurchaseOrderReceiveHeader databaseModel)
    {
        throw new NotImplementedException();
    }
}
