using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Prometheus.Api.Models.Module.Shipment.Dto;
using Prometheus.Api.Models.Module.Shipment.Command.Create;
using Prometheus.Api.Models.Module.Shipment.Command.Delete;
using Prometheus.Api.Models.Module.Shipment.Command.Find;
using Prometheus.Api.Modules;
using Prometheus.Models;
using Prometheus.Module;
using Prometheus.Api.Models.Module.User.ListProfiles;

namespace Prometheus.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/v1/[controller]")]
    public class ShipmentController : ERPApiController
    {
        private IShipmentModule _Module;

        public ShipmentController(IShipmentModule module) : base(module)
        {
            _Module = module;
        }

        
        [HttpGet("GetShipmentHeader", Name = "GetShipmentHeader")]
        [ProducesResponseType(typeof(Response<ShipmentDto>), 200)]
        [ProducesResponseType(204)]
        [ProducesResponseType(401)]
        public async Task<ActionResult> Get([FromQuery] int id)
        {
            var result = await _Module.GetDto(id);

            return new JsonResult(result);
        }

        [HttpGet("GetShipmentLine", Name = "GetShipmentLine")]
        [ProducesResponseType(typeof(Response<ShipmentLineDto>), 200)]
        [ProducesResponseType(204)]
        [ProducesResponseType(401)]
        public async Task<ActionResult> GetLine([FromQuery] int id)
        {
            var result = await _Module.GetLineDto(id);

            return new JsonResult(result);
        }

        [HttpPost("FindShipmentHeader", Name = "FindShipmentHeader")]
        [ProducesResponseType(typeof(PagingResult<ShipmentListDto>), 200)]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<ActionResult> Find([FromQuery] GeneralListProfile listProfile, [FromBody] ShipmentHeaderFindCommand command)
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

        [HttpPost("CreateShipmentHeader", Name = "CreateShipmentHeader")]
        [ProducesResponseType(typeof(Response<ShipmentDto>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult> Create([FromBody] ShipmentHeaderCreateCommand createCommand)
        {
            var result = await _Module.Create(createCommand);

            if (!result.Success)
                return BadRequest(result);

            return new JsonResult(result);
        }

        [HttpPost("CreateShipmentLine", Name = "CreateShipmentLine")]
        [ProducesResponseType(typeof(Response<ShipmentLineDto>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult> CreateLine([FromBody] ShipmentLineCreateCommand createCommand)
        {
            var result = await _Module.CreateLine(createCommand);

            if (!result.Success)
                return BadRequest(result);

            return new JsonResult(result);
        }

        [HttpPut("UpdateShipmentHeader", Name = "UpdateShipmentHeader")]
        [ProducesResponseType(typeof(Response<ShipmentDto>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult> Edit([FromBody] ShipmentHeaderEditCommand editCommand)
        {
            var result = await _Module.Edit(editCommand);

            if (!result.Success)
                return BadRequest(result);

            return new JsonResult(result);
        }

        [HttpPut("UpdateShipmentLine", Name = "UpdateShipmentLine")]
        [ProducesResponseType(typeof(Response<ShipmentLineDto>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult> EditLine([FromBody] ShipmentLineEditCommand editCommand)
        {
            var result = await _Module.EditLine(editCommand);

            if (!result.Success)
                return BadRequest(result);

            return new JsonResult(result);
        }

        [HttpDelete("DeleteShipmentHeader", Name = "DeleteShipmentHeader")]
        [ProducesResponseType(typeof(Response<ShipmentDto>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult> Delete([FromBody] ShipmentHeaderDeleteCommand deleteCommand)
        {
            var result = await _Module.Delete(deleteCommand);

            if (!result.Success)
                return BadRequest(result);

            return new JsonResult(result);
        }

        [HttpDelete("DeleteShipmentLine", Name = "DeleteShipmentLine")]
        [ProducesResponseType(typeof(Response<ShipmentDto>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult> DeleteLine([FromBody] ShipmentLineDeleteCommand deleteCommand)
        {
            var result = await _Module.DeleteLine(deleteCommand);

            if (!result.Success)
                return BadRequest(result);

            return new JsonResult(result);
        }
    }
}
