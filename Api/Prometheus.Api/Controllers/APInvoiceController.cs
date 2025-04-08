using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Prometheus.Api.Modules;
using Prometheus.Models;
using Prometheus.Module;
using Prometheus.BusinessLayer.Models.Module.APInvoice.Command;
using Prometheus.BusinessLayer.Models.Module.APInvoiceHeader.Command.Create;
using Prometheus.BusinessLayer.Models.Module.APInvoiceHeader.Command.Delete;
using Prometheus.BusinessLayer.Models.Module.APInvoiceHeader.Command.Edit;
using Prometheus.BusinessLayer.Models.Module.APInvoiceHeader.Command.Find;
using Prometheus.BusinessLayer.Models.Module.APInvoiceHeader.Dto;
using Prometheus.BusinessLayer.Models.Module.APInvoiceLine.Command.Create;
using Prometheus.BusinessLayer.Models.Module.APInvoiceLine.Command.Delete;
using Prometheus.BusinessLayer.Models.Module.APInvoiceLine.Command.Edit;
using Prometheus.BusinessLayer.Models.Module.APInvoiceLine.Dto;
using Prometheus.BusinessLayer.Models.Module.User.ListProfiles;

namespace Prometheus.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/[controller]")]
public class APInvoiceController : ERPApiController
{
    private IAPInvoiceModule _Module;

    public APInvoiceController(IAPInvoiceModule module) : base(module)
    {
        _Module = module;
    }


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


    [HttpPost("AssociateAPInvoice", Name = "AssociateAPInvoice")]
    [ProducesResponseType(typeof(PagingResult<APInvoiceHeaderDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> AssociateAPInvoiceHeaderObject([FromBody] APInvoiceAssoicationCommand command)
    {
        var result = await _Module.AssociateHeaderObject(command);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPost("AssociateAPInvoiceLine", Name = "AssociateAPInvoiceLine")]
    [ProducesResponseType(typeof(PagingResult<APInvoiceLineDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> AssociateAPInvoiceLine([FromBody] APInvoiceAssoicationCommand command)
    {
        var result = await _Module.AssociateHeaderObject(command);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPost("AssociateReceivedPO", Name = "AssociateReceivedPO")]
    [ProducesResponseType(typeof(PagingResult<APInvoiceLineDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> AssociateReceivedPO([FromBody] APInvoiceAssociatePOCommand command)
    {
        var result = await _Module.AssociateReceivedPO(command);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPost("FindAPInvoice", Name = "FindAPInvoice")]
    [ProducesResponseType(typeof(PagingResult<APInvoiceHeaderListDto>), 200)]
    [ProducesResponseType(500)]
    public async Task<ActionResult> Find([FromQuery] GeneralListProfile listProfile, [FromBody] APInvoiceHeaderFindCommand command)
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

    [HttpPost("CreateAPInvoice", Name = "CreateAPInvoice")]
    [ProducesResponseType(typeof(Response<APInvoiceHeaderDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> Create([FromBody] APInvoiceHeaderCreateCommand createCommand)
    {
        var result = await _Module.Create(createCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPost("CreateAPInvoiceLine", Name = "CreateAPInvoiceLine")]
    [ProducesResponseType(typeof(Response<APInvoiceLineDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> CreateLine([FromBody] APInvoiceLineCreateCommand createCommand)
    {
        var result = await _Module.CreateLine(createCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPut("UpdateAPInvoice", Name = "UpdateAPInvoice")]
    [ProducesResponseType(typeof(Response<APInvoiceHeaderDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> Edit([FromBody] APInvoiceHeaderEditCommand editCommand)
    {
        var result = await _Module.Edit(editCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPut("UpdateAPInvoiceLine", Name = "UpdateAPInvoiceLine")]
    [ProducesResponseType(typeof(Response<APInvoiceLineDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> EditLine([FromBody] APInvoiceLineEditCommand editCommand)
    {
        var result = await _Module.EditLine(editCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpDelete("DeleteAPInvoice", Name = "DeleteAPInvoice")]
    [ProducesResponseType(typeof(Response<APInvoiceHeaderDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> Delete([FromBody] APInvoiceHeaderDeleteCommand deleteCommand)
    {
        var result = await _Module.Delete(deleteCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpDelete("DeleteAPInvoiceLine", Name = "DeleteAPInvoiceLine")]
    [ProducesResponseType(typeof(Response<APInvoiceLineDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> DeleteLine([FromBody] APInvoiceLineDeleteCommand deleteCommand)
    {
        var result = await _Module.DeleteLine(deleteCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
}