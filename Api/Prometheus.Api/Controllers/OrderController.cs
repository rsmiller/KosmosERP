using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Prometheus.Models;
using Prometheus.Module;
using Prometheus.BusinessLayer.Modules;
using Prometheus.BusinessLayer.Models.Module.Order.Command.Create;
using Prometheus.BusinessLayer.Models.Module.Order.Command.Delete;
using Prometheus.BusinessLayer.Models.Module.Order.Command.Edit;
using Prometheus.BusinessLayer.Models.Module.Order.Command.Find;
using Prometheus.BusinessLayer.Models.Module.Order.Dto;
using Prometheus.BusinessLayer.Models.Module.User.ListProfiles;


namespace Prometheus.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/[controller]")]
public class OrderController : ERPApiController
{
    private IOrderModule _Module;

    public OrderController(IOrderModule module) : base(module)
    {
        _Module = module;
    }


    [HttpGet("GetOrder", Name = "GetOrder")]
    [ProducesResponseType(typeof(Response<OrderHeaderDto>), 200)]
    [ProducesResponseType(500)]
    public async Task<ActionResult> Get([FromQuery] int id)
    {
        var result = await _Module.GetDto(id);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPost("FindOrder", Name = "FindOrder")]
    [ProducesResponseType(typeof(PagingResult<OrderHeaderListDto>), 200)]
    [ProducesResponseType(500)]
    public async Task<ActionResult> Find([FromQuery] GeneralListProfile listProfile, [FromBody] OrderHeaderFindCommand command)
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

    [HttpPost("CreateOrder", Name = "CreateOrder")]
    [ProducesResponseType(typeof(Response<OrderHeaderDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> Create([FromBody] OrderHeaderCreateCommand createCommand)
    {
        var result = await _Module.Create(createCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPost("CreateOrderLine", Name = "CreateOrderLine")]
    [ProducesResponseType(typeof(Response<OrderLineDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> CreateLine([FromBody] OrderLineCreateCommand createCommand)
    {
        var result = await _Module.CreateLine(createCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPost("CreateOrderLineAttribute", Name = "CreateOrderLineAttribute")]
    [ProducesResponseType(typeof(Response<OrderLineAttributeDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> CreateOrderLineAttribute([FromBody] OrderLineAttributeCreateCommand createCommand)
    {
        var result = await _Module.CreateAttribute(createCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPut("UpdateOrder", Name = "UpdateOrder")]
    [ProducesResponseType(typeof(Response<OrderHeaderDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> Edit([FromBody] OrderHeaderEditCommand editCommand)
    {
        var result = await _Module.Edit(editCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPut("UpdateOrderLine", Name = "UpdateOrderLine")]
    [ProducesResponseType(typeof(Response<OrderLineDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> EditLine([FromBody] OrderLineEditCommand editCommand)
    {
        var result = await _Module.EditLine(editCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPut("UpdateOrderLineAttribute", Name = "UpdateOrderLineAttribute")]
    [ProducesResponseType(typeof(Response<OrderLineAttributeDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> UpdateOrderLineAttribute([FromBody] OrderLineAttributeEditCommand editCommand)
    {
        var result = await _Module.EditAttribute(editCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpDelete("DeleteOrder", Name = "DeleteOrder")]
    [ProducesResponseType(typeof(Response<OrderHeaderDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> Delete([FromBody] OrderHeaderDeleteCommand deleteCommand)
    {
        var result = await _Module.Delete(deleteCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpDelete("DeleteOrderLine", Name = "DeleteOrderLine")]
    [ProducesResponseType(typeof(Response<OrderLineDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> DeleteLine([FromBody] OrderLineDeleteCommand deleteCommand)
    {
        var result = await _Module.DeleteLine(deleteCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpDelete("DeleteOrderLineAttribute", Name = "DeleteOrderLineAttribute")]
    [ProducesResponseType(typeof(Response<OrderLineAttributeDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> DeleteOrderLineAttribute([FromBody] OrderLineAttributeDeleteCommand deleteCommand)
    {
        var result = await _Module.DeleteAttribute(deleteCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
}