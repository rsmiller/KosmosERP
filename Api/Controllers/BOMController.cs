using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using KosmosERP.BusinessLayer.Models.Module.BOM.Command.Create;
using KosmosERP.BusinessLayer.Models.Module.BOM.Command.Delete;
using KosmosERP.BusinessLayer.Models.Module.BOM.Command.Edit;
using KosmosERP.BusinessLayer.Models.Module.BOM.Command.Find;
using KosmosERP.BusinessLayer.Models.Module.BOM.Dto;
using KosmosERP.BusinessLayer.Models.Module.User.ListProfiles;
using KosmosERP.BusinessLayer.Modules;
using KosmosERP.Models;
using KosmosERP.Module;

namespace KosmosERP.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/[controller]")]
public class BOMController : ERPApiController
{
    private IBOMModule _Module;

    public BOMController(IBOMModule module) : base(module)
    {
        _Module = module;
    }


    [HttpGet("GetBOM", Name = "GetBOM")]
    [ProducesResponseType(typeof(Response<BOMDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> Get([FromQuery] int id)
    {
        var result = await _Module.GetDto(id);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPost("FindBOM", Name = "FindBOM")]
    [ProducesResponseType(typeof(PagingResult<BOMListDto>), 200)]
    [ProducesResponseType(500)]
    public async Task<ActionResult> Find([FromQuery] GeneralListProfile listProfile, [FromBody] BOMFindCommand command)
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

    [HttpPost("CreateBOM", Name = "CreateBOM")]
    [ProducesResponseType(typeof(Response<BOMDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> Create([FromBody] BOMCreateCommand createCommand)
    {
        var result = await _Module.Create(createCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPut("UpdateBOM", Name = "UpdateBOM")]
    [ProducesResponseType(typeof(Response<BOMDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> Edit([FromBody] BOMEditCommand editCommand)
    {
        var result = await _Module.Edit(editCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpDelete("DeleteBOM", Name = "DeleteBOM")]
    [ProducesResponseType(typeof(Response<BOMDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> Delete([FromBody] BOMDeleteCommand deleteCommand)
    {
        var result = await _Module.Delete(deleteCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
}
