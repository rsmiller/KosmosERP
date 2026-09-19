using KosmosERP.Api.Authorization;
using KosmosERP.Api.Models;
using Microsoft.AspNetCore.Mvc;
using KosmosERP.BusinessLayer.Models.Module.ChartOfAccount.Dto;
using KosmosERP.BusinessLayer.Models.Module.ChartOfAccount.Command.Create;
using KosmosERP.BusinessLayer.Models.Module.ChartOfAccount.Command.Delete;
using KosmosERP.BusinessLayer.Models.Module.ChartOfAccount.Command.Edit;
using KosmosERP.BusinessLayer.Models.Module.ChartOfAccount.Command.Find;
using KosmosERP.BusinessLayer.Modules;
using KosmosERP.Models;
using KosmosERP.Module;
using KosmosERP.BusinessLayer.Models.Module.User.ListProfiles;

namespace KosmosERP.Api.Controllers;

//[Authorize]
[ApiController]
[Route("api/v1/[controller]")]
public class ChartOfAccountController : ERPApiController
{
    private IChartOfAccountModule _Module;

    public ChartOfAccountController(IChartOfAccountModule module) : base(module)
    {
        _Module = module;
    }

    [ERPAuthorize(new[] { ERPPermission.Read }, "chart_of_account_read")]
    [HttpGet("GetChartOfAccount", Name = "GetChartOfAccount")]
    [ProducesResponseType(typeof(Response<ChartOfAccountDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> Get([FromQuery] int id)
    {
        var result = await _Module.GetDto(id);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [ERPAuthorize(new[] { ERPPermission.Read }, "chart_of_account_read")]
    [HttpGet("GetChartOfAccountByGuid", Name = "GetChartOfAccountByGuid")]
    [ProducesResponseType(typeof(Response<ChartOfAccountDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> GetByGuid([FromQuery] string guid)
    {
        var result = await _Module.GetDtoByGuid(guid);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [ERPAuthorize(new[] { ERPPermission.Read }, "chart_of_account_read")]
    [HttpGet("GetChartOfAccountByAccountNumber", Name = "GetChartOfAccountByAccountNumber")]
    [ProducesResponseType(typeof(Response<ChartOfAccountDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> GetByAccountNumber([FromQuery] string accountNumber)
    {
        var result = await _Module.GetDtoByAccountNumber(accountNumber);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [ERPAuthorize(new[] { ERPPermission.Read }, "chart_of_account_read")]
    [HttpGet("GetChildAccounts", Name = "GetChildAccounts")]
    [ProducesResponseType(typeof(Response<List<ChartOfAccountListDto>>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> GetChildAccounts([FromQuery] int parentAccountId)
    {
        var result = await _Module.GetChildAccounts(parentAccountId);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [ERPAuthorize(new[] { ERPPermission.Read }, "chart_of_account_read")]
    [HttpGet("GetAccountsByType", Name = "GetAccountsByType")]
    [ProducesResponseType(typeof(Response<List<ChartOfAccountListDto>>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> GetAccountsByType([FromQuery] int accountType)
    {
        var result = await _Module.GetAccountsByType(accountType);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [ERPAuthorize(new[] { ERPPermission.Read }, "chart_of_account_read")]
    [HttpPost("FindChartOfAccount", Name = "FindChartOfAccount")]
    [ProducesResponseType(typeof(PagingResult<ChartOfAccountListDto>), 200)]
    [ProducesResponseType(500)]
    public async Task<ActionResult> Find([FromQuery] GeneralListProfile listProfile, [FromBody] ChartOfAccountFindCommand command)
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

    [ERPAuthorize(new[] { ERPPermission.Write }, "chart_of_account_write")]
    [HttpPost("CreateChartOfAccount", Name = "CreateChartOfAccount")]
    [ProducesResponseType(typeof(Response<ChartOfAccountDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> Create([FromBody] ChartOfAccountCreateCommand createCommand)
    {
        createCommand.calling_user_id = this.CurrentUserId;

        var result = await _Module.Create(createCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [ERPAuthorize(new[] { ERPPermission.Edit }, "chart_of_account_edit")]
    [HttpPut("UpdateChartOfAccount", Name = "UpdateChartOfAccount")]
    [ProducesResponseType(typeof(Response<ChartOfAccountDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> Edit([FromBody] ChartOfAccountEditCommand editCommand)
    {
        editCommand.calling_user_id = this.CurrentUserId;

        var result = await _Module.Edit(editCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [ERPAuthorize(new[] { ERPPermission.Delete }, "chart_of_account_delete")]
    [HttpPost("DeleteChartOfAccount", Name = "DeleteChartOfAccount")]
    [ProducesResponseType(typeof(Response<ChartOfAccountDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> Delete([FromBody] ChartOfAccountDeleteCommand deleteCommand)
    {
        deleteCommand.calling_user_id = this.CurrentUserId;

        var result = await _Module.Delete(deleteCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
}
