using KosmosERP.Api.Authorization;
using KosmosERP.Api.Models;
using Microsoft.AspNetCore.Mvc;
using KosmosERP.BusinessLayer.Models.Module.Product.Dto;
using KosmosERP.BusinessLayer.Models.Module.Product.Command.Create;
using KosmosERP.BusinessLayer.Models.Module.Product.Command.Delete;
using KosmosERP.BusinessLayer.Models.Module.Product.Command.Edit;
using KosmosERP.BusinessLayer.Models.Module.Product.Command.Find;
using KosmosERP.BusinessLayer.Modules;
using KosmosERP.Models;
using KosmosERP.Module;
using KosmosERP.BusinessLayer.Models.Module.User.ListProfiles;

namespace KosmosERP.Api.Controllers;

//[Authorize]
[ApiController]
[Route("api/v1/[controller]")]
public class ProductController : ERPApiController
{
    private IProductModule _Module;

    public ProductController(IProductModule module) : base(module)
    {
        _Module = module;
    }

    
    [ERPAuthorize(new[] { ERPPermission.Read }, "product_read")]
    [HttpGet("GetProduct", Name = "GetProduct")]
    [ProducesResponseType(typeof(Response<ProductDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> Get([FromQuery] int id)
    {
        var result = await _Module.GetDto(id);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [ERPAuthorize(new[] { ERPPermission.Read }, "product_read")]
    [HttpGet("GetProductByGuid", Name = "GetProductByGuid")]
    [ProducesResponseType(typeof(Response<ProductDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> GetByGuid([FromQuery] string guid)
    {
        var result = await _Module.GetDtoByGuid(guid);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [ERPAuthorize(new[] { ERPPermission.Read }, "product_read")]
    [HttpGet("GetProductCategories", Name = "GetProductCategories")]
    [ProducesResponseType(typeof(List<ActionResult>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> GetProductCategories()
    {
        var results = await _Module.GetProductCategories();

        return Ok(results);
    }

    [ERPAuthorize(new[] { ERPPermission.Read }, "product_read")]
    [HttpPost("FindProduct", Name = "FindProduct")]
    [ProducesResponseType(typeof(PagingResult<ProductListDto>), 200)]
    [ProducesResponseType(500)]
    public async Task<ActionResult> Find([FromQuery] GeneralListProfile listProfile, [FromBody] ProductFindCommand command)
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

    [ERPAuthorize(new[] { ERPPermission.Write }, "product_write")]
    [HttpPost("CreateProduct", Name = "CreateProduct")]
    [ProducesResponseType(typeof(Response<ProductDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> Create([FromBody] ProductCreateCommand createCommand)
    {
        createCommand.calling_user_id = this.CurrentUserId;

        var result = await _Module.Create(createCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [ERPAuthorize(new[] { ERPPermission.Edit }, "product_edit")]
    [HttpPut("UpdateProduct", Name = "UpdateProduct")]
    [ProducesResponseType(typeof(Response<ProductDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> Edit([FromBody] ProductEditCommand editCommand)
    {
        editCommand.calling_user_id = this.CurrentUserId;

        var result = await _Module.Edit(editCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [ERPAuthorize(new[] { ERPPermission.Delete }, "product_delete")]
    [HttpPost("DeleteProduct", Name = "DeleteProduct")]
    [ProducesResponseType(typeof(Response<ProductDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> Delete([FromBody] ProductDeleteCommand deleteCommand)
    {
        deleteCommand.calling_user_id = this.CurrentUserId;

        var result = await _Module.Delete(deleteCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
}
