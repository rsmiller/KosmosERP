using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Prometheus.BusinessLayer.Models.Module.Vendor.Dto;
using Prometheus.BusinessLayer.Models.Module.Vendor.Command.Create;
using Prometheus.BusinessLayer.Models.Module.Vendor.Command.Delete;
using Prometheus.BusinessLayer.Models.Module.Vendor.Command.Edit;
using Prometheus.BusinessLayer.Models.Module.Vendor.Command.Find;
using Prometheus.BusinessLayer.Modules;
using Prometheus.Models;
using Prometheus.Module;
using Prometheus.BusinessLayer.Models.Module.User.ListProfiles;

namespace Prometheus.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/[controller]")]
public class VendorController : ERPApiController
{
    private IVendorModule _Module;

    public VendorController(IVendorModule module) : base(module)
    {
        _Module = module;
    }

    
    [HttpGet("GetVendor", Name = "GetVendor")]
    [ProducesResponseType(typeof(Response<VendorDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> Get([FromQuery] int id)
    {
        var result = await _Module.GetDto(id);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPost("FindVendor", Name = "FindVendor")]
    [ProducesResponseType(typeof(PagingResult<VendorListDto>), 200)]
    [ProducesResponseType(500)]
    public async Task<ActionResult> Find([FromQuery] GeneralListProfile listProfile, [FromBody] VendorFindCommand command)
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

    [HttpPost("CreateVendor", Name = "CreateVendor")]
    [ProducesResponseType(typeof(Response<VendorDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> Create([FromBody] VendorCreateCommand createCommand)
    {
        var result = await _Module.Create(createCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPut("UpdateVendor", Name = "UpdateVendor")]
    [ProducesResponseType(typeof(Response<VendorDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> Edit([FromBody] VendorEditCommand editCommand)
    {
        var result = await _Module.Edit(editCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpDelete("DeleteVendor", Name = "DeleteVendor")]
    [ProducesResponseType(typeof(Response<VendorDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> Delete([FromBody] VendorDeleteCommand deleteCommand)
    {
        var result = await _Module.Delete(deleteCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
}
