using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Prometheus.BusinessLayer.Models.Module.Lead.Dto;
using Prometheus.BusinessLayer.Models.Module.Lead.Command.Create;
using Prometheus.BusinessLayer.Models.Module.Lead.Command.Delete;
using Prometheus.BusinessLayer.Models.Module.Lead.Command.Edit;
using Prometheus.BusinessLayer.Models.Module.Lead.Command.Find;
using Prometheus.BusinessLayer.Modules;
using Prometheus.Models;
using Prometheus.Module;
using Prometheus.BusinessLayer.Models.Module.User.ListProfiles;

namespace Prometheus.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/v1/[controller]")]
    public class LeadController : ERPApiController
    {
        private ILeadModule _Module;

        public LeadController(ILeadModule module) : base(module)
        {
            _Module = module;
        }

        
        [HttpGet("GetLead", Name = "GetLead")]
        [ProducesResponseType(typeof(Response<LeadDto>), 200)]
        [ProducesResponseType(204)]
        [ProducesResponseType(401)]
        public async Task<ActionResult> Get([FromQuery] int id)
        {
            var result = await _Module.GetDto(id);

            return new JsonResult(result);
        }

        [HttpPost("FindLead", Name = "FindLead")]
        [ProducesResponseType(typeof(PagingResult<LeadListDto>), 200)]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<ActionResult> Find([FromQuery] GeneralListProfile listProfile, [FromBody] LeadFindCommand command)
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

        [HttpPost("CreateLead", Name = "CreateLead")]
        [ProducesResponseType(typeof(Response<LeadDto>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult> Create([FromBody] LeadCreateCommand createCommand)
        {
            var result = await _Module.Create(createCommand);

            if (!result.Success)
                return BadRequest(result);

            return new JsonResult(result);
        }

        [HttpPut("UpdateLead", Name = "UpdateLead")]
        [ProducesResponseType(typeof(Response<LeadDto>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult> Edit([FromBody] LeadEditCommand editCommand)
        {
            var result = await _Module.Edit(editCommand);

            if (!result.Success)
                return BadRequest(result);

            return new JsonResult(result);
        }

        [HttpDelete("DeleteLead", Name = "DeleteLead")]
        [ProducesResponseType(typeof(Response<LeadDto>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult> Delete([FromBody] LeadDeleteCommand deleteCommand)
        {
            var result = await _Module.Delete(deleteCommand);

            if (!result.Success)
                return BadRequest(result);

            return new JsonResult(result);
        }
    }
}
