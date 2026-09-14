using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using KosmosERP.BusinessLayer.Models.Module.FinancialTransaction.Dto;
using KosmosERP.BusinessLayer.Models.Module.FinancialTransaction.Command.Find;
using KosmosERP.BusinessLayer.Modules;
using KosmosERP.Models;
using KosmosERP.Module;
using KosmosERP.BusinessLayer.Models.Module.User.ListProfiles;

namespace KosmosERP.Api.Controllers;

//[Authorize]
[ApiController]
[Route("api/v1/[controller]")]
public class FinancialTransactionController : ERPApiController
{
    private IFinancialTransactionModule _Module;

    public FinancialTransactionController(IFinancialTransactionModule module) : base(module)
    {
        _Module = module;
    }

    [Authorize(Roles = "financial_transaction_read")]
    [HttpGet("GetFinancialTransaction", Name = "GetFinancialTransaction")]
    [ProducesResponseType(typeof(Response<FinancialTransactionDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> Get([FromQuery] int id)
    {
        var result = await _Module.GetDto(id);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [Authorize(Roles = "financial_transaction_read")]
    [HttpGet("GetFinancialTransactionByGuid", Name = "GetFinancialTransactionByGuid")]
    [ProducesResponseType(typeof(Response<FinancialTransactionDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> GetByGuid([FromQuery] string guid)
    {
        var result = await _Module.GetDtoByGuid(guid);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [Authorize(Roles = "financial_transaction_read")]
    [HttpPost("GetAccountBalance", Name = "GetAccountBalance")]
    [ProducesResponseType(typeof(Response<AccountBalanceDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> GetAccountBalance([FromBody] AccountBalanceFindCommand command)
    {
        var result = await _Module.GetAccountBalance(command);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [Authorize(Roles = "financial_transaction_read")]
    [HttpGet("GetAccountLedger", Name = "GetAccountLedger")]
    [ProducesResponseType(typeof(PagingResult<FinancialTransactionListDto>), 200)]
    [ProducesResponseType(500)]
    public async Task<ActionResult> GetAccountLedger(
        [FromQuery] GeneralListProfile listProfile,
        [FromQuery] int chartOfAccountId,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate)
    {
        try
        {
            var sortingParams = new PagingSortingParameters(listProfile.Start, listProfile.ResultCount, listProfile.SortOrder);

            var result = await _Module.GetAccountLedger(sortingParams, chartOfAccountId, fromDate, toDate);

            return Ok(result);
        }
        catch (Exception e)
        {
            return StatusCode(500, e.Message);
        }
    }

    [Authorize(Roles = "financial_transaction_read")]
    [HttpPost("FindFinancialTransaction", Name = "FindFinancialTransaction")]
    [ProducesResponseType(typeof(PagingResult<FinancialTransactionListDto>), 200)]
    [ProducesResponseType(500)]
    public async Task<ActionResult> Find([FromQuery] GeneralListProfile listProfile, [FromBody] FinancialTransactionFindCommand command)
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
}
