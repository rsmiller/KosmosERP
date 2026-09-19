using KosmosERP.Api.Authorization;
using KosmosERP.Api.Models;
using Microsoft.AspNetCore.Mvc;
using KosmosERP.BusinessLayer.Models.Module.Address.Dto;
using KosmosERP.BusinessLayer.Models.Module.Address.Command.Create;
using KosmosERP.BusinessLayer.Models.Module.Address.Command.Delete;
using KosmosERP.BusinessLayer.Models.Module.Address.Command.Edit;
using KosmosERP.BusinessLayer.Models.Module.Address.Command.Find;
using KosmosERP.Models;
using KosmosERP.Module;
using KosmosERP.BusinessLayer.Models.Module.User.ListProfiles;
using KosmosERP.BusinessLayer.Modules;

namespace KosmosERP.Api.Controllers;


[ApiController]
[Route("api/v1/[controller]")]
public class AddressController : ERPApiController
{
    private IAddressModule _Module;

    public AddressController(IAddressModule module) : base(module)
    {
        _Module = module;
    }

    
    [ERPAuthorize(new[] { ERPPermission.Read }, "contact_read")]
    [HttpGet("GetAddress", Name = "GetAddress")]
    [ProducesResponseType(typeof(Response<AddressDto>), 200)]
    public async Task<ActionResult> Get([FromQuery] int id)
    {
        var result = await _Module.GetDto(id);

        return Ok(result);
    }

    [ERPAuthorize(new[] { ERPPermission.Read }, "contact_read")]
    [HttpGet("GetAddressByGuid", Name = "GetAddressByGuid")]
    [ProducesResponseType(typeof(Response<AddressDto>), 200)]
    public async Task<ActionResult> GetByGuid([FromQuery] string guid)
    {
        var result = await _Module.GetDtoByGuid(guid);

        return Ok(result);
    }

    [ERPAuthorize(new[] { ERPPermission.Read }, "contact_read")]
    [HttpPost("FindAddress", Name = "FindAddress")]
    [ProducesResponseType(typeof(PagingResult<AddressListDto>), 200)]
    [ProducesResponseType(500)]
    public async Task<ActionResult> Find([FromQuery] GeneralListProfile listProfile, [FromBody] AddressFindCommand command)
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

    [ERPAuthorize(new[] { ERPPermission.Write }, "contact_write")]
    [HttpPost("CreateAddress", Name = "CreateAddress")]
    [ProducesResponseType(typeof(Response<AddressDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> Create([FromBody] AddressCreateCommand createCommand)
    {
        createCommand.calling_user_id = this.CurrentUserId;

        var result = await _Module.Create(createCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [ERPAuthorize(new[] { ERPPermission.Edit }, "contact_edit")]
    [HttpPut("UpdateAddress", Name = "UpdateAddress")]
    [ProducesResponseType(typeof(Response<AddressDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> Edit([FromBody] AddressEditCommand editCommand)
    {
        editCommand.calling_user_id = this.CurrentUserId;

        var result = await _Module.Edit(editCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [ERPAuthorize(new[] { ERPPermission.Delete }, "contact_delete")]
    [HttpPost("DeleteAddress", Name = "DeleteAddress")]
    [ProducesResponseType(typeof(Response<AddressDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> Delete([FromBody] AddressDeleteCommand deleteCommand)
    {
        deleteCommand.calling_user_id = this.CurrentUserId;

        var result = await _Module.Delete(deleteCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
}

