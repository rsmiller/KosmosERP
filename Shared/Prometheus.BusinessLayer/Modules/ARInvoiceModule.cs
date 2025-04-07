using Prometheus.BusinessLayer.Models.Module.ARInvoice.Command.Create;
using Prometheus.BusinessLayer.Models.Module.ARInvoice.Command.Delete;
using Prometheus.BusinessLayer.Models.Module.ARInvoice.Command.Edit;
using Prometheus.BusinessLayer.Models.Module.ARInvoice.Command.Find;
using Prometheus.BusinessLayer.Models.Module.ARInvoice.Dto;
using Prometheus.Database;
using Prometheus.Database.Models;
using Prometheus.Models;
using Prometheus.Models.Interfaces;
using Prometheus.Module;

namespace Prometheus.BusinessLayer.Modules;

public interface IARInvoiceModule : IERPModule<ARInvoiceHeader, ARInvoiceHeaderDto, ARInvoiceHeaderListDto, ARInvoiceHeaderCreateCommand, ARInvoiceHeaderEditCommand, ARInvoiceHeaderDeleteCommand, ARInvoiceHeaderFindCommand>, IBaseERPModule
{
    Task<Response<ARInvoiceLineDto>> GetLineDto(int object_id);
    Task<Response<ARInvoiceLineDto>> CreateLine(ARInvoiceLineCreateCommand commandModel);
    Task<Response<ARInvoiceLineDto>> EditLine(ARInvoiceLineEditCommand commandModel);
    Task<Response<ARInvoiceLineDto>> DeleteLine(ARInvoiceLineDeleteCommand commandModel);
}

public class ARInvoiceModule : BaseERPModule, IARInvoiceModule
{
	public override Guid ModuleIdentifier => Guid.Parse("fab75a7f-af8c-4416-9e40-9c774aba1811");
	public override string ModuleName => "ARInvoice";

	private IBaseERPContext _Context;

	public ARInvoiceModule(IBaseERPContext context) : base(context)
	{
		_Context = context;
	}

	public override void SeedPermissions()
	{
	}

    public ARInvoiceHeader? Get(int object_id)
    {
        throw new NotImplementedException();
    }

    public async Task<ARInvoiceHeader?> GetAsync(int object_id)
    {
        throw new NotImplementedException();
    }

    public async Task<Response<ARInvoiceHeaderDto>> GetDto(int object_id)
    {
        throw new NotImplementedException();
    }

    public async Task<Response<ARInvoiceLineDto>> GetLineDto(int object_id)
	{
        throw new NotImplementedException();
    }

    public async Task<Response<ARInvoiceHeaderDto>> Create(ARInvoiceHeaderCreateCommand commandModel)
	{
		throw new NotImplementedException();
	}

	public async Task<Response<ARInvoiceHeaderDto>> Delete(ARInvoiceHeaderDeleteCommand commandModel)
	{
		throw new NotImplementedException();
	}

	public async Task<Response<ARInvoiceHeaderDto>> Edit(ARInvoiceHeaderEditCommand commandModel)
	{
		throw new NotImplementedException();
	}

	public async Task<PagingResult<ARInvoiceHeaderListDto>> Find(PagingSortingParameters parameters, ARInvoiceHeaderFindCommand commandModel)
	{
		throw new NotImplementedException();
	}

	public async Task<Response<List<ARInvoiceHeaderListDto>>> GlobalSearch(PagingSortingParameters parameters, string wildcard)
	{
		throw new NotImplementedException();
	}

    public async Task<Response<ARInvoiceLineDto>> CreateLine(ARInvoiceLineCreateCommand commandModel)
    {
        throw new NotImplementedException();
    }
    
	public async Task<Response<ARInvoiceLineDto>> EditLine(ARInvoiceLineEditCommand commandModel)
    {
        throw new NotImplementedException();
    }
    
	public async Task<Response<ARInvoiceLineDto>> DeleteLine(ARInvoiceLineDeleteCommand commandModel)
    {
        throw new NotImplementedException();
    }

    public ARInvoiceHeader MapToDatabaseModel(ARInvoiceHeaderDto dtoModel)
	{
		throw new NotImplementedException();
	}

	public Task<ARInvoiceHeaderDto> MapToDto(ARInvoiceHeader databaseModel)
	{
		throw new NotImplementedException();
	}

	public Task<ARInvoiceHeaderListDto> MapToListDto(ARInvoiceHeader databaseModel)
	{
		throw new NotImplementedException();
	}
}