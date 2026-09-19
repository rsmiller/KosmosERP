using Microsoft.AspNetCore.Mvc;
using KosmosERP.Models;
using KosmosERP.BusinessLayer.Modules;
using KosmosERP.BusinessLayer.Models.Module.Inventory.Dto;
using KosmosERP.Api.Authorization;
using KosmosERP.Api.Models;

namespace KosmosERP.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class InventoryController : ControllerBase
{
    private IInventoryModule _Module;

    public InventoryController(IInventoryModule module)
    {
        _Module = module;
    }

    [ERPAuthorize(new[] { ERPPermission.Read }, "inventory_read")]
    [HttpGet("GetCounts", Name = "GetCounts")]
    [ProducesResponseType(typeof(Response<List<InventoryDto>>), 200)]
    public async Task<ActionResult> GetCounts()
    {
        var result = await _Module.GetCounts();
        return Ok(result);
    }

    [ERPAuthorize(new[] { ERPPermission.Write }, "inventory_write")]
    [HttpPost("RebuildCounts", Name = "RebuildCounts")]
    [ProducesResponseType(typeof(Response<bool>), 200)]
    public async Task<ActionResult> RebuildCounts()
    {
        var result = await _Module.RebuildCounts();
        return Ok(result);
    }
} 