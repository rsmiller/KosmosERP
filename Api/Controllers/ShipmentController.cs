using KosmosERP.Api.Authorization;
using KosmosERP.Api.Models;
using Microsoft.AspNetCore.Mvc;
using KosmosERP.BusinessLayer.Models.Module.Shipment.Dto;
using KosmosERP.BusinessLayer.Models.Module.Shipment.Command.Create;
using KosmosERP.BusinessLayer.Models.Module.Shipment.Command.Delete;
using KosmosERP.BusinessLayer.Models.Module.Shipment.Command.Find;
using KosmosERP.BusinessLayer.Modules;
using KosmosERP.Models;
using KosmosERP.Module;
using KosmosERP.BusinessLayer.Models.Module.User.ListProfiles;
using KosmosERP.BusinessLayer.Models.Module.Shipment.Command.Edit;
using KosmosERP.Database.Views;

namespace KosmosERP.Api.Controllers;

//[Authorize]
[ApiController]
[Route("api/v1/[controller]")]
public class ShipmentController : ERPApiController
{
    private IShipmentModule _Module;

    public ShipmentController(IShipmentModule module) : base(module)
    {
        _Module = module;
    }

    
    [ERPAuthorize(new[] { ERPPermission.Read }, "shipping_read")]
    [HttpGet("GetShipmentHeader", Name = "GetShipmentHeader")]
    [ProducesResponseType(typeof(Response<ShipmentHeaderDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> Get([FromQuery] int id)
    {
        var result = await _Module.GetDto(id);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [ERPAuthorize(new[] { ERPPermission.Read }, "shipping_read")]
    [HttpGet("GetShipmentHeaderByGuid", Name = "GetShipmentHeaderByGuid")]
    [ProducesResponseType(typeof(Response<ShipmentHeaderDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> GetByGuid([FromQuery] string guid)
    {
        var result = await _Module.GetDtoByGuid(guid);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [ERPAuthorize(new[] { ERPPermission.Read }, "shipping_read")]
    [HttpGet("GetShipmentLine", Name = "GetShipmentLine")]
    [ProducesResponseType(typeof(Response<ShipmentLineDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> GetLine([FromQuery] int id)
    {
        var result = await _Module.GetLineDto(id);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [ERPAuthorize(new[] { ERPPermission.Read }, "shipping_read")]
    [HttpPost("FindShipmentHeader", Name = "FindShipmentHeader")]
    [ProducesResponseType(typeof(PagingResult<ShipmentHeaderListDto>), 200)]
    [ProducesResponseType(500)]
    public async Task<ActionResult> Find([FromQuery] GeneralListProfile listProfile, [FromBody] ShipmentHeaderFindCommand command)
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

    [ERPAuthorize(new[] { ERPPermission.Read }, "shipping_read")]
    [HttpGet("GetReadyToShip", Name = "GetReadyToShip")]
    [ProducesResponseType(typeof(Response<vw_ReadyToShip>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> GetReadyToShip()
    {
        var result = await _Module.GetReadyToShip();

        return Ok(result);
    }

    [ERPAuthorize(new[] { ERPPermission.Write }, "shipping_write")]
    [HttpPost("CreateShipmentHeader", Name = "CreateShipmentHeader")]
    [ProducesResponseType(typeof(Response<ShipmentHeaderDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> Create([FromBody] ShipmentHeaderCreateCommand createCommand)
    {
        createCommand.calling_user_id = this.CurrentUserId;
        var result = await _Module.Create(createCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [ERPAuthorize(new[] { ERPPermission.Write }, "shipping_write")]
    [HttpPost("CreateShipmentLine", Name = "CreateShipmentLine")]
    [ProducesResponseType(typeof(Response<ShipmentLineDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> CreateLine([FromBody] ShipmentLineCreateCommand createCommand)
    {
        createCommand.calling_user_id = this.CurrentUserId;
        var result = await _Module.CreateLine(createCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [ERPAuthorize(new[] { ERPPermission.Edit }, "shipping_edit")]
    [HttpPut("UpdateShipmentHeader", Name = "UpdateShipmentHeader")]
    [ProducesResponseType(typeof(Response<ShipmentHeaderDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> Edit([FromBody] ShipmentHeaderEditCommand editCommand)
    {
        editCommand.calling_user_id = this.CurrentUserId;
        var result = await _Module.Edit(editCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [ERPAuthorize(new[] { ERPPermission.Edit }, "shipping_edit")]
    [HttpPut("UpdateShipmentLine", Name = "UpdateShipmentLine")]
    [ProducesResponseType(typeof(Response<ShipmentLineDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> EditLine([FromBody] ShipmentLineEditCommand editCommand)
    {
        editCommand.calling_user_id = this.CurrentUserId;
        var result = await _Module.EditLine(editCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [ERPAuthorize(new[] { ERPPermission.Delete }, "shipping_delete")]
    [HttpPost("DeleteShipmentHeader", Name = "DeleteShipmentHeader")]
    [ProducesResponseType(typeof(Response<ShipmentHeaderDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> Delete([FromBody] ShipmentHeaderDeleteCommand deleteCommand)
    {
        deleteCommand.calling_user_id = this.CurrentUserId;
        var result = await _Module.Delete(deleteCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [ERPAuthorize(new[] { ERPPermission.Delete }, "shipping_delete")]
    [HttpPost("DeleteShipmentLine", Name = "DeleteShipmentLine")]
    [ProducesResponseType(typeof(Response<ShipmentHeaderDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> DeleteLine([FromBody] ShipmentLineDeleteCommand deleteCommand)
    {
        deleteCommand.calling_user_id = this.CurrentUserId;
        var result = await _Module.DeleteLine(deleteCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
}
