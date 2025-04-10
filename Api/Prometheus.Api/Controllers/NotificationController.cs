using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Prometheus.BusinessLayer.Models.Module.Notification.Command.Find;
using Prometheus.BusinessLayer.Models.Module.Notification.Dto;
using Prometheus.BusinessLayer.Models.Module.User.ListProfiles;
using Prometheus.BusinessLayer.Modules;
using Prometheus.Models;

namespace Prometheus.Api.Controllers;

//[Authorize]
[ApiController]
[Route("api/v1/[controller]")]
public class NotificationController : Controller
{
    private INotificationModule _Module;

    public NotificationController(INotificationModule module)
    {
        _Module = module;
    }

    [HttpGet("GetNotification", Name = "GetNotification")]
    [ProducesResponseType(typeof(Response<NotificationDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> Get([FromQuery] int id)
    {
        var result = await _Module.GetDto(id);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPost("FindNotification", Name = "FindNotification")]
    [ProducesResponseType(typeof(PagingResult<NotificationDto>), 200)]
    [ProducesResponseType(500)]
    public async Task<ActionResult> Find([FromQuery] GeneralListProfile listProfile, [FromBody] NotificationFindCommand command)
    {
        try
        {
            if (command != null)
            {
                var sortingParams = new PagingSortingParameters(listProfile.Start, listProfile.ResultCount, listProfile.SortOrder);

                var result = await _Module.GetNotifications(sortingParams, command);

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
}
