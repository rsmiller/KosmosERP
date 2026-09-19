using KosmosERP.Api.Authorization;
using KosmosERP.Api.Models;
using Microsoft.AspNetCore.Mvc;
using KosmosERP.Models;
using KosmosERP.Module;
using KosmosERP.BusinessLayer.Modules;
using KosmosERP.BusinessLayer.Models.Module.Order.Command.Create;
using KosmosERP.BusinessLayer.Models.Module.Order.Command.Delete;
using KosmosERP.BusinessLayer.Models.Module.Order.Command.Edit;
using KosmosERP.BusinessLayer.Models.Module.Order.Command.Find;
using KosmosERP.BusinessLayer.Models.Module.Order.Dto;
using KosmosERP.BusinessLayer.Models.Module.User.ListProfiles;


namespace KosmosERP.Api.Controllers;

//[Authorize]
[ApiController]
[Route("api/v1/[controller]")]
public class OrderController : ERPApiController
{
    private IOrderModule _Module;

    public OrderController(IOrderModule module) : base(module)
    {
        _Module = module;
    }


    [ERPAuthorize(new[] { ERPPermission.Read }, "sales_order_read")]
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

    [ERPAuthorize(new[] { ERPPermission.Read }, "sales_order_read")]
    [HttpGet("GetOrderByGuid", Name = "GetOrderByGuid")]
    [ProducesResponseType(typeof(Response<OrderHeaderDto>), 200)]
    [ProducesResponseType(500)]
    public async Task<ActionResult> GetByGuid([FromQuery] string guid)
    {
        var result = await _Module.GetDtoByGuid(guid);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [ERPAuthorize(new[] { ERPPermission.Read }, "sales_order_read")]
    [HttpPost("FindOrder", Name = "FindOrder")]
    [ProducesResponseType(typeof(PagingResult<OrderHeaderListDto>), 200)]
    [ProducesResponseType(500)]
    public async Task<ActionResult> Find([FromQuery] GeneralListProfile listProfile, [FromBody] OrderHeaderFindCommand command)
    {
        try
        {
            if (command != null)
            {
                command.calling_user_id = this.CurrentUserId;

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

    [ERPAuthorize(new[] { ERPPermission.Write }, "sales_order_write")]
    [HttpPost("CreateOrder", Name = "CreateOrder")]
    [ProducesResponseType(typeof(Response<OrderHeaderDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> Create([FromBody] OrderHeaderCreateCommand createCommand)
    {
        createCommand.calling_user_id = this.CurrentUserId;

        var result = await _Module.Create(createCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [ERPAuthorize(new[] { ERPPermission.Write }, "sales_order_write")]
    [HttpPost("CreateOrderLine", Name = "CreateOrderLine")]
    [ProducesResponseType(typeof(Response<OrderLineDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> CreateLine([FromBody] OrderLineCreateCommand createCommand)
    {
        createCommand.calling_user_id = this.CurrentUserId;

        var result = await _Module.CreateLine(createCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [ERPAuthorize(new[] { ERPPermission.Write }, "sales_order_write")]
    [HttpPost("CreateOrderLineAttribute", Name = "CreateOrderLineAttribute")]
    [ProducesResponseType(typeof(Response<OrderLineAttributeDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> CreateOrderLineAttribute([FromBody] OrderLineAttributeCreateCommand createCommand)
    {
        createCommand.calling_user_id = this.CurrentUserId;

        var result = await _Module.CreateAttribute(createCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [ERPAuthorize(new[] { ERPPermission.Edit }, "sales_order_edit")]
    [HttpPut("UpdateOrder", Name = "UpdateOrder")]
    [ProducesResponseType(typeof(Response<OrderHeaderDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> Edit([FromBody] OrderHeaderEditCommand editCommand)
    {
        editCommand.calling_user_id = this.CurrentUserId;

        var result = await _Module.Edit(editCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [ERPAuthorize(new[] { ERPPermission.Edit }, "sales_order_edit")]
    [HttpPut("UpdateOrderLine", Name = "UpdateOrderLine")]
    [ProducesResponseType(typeof(Response<OrderLineDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> EditLine([FromBody] OrderLineEditCommand editCommand)
    {
        editCommand.calling_user_id = this.CurrentUserId;

        var result = await _Module.EditLine(editCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [ERPAuthorize(new[] { ERPPermission.Edit }, "sales_order_edit")]
    [HttpPut("UpdateOrderLineAttribute", Name = "UpdateOrderLineAttribute")]
    [ProducesResponseType(typeof(Response<OrderLineAttributeDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> UpdateOrderLineAttribute([FromBody] OrderLineAttributeEditCommand editCommand)
    {
        editCommand.calling_user_id = this.CurrentUserId;

        var result = await _Module.EditAttribute(editCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [ERPAuthorize(new[] { ERPPermission.Delete }, "sales_order_delete")]
    [HttpPost("DeleteOrder", Name = "DeleteOrder")]
    [ProducesResponseType(typeof(Response<OrderHeaderDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> Delete([FromBody] OrderHeaderDeleteCommand deleteCommand)
    {
        deleteCommand.calling_user_id = this.CurrentUserId;

        var result = await _Module.Delete(deleteCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [ERPAuthorize(new[] { ERPPermission.Delete }, "sales_order_delete")]
    [HttpPost("DeleteOrderLine", Name = "DeleteOrderLine")]
    [ProducesResponseType(typeof(Response<OrderLineDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> DeleteLine([FromBody] OrderLineDeleteCommand deleteCommand)
    {
        deleteCommand.calling_user_id = this.CurrentUserId;

        var result = await _Module.DeleteLine(deleteCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [ERPAuthorize(new[] { ERPPermission.Delete }, "sales_order_delete")]
    [HttpPost("DeleteOrderLineAttribute", Name = "DeleteOrderLineAttribute")]
    [ProducesResponseType(typeof(Response<OrderLineAttributeDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> DeleteOrderLineAttribute([FromBody] OrderLineAttributeDeleteCommand deleteCommand)
    {
        deleteCommand.calling_user_id = this.CurrentUserId;

        var result = await _Module.DeleteAttribute(deleteCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
}