using KosmosERP.Api.Models;
using KosmosERP.Api.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using KosmosERP.BusinessLayer.Models.Module.Country.Dto;
using KosmosERP.BusinessLayer.Models.Module.Country.Command.Create;
using KosmosERP.BusinessLayer.Models.Module.Country.Command.Delete;
using KosmosERP.BusinessLayer.Models.Module.Country.Command.Edit;
using KosmosERP.BusinessLayer.Models.Module.Country.Command.Find;
using KosmosERP.BusinessLayer.Modules;
using KosmosERP.Models;
using KosmosERP.Module;
using KosmosERP.BusinessLayer.Models.Module.User.ListProfiles;

namespace KosmosERP.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class CountryController : ERPApiController
{
    private ICountryModule _Module;

    public CountryController(ICountryModule module) : base(module)
    {
        _Module = module;
    }

    
    [ERPAuthorize(new ERPPermission[] { })]
    [HttpGet("GetCountry", Name = "GetCountry")]
    [ProducesResponseType(typeof(Response<CountryDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> Get([FromQuery] int id)
    {
        var result = await _Module.GetDto(id);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [ERPAuthorize(new ERPPermission[] { })]
    [HttpGet("GetCountryByGuid", Name = "GetCountryByGuid")]
    [ProducesResponseType(typeof(Response<CountryDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> GetByGuid([FromQuery] string guid)
    {
        var result = await _Module.GetDtoByGuid(guid);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [ERPAuthorize(new ERPPermission[] { })]
    [HttpGet("GetCountryByISOAsync", Name = "GetCountryByISOAsync")]
    [ProducesResponseType(typeof(Response<CountryDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> GetByISOAsync([FromQuery] string iso3)
    {
        var result = await _Module.GetByISOAsync(iso3);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }


    [ERPAuthorize(new ERPPermission[] { })]
    [HttpPost("FindCountry", Name = "FindCountry")]
    [ProducesResponseType(typeof(PagingResult<CountryListDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> Find([FromQuery] GeneralListProfile listProfile, [FromBody] CountryFindCommand command)
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

    [ERPAuthorize(new ERPPermission[] { }, "admin")]
    [HttpPost("CreateCountry", Name = "CreateCountry")]
    [ProducesResponseType(typeof(Response<CountryDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> Create([FromBody] CountryCreateCommand createCommand)
    {
        createCommand.calling_user_id = this.CurrentUserId;
        var result = await _Module.Create(createCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [ERPAuthorize(new ERPPermission[] { }, "admin")]
    [HttpPut("UpdateCountry", Name = "UpdateCountry")]
    [ProducesResponseType(typeof(Response<CountryDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> Edit([FromBody] CountryEditCommand editCommand)
    {
        editCommand.calling_user_id = this.CurrentUserId;
        var result = await _Module.Edit(editCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [ERPAuthorize(new ERPPermission[] { }, "admin")]
    [HttpPost("DeleteCountry", Name = "DeleteCountry")]
    [ProducesResponseType(typeof(Response<CountryDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> Delete([FromBody] CountryDeleteCommand deleteCommand)
    {
        deleteCommand.calling_user_id = this.CurrentUserId;
        var result = await _Module.Delete(deleteCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
}
