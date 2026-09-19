using KosmosERP.Api.Authorization;
using KosmosERP.Api.Models;
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

[ApiController]
[Route("api/v1/[controller]")]
public class TransactionController : ERPApiController
{
    private ITransactionModule _Module;

    public TransactionController(ITransactionModule module) : base(module)
    {
        _Module = module;
    }

    
    [ERPAuthorize(new[] { ERPPermission.Read }, "transaction_read")]
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

    [ERPAuthorize(new[] { ERPPermission.Read }, "transaction_read")]
    [HttpGet("GetTransactionByGuid", Name = "GetTransactionByGuid")]
    [ProducesResponseType(typeof(Response<TransactionDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> GetByGuid([FromQuery] string guid)
    {
        var result = await _Module.GetDtoByGuid(guid);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [ERPAuthorize(new[] { ERPPermission.Read }, "transaction_read")]
    [HttpPost("FindTransaction", Name = "FindTransaction")]
    [ProducesResponseType(typeof(PagingResult<TransactionListDto>), 200)]
    [ProducesResponseType(500)]
    public async Task<ActionResult> Find([FromQuery] GeneralListProfile listProfile, [FromBody] TransactionFindCommand command)
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

    [ERPAuthorize(new[] { ERPPermission.Write }, "transaction_write")]
    [HttpPost("CreateTransaction", Name = "CreateTransaction")]
    [ProducesResponseType(typeof(Response<TransactionDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> Create([FromBody] TransactionCreateCommand createCommand)
    {
        createCommand.calling_user_id = this.CurrentUserId;

        var result = await _Module.Create(createCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [ERPAuthorize(new[] { ERPPermission.Edit }, "transaction_edit")]
    [HttpPut("UpdateTransaction", Name = "UpdateTransaction")]
    [ProducesResponseType(typeof(Response<TransactionDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> Edit([FromBody] TransactionEditCommand editCommand)
    {
        editCommand.calling_user_id = this.CurrentUserId;

        var result = await _Module.Edit(editCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [ERPAuthorize(new[] { ERPPermission.Delete }, "transaction_delete")]
    [HttpPost("DeleteTransaction", Name = "DeleteTransaction")]
    [ProducesResponseType(typeof(Response<TransactionDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> Delete([FromBody] TransactionDeleteCommand deleteCommand)
    {
        deleteCommand.calling_user_id = this.CurrentUserId;

        var result = await _Module.Delete(deleteCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
}
