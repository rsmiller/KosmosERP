using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using KosmosERP.BusinessLayer.Models.Module.JournalEntry.Dto;
using KosmosERP.BusinessLayer.Models.Module.JournalEntry.Command;
using KosmosERP.BusinessLayer.Models.Module.JournalEntry.Command.Create;
using KosmosERP.BusinessLayer.Models.Module.JournalEntry.Command.Delete;
using KosmosERP.BusinessLayer.Models.Module.JournalEntry.Command.Edit;
using KosmosERP.BusinessLayer.Models.Module.JournalEntry.Command.Find;
using KosmosERP.BusinessLayer.Modules;
using KosmosERP.Models;
using KosmosERP.Module;
using KosmosERP.BusinessLayer.Models.Module.User.ListProfiles;

namespace KosmosERP.Api.Controllers;

//[Authorize]
[ApiController]
[Route("api/v1/[controller]")]
public class JournalEntryController : ERPApiController
{
    private IJournalEntryModule _Module;

    public JournalEntryController(IJournalEntryModule module) : base(module)
    {
        _Module = module;
    }

    [Authorize(Roles = "journal_entry_read")]
    [HttpGet("GetJournalEntry", Name = "GetJournalEntry")]
    [ProducesResponseType(typeof(Response<JournalEntryHeaderDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> Get([FromQuery] int id)
    {
        var result = await _Module.GetDto(id);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [Authorize(Roles = "journal_entry_read")]
    [HttpGet("GetJournalEntryByGuid", Name = "GetJournalEntryByGuid")]
    [ProducesResponseType(typeof(Response<JournalEntryHeaderDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> GetByGuid([FromQuery] string guid)
    {
        var result = await _Module.GetDtoByGuid(guid);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [Authorize(Roles = "journal_entry_read")]
    [HttpGet("GetJournalEntryLine", Name = "GetJournalEntryLine")]
    [ProducesResponseType(typeof(Response<JournalEntryLineDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> GetLine([FromQuery] int id)
    {
        var result = await _Module.GetLineDto(id);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [Authorize(Roles = "journal_entry_read")]
    [HttpGet("GetUnpostedEntries", Name = "GetUnpostedEntries")]
    [ProducesResponseType(typeof(PagingResult<JournalEntryHeaderListDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> GetUnpostedEntries([FromQuery] GeneralListProfile listProfile)
    {
        var sortingParams = new PagingSortingParameters(listProfile.Start, listProfile.ResultCount, listProfile.SortOrder);
        var result = await _Module.GetUnpostedEntries(sortingParams);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [Authorize(Roles = "journal_entry_read")]
    [HttpPost("ValidateJournalBalance", Name = "ValidateJournalBalance")]
    [ProducesResponseType(typeof(Response<bool>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> ValidateBalance([FromQuery] int journalEntryId)
    {
        var result = await _Module.ValidateBalance(journalEntryId);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [Authorize(Roles = "journal_entry_read")]
    [HttpPost("FindJournalEntry", Name = "FindJournalEntry")]
    [ProducesResponseType(typeof(PagingResult<JournalEntryHeaderListDto>), 200)]
    [ProducesResponseType(500)]
    public async Task<ActionResult> Find([FromQuery] GeneralListProfile listProfile, [FromBody] JournalEntryHeaderFindCommand command)
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

    [Authorize(Roles = "journal_entry_write")]
    [HttpPost("CreateJournalEntry", Name = "CreateJournalEntry")]
    [ProducesResponseType(typeof(Response<JournalEntryHeaderDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> Create([FromBody] JournalEntryHeaderCreateCommand createCommand)
    {
        createCommand.calling_user_id = this.CurrentUserId;

        var result = await _Module.Create(createCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [Authorize(Roles = "journal_entry_write")]
    [HttpPost("CreateJournalEntryLine", Name = "CreateJournalEntryLine")]
    [ProducesResponseType(typeof(Response<JournalEntryLineDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> CreateLine([FromBody] JournalEntryLineCreateCommand createCommand)
    {
        createCommand.calling_user_id = this.CurrentUserId;

        var result = await _Module.CreateLine(createCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [Authorize(Roles = "journal_entry_edit")]
    [HttpPut("UpdateJournalEntry", Name = "UpdateJournalEntry")]
    [ProducesResponseType(typeof(Response<JournalEntryHeaderDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> Edit([FromBody] JournalEntryHeaderEditCommand editCommand)
    {
        editCommand.calling_user_id = this.CurrentUserId;

        var result = await _Module.Edit(editCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [Authorize(Roles = "journal_entry_edit")]
    [HttpPut("UpdateJournalEntryLine", Name = "UpdateJournalEntryLine")]
    [ProducesResponseType(typeof(Response<JournalEntryLineDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> EditLine([FromBody] JournalEntryLineEditCommand editCommand)
    {
        editCommand.calling_user_id = this.CurrentUserId;

        var result = await _Module.EditLine(editCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [Authorize(Roles = "journal_entry_delete")]
    [HttpPost("DeleteJournalEntry", Name = "DeleteJournalEntry")]
    [ProducesResponseType(typeof(Response<JournalEntryHeaderDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> Delete([FromBody] JournalEntryHeaderDeleteCommand deleteCommand)
    {
        deleteCommand.calling_user_id = this.CurrentUserId;

        var result = await _Module.Delete(deleteCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [Authorize(Roles = "journal_entry_delete")]
    [HttpPost("DeleteJournalEntryLine", Name = "DeleteJournalEntryLine")]
    [ProducesResponseType(typeof(Response<JournalEntryLineDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> DeleteLine([FromBody] JournalEntryLineDeleteCommand deleteCommand)
    {
        deleteCommand.calling_user_id = this.CurrentUserId;

        var result = await _Module.DeleteLine(deleteCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [Authorize(Roles = "journal_entry_edit")]
    [HttpPost("PostJournalEntry", Name = "PostJournalEntry")]
    [ProducesResponseType(typeof(Response<JournalEntryHeaderDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> Post([FromBody] JournalEntryPostCommand postCommand)
    {
        postCommand.calling_user_id = this.CurrentUserId;

        var result = await _Module.Post(postCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [Authorize(Roles = "journal_entry_edit")]
    [HttpPost("ReverseJournalEntry", Name = "ReverseJournalEntry")]
    [ProducesResponseType(typeof(Response<JournalEntryHeaderDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> Reverse([FromBody] JournalEntryReverseCommand reverseCommand)
    {
        reverseCommand.calling_user_id = this.CurrentUserId;

        var result = await _Module.Reverse(reverseCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
}
