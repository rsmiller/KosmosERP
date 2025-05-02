using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using KosmosERP.BusinessLayer.Models.Module.Address.Dto;
using KosmosERP.BusinessLayer.Models.Module.Address.Command.Create;
using KosmosERP.BusinessLayer.Models.Module.Address.Command.Delete;
using KosmosERP.BusinessLayer.Models.Module.Address.Command.Edit;
using KosmosERP.BusinessLayer.Models.Module.Address.Command.Find;
using KosmosERP.Models;
using KosmosERP.Module;
using KosmosERP.BusinessLayer.Models.Module.User.ListProfiles;
using KosmosERP.BusinessLayer.Modules;

namespace KosmosERP.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/v1/[controller]")]
    public class AddressController : ERPApiController
    {
        private IAddressModule _Module;

        public AddressController(IAddressModule module) : base(module)
        {
            _Module = module;
        }

        
        [HttpGet("GetAddress", Name = "GetAddress")]
        [ProducesResponseType(typeof(Response<AddressDto>), 200)]
        public async Task<ActionResult> Get([FromQuery] int id)
        {
            var result = await _Module.GetDto(id);

            return Ok(result);
        }

        [HttpPost("FindAddress", Name = "FindAddress")]
        [ProducesResponseType(typeof(PagingResult<AddressListDto>), 200)]
        [ProducesResponseType(500)]
        public async Task<ActionResult> Find([FromQuery] GeneralListProfile listProfile, [FromBody] AddressFindCommand command)
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

        [HttpPost("CreateAddress", Name = "CreateAddress")]
        [ProducesResponseType(typeof(Response<AddressDto>), 200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult> Create([FromBody] AddressCreateCommand createCommand)
        {
            var result = await _Module.Create(createCommand);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPut("UpdateAddress", Name = "UpdateAddress")]
        [ProducesResponseType(typeof(Response<AddressDto>), 200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult> Edit([FromBody] AddressEditCommand editCommand)
        {
            var result = await _Module.Edit(editCommand);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpDelete("DeleteAddress", Name = "DeleteAddress")]
        [ProducesResponseType(typeof(Response<AddressDto>), 200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult> Delete([FromBody] AddressDeleteCommand deleteCommand)
        {
            var result = await _Module.Delete(deleteCommand);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }
    }
}
