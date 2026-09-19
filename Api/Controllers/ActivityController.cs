using KosmosERP.Api.Authorization;
using KosmosERP.Api.Models;
using Microsoft.AspNetCore.Mvc;
using KosmosERP.Models;
using KosmosERP.Module;
using KosmosERP.BusinessLayer.Modules;
using KosmosERP.BusinessLayer.Models.Module.Activity.Dto;
using KosmosERP.BusinessLayer.Models.Module.Activity.Command.Create;
using KosmosERP.BusinessLayer.Models.Module.Activity.Command.Delete;
using KosmosERP.BusinessLayer.Models.Module.Activity.Command.Edit;
using KosmosERP.BusinessLayer.Models.Module.Activity.Command.Find;
using KosmosERP.BusinessLayer.Models.Module.User.ListProfiles;

namespace KosmosERP.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class ActivityController : ERPApiController
{
    private IActivityModule _Module;

    public ActivityController(IActivityModule module) : base(module)
    {
        _Module = module;
    }

    [ERPAuthorize(new[] { ERPPermission.Read }, "activity_read")]
    [HttpGet("GetActivity", Name = "GetActivity")]
    [ProducesResponseType(typeof(Response<ActivityDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> Get([FromQuery] int id)
    {
        var result = await _Module.GetDto(id);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [ERPAuthorize(new[] { ERPPermission.Read }, "activity_read")]
    [HttpGet("GetActivityByGuid", Name = "GetActivityByGuid")]
    [ProducesResponseType(typeof(Response<ActivityDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> GetByGuid([FromQuery] string guid)
    {
        var result = await _Module.GetDtoByGuid(guid);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [ERPAuthorize(new[] { ERPPermission.Read }, "activity_read")]
    [HttpPost("FindActivity", Name = "FindActivity")]
    [ProducesResponseType(typeof(PagingResult<ActivityListDto>), 200)]
    [ProducesResponseType(500)]
    public async Task<ActionResult> Find([FromQuery] GeneralListProfile listProfile, [FromBody] ActivityFindCommand command)
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

    [ERPAuthorize(new[] { ERPPermission.Write }, "activity_write")]
    [HttpPost("CreateActivity", Name = "CreateActivity")]
    [ProducesResponseType(typeof(Response<ActivityDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> Create([FromBody] ActivityCreateCommand createCommand)
    {
        createCommand.calling_user_id = this.CurrentUserId;

        var result = await _Module.Create(createCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [ERPAuthorize(new[] { ERPPermission.Edit }, "activity_edit")]
    [HttpPut("UpdateActivity", Name = "UpdateActivity")]
    [ProducesResponseType(typeof(Response<ActivityDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> Edit([FromBody] ActivityEditCommand editCommand)
    {
        editCommand.calling_user_id = this.CurrentUserId;

        var result = await _Module.Edit(editCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [ERPAuthorize(new[] { ERPPermission.Delete }, "activity_delete")]
    [HttpPost("DeleteActivity", Name = "DeleteActivity")]
    [ProducesResponseType(typeof(Response<ActivityDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> Delete([FromBody] ActivityDeleteCommand deleteCommand)
    {
        deleteCommand.calling_user_id = this.CurrentUserId;

        var result = await _Module.Delete(deleteCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
} 