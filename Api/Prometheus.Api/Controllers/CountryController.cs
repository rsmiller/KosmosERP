using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Prometheus.Api.Models.Module.Country.Dto;
using Prometheus.Api.Models.Module.Country.Command.Create;
using Prometheus.Api.Models.Module.Country.Command.Delete;
using Prometheus.Api.Models.Module.Country.Command.Edit;
using Prometheus.Api.Models.Module.Country.Command.Find;
using Prometheus.Api.Modules;
using Prometheus.Models;
using Prometheus.Module;
using Prometheus.Api.Models.Module.User.ListProfiles;

namespace Prometheus.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/v1/[controller]")]
    public class CountryController : ERPApiController
    {
        private ICountryModule _Module;

        public CountryController(ICountryModule module) : base(module)
        {
            _Module = module;
        }

        
        [HttpGet("GetCountry", Name = "GetCountry")]
        [ProducesResponseType(typeof(Response<CountryDto>), 200)]
        [ProducesResponseType(204)]
        [ProducesResponseType(401)]
        public async Task<ActionResult> Get([FromQuery] int id)
        {
            var result = await _Module.GetDto(id);

            return new JsonResult(result);
        }

        [HttpPost("FindCountry", Name = "FindCountry")]
        [ProducesResponseType(typeof(PagingResult<CountryListDto>), 200)]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<ActionResult> Find([FromQuery] GeneralListProfile listProfile, [FromBody] CountryFindCommand command)
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

        [HttpPost("CreateCountry", Name = "CreateCountry")]
        [ProducesResponseType(typeof(Response<CountryDto>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult> Create([FromBody] CountryCreateCommand createCommand)
        {
            var result = await _Module.Create(createCommand);

            if (!result.Success)
                return BadRequest(result);

            return new JsonResult(result);
        }

        [HttpPut("UpdateCountry", Name = "UpdateCountry")]
        [ProducesResponseType(typeof(Response<CountryDto>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult> Edit([FromBody] CountryEditCommand editCommand)
        {
            var result = await _Module.Edit(editCommand);

            if (!result.Success)
                return BadRequest(result);

            return new JsonResult(result);
        }

        [HttpDelete("DeleteCountry", Name = "DeleteCountry")]
        [ProducesResponseType(typeof(Response<CountryDto>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult> Delete([FromBody] CountryDeleteCommand deleteCommand)
        {
            var result = await _Module.Delete(deleteCommand);

            if (!result.Success)
                return BadRequest(result);

            return new JsonResult(result);
        }
    }
}
