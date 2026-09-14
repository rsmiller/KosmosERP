using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using KosmosERP.Models;
using KosmosERP.Module;
using KosmosERP.BusinessLayer.Modules;
using KosmosERP.BusinessLayer.Models.Module.Comment.Dto;
using KosmosERP.BusinessLayer.Models.Module.Comment.Command.Create;
using KosmosERP.BusinessLayer.Models.Module.Comment.Command.Delete;
using KosmosERP.BusinessLayer.Models.Module.Comment.Command.Edit;
using KosmosERP.BusinessLayer.Models.Module.Comment.Command.Find;
using KosmosERP.BusinessLayer.Models.Module.User.ListProfiles;

namespace KosmosERP.Api.Controllers;


[ApiController]
[Route("api/v1/[controller]")]
public class CommentController : ERPApiController
{
    private ICommentModule _Module;

    public CommentController(ICommentModule module) : base(module)
    {
        _Module = module;
    }

    [Authorize(Roles = "comment_read")]
    [HttpGet("GetComment", Name = "GetComment")]
    [ProducesResponseType(typeof(Response<CommentDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> Get([FromQuery] int id)
    {
        var result = await _Module.GetDto(id);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [Authorize(Roles = "comment_read")]
    [HttpGet("GetCommentByGuid", Name = "GetCommentByGuid")]
    [ProducesResponseType(typeof(Response<CommentDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> GetByGuid([FromQuery] string guid)
    {
        var result = await _Module.GetDtoByGuid(guid);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [Authorize(Roles = "comment_read")]
    [HttpPost("FindComment", Name = "FindComment")]
    [ProducesResponseType(typeof(PagingResult<CommentListDto>), 200)]
    [ProducesResponseType(500)]
    public async Task<ActionResult> Find([FromQuery] GeneralListProfile listProfile, [FromBody] CommentFindCommand command)
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

    [Authorize(Roles = "comment_write")]
    [HttpPost("CreateComment", Name = "CreateComment")]
    [ProducesResponseType(typeof(Response<CommentDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> Create([FromBody] CommentCreateCommand createCommand)
    {
        createCommand.calling_user_id = this.CurrentUserId;

        var result = await _Module.Create(createCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [Authorize(Roles = "comment_edit")]
    [HttpPut("UpdateComment", Name = "UpdateComment")]
    [ProducesResponseType(typeof(Response<CommentDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> Edit([FromBody] CommentEditCommand editCommand)
    {
        editCommand.calling_user_id = this.CurrentUserId;

        var result = await _Module.Edit(editCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [Authorize(Roles = "comment_delete")]
    [HttpPost("DeleteComment", Name = "DeleteComment")]
    [ProducesResponseType(typeof(Response<CommentDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> Delete([FromBody] CommentDeleteCommand deleteCommand)
    {
        deleteCommand.calling_user_id = this.CurrentUserId;

        var result = await _Module.Delete(deleteCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
}
