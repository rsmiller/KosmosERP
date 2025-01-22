using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Prometheus.Api.Models.Module.Opportunity.Dto;
using Prometheus.Api.Models.Module.Opportunity.Command.Create;
using Prometheus.Api.Models.Module.Opportunity.Command.Delete;
using Prometheus.Api.Models.Module.Opportunity.Command.Edit;
using Prometheus.Api.Models.Module.Opportunity.Command.Find;
using Prometheus.Api.Modules;
using Prometheus.Models;
using Prometheus.Module;
using Prometheus.Api.Models.Module.User.ListProfiles;

namespace Prometheus.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/v1/[controller]")]
    public class OpportunityController : ERPApiController
    {
        private IOpportunityModule _Module;

        public OpportunityController(IOpportunityModule module) : base(module)
        {
            _Module = module;
        }

        
        [HttpGet("GetOpportunity", Name = "GetOpportunity")]
        [ProducesResponseType(typeof(Response<OpportunityDto>), 200)]
        [ProducesResponseType(204)]
        [ProducesResponseType(401)]
        public async Task<ActionResult> Get([FromQuery] int id)
        {
            var result = await _Module.GetDto(id);

            return new JsonResult(result);
        }

        [HttpPost("FindOpportunity", Name = "FindOpportunity")]
        [ProducesResponseType(typeof(PagingResult<OpportunityListDto>), 200)]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<ActionResult> Find([FromQuery] GeneralListProfile listProfile, [FromBody] OpportunityFindCommand command)
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

        [HttpPost("CreateOpportunity", Name = "CreateOpportunity")]
        [ProducesResponseType(typeof(Response<OpportunityDto>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult> Create([FromBody] OpportunityCreateCommand createCommand)
        {
            var result = await _Module.Create(createCommand);

            if (!result.Success)
                return BadRequest(result);

            return new JsonResult(result);
        }

        [HttpPut("UpdateOpportunity", Name = "UpdateOpportunity")]
        [ProducesResponseType(typeof(Response<OpportunityDto>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult> Edit([FromBody] OpportunityEditCommand editCommand)
        {
            var result = await _Module.Edit(editCommand);

            if (!result.Success)
                return BadRequest(result);

            return new JsonResult(result);
        }

        [HttpDelete("DeleteOpportunity", Name = "DeleteOpportunity")]
        [ProducesResponseType(typeof(Response<OpportunityDto>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult> Delete([FromBody] OpportunityDeleteCommand deleteCommand)
        {
            var result = await _Module.Delete(deleteCommand);

            if (!result.Success)
                return BadRequest(result);

            return new JsonResult(result);
        }
    }
}
