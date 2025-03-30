using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Prometheus.BusinessLayer.Models.Module.Contact.Dto;
using Prometheus.BusinessLayer.Models.Module.Contact.Command.Create;
using Prometheus.BusinessLayer.Models.Module.Contact.Command.Delete;
using Prometheus.BusinessLayer.Models.Module.Contact.Command.Edit;
using Prometheus.BusinessLayer.Models.Module.Contact.Command.Find;
using Prometheus.BusinessLayer.Modules;
using Prometheus.Models;
using Prometheus.Module;
using Prometheus.BusinessLayer.Models.Module.User.ListProfiles;

namespace Prometheus.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/v1/[controller]")]
    public class ContactController : ERPApiController
    {
        private IContactModule _Module;

        public ContactController(IContactModule module) : base(module)
        {
            _Module = module;
        }

        
        [HttpGet("GetContact", Name = "GetContact")]
        [ProducesResponseType(typeof(Response<ContactDto>), 200)]
        [ProducesResponseType(204)]
        [ProducesResponseType(401)]
        public async Task<ActionResult> Get([FromQuery] int id)
        {
            var result = await _Module.GetDto(id);

            return new JsonResult(result);
        }

        [HttpPost("FindContact", Name = "FindContact")]
        [ProducesResponseType(typeof(PagingResult<ContactListDto>), 200)]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<ActionResult> Find([FromQuery] GeneralListProfile listProfile, [FromBody] ContactFindCommand command)
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

        [HttpPost("CreateContact", Name = "CreateContact")]
        [ProducesResponseType(typeof(Response<ContactDto>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult> Create([FromBody] ContactCreateCommand createCommand)
        {
            var result = await _Module.Create(createCommand);

            if (!result.Success)
                return BadRequest(result);

            return new JsonResult(result);
        }

        [HttpPut("UpdateContact", Name = "UpdateContact")]
        [ProducesResponseType(typeof(Response<ContactDto>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult> Edit([FromBody] ContactEditCommand editCommand)
        {
            var result = await _Module.Edit(editCommand);

            if (!result.Success)
                return BadRequest(result);

            return new JsonResult(result);
        }

        [HttpDelete("DeleteContact", Name = "DeleteContact")]
        [ProducesResponseType(typeof(Response<ContactDto>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult> Delete([FromBody] ContactDeleteCommand deleteCommand)
        {
            var result = await _Module.Delete(deleteCommand);

            if (!result.Success)
                return BadRequest(result);

            return new JsonResult(result);
        }
    }
}
