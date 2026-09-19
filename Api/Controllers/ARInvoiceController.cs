using KosmosERP.Api.Authorization;
using KosmosERP.Api.Models;
using Microsoft.AspNetCore.Mvc;
using KosmosERP.Models;
using KosmosERP.Module;
using KosmosERP.BusinessLayer.Modules;
using KosmosERP.BusinessLayer.Models.Module.ARInvoice.Command.Create;
using KosmosERP.BusinessLayer.Models.Module.ARInvoice.Command.Delete;
using KosmosERP.BusinessLayer.Models.Module.ARInvoice.Command.Edit;
using KosmosERP.BusinessLayer.Models.Module.ARInvoice.Command.Find;
using KosmosERP.BusinessLayer.Models.Module.ARInvoice.Dto;
using KosmosERP.BusinessLayer.Models.Module.User.ListProfiles;
using KosmosERP.Database.Views;


namespace KosmosERP.Api.Controllers;


[ApiController]
[Route("api/v1/[controller]")]
public class ARInvoiceController : ERPApiController
{
    private IARInvoiceModule _Module;

    public ARInvoiceController(IARInvoiceModule module) : base(module)
    {
        _Module = module;
    }


    [ERPAuthorize(new[] { ERPPermission.Read }, "ar_invoice_read")]
    [HttpGet("GetARInvoice", Name = "GetARInvoice")]
    [ProducesResponseType(typeof(Response<ARInvoiceHeaderDto>), 200)]
    [ProducesResponseType(500)]
    public async Task<ActionResult> Get([FromQuery] int id)
    {
        var result = await _Module.GetDto(id);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [ERPAuthorize(new[] { ERPPermission.Read }, "ar_invoice_read")]
    [HttpGet("GetARInvoiceByGuid", Name = "GetARInvoiceByGuid")]
    [ProducesResponseType(typeof(Response<ARInvoiceHeaderDto>), 200)]
    [ProducesResponseType(500)]
    public async Task<ActionResult> GetByGuid([FromQuery] string guid)
    {
        var result = await _Module.GetDtoByGuid(guid);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [ERPAuthorize(new[] { ERPPermission.Read }, "ar_invoice_read")]
    [HttpGet("GetOrdersReadyForInvoicing", Name = "GetOrdersReadyForInvoicing")]
    [ProducesResponseType(typeof(Response<List<vw_OrdersReadyForInvoicing>>), 200)]
    [ProducesResponseType(500)]
    public async Task<ActionResult> GetOrdersReadyForInvoicing([FromQuery] string? customer_guid)
    {
        
        var result = await _Module.GetOrdersReadyForInvoicing(customer_guid);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [ERPAuthorize(new[] { ERPPermission.Read }, "ar_invoice_read")]
    [HttpGet("GetPartialInvoices", Name = "GetPartialInvoices")]
    [ProducesResponseType(typeof(Response<List<vw_PartialInvoices>>), 200)]
    [ProducesResponseType(500)]
    public async Task<ActionResult> GetPartialInvoices()
    {
        var result = await _Module.GetPartialInvoices();

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [ERPAuthorize(new[] { ERPPermission.Read }, "ar_invoice_read")]
    [HttpPost("FindARInvoice", Name = "FindARInvoice")]
    [ProducesResponseType(typeof(PagingResult<ARInvoiceHeaderListDto>), 200)]
    [ProducesResponseType(500)]
    public async Task<ActionResult> Find([FromQuery] GeneralListProfile listProfile, [FromBody] ARInvoiceHeaderFindCommand command)
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

    [ERPAuthorize(new[] { ERPPermission.Write }, "ar_invoice_write")]
    [HttpPost("CreateARInvoice", Name = "CreateARInvoice")]
    [ProducesResponseType(typeof(Response<ARInvoiceHeaderDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> Create([FromBody] ARInvoiceHeaderCreateCommand createCommand)
    {
        createCommand.calling_user_id = this.CurrentUserId;
        
        var result = await _Module.Create(createCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [ERPAuthorize(new[] { ERPPermission.Write }, "ar_invoice_write")]
    [HttpPost("CreateARInvoiceLine", Name = "CreateARInvoiceLine")]
    [ProducesResponseType(typeof(Response<ARInvoiceLineDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> CreateLine([FromBody] ARInvoiceLineCreateCommand createCommand)
    {
        createCommand.calling_user_id = this.CurrentUserId;

        var result = await _Module.CreateLine(createCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [ERPAuthorize(new[] { ERPPermission.Edit }, "ar_invoice_edit")]
    [HttpPut("UpdateARInvoice", Name = "UpdateARInvoice")]
    [ProducesResponseType(typeof(Response<ARInvoiceHeaderDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> Edit([FromBody] ARInvoiceHeaderEditCommand editCommand)
    {
        editCommand.calling_user_id = this.CurrentUserId;

        var result = await _Module.Edit(editCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [ERPAuthorize(new[] { ERPPermission.Edit }, "ar_invoice_edit")]
    [HttpPut("UpdateARInvoiceLine", Name = "UpdateARInvoiceLine")]
    [ProducesResponseType(typeof(Response<ARInvoiceLineDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> EditLine([FromBody] ARInvoiceLineEditCommand editCommand)
    {
        editCommand.calling_user_id = this.CurrentUserId;

        var result = await _Module.EditLine(editCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [ERPAuthorize(new[] { ERPPermission.Delete }, "ar_invoice_delete")]
    [HttpPost("DeleteARInvoice", Name = "DeleteARInvoice")]
    [ProducesResponseType(typeof(Response<ARInvoiceHeaderDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> Delete([FromBody] ARInvoiceHeaderDeleteCommand deleteCommand)
    {
        deleteCommand.calling_user_id = this.CurrentUserId;

        var result = await _Module.Delete(deleteCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [ERPAuthorize(new[] { ERPPermission.Delete }, "ar_invoice_delete")]
    [HttpPost("DeleteARInvoiceLine", Name = "DeleteARInvoiceLine")]
    [ProducesResponseType(typeof(Response<ARInvoiceLineDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> DeleteLine([FromBody] ARInvoiceLineDeleteCommand deleteCommand)
    {
        deleteCommand.calling_user_id = this.CurrentUserId;
        
        var result = await _Module.DeleteLine(deleteCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
}