using KosmosERP.Api.Authorization;
using KosmosERP.Api.Models;
using Microsoft.AspNetCore.Mvc;
using KosmosERP.Models;
using KosmosERP.Module;
using KosmosERP.BusinessLayer.Models.Module.APInvoice.Command;
using KosmosERP.BusinessLayer.Models.Module.APInvoice.Command.Create;
using KosmosERP.BusinessLayer.Models.Module.APInvoice.Command.Delete;
using KosmosERP.BusinessLayer.Models.Module.APInvoice.Command.Edit;
using KosmosERP.BusinessLayer.Models.Module.APInvoice.Command.Find;
using KosmosERP.BusinessLayer.Models.Module.APInvoice.Dto;
using KosmosERP.BusinessLayer.Models.Module.User.ListProfiles;
using KosmosERP.BusinessLayer.Modules;

namespace KosmosERP.Api.Controllers;


[ApiController]
[Route("api/v1/[controller]")]
public class APInvoiceController : ERPApiController
{
    private IAPInvoiceModule _Module;

    public APInvoiceController(IAPInvoiceModule module) : base(module)
    {
        _Module = module;
    }

    [ERPAuthorize(new[] { ERPPermission.Read }, "ap_invoice_read")]
    [HttpGet("GetAPInvoice", Name = "GetAPInvoice")]
    [ProducesResponseType(typeof(Response<APInvoiceHeaderDto>), 200)]
    [ProducesResponseType(500)]
    public async Task<ActionResult> Get([FromQuery] int id)
    {
        var result = await _Module.GetDto(id);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [ERPAuthorize(new[] { ERPPermission.Read }, "ap_invoice_read")]
    [HttpGet("GetAPInvoiceByGuid", Name = "GetAPInvoiceByGuid")]
    [ProducesResponseType(typeof(Response<APInvoiceHeaderDto>), 200)]
    [ProducesResponseType(500)]
    public async Task<ActionResult> GetByGuid([FromQuery] string guid)
    {
        var result = await _Module.GetDtoByGuid(guid);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [ERPAuthorize(new[] { ERPPermission.Edit, ERPPermission.Write }, "ap_invoice_edit", "ap_invoice_write")]
    [HttpPost("AssociateAPInvoice", Name = "AssociateAPInvoice")]
    [ProducesResponseType(typeof(PagingResult<APInvoiceHeaderDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> AssociateAPInvoiceHeaderObject([FromBody] APInvoiceAssoicationCommand command)
    {
        command.calling_user_id = this.CurrentUserId;

        var result = await _Module.AssociateHeaderObject(command);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [ERPAuthorize(new[] { ERPPermission.Edit, ERPPermission.Write }, "ap_invoice_edit", "ap_invoice_write")]
    [HttpPost("AssociateAPInvoiceLine", Name = "AssociateAPInvoiceLine")]
    [ProducesResponseType(typeof(PagingResult<APInvoiceLineDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> AssociateAPInvoiceLine([FromBody] APInvoiceAssoicationCommand command)
    {
        command.calling_user_id = this.CurrentUserId;

        var result = await _Module.AssociateHeaderObject(command);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [ERPAuthorize(new[] { ERPPermission.Edit, ERPPermission.Write }, "ap_invoice_edit", "ap_invoice_write")]
    [HttpPost("AssociateReceivedPO", Name = "AssociateReceivedPO")]
    [ProducesResponseType(typeof(PagingResult<APInvoiceLineDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> AssociateReceivedPO([FromBody] APInvoiceAssociatePOCommand command)
    {
        command.calling_user_id = this.CurrentUserId;

        var result = await _Module.AssociateReceivedPO(command);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [ERPAuthorize(new[] { ERPPermission.Read }, "ap_invoice_read")]
    [HttpPost("FindAPInvoice", Name = "FindAPInvoice")]
    [ProducesResponseType(typeof(PagingResult<APInvoiceHeaderListDto>), 200)]
    [ProducesResponseType(500)]
    public async Task<ActionResult> Find([FromQuery] GeneralListProfile listProfile, [FromBody] APInvoiceHeaderFindCommand command)
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

    [ERPAuthorize(new[] { ERPPermission.Write }, "ap_invoice_write")]
    [HttpPost("CreateAPInvoice", Name = "CreateAPInvoice")]
    [ProducesResponseType(typeof(Response<APInvoiceHeaderDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> Create([FromBody] APInvoiceHeaderCreateCommand createCommand)
    {
        createCommand.calling_user_id = this.CurrentUserId;

        var result = await _Module.Create(createCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [ERPAuthorize(new[] { ERPPermission.Write }, "ap_invoice_write")]
    [HttpPost("CreateAPInvoiceLine", Name = "CreateAPInvoiceLine")]
    [ProducesResponseType(typeof(Response<APInvoiceLineDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> CreateLine([FromBody] APInvoiceLineCreateCommand createCommand)
    {
        createCommand.calling_user_id = this.CurrentUserId;

        var result = await _Module.CreateLine(createCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [ERPAuthorize(new[] { ERPPermission.Edit }, "ap_invoice_edit")]
    [HttpPut("UpdateAPInvoice", Name = "UpdateAPInvoice")]
    [ProducesResponseType(typeof(Response<APInvoiceHeaderDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> Edit([FromBody] APInvoiceHeaderEditCommand editCommand)
    {
        editCommand.calling_user_id = this.CurrentUserId;

        var result = await _Module.Edit(editCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [ERPAuthorize(new[] { ERPPermission.Edit }, "ap_invoice_edit")]
    [HttpPut("UpdateAPInvoiceLine", Name = "UpdateAPInvoiceLine")]
    [ProducesResponseType(typeof(Response<APInvoiceLineDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> EditLine([FromBody] APInvoiceLineEditCommand editCommand)
    {
        editCommand.calling_user_id = this.CurrentUserId;

        var result = await _Module.EditLine(editCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [ERPAuthorize(new[] { ERPPermission.Delete }, "ap_invoice_delete")]
    [HttpPost("DeleteAPInvoice", Name = "DeleteAPInvoice")]
    [ProducesResponseType(typeof(Response<APInvoiceHeaderDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> Delete([FromBody] APInvoiceHeaderDeleteCommand deleteCommand)
    {
        deleteCommand.calling_user_id = this.CurrentUserId;

        var result = await _Module.Delete(deleteCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [ERPAuthorize(new[] { ERPPermission.Delete }, "ap_invoice_delete")]
    [HttpPost("DeleteAPInvoiceLine", Name = "DeleteAPInvoiceLine")]
    [ProducesResponseType(typeof(Response<APInvoiceLineDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> DeleteLine([FromBody] APInvoiceLineDeleteCommand deleteCommand)
    {
        deleteCommand.calling_user_id = this.CurrentUserId;
        
        var result = await _Module.DeleteLine(deleteCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
}