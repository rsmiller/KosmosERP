using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using KosmosERP.BusinessLayer.Models.Module.Customer.Dto;
using KosmosERP.BusinessLayer.Models.Module.Customer.Command.Create;
using KosmosERP.BusinessLayer.Models.Module.Customer.Command.Delete;
using KosmosERP.BusinessLayer.Models.Module.Customer.Command.Edit;
using KosmosERP.BusinessLayer.Models.Module.Customer.Command.Find;
using KosmosERP.BusinessLayer.Modules;
using KosmosERP.Models;
using KosmosERP.Module;
using KosmosERP.BusinessLayer.Models.Module.User.ListProfiles;

namespace KosmosERP.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/v1/[controller]")]
    public class CustomerController : ERPApiController
    {
        private ICustomerModule _Module;

        public CustomerController(ICustomerModule module) : base(module)
        {
            _Module = module;
        }

        
        [HttpGet("GetCustomer", Name = "GetCustomer")]
        [ProducesResponseType(typeof(Response<CustomerDto>), 200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult> Get([FromQuery] int id)
        {
            var result = await _Module.GetDto(id);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPost("FindCustomer", Name = "FindCustomer")]
        [ProducesResponseType(typeof(PagingResult<CustomerListDto>), 200)]
        [ProducesResponseType(500)]
        public async Task<ActionResult> Find([FromQuery] GeneralListProfile listProfile, [FromBody] CustomerFindCommand command)
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

        [HttpPost("CreateCustomer", Name = "CreateCustomer")]
        [ProducesResponseType(typeof(Response<CustomerDto>), 200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult> Create([FromBody] CustomerCreateCommand createCommand)
        {
            var result = await _Module.Create(createCommand);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPut("UpdateCustomer", Name = "UpdateCustomer")]
        [ProducesResponseType(typeof(Response<CustomerDto>), 200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult> Edit([FromBody] CustomerEditCommand editCommand)
        {
            var result = await _Module.Edit(editCommand);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpDelete("DeleteCustomer", Name = "DeleteCustomer")]
        [ProducesResponseType(typeof(Response<CustomerDto>), 200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult> Delete([FromBody] CustomerDeleteCommand deleteCommand)
        {
            var result = await _Module.Delete(deleteCommand);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }
    }
}
