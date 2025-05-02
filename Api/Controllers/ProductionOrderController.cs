using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using KosmosERP.Models;
using KosmosERP.Module;
using KosmosERP.BusinessLayer.Modules;
using KosmosERP.BusinessLayer.Models.Module.ProductionOrder.Command.Create;
using KosmosERP.BusinessLayer.Models.Module.ProductionOrder.Command.Delete;
using KosmosERP.BusinessLayer.Models.Module.ProductionOrder.Command.Edit;
using KosmosERP.BusinessLayer.Models.Module.ProductionOrder.Command.Find;
using KosmosERP.BusinessLayer.Models.Module.ProductionOrder.Dto;
using KosmosERP.BusinessLayer.Models.Module.User.ListProfiles;


namespace KosmosERP.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/[controller]")]
public class ProductionOrderController : ERPApiController
{
    private IProductionOrderModule _Module;

    public ProductionOrderController(IProductionOrderModule module) : base(module)
    {
        _Module = module;
    }


    [HttpGet("GetProductionOrder", Name = "GetProductionOrder")]
    [ProducesResponseType(typeof(Response<ProductionOrderHeaderDto>), 200)]
    [ProducesResponseType(500)]
    public async Task<ActionResult> Get([FromQuery] int id)
    {
        var result = await _Module.GetDto(id);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPost("FindProductionOrder", Name = "FindProductionOrder")]
    [ProducesResponseType(typeof(PagingResult<ProductionOrderHeaderListDto>), 200)]
    [ProducesResponseType(500)]
    public async Task<ActionResult> Find([FromQuery] GeneralListProfile listProfile, [FromBody] ProductionOrderHeaderFindCommand command)
    {
        try
        {
            if (command != null)
            {
                var sortingParams = new PagingSortingParameters(listProfile.Start, listProfile.ResultCount, listProfile.SortOrder);

                var result = await _Module.Find(sortingParams, command);

                return Ok(result);
            }
            else
            {
                return StatusCode(500, "Api body is null");
            }
        }
        catch (Exception e)
        {
            return StatusCode(500, e.Message);
        }
    }

    [HttpPost("CreateProductionOrder", Name = "CreateProductionOrder")]
    [ProducesResponseType(typeof(Response<ProductionOrderHeaderDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> Create([FromBody] ProductionOrderHeaderCreateCommand createCommand)
    {
        var result = await _Module.Create(createCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPost("CreateProductionOrderLine", Name = "CreateProductionOrderLine")]
    [ProducesResponseType(typeof(Response<ProductionOrderLineDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> CreateLine([FromBody] ProductionOrderLineCreateCommand createCommand)
    {
        var result = await _Module.CreateLine(createCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPut("UpdateProductionOrder", Name = "UpdateProductionOrder")]
    [ProducesResponseType(typeof(Response<ProductionOrderHeaderDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> Edit([FromBody] ProductionOrderHeaderEditCommand editCommand)
    {
        var result = await _Module.Edit(editCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPut("UpdateProductionOrderLine", Name = "UpdateProductionOrderLine")]
    [ProducesResponseType(typeof(Response<ProductionOrderLineDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> EditLine([FromBody] ProductionOrderLineEditCommand editCommand)
    {
        var result = await _Module.EditLine(editCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpDelete("DeleteProductionOrder", Name = "DeleteProductionOrder")]
    [ProducesResponseType(typeof(Response<ProductionOrderHeaderDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> Delete([FromBody] ProductionOrderHeaderDeleteCommand deleteCommand)
    {
        var result = await _Module.Delete(deleteCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpDelete("DeleteProductionOrderLine", Name = "DeleteProductionOrderLine")]
    [ProducesResponseType(typeof(Response<ProductionOrderLineDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> DeleteLine([FromBody] ProductionOrderLineDeleteCommand deleteCommand)
    {
        var result = await _Module.DeleteLine(deleteCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
}