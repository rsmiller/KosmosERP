using KosmosERP.Api.Authorization;
using KosmosERP.Api.Models;
using Microsoft.AspNetCore.Mvc;
using KosmosERP.BusinessLayer.Models.Module.Lead.Dto;
using KosmosERP.BusinessLayer.Models.Module.Lead.Command.Create;
using KosmosERP.BusinessLayer.Models.Module.Lead.Command.Delete;
using KosmosERP.BusinessLayer.Models.Module.Lead.Command.Edit;
using KosmosERP.BusinessLayer.Models.Module.Lead.Command.Find;
using KosmosERP.BusinessLayer.Modules;
using KosmosERP.Models;
using KosmosERP.Module;
using KosmosERP.BusinessLayer.Models.Module.User.ListProfiles;

namespace KosmosERP.Api.Controllers;


[ApiController]
[Route("api/v1/[controller]")]
public class LeadController : ERPApiController
{
    private ILeadModule _Module;

    public LeadController(ILeadModule module) : base(module)
    {
        _Module = module;
    }

    
    [ERPAuthorize(new[] { ERPPermission.Read }, "crm_read")]
    [HttpGet("GetLead", Name = "GetLead")]
    [ProducesResponseType(typeof(Response<LeadDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> Get([FromQuery] int id)
    {
        var result = await _Module.GetDto(id);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
    
    [ERPAuthorize(new[] { ERPPermission.Read }, "crm_read")]
    [HttpGet("GetLeadByGuid", Name = "GetLeadByGuid")]
    [ProducesResponseType(typeof(Response<LeadDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> GetByGuid([FromQuery] string guid)
    {
        var result = await _Module.GetDtoByGuid(guid);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [ERPAuthorize(new[] { ERPPermission.Read }, "crm_read")]
    [HttpPost("FindLead", Name = "FindLead")]
    [ProducesResponseType(typeof(PagingResult<LeadListDto>), 200)]
    [ProducesResponseType(500)]
    public async Task<ActionResult> Find([FromQuery] GeneralListProfile listProfile, [FromBody] LeadFindCommand command)
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

    [ERPAuthorize(new[] { ERPPermission.Write }, "crm_create")]
    [HttpPost("CreateLead", Name = "CreateLead")]
    [ProducesResponseType(typeof(Response<LeadDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> Create([FromBody] LeadCreateCommand createCommand)
    {
        createCommand.calling_user_id = this.CurrentUserId;

        var result = await _Module.Create(createCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [ERPAuthorize(new[] { ERPPermission.Edit }, "crm_edit")]
    [HttpPut("UpdateLead", Name = "UpdateLead")]
    [ProducesResponseType(typeof(Response<LeadDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> Edit([FromBody] LeadEditCommand editCommand)
    {
        editCommand.calling_user_id = this.CurrentUserId;

        var result = await _Module.Edit(editCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [ERPAuthorize(new[] { ERPPermission.Delete }, "crm_delete")]
    [HttpPost("DeleteLead", Name = "DeleteLead")]
    [ProducesResponseType(typeof(Response<LeadDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> Delete([FromBody] LeadDeleteCommand deleteCommand)
    {
        deleteCommand.calling_user_id = this.CurrentUserId;

        var result = await _Module.Delete(deleteCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
}
