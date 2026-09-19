using KosmosERP.Api.Authorization;
using KosmosERP.Api.Models;
using Microsoft.AspNetCore.Mvc;
using KosmosERP.BusinessLayer.Models.Module.DocumentUpload.Command.Create;
using KosmosERP.BusinessLayer.Models.Module.DocumentUpload.Command.Edit;
using KosmosERP.BusinessLayer.Models.Module.DocumentUpload.Command.Find;
using KosmosERP.BusinessLayer.Models.Module.DocumentUpload.Command.Delete;
using KosmosERP.BusinessLayer.Models.Module.DocumentUpload.Dto;
using KosmosERP.BusinessLayer.Models.Module.User.ListProfiles;
using KosmosERP.BusinessLayer.Modules;
using KosmosERP.Models;
using KosmosERP.Module;

namespace KosmosERP.Api.Controllers;


[ApiController]
[Route("api/v1/[controller]")]
public class DocumentController : ERPApiController
{
    private IDocumentUploadModule _Module;

    public DocumentController(IDocumentUploadModule module) : base(module)
    {
        _Module = module;
    }

    [ERPAuthorize(new[] { ERPPermission.Read }, "document_read")]
    [HttpGet("GetDocument", Name = "GetDocument")]
    [ProducesResponseType(typeof(Response<DocumentUploadDto>), 200)]
    public async Task<ActionResult> Get([FromQuery] int id)
    {
        var result = await _Module.GetDto(id);

        return Ok(result);
    }

    [ERPAuthorize(new[] { ERPPermission.Read }, "document_read")]
    [HttpGet("GetDocumentByGuid", Name = "GetDocumentByGuid")]
    [ProducesResponseType(typeof(Response<DocumentUploadDto>), 200)]
    public async Task<ActionResult> GetByGuid([FromQuery] string guid)
    {
        var result = await _Module.GetDtoByGuid(guid);

        return Ok(result);
    }

    [ERPAuthorize(new[] { ERPPermission.Read }, "document_read")]
    [HttpPost("FindDocument", Name = "FindDocument")]
    [ProducesResponseType(typeof(PagingResult<DocumentUploadListDto>), 200)]
    [ProducesResponseType(500)]
    public async Task<ActionResult> Find([FromQuery] GeneralListProfile listProfile, [FromBody] DocumentUploadFindCommand command)
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

    [ERPAuthorize(new[] { ERPPermission.Read }, "document_read")]
    [HttpPost("SearchDocuments", Name = "SearchDocuments")]
    [ProducesResponseType(typeof(PagingResult<DocumentUploadDto>), 200)]
    [ProducesResponseType(500)]
    public async Task<ActionResult> SearchDocuments([FromQuery] GeneralListProfile listProfile, [FromBody] DocumentUploadFindCommand command)
    {
        try
        {
            if (command != null)
            {
                command.calling_user_id = this.CurrentUserId;
                var sortingParams = new PagingSortingParameters(listProfile.Start, listProfile.ResultCount, listProfile.SortOrder);

                var result = await _Module.FindOverride(sortingParams, command);

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

    [ERPAuthorize(new[] { ERPPermission.Write }, "document_write")]
    [HttpPost("CreateDocument", Name = "CreateDocument")]
    [ProducesResponseType(typeof(Response<DocumentUploadDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> Create(IFormFile file, [FromForm] DocumentUploadCreateCommand createCommand)
    {
        createCommand.calling_user_id = this.CurrentUserId;
        var result = await _Module.CreateOverride(file, createCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [ERPAuthorize(new[] { ERPPermission.Write }, "document_write")]
    [HttpPost("CreateNewFileRevision", Name = "CreateNewFileRevision")]
    [ProducesResponseType(typeof(Response<DocumentUploadDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> CreateNewFileRevision(IFormFile file, [FromForm] DocumentUploadEditCommand editCommand)
    {
        editCommand.calling_user_id = this.CurrentUserId;
        var result = await _Module.CreateNewFileRevision(file, editCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [ERPAuthorize(new[] { ERPPermission.Read }, "document_read")]
    [HttpGet("GetObjectCategories", Name = "GetObjectCategories")]
    [ProducesResponseType(typeof(Response<List<DocumentUploadCategoryDto>>), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult> GetObjectCategories()
    {
        var result = await _Module.GetObjectCategories();

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [ERPAuthorize(new[] { ERPPermission.Read }, "document_read")]
    [HttpGet("GetUploadObjects", Name = "GetUploadObjects")]
    [ProducesResponseType(typeof(Response<List<DocumentUploadObjectDto>>), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult> GetUploadObjects()
    {
        var result = await _Module.GetUploadObjects();

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [ERPAuthorize(new[] { ERPPermission.Read }, "document_read")]
    [HttpGet("GetDocumentObjectTags", Name = "GetDocumentObjectTags")]
    [ProducesResponseType(typeof(Response<List<DocumentUploadObjectTagDto>>), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult> GetDocumentObjectTags(int document_object_id)
    {
        var result = await _Module.GetDocumentObjectTags(document_object_id);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [ERPAuthorize(new[] { ERPPermission.Write }, "document_write")]
    [HttpPost("CreateCategory", Name = "CreateCategory")]
    [ProducesResponseType(typeof(Response<DocumentUploadCategoryDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> CreateObjectCategory([FromBody] DocumentUploadCategoryCreateCommand createCommand)
    {
        createCommand.calling_user_id = this.CurrentUserId;
        var result = await _Module.CreateCategory(createCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [ERPAuthorize(new[] { ERPPermission.Write }, "document_write")]
    [HttpPost("CreateUploadObject", Name = "CreateUploadObject")]
    [ProducesResponseType(typeof(Response<DocumentUploadCategoryDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> CreateUploadObject([FromBody] DocumentUploadObjectCreateCommand createCommand)
    {
        createCommand.calling_user_id = this.CurrentUserId;
        var result = await _Module.CreateUploadObject(createCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [ERPAuthorize(new[] { ERPPermission.Write }, "document_write")]
    [HttpPost("CreateDocumentObjectTags", Name = "CreateDocumentObjectTags")]
    [ProducesResponseType(typeof(Response<DocumentUploadObjectTagDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> CreateDocumentObjectTags([FromBody] DocumentUploadObjectTagCreateCommand createCommand)
    {
        createCommand.calling_user_id = this.CurrentUserId;
        var result = await _Module.CreateDocumentObjectTags(createCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [ERPAuthorize(new[] { ERPPermission.Edit }, "document_edit")]
    [HttpPut("EditDocumentObjectTags", Name = "EditDocumentObjectTags")]
    [ProducesResponseType(typeof(Response<DocumentUploadObjectTagDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> EditDocumentObjectTags([FromBody] DocumentUploadObjectTagEditCommand createCommand)
    {
        createCommand.calling_user_id = this.CurrentUserId;
        var result = await _Module.EditDocumentObjectTags(createCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [ERPAuthorize(new[] { ERPPermission.Edit }, "document_edit")]
    [HttpPut("EditCategory", Name = "EditCategory")]
    [ProducesResponseType(typeof(Response<DocumentUploadCategoryDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> EditObjectCategory([FromBody] DocumentUploadCategoryEditCommand editCommand)
    {
        editCommand.calling_user_id = this.CurrentUserId;
        var result = await _Module.EditCategory(editCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [ERPAuthorize(new[] { ERPPermission.Edit }, "document_edit")]
    [HttpPut("EditUploadObject", Name = "EditUploadObject")]
    [ProducesResponseType(typeof(Response<DocumentUploadObjectDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> EditUploadObject([FromBody] DocumentUploadObjectEditCommand editCommand)
    {
        editCommand.calling_user_id = this.CurrentUserId;
        var result = await _Module.EditUploadObject(editCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [ERPAuthorize(new[] { ERPPermission.Delete }, "document_delete")]
    [HttpDelete("DeleteCategory", Name = "DeleteCategory")]
    [ProducesResponseType(typeof(Response<DocumentUploadCategoryDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> DeleteObjectCategory([FromBody] DocumentUploadCategoryDeleteCommand deleteCommand)
    {
        deleteCommand.calling_user_id = this.CurrentUserId;
        var result = await _Module.DeleteCategory(deleteCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [ERPAuthorize(new[] { ERPPermission.Delete }, "document_delete")]
    [HttpDelete("DeleteUploadObject", Name = "DeleteUploadObject")]
    [ProducesResponseType(typeof(Response<DocumentUploadObjectDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> DeleteUploadObject([FromBody] DocumentUploadObjectDeleteCommand deleteCommand)
    {
        deleteCommand.calling_user_id = this.CurrentUserId;
        var result = await _Module.DeleteUploadObject(deleteCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [ERPAuthorize(new[] { ERPPermission.Delete }, "document_delete")]
    [HttpDelete("DeleteDocumentObjectTags", Name = "DeleteDocumentObjectTags")]
    [ProducesResponseType(typeof(Response<DocumentUploadObjectDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> DeleteDocumentObjectTags([FromBody] DocumentUploadObjectTagDeleteCommand deleteCommand)
    {
        deleteCommand.calling_user_id = this.CurrentUserId;
        var result = await _Module.DeleteDocumentObjectTags(deleteCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [ERPAuthorize(new[] { ERPPermission.Read }, "document_read")]
    [HttpGet("GetFile", Name = "GetFile")]
    [ProducesResponseType(typeof(FileStreamResult), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult> GetFile([FromQuery] int document_revision_id)
    {
        try
        {
            var method_data = await _Module.GetFile(document_revision_id);
            
            if (method_data.Item1 == null || method_data.Item1.Length == 0)
                return NotFound("File not found or empty");

            if (method_data.Item2 == null)
                return NotFound("Document revision not found");

            var fileName = method_data.Item2?.document_name ?? "document";
            var contentType = GetContentType(fileName);

            var stream = new MemoryStream(method_data.Item1);
            return File(stream, contentType, fileName);
        }
        catch (Exception e)
        {
            return StatusCode(500, e.Message);
        }
    }

    [ERPAuthorize(new[] { ERPPermission.Read }, "document_read")]
    [HttpGet("GetFileByGuid", Name = "GetFileByGuid")]
    [ProducesResponseType(typeof(FileStreamResult), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult> GetFileByGuid([FromQuery] string document_revision_guid)
    {
        try
        {
            var method_data = await _Module.GetFileByGuid(document_revision_guid);
            
            if (method_data.Item1 == null || method_data.Item1.Length == 0)
                return NotFound("File not found or empty");

            if (method_data.Item2 == null)
                return NotFound("Document revision not found");

            var fileName = method_data.Item2?.document_name ?? "document";
            var contentType = GetContentType(fileName);

            var stream = new MemoryStream(method_data.Item1);
            return File(stream, contentType, fileName);
        }
        catch (Exception e)
        {
            return StatusCode(500, e.Message);
        }
    }

    private string GetContentType(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension switch
        {
            ".pdf" => "application/pdf",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".xls" => "application/vnd.ms-excel",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ".txt" => "text/plain",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".bmp" => "image/bmp",
            ".tiff" => "image/tiff",
            ".zip" => "application/zip",
            ".rar" => "application/x-rar-compressed",
            _ => "application/octet-stream"
        };
    }
}
