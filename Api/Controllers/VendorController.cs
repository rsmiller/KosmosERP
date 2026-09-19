using KosmosERP.Api.Authorization;
using KosmosERP.Api.Models;
using Microsoft.AspNetCore.Mvc;
using KosmosERP.BusinessLayer.Models.Module.Vendor.Dto;
using KosmosERP.BusinessLayer.Models.Module.Vendor.Command.Create;
using KosmosERP.BusinessLayer.Models.Module.Vendor.Command.Delete;
using KosmosERP.BusinessLayer.Models.Module.Vendor.Command.Edit;
using KosmosERP.BusinessLayer.Models.Module.Vendor.Command.Find;
using KosmosERP.BusinessLayer.Modules;
using KosmosERP.Models;
using KosmosERP.Module;
using KosmosERP.BusinessLayer.Models.Module.User.ListProfiles;

namespace KosmosERP.Api.Controllers;

//[Authorize]
[ApiController]
[Route("api/v1/[controller]")]
public class VendorController : ERPApiController
{
    private IVendorModule _Module;

    public VendorController(IVendorModule module) : base(module)
    {
        _Module = module;
    }

    
    [ERPAuthorize(new[] { ERPPermission.Read }, "vendor_read")]
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

    [ERPAuthorize(new[] { ERPPermission.Read }, "vendor_read")]
    [HttpGet("GetVendorByGuid", Name = "GetVendorByGuid")]
    [ProducesResponseType(typeof(Response<VendorDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> GetByGuid([FromQuery] string guid)
    {
        var result = await _Module.GetDtoByGuid(guid);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [ERPAuthorize(new[] { ERPPermission.Read }, "vendor_read")]
    [HttpPost("FindVendor", Name = "FindVendor")]
    [ProducesResponseType(typeof(PagingResult<VendorListDto>), 200)]
    [ProducesResponseType(500)]
    public async Task<ActionResult> Find([FromQuery] GeneralListProfile listProfile, [FromBody] VendorFindCommand command)
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

    [ERPAuthorize(new[] { ERPPermission.Write }, "vendor_write")]
    [HttpPost("CreateVendor", Name = "CreateVendor")]
    [ProducesResponseType(typeof(Response<VendorDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> Create([FromBody] VendorCreateCommand createCommand)
    {
        createCommand.calling_user_id = this.CurrentUserId;

        var result = await _Module.Create(createCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [ERPAuthorize(new[] { ERPPermission.Edit }, "vendor_edit")]
    [HttpPut("UpdateVendor", Name = "UpdateVendor")]
    [ProducesResponseType(typeof(Response<VendorDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> Edit([FromBody] VendorEditCommand editCommand)
    {
        editCommand.calling_user_id = this.CurrentUserId;

        var result = await _Module.Edit(editCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [ERPAuthorize(new[] { ERPPermission.Delete }, "vendor_delete")]
    [HttpPost("DeleteVendor", Name = "DeleteVendor")]
    [ProducesResponseType(typeof(Response<VendorDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> Delete([FromBody] VendorDeleteCommand deleteCommand)
    {
        deleteCommand.calling_user_id = this.CurrentUserId;

        var result = await _Module.Delete(deleteCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
}
