using Microsoft.AspNetCore.Mvc;
using Prometheus.BusinessLayer.Models.Module.Token.ListProfiles;
using Prometheus.BusinessLayer.Modules;
using Prometheus.Models;

namespace Prometheus.Api.Controllers;

public class TokenController : Controller
{
    private ITokenModule _Module { get; set; }

    public TokenController(ITokenModule module)
    {
        _Module = module;
    }

    [HttpPost("Request_Token", Name = "Request_Token")]
    [ProducesResponseType(typeof(Response<JwtToken>), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public ActionResult Create([FromBody] TokenAuthenticationListProfile listProfile)
    {
        var result = _Module.Request(listProfile.username, listProfile.password);

        if (!result.Success)
            return BadRequest(result);

        return Created("", result.Data);
    }
}
