using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using KosmosERP.Models;
using KosmosERP.BusinessLayer.Models.Module.GlobalSearch.Dto;

namespace KosmosERP.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/[controller]")]
public class GlobalSearchController : ControllerBase
{
    private IGlobalSearchModule _Module;

    public GlobalSearchController(IGlobalSearchModule module)
    {
        _Module = module;
    }

    
    [HttpPost("Search", Name = "Search")]
    [ProducesResponseType(typeof(Response<GlobalSearchResultDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> GlobalSeach([FromBody] GlobalSearchFindCommand commandModel)
    {
        var result = await _Module.GlobalSearch(commandModel);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
}

