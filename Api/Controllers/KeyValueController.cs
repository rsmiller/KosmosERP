
using Microsoft.AspNetCore.Mvc;
using KosmosERP.Models;
using KosmosERP.BusinessLayer.Modules;
using KosmosERP.BusinessLayer.Models.Module.KeyValue.Dto;
using KosmosERP.BusinessLayer.Models.Module.User.ListProfiles;
using KosmosERP.BusinessLayer.Models.Module.KeyValue.Command.Find;
using KosmosERP.BusinessLayer.Models.Module.KeyValue.Command.Create;
using KosmosERP.BusinessLayer.Models.Module.KeyValue.Command.Delete;
using KosmosERP.BusinessLayer.Models.Module.KeyValue.Command.Edit;
using KosmosERP.BusinessLayer.Models.Module;
using Microsoft.AspNetCore.Authorization;

namespace KosmosERP.Api.Controllers;


[ApiController]
[Route("api/v1/[controller]")]
public class KeyValueController : ControllerBase
{
    private IKeyValueModule _Module;

    public KeyValueController(IKeyValueModule module)
    {
        _Module = module;
    }

    [Authorize(Roles = "data_type_read")]
    [HttpGet("GetModuleInfo", Name = "GetModuleInfo")]
    [ProducesResponseType(typeof(Response<List<ModuleObjectDto>>), 200)]
    public async Task<ActionResult> GetModuleInfo()
    {
        var result = await _Module.GetModuleInfo();

        return Ok(result);
    }

    [Authorize(Roles = "data_type_read")]
    [HttpGet("GetKeyValuesByModule", Name = "GetKeyValuesByModule")]
    [ProducesResponseType(typeof(Response<List<KeyValueDto>>), 200)]
    public async Task<ActionResult> GetKeyValuesByModule([FromQuery] string module_id)
    {
        var result = await _Module.GetDtoByModule(module_id);

        return Ok(result);
    }

    [Authorize(Roles = "data_type_read")]
    [HttpPost("FindKeyValue", Name = "FindKeyValue")]
    [ProducesResponseType(typeof(PagingResult<KeyValueDto>), 200)]
    [ProducesResponseType(500)]
    public async Task<ActionResult> Find([FromQuery] GeneralListProfile listProfile, [FromBody] KeyValueFindCommand command)
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

    [Authorize(Roles = "data_type_write")]
    [HttpPost("CreateKeyValue", Name = "CreateKeyValue")]
    [ProducesResponseType(typeof(Response<KeyValueDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> Create([FromBody] KeyValueCreateCommand createCommand)
    {
        var result = await _Module.Create(createCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [Authorize(Roles = "data_type_edit")]
    [HttpPut("UpdateKeyValue", Name = "UpdateKeyValue")]
    [ProducesResponseType(typeof(Response<KeyValueDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> Edit([FromBody] KeyValueEditCommand editCommand)
    {
        var result = await _Module.Edit(editCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [Authorize(Roles = "data_type_delete")]
    [HttpPost("DeleteKeyValue", Name = "DeleteKeyValue")]
    [ProducesResponseType(typeof(Response<KeyValueDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> Delete([FromBody] KeyValueDeleteCommand deleteCommand)
    {
        var result = await _Module.Delete(deleteCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
}
