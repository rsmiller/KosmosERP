using Prometheus.BusinessLayer.Models.Module.Order.Command.Create;
using Prometheus.BusinessLayer.Models.Module.Order.Command.Delete;
using Prometheus.BusinessLayer.Models.Module.Order.Command.Edit;
using Prometheus.BusinessLayer.Models.Module.Order.Command.Find;
using Prometheus.BusinessLayer.Models.Module.Order.Dto;
using Prometheus.Database;
using Prometheus.Database.Models;
using Prometheus.Models;
using Prometheus.Models.Interfaces;
using Prometheus.Module;

namespace Prometheus.BusinessLayer.Modules;

public interface IOrderModule : IERPModule<OrderHeader, OrderHeaderDto, OrderHeaderListDto, OrderHeaderCreateCommand, OrderHeaderEditCommand, OrderHeaderDeleteCommand, OrderHeaderFindCommand>, IBaseERPModule
{
    Task<Response<OrderLineDto>> GetLineDto(int object_id);
    Task<Response<OrderLineDto>> CreateLine(OrderLineCreateCommand commandModel);
    Task<Response<OrderLineDto>> EditLine(OrderLineEditCommand commandModel);
    Task<Response<OrderLineDto>> DeleteLine(OrderLineDeleteCommand commandModel);
}

public class OrderModule : BaseERPModule, IOrderModule
{
	public override Guid ModuleIdentifier => Guid.Parse("68c1862f-6043-4b09-8674-dd844a3a6fed");
	public override string ModuleName => "SalesOrder";

	private IBaseERPContext _Context;

	public OrderModule(IBaseERPContext context) : base(context)
	{
		_Context = context;
	}

	public override void SeedPermissions()
	{
	}

    public OrderHeader? Get(int object_id)
    {
        throw new NotImplementedException();
    }

    public async Task<OrderHeader?> GetAsync(int object_id)
    {
        throw new NotImplementedException();
    }

    public async Task<Response<OrderHeaderDto>> GetDto(int object_id)
    {
        throw new NotImplementedException();
    }

    public async Task<Response<OrderLineDto>> GetLineDto(int object_id)
	{
        throw new NotImplementedException();
    }

    public async Task<Response<OrderHeaderDto>> Create(OrderHeaderCreateCommand commandModel)
	{
		throw new NotImplementedException();
	}

	public async Task<Response<OrderHeaderDto>> Delete(OrderHeaderDeleteCommand commandModel)
	{
		throw new NotImplementedException();
	}

	public async Task<Response<OrderHeaderDto>> Edit(OrderHeaderEditCommand commandModel)
	{
		throw new NotImplementedException();
	}

	public async Task<PagingResult<OrderHeaderListDto>> Find(PagingSortingParameters parameters, OrderHeaderFindCommand commandModel)
	{
		throw new NotImplementedException();
	}

	public async Task<Response<List<OrderHeaderListDto>>> GlobalSearch(PagingSortingParameters parameters, string wildcard)
	{
		throw new NotImplementedException();
	}

    public async Task<Response<OrderLineDto>> CreateLine(OrderLineCreateCommand commandModel)
    {
        throw new NotImplementedException();
    }
    
	public async Task<Response<OrderLineDto>> EditLine(OrderLineEditCommand commandModel)
    {
        throw new NotImplementedException();
    }
    
	public async Task<Response<OrderLineDto>> DeleteLine(OrderLineDeleteCommand commandModel)
    {
        throw new NotImplementedException();
    }

    public OrderHeader MapToDatabaseModel(OrderHeaderDto dtoModel)
	{
		throw new NotImplementedException();
	}

	public Task<OrderHeaderDto> MapToDto(OrderHeader databaseModel)
	{
		throw new NotImplementedException();
	}

	public Task<OrderHeaderListDto> MapToListDto(OrderHeader databaseModel)
	{
		throw new NotImplementedException();
	}
}