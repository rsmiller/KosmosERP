using KosmosERP.Api.Authorization;
using KosmosERP.Api.Models;
using Microsoft.AspNetCore.Mvc;
using KosmosERP.BusinessLayer.Models.Module.PurchaseOrder.Dto;
using KosmosERP.BusinessLayer.Models.Module.PurchaseOrder.Command.Create;
using KosmosERP.BusinessLayer.Models.Module.PurchaseOrder.Command.Delete;
using KosmosERP.BusinessLayer.Models.Module.PurchaseOrder.Command.Find;
using KosmosERP.BusinessLayer.Modules;
using KosmosERP.Models;
using KosmosERP.Module;
using KosmosERP.BusinessLayer.Models.Module.User.ListProfiles;
using KosmosERP.BusinessLayer.Models.Module.PurchaseOrder.Command.Edit;

namespace KosmosERP.Api.Controllers;

//[Authorize]
[ApiController]
[Route("api/v1/[controller]")]
public class PurchaseOrderController : ERPApiController
{
    private IPurchaseOrderModule _Module;

    public PurchaseOrderController(IPurchaseOrderModule module) : base(module)
    {
        _Module = module;
    }

    
    [ERPAuthorize(new[] { ERPPermission.Read }, "purchase_order_read")]
    [HttpGet("GetPurchaseOrderHeader", Name = "GetPurchaseOrderHeader")]
    [ProducesResponseType(typeof(Response<PurchaseOrderHeaderDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> Get([FromQuery] int id)
    {
        var result = await _Module.GetDto(id);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [ERPAuthorize(new[] { ERPPermission.Read }, "purchase_order_read")]
    [HttpGet("GetPurchaseOrderHeaderByGuid", Name = "GetPurchaseOrderHeaderByGuid")]
    [ProducesResponseType(typeof(Response<PurchaseOrderHeaderDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> GetByGuid([FromQuery] string guid)
    {
        var result = await _Module.GetDtoByGuid(guid);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [ERPAuthorize(new[] { ERPPermission.Read }, "purchase_order_read")]
    [HttpGet("GetByPONumber", Name = "GetByPONumber")]
    [ProducesResponseType(typeof(Response<PurchaseOrderHeaderDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> GetByPONumber([FromQuery] int po_number)
    {
        var result = await _Module.GetByPONumber(po_number);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [ERPAuthorize(new[] { ERPPermission.Read }, "purchase_order_read")]
    [HttpGet("GetPurchaseOrderLine", Name = "GetPurchaseOrderLine")]
    [ProducesResponseType(typeof(Response<PurchaseOrderLineDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> GetLine([FromQuery] int id)
    {
        var result = await _Module.GetLineDto(id);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [ERPAuthorize(new[] { ERPPermission.Read }, "purchase_order_read")]
    [HttpPost("FindPurchaseOrderHeader", Name = "FindPurchaseOrderHeader")]
    [ProducesResponseType(typeof(PagingResult<PurchaseOrderHeaderListDto>), 200)]
    [ProducesResponseType(500)]
    public async Task<ActionResult> Find([FromQuery] GeneralListProfile listProfile, [FromBody] PurchaseOrderHeaderFindCommand command)
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

    [ERPAuthorize(new[] { ERPPermission.Write }, "purchase_order_write")]
    [HttpPost("CreatePurchaseOrderHeader", Name = "CreatePurchaseOrderHeader")]
    [ProducesResponseType(typeof(Response<PurchaseOrderHeaderDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> Create([FromBody] PurchaseOrderHeaderCreateCommand createCommand)
    {
        createCommand.calling_user_id = this.CurrentUserId;
        var result = await _Module.Create(createCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [ERPAuthorize(new[] { ERPPermission.Write }, "purchase_order_write")]
    [HttpPost("CreatePurchaseOrderLine", Name = "CreatePurchaseOrderLine")]
    [ProducesResponseType(typeof(Response<PurchaseOrderLineDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> CreateLine([FromBody] PurchaseOrderLineCreateCommand createCommand)
    {
        createCommand.calling_user_id = this.CurrentUserId;
        var result = await _Module.CreateLine(createCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [ERPAuthorize(new[] { ERPPermission.Edit }, "purchase_order_edit")]
    [HttpPut("UpdatePurchaseOrderHeader", Name = "UpdatePurchaseOrderHeader")]
    [ProducesResponseType(typeof(Response<PurchaseOrderHeaderDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> Edit([FromBody] PurchaseOrderHeaderEditCommand editCommand)
    {
        editCommand.calling_user_id = this.CurrentUserId;
        var result = await _Module.Edit(editCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [ERPAuthorize(new[] { ERPPermission.Edit }, "purchase_order_edit")]
    [HttpPut("UpdatePurchaseOrderLine", Name = "UpdatePurchaseOrderLine")]
    [ProducesResponseType(typeof(Response<PurchaseOrderLineDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> EditLine([FromBody] PurchaseOrderLineEditCommand editCommand)
    {
        editCommand.calling_user_id = this.CurrentUserId;
        var result = await _Module.EditLine(editCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [ERPAuthorize(new[] { ERPPermission.Delete }, "purchase_order_delete")]
    [HttpPost("DeletePurchaseOrderHeader", Name = "DeletePurchaseOrderHeader")]
    [ProducesResponseType(typeof(Response<PurchaseOrderHeaderDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> Delete([FromBody] PurchaseOrderHeaderDeleteCommand deleteCommand)
    {
        deleteCommand.calling_user_id = this.CurrentUserId;
        var result = await _Module.Delete(deleteCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [ERPAuthorize(new[] { ERPPermission.Delete }, "purchase_order_delete")]
    [HttpPost("DeletePurchaseOrderLine", Name = "DeletePurchaseOrderLine")]
    [ProducesResponseType(typeof(Response<PurchaseOrderHeaderDto>), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<ActionResult> DeleteLine([FromBody] PurchaseOrderLineDeleteCommand deleteCommand)
    {
        deleteCommand.calling_user_id = this.CurrentUserId;
        var result = await _Module.DeleteLine(deleteCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
}
