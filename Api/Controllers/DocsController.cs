using KosmosERP.BusinessLayer.Models.Module.ARInvoice.Dto;
using KosmosERP.BusinessLayer.Models.Module.Order.Dto;
using KosmosERP.BusinessLayer.Models.Module.ProductionOrder.Dto;
using KosmosERP.BusinessLayer.Models.Module.PurchaseOrder.Dto;
using KosmosERP.BusinessLayer.Models.Module.Shipment.Dto;
using KosmosERP.BusinessLayer.Modules;
using KosmosERP.Models;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/v1/[controller]")]
public class DocsController : ControllerBase
{

    private IPurchaseOrderModule _PurchaseOrderModule;
    private IOrderModule _OrderModule;
    private IShipmentModule _ShipmentModule;
    private IARInvoiceModule _ARModule;
    private IProductionOrderModule _ProductionOrderModule;

    public DocsController(IPurchaseOrderModule purchaseOrderModule, 
                            IOrderModule salesOrderModule, 
                            IShipmentModule shipmentModule, 
                            IARInvoiceModule arInvoiceModule,
                            IProductionOrderModule productionOrderModule)
    {
        _PurchaseOrderModule = purchaseOrderModule;
        _OrderModule = salesOrderModule;
        _ShipmentModule = shipmentModule;
        _ARModule = arInvoiceModule;
        _ProductionOrderModule = productionOrderModule;
    }

    [HttpGet("DocsGetPurchaseOrderByGuid", Name = "DocsGetPurchaseOrderByGuid")]
    [ProducesResponseType(typeof(Response<PurchaseOrderHeaderDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> DocsGetPurchaseOrderByGuid([FromQuery] string guid)
    {
        var result = await _PurchaseOrderModule.GetDtoByGuid(guid);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpGet("DocsGetOrderByGuid", Name = "DocsGetOrderByGuid")]
    [ProducesResponseType(typeof(Response<OrderHeaderDto>), 200)]
    [ProducesResponseType(500)]
    public async Task<ActionResult> DocsGetOrderByGuid([FromQuery] string guid)
    {
        var result = await _OrderModule.GetDtoByGuid(guid);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpGet("DocsGetShipmentByGuid", Name = "DocsGetShipmentByGuid")]
    [ProducesResponseType(typeof(Response<ShipmentHeaderDto>), 200)]
    [ProducesResponseType(500)]
    public async Task<ActionResult> DocsGetShipmentByGuid([FromQuery] string guid)
    {
        var result = await _ShipmentModule.GetDtoByGuid(guid);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpGet("DocsGetARInvoiceByGuid", Name = "DocsGetARInvoiceByGuid")]
    [ProducesResponseType(typeof(Response<ARInvoiceHeaderDto>), 200)]
    [ProducesResponseType(500)]
    public async Task<ActionResult> DocsGetARInvoiceByGuid([FromQuery] string guid)
    {
        var result = await _ARModule.GetDtoByGuid(guid);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpGet("DocsGetProductionOrderByGuid", Name = "DocsGetProductionOrderByGuid")]
    [ProducesResponseType(typeof(Response<ProductionOrderHeaderDto>), 200)]
    [ProducesResponseType(500)]
    public async Task<ActionResult> DocsGetProductionOrderByGuid([FromQuery] string guid)
    {
        var result = await _ProductionOrderModule.GetDtoByGuid(guid);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
}