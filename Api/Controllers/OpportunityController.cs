using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using KosmosERP.BusinessLayer.Models.Module.Opportunity.Dto;
using KosmosERP.BusinessLayer.Models.Module.Opportunity.Command.Create;
using KosmosERP.BusinessLayer.Models.Module.Opportunity.Command.Delete;
using KosmosERP.BusinessLayer.Models.Module.Opportunity.Command.Edit;
using KosmosERP.BusinessLayer.Models.Module.Opportunity.Command.Find;
using KosmosERP.BusinessLayer.Modules;
using KosmosERP.Models;
using KosmosERP.Module;
using KosmosERP.BusinessLayer.Models.Module.User.ListProfiles;

namespace KosmosERP.Api.Controllers;


[ApiController]
[Route("api/v1/[controller]")]
public class OpportunityController : ERPApiController
{
    private IOpportunityModule _Module;

    public OpportunityController(IOpportunityModule module) : base(module)
    {
        _Module = module;
    }

    [Authorize(Roles = "crm_read")]
    [HttpGet("GetOpportunity", Name = "GetOpportunity")]
    [ProducesResponseType(typeof(Response<OpportunityDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> Get([FromQuery] int id)
    {
        var result = await _Module.GetDto(id);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [Authorize(Roles = "crm_read")]
    [HttpGet("GetOpportunityByGuid", Name = "GetOpportunityByGuid")]
    [ProducesResponseType(typeof(Response<OpportunityDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> GetByGuid([FromQuery] string guid)
    {
        var result = await _Module.GetDtoByGuid(guid);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPost("FindOpportunity", Name = "FindOpportunity")]
    [ProducesResponseType(typeof(PagingResult<OpportunityListDto>), 200)]
    [ProducesResponseType(500)]
    public async Task<ActionResult> Find([FromQuery] GeneralListProfile listProfile, [FromBody] OpportunityFindCommand command)
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

    [HttpPost("CreateOpportunity", Name = "CreateOpportunity")]
    [ProducesResponseType(typeof(Response<OpportunityDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> Create([FromBody] OpportunityCreateCommand createCommand)
    {
        createCommand.calling_user_id = this.CurrentUserId;

        var result = await _Module.Create(createCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [Authorize(Roles = "crm_create")]
    [HttpPost("CreateOpportunityLine", Name = "CreateOpportunityLine")]
    [ProducesResponseType(typeof(Response<OpportunityLineDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> CreateLine([FromBody] OpportunityLineCreateCommand createCommand)
    {
        createCommand.calling_user_id = this.CurrentUserId;

        var result = await _Module.CreateLine(createCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [Authorize(Roles = "crm_edit")]
    [HttpPut("UpdateOpportunity", Name = "UpdateOpportunity")]
    [ProducesResponseType(typeof(Response<OpportunityDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> Edit([FromBody] OpportunityEditCommand editCommand)
    {
        editCommand.calling_user_id = this.CurrentUserId;

        var result = await _Module.Edit(editCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [Authorize(Roles = "crm_edit")]
    [HttpPut("UpdateOpportunityLine", Name = "UpdateOpportunityLine")]
    [ProducesResponseType(typeof(Response<OpportunityLineDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> EditLine([FromBody] OpportunityLineEditCommand editCommand)
    {
        editCommand.calling_user_id = this.CurrentUserId;

        var result = await _Module.EditLine(editCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [Authorize(Roles = "crm_delete")]
    [HttpPost("DeleteOpportunity", Name = "DeleteOpportunity")]
    [ProducesResponseType(typeof(Response<OpportunityDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> Delete([FromBody] OpportunityDeleteCommand deleteCommand)
    {
        deleteCommand.calling_user_id = this.CurrentUserId;

        var result = await _Module.Delete(deleteCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
    
    [Authorize(Roles = "crm_delete")]
    [HttpPost("DeleteOpportunityLine", Name = "DeleteOpportunityLine")]
    [ProducesResponseType(typeof(Response<OpportunityLineDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> DeleteLine([FromBody] OpportunityLineDeleteCommand deleteCommand)
    {
        deleteCommand.calling_user_id = this.CurrentUserId;

        var result = await _Module.DeleteLine(deleteCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
}
