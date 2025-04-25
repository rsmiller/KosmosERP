using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Prometheus.BusinessLayer.Models.Module.Customer.Command.Edit;
using Prometheus.BusinessLayer.Models.Module.DocumentUpload.Command.Create;
using Prometheus.BusinessLayer.Models.Module.DocumentUpload.Command.Edit;
using Prometheus.BusinessLayer.Models.Module.DocumentUpload.Command.Find;
using Prometheus.BusinessLayer.Models.Module.DocumentUpload.Dto;
using Prometheus.BusinessLayer.Models.Module.User.ListProfiles;
using Prometheus.BusinessLayer.Modules;
using Prometheus.Models;
using Prometheus.Module;

namespace Prometheus.Api.Controllers;

//[Authorize]
[ApiController]
[Route("api/v1/[controller]")]
public class DocumentController : ERPApiController
{
    private IDocumentUploadModule _Module;

    public DocumentController(IDocumentUploadModule module) : base(module)
    {
        _Module = module;
    }

    [HttpGet("GetDocument", Name = "GetDocument")]
    [ProducesResponseType(typeof(Response<DocumentUploadDto>), 200)]
    public async Task<ActionResult> Get([FromQuery] int id)
    {
        var result = await _Module.GetDto(id);

        return Ok(result);
    }

    [HttpPost("FindDocument", Name = "FindDocument")]
    [ProducesResponseType(typeof(PagingResult<DocumentUploadListDto>), 200)]
    [ProducesResponseType(500)]
    public async Task<ActionResult> Find([FromQuery] GeneralListProfile listProfile, [FromBody] DocumentUploadFindCommand command)
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

    [HttpPost("CreateDocument", Name = "CreateDocument")]
    [ProducesResponseType(typeof(Response<DocumentUploadDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> Create(IFormFile file, [FromForm] DocumentUploadCreateCommand createCommand)
    {
        var result = await _Module.CreateOverride(file, createCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    
    [HttpPost("CreateNewFileRevision", Name = "CreateNewFileRevision")]
    [ProducesResponseType(typeof(Response<DocumentUploadDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> CreateNewFileRevision(IFormFile file, [FromForm] DocumentUploadEditCommand editCommand)
    {
        var result = await _Module.CreateNewFileRevision(file, editCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
}
