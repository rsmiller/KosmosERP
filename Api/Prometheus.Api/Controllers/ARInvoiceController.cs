using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Prometheus.Models;
using Prometheus.Module;
using Prometheus.BusinessLayer.Modules;
using Prometheus.BusinessLayer.Models.Module.ARInvoice.Command.Create;
using Prometheus.BusinessLayer.Models.Module.ARInvoice.Command.Delete;
using Prometheus.BusinessLayer.Models.Module.ARInvoice.Command.Edit;
using Prometheus.BusinessLayer.Models.Module.ARInvoice.Command.Find;
using Prometheus.BusinessLayer.Models.Module.ARInvoice.Dto;
using Prometheus.BusinessLayer.Models.Module.User.ListProfiles;


namespace Prometheus.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/[controller]")]
public class ARInvoiceController : ERPApiController
{
    private IARInvoiceModule _Module;

    public ARInvoiceController(IARInvoiceModule module) : base(module)
    {
        _Module = module;
    }


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

    [HttpPost("FindARInvoice", Name = "FindARInvoice")]
    [ProducesResponseType(typeof(PagingResult<ARInvoiceHeaderListDto>), 200)]
    [ProducesResponseType(500)]
    public async Task<ActionResult> Find([FromQuery] GeneralListProfile listProfile, [FromBody] ARInvoiceHeaderFindCommand command)
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

    [HttpPost("CreateARInvoice", Name = "CreateARInvoice")]
    [ProducesResponseType(typeof(Response<ARInvoiceHeaderDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> Create([FromBody] ARInvoiceHeaderCreateCommand createCommand)
    {
        var result = await _Module.Create(createCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPost("CreateARInvoiceLine", Name = "CreateARInvoiceLine")]
    [ProducesResponseType(typeof(Response<ARInvoiceLineDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> CreateLine([FromBody] ARInvoiceLineCreateCommand createCommand)
    {
        var result = await _Module.CreateLine(createCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPut("UpdateARInvoice", Name = "UpdateARInvoice")]
    [ProducesResponseType(typeof(Response<ARInvoiceHeaderDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> Edit([FromBody] ARInvoiceHeaderEditCommand editCommand)
    {
        var result = await _Module.Edit(editCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPut("UpdateARInvoiceLine", Name = "UpdateARInvoiceLine")]
    [ProducesResponseType(typeof(Response<ARInvoiceLineDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> EditLine([FromBody] ARInvoiceLineEditCommand editCommand)
    {
        var result = await _Module.EditLine(editCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpDelete("DeleteARInvoice", Name = "DeleteARInvoice")]
    [ProducesResponseType(typeof(Response<ARInvoiceHeaderDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> Delete([FromBody] ARInvoiceHeaderDeleteCommand deleteCommand)
    {
        var result = await _Module.Delete(deleteCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpDelete("DeleteARInvoiceLine", Name = "DeleteARInvoiceLine")]
    [ProducesResponseType(typeof(Response<ARInvoiceLineDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> DeleteLine([FromBody] ARInvoiceLineDeleteCommand deleteCommand)
    {
        var result = await _Module.DeleteLine(deleteCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
}