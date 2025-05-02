using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using KosmosERP.BusinessLayer.Models.Module.Transaction.Dto;
using KosmosERP.BusinessLayer.Models.Module.Transaction.Command.Create;
using KosmosERP.BusinessLayer.Models.Module.Transaction.Command.Delete;
using KosmosERP.BusinessLayer.Models.Module.Transaction.Command.Edit;
using KosmosERP.BusinessLayer.Models.Module.Transaction.Command.Find;
using KosmosERP.BusinessLayer.Modules;
using KosmosERP.Models;
using KosmosERP.Module;
using KosmosERP.BusinessLayer.Models.Module.User.ListProfiles;

namespace KosmosERP.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/[controller]")]
public class TransactionController : ERPApiController
{
    private ITransactionModule _Module;

    public TransactionController(ITransactionModule module) : base(module)
    {
        _Module = module;
    }

    
    [HttpGet("GetTransaction", Name = "GetTransaction")]
    [ProducesResponseType(typeof(Response<TransactionDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> Get([FromQuery] int id)
    {
        var result = await _Module.GetDto(id);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPost("FindTransaction", Name = "FindTransaction")]
    [ProducesResponseType(typeof(PagingResult<TransactionListDto>), 200)]
    [ProducesResponseType(500)]
    public async Task<ActionResult> Find([FromQuery] GeneralListProfile listProfile, [FromBody] TransactionFindCommand command)
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

    [HttpPost("CreateTransaction", Name = "CreateTransaction")]
    [ProducesResponseType(typeof(Response<TransactionDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> Create([FromBody] TransactionCreateCommand createCommand)
    {
        var result = await _Module.Create(createCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPut("UpdateTransaction", Name = "UpdateTransaction")]
    [ProducesResponseType(typeof(Response<TransactionDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> Edit([FromBody] TransactionEditCommand editCommand)
    {
        var result = await _Module.Edit(editCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpDelete("DeleteTransaction", Name = "DeleteTransaction")]
    [ProducesResponseType(typeof(Response<TransactionDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> Delete([FromBody] TransactionDeleteCommand deleteCommand)
    {
        var result = await _Module.Delete(deleteCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
}
