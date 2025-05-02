using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using KosmosERP.BusinessLayer.Models.Module.State.Dto;
using KosmosERP.BusinessLayer.Models.Module.State.Command.Create;
using KosmosERP.BusinessLayer.Models.Module.State.Command.Delete;
using KosmosERP.BusinessLayer.Models.Module.State.Command.Edit;
using KosmosERP.BusinessLayer.Models.Module.State.Command.Find;
using KosmosERP.BusinessLayer.Modules;
using KosmosERP.Models;
using KosmosERP.Module;
using KosmosERP.BusinessLayer.Models.Module.User.ListProfiles;

namespace KosmosERP.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/[controller]")]
public class StateController : ERPApiController
{
    private IStateModule _Module;

    public StateController(IStateModule module) : base(module)
    {
        _Module = module;
    }

    
    [HttpGet("GetState", Name = "GetState")]
    [ProducesResponseType(typeof(Response<StateDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> Get([FromQuery] int id)
    {
        var result = await _Module.GetDto(id);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPost("FindState", Name = "FindState")]
    [ProducesResponseType(typeof(PagingResult<StateListDto>), 200)]
    [ProducesResponseType(500)]
    public async Task<ActionResult> Find([FromQuery] GeneralListProfile listProfile, [FromBody] StateFindCommand command)
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

    [HttpPost("CreateState", Name = "CreateState")]
    [ProducesResponseType(typeof(Response<StateDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> Create([FromBody] StateCreateCommand createCommand)
    {
        var result = await _Module.Create(createCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPut("UpdateState", Name = "UpdateState")]
    [ProducesResponseType(typeof(Response<StateDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> Edit([FromBody] StateEditCommand editCommand)
    {
        var result = await _Module.Edit(editCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpDelete("DeleteState", Name = "DeleteState")]
    [ProducesResponseType(typeof(Response<StateDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> Delete([FromBody] StateDeleteCommand deleteCommand)
    {
        var result = await _Module.Delete(deleteCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
}
