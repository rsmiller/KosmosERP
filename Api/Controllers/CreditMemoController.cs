using KosmosERP.Api.Authorization;
using KosmosERP.Api.Models;
using Microsoft.AspNetCore.Mvc;
using KosmosERP.Models;
using KosmosERP.Module;
using KosmosERP.BusinessLayer.Models.Module.CreditMemo.Command.Create;
using KosmosERP.BusinessLayer.Models.Module.CreditMemo.Command.Delete;
using KosmosERP.BusinessLayer.Models.Module.CreditMemo.Command.Edit;
using KosmosERP.BusinessLayer.Models.Module.CreditMemo.Command.Find;
using KosmosERP.BusinessLayer.Models.Module.CreditMemo.Dto;
using KosmosERP.BusinessLayer.Models.Module.User.ListProfiles;
using KosmosERP.BusinessLayer.Modules;

namespace KosmosERP.Api.Controllers;


[ApiController]
[Route("api/v1/[controller]")]
public class CreditMemoController : ERPApiController
{
    private ICreditMemoModule _Module;

    public CreditMemoController(ICreditMemoModule module) : base(module)
    {
        _Module = module;
    }

    [ERPAuthorize(new[] { ERPPermission.Read }, "credit_memo_read")]
    [HttpGet("GetCreditMemo", Name = "GetCreditMemo")]
    [ProducesResponseType(typeof(Response<CreditMemoHeaderDto>), 200)]
    [ProducesResponseType(500)]
    public async Task<ActionResult> Get([FromQuery] int id)
    {
        var result = await _Module.GetDto(id);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [ERPAuthorize(new[] { ERPPermission.Read }, "credit_memo_read")]
    [HttpGet("GetCreditMemoByGuid", Name = "GetCreditMemoByGuid")]
    [ProducesResponseType(typeof(Response<CreditMemoHeaderDto>), 200)]
    [ProducesResponseType(500)]
    public async Task<ActionResult> GetByGuid([FromQuery] string guid)
    {
        var result = await _Module.GetDtoByGuid(guid);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [ERPAuthorize(new[] { ERPPermission.Read }, "credit_memo_read")]
    [HttpPost("FindCreditMemo", Name = "FindCreditMemo")]
    [ProducesResponseType(typeof(PagingResult<CreditMemoHeaderListDto>), 200)]
    [ProducesResponseType(500)]
    public async Task<ActionResult> Find([FromQuery] GeneralListProfile listProfile, [FromBody] CreditMemoHeaderFindCommand command)
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

    [ERPAuthorize(new[] { ERPPermission.Write }, "credit_memo_write")]
    [HttpPost("CreateCreditMemo", Name = "CreateCreditMemo")]
    [ProducesResponseType(typeof(Response<CreditMemoHeaderDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> Create([FromBody] CreditMemoHeaderCreateCommand createCommand)
    {
        createCommand.calling_user_id = this.CurrentUserId;
        var result = await _Module.Create(createCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [ERPAuthorize(new[] { ERPPermission.Write }, "credit_memo_write")]
    [HttpPost("CreateCreditMemoLine", Name = "CreateCreditMemoLine")]
    [ProducesResponseType(typeof(Response<CreditMemoLineDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> CreateLine([FromBody] CreditMemoLineCreateCommand createCommand)
    {
        createCommand.calling_user_id = this.CurrentUserId;
        var result = await _Module.CreateLine(createCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [ERPAuthorize(new[] { ERPPermission.Edit }, "credit_memo_edit")]
    [HttpPut("UpdateCreditMemo", Name = "UpdateCreditMemo")]
    [ProducesResponseType(typeof(Response<CreditMemoHeaderDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> Edit([FromBody] CreditMemoHeaderEditCommand editCommand)
    {
        editCommand.calling_user_id = this.CurrentUserId;
        var result = await _Module.Edit(editCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [ERPAuthorize(new[] { ERPPermission.Edit }, "credit_memo_edit")]
    [HttpPut("UpdateCreditMemoLine", Name = "UpdateCreditMemoLine")]
    [ProducesResponseType(typeof(Response<CreditMemoLineDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> EditLine([FromBody] CreditMemoLineEditCommand editCommand)
    {
        editCommand.calling_user_id = this.CurrentUserId;
        var result = await _Module.EditLine(editCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [ERPAuthorize(new[] { ERPPermission.Delete }, "credit_memo_delete")]
    [HttpPost("DeleteCreditMemo", Name = "DeleteCreditMemo")]
    [ProducesResponseType(typeof(Response<CreditMemoHeaderDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> Delete([FromBody] CreditMemoHeaderDeleteCommand deleteCommand)
    {
        deleteCommand.calling_user_id = this.CurrentUserId;
        var result = await _Module.Delete(deleteCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [ERPAuthorize(new[] { ERPPermission.Delete }, "credit_memo_delete")]
    [HttpPost("DeleteCreditMemoLine", Name = "DeleteCreditMemoLine")]
    [ProducesResponseType(typeof(Response<CreditMemoLineDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> DeleteLine([FromBody] CreditMemoLineDeleteCommand deleteCommand)
    {
        deleteCommand.calling_user_id = this.CurrentUserId;
        var result = await _Module.DeleteLine(deleteCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
} 