using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using KosmosERP.Models;
using KosmosERP.Module;
using KosmosERP.BusinessLayer.Modules;
using KosmosERP.BusinessLayer.Models.Module.PurchaseOrderReceive.Dto;
using KosmosERP.BusinessLayer.Models.Module.PurchaseOrderReceive.Command.Create;
using KosmosERP.BusinessLayer.Models.Module.PurchaseOrderReceive.Command.Delete;
using KosmosERP.BusinessLayer.Models.Module.PurchaseOrderReceive.Command.Find;
using KosmosERP.BusinessLayer.Models.Module.User.ListProfiles;
using KosmosERP.BusinessLayer.Models.Module.PurchaseOrderReceive.Command.Edit;

namespace KosmosERP.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/[controller]")]
public class PurchaseOrderReceiveController : ERPApiController
{
    private IPurchaseOrderReceiveModule _Module;

    public PurchaseOrderReceiveController(IPurchaseOrderReceiveModule module) : base(module)
    {
        _Module = module;
    }

    
    [HttpGet("GetPurchaseOrderReceiveHeader", Name = "GetPurchaseOrderReceiveHeader")]
    [ProducesResponseType(typeof(Response<PurchaseOrderReceiveHeaderDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> Get([FromQuery] int id)
    {
        var result = await _Module.GetDto(id);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpGet("GetPurchaseOrderReceiveLine", Name = "GetPurchaseOrderReceiveLine")]
    [ProducesResponseType(typeof(Response<PurchaseOrderReceiveLineDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> GetLine([FromQuery] int id)
    {
        var result = await _Module.GetLineDto(id);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPost("FindPurchaseOrderReceiveHeader", Name = "FindPurchaseOrderReceiveHeader")]
    [ProducesResponseType(typeof(PagingResult<PurchaseOrderReceiveHeaderListDto>), 200)]
    [ProducesResponseType(500)]
    public async Task<ActionResult> Find([FromQuery] GeneralListProfile listProfile, [FromBody] PurchaseOrderReceiveHeaderFindCommand command)
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

    [HttpPost("CreatePurchaseOrderReceiveHeader", Name = "CreatePurchaseOrderReceiveHeader")]
    [ProducesResponseType(typeof(Response<PurchaseOrderReceiveHeaderDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> Create([FromBody] PurchaseOrderReceiveHeaderCreateCommand createCommand)
    {
        var result = await _Module.Create(createCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPost("CreatePurchaseOrderReceiveLine", Name = "CreatePurchaseOrderReceiveLine")]
    [ProducesResponseType(typeof(Response<PurchaseOrderReceiveLineDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> CreateLine([FromBody] PurchaseOrderReceiveLineCreateCommand createCommand)
    {
        var result = await _Module.CreateLine(createCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPost("CreatePurchaseOrderReceiveUpload", Name = "CreatePurchaseOrderReceiveUpload")]
    [ProducesResponseType(typeof(Response<PurchaseOrderReceiveUploadDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> CreatePurchaseOrderReceiveUpload([FromBody] PurchaseOrderReceiveUploadCreateCommand createCommand)
    {
        var result = await _Module.CreateUpload(createCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPut("UpdatePurchaseOrderReceiveHeader", Name = "UpdatePurchaseOrderReceiveHeader")]
    [ProducesResponseType(typeof(Response<PurchaseOrderReceiveHeaderDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> Edit([FromBody] PurchaseOrderReceiveHeaderEditCommand editCommand)
    {
        var result = await _Module.Edit(editCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPut("UpdatePurchaseOrderReceiveLine", Name = "UpdatePurchaseOrderReceiveLine")]
    [ProducesResponseType(typeof(Response<PurchaseOrderReceiveLineDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> EditLine([FromBody] PurchaseOrderReceiveLineEditCommand editCommand)
    {
        var result = await _Module.EditLine(editCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpDelete("DeletePurchaseOrderReceiveHeader", Name = "DeletePurchaseOrderReceiveHeader")]
    [ProducesResponseType(typeof(Response<PurchaseOrderReceiveHeaderDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> Delete([FromBody] PurchaseOrderReceiveHeaderDeleteCommand deleteCommand)
    {
        var result = await _Module.Delete(deleteCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpDelete("DeletePurchaseOrderReceiveLine", Name = "DeletePurchaseOrderReceiveLine")]
    [ProducesResponseType(typeof(Response<PurchaseOrderReceiveLineDto>), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<ActionResult> DeleteLine([FromBody] PurchaseOrderReceiveLineDeleteCommand deleteCommand)
    {
        var result = await _Module.DeleteLine(deleteCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpDelete("DeletePurchaseOrderReceiveUpload", Name = "DeletePurchaseOrderReceiveUpload")]
    [ProducesResponseType(typeof(Response<PurchaseOrderReceiveUploadDto>), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<ActionResult> DeletePurchaseOrderReceiveUpload([FromBody] PurchaseOrderReceiveUploadDeleteCommand deleteCommand)
    {
        var result = await _Module.DeleteUpload(deleteCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
}
