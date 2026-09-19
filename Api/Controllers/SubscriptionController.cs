using KosmosERP.Api.Authorization;
using KosmosERP.Api.Models;
using Microsoft.AspNetCore.Mvc;
using KosmosERP.BusinessLayer.Models.Module.Subscription.Dto;
using KosmosERP.BusinessLayer.Models.Module.Subscription.Command.Create;
using KosmosERP.BusinessLayer.Models.Module.Subscription.Command.Delete;
using KosmosERP.BusinessLayer.Models.Module.Subscription.Command.Edit;
using KosmosERP.BusinessLayer.Models.Module.Subscription.Command.Find;
using KosmosERP.BusinessLayer.Modules;
using KosmosERP.Models;
using KosmosERP.Module;
using KosmosERP.BusinessLayer.Models.Module.User.ListProfiles;

namespace KosmosERP.Api.Controllers;


[ApiController]
[Route("api/v1/[controller]")]
public class SubscriptionController : ERPApiController
{
    private ISubscriptionModule _Module;

    public SubscriptionController(ISubscriptionModule module) : base(module)
    {
        _Module = module;
    }

    
    [ERPAuthorize(new[] { ERPPermission.Read }, "subscription_read")]
    [HttpGet("GetSubscription", Name = "GetSubscription")]
    [ProducesResponseType(typeof(Response<SubscriptionDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> Get([FromQuery] int id)
    {
        var result = await _Module.GetDto(id);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [ERPAuthorize(new[] { ERPPermission.Read }, "subscription_read")]
    [HttpGet("GetSubscriptionByGuid", Name = "GetSubscriptionByGuid")]
    [ProducesResponseType(typeof(Response<SubscriptionDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> GetByGuid([FromQuery] string guid)
    {
        var result = await _Module.GetDtoByGuid(guid);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [ERPAuthorize(new[] { ERPPermission.Read }, "subscription_read")]
    [HttpPost("FindSubscription", Name = "FindSubscription")]
    [ProducesResponseType(typeof(PagingResult<SubscriptionListDto>), 200)]
    [ProducesResponseType(500)]
    public async Task<ActionResult> Find([FromQuery] GeneralListProfile listProfile, [FromBody] SubscriptionFindCommand command)
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

    [ERPAuthorize(new[] { ERPPermission.Write }, "subscription_write")]
    [HttpPost("CreateSubscription", Name = "CreateSubscription")]
    [ProducesResponseType(typeof(Response<SubscriptionDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> Create([FromBody] SubscriptionCreateCommand createCommand)
    {
        var result = await _Module.Create(createCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [ERPAuthorize(new[] { ERPPermission.Edit }, "subscription_edit")]
    [HttpPut("UpdateSubscription", Name = "UpdateSubscription")]
    [ProducesResponseType(typeof(Response<SubscriptionDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> Edit([FromBody] SubscriptionEditCommand editCommand)
    {
        var result = await _Module.Edit(editCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [ERPAuthorize(new[] { ERPPermission.Delete }, "subscription_delete")]
    [HttpPost("DeleteSubscription", Name = "DeleteSubscription")]
    [ProducesResponseType(typeof(Response<SubscriptionDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> Delete([FromBody] SubscriptionDeleteCommand deleteCommand)
    {
        var result = await _Module.Delete(deleteCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
}
