using KosmosERP.BusinessLayer.AuthenticiationProviders.Models;
using KosmosERP.BusinessLayer.Models.Module.User.Dto;
using KosmosERP.BusinessLayer.Modules;
using KosmosERP.Models;
using KosmosERP.Module;
using Microsoft.AspNetCore.Mvc;

namespace KosmosERP.Api.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class SAMLController : ERPApiController
    {
        private ISAMLModule _Module;

        public SAMLController(ISAMLModule module) : base(module)
        {
            _Module = module;
        }

        [HttpGet("metadata")]
        [Produces("application/samlmetadata+xml")]
        [ProducesResponseType(typeof(string), 200)]
        public IActionResult Metadata()
        {
            var metadata = _Module.GetMetadata();
            return Content(metadata, "application/samlmetadata+xml");
        }

        [HttpPost("init")]
        [ProducesResponseType(typeof(Response<SAMLRequest>), 200)]
        [ProducesResponseType(typeof(Response<SAMLRequest>), 400)]
        public async Task<IActionResult> BeginSAML([FromBody] SAMLRequest command)
        {
            var result = await _Module.BeginSAML(command);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPost("consume")]
        [ProducesResponseType(typeof(Response<AuthenticatedUserDto>), 200)]
        [ProducesResponseType(typeof(Response<AuthenticatedUserDto>), 401)]
        public async Task<IActionResult> LoginSAML([FromBody] SAMLWrapper samlResponse)
        {
            var result = await _Module.LoginSAML(samlResponse);

            if (!result.Success)
                return Unauthorized(result);

            return Ok(result);

        }

        [HttpPost("logout")]
        [ProducesResponseType(typeof(Response<SAMLRequest>), 200)]
        [ProducesResponseType(typeof(Response<SAMLRequest>), 400)]
        public async Task<IActionResult> BeginLogout([FromBody] SAMLLogoutCommand command)
        {
            var result = await _Module.BeginLogout(command?.SessionId, command?.RelayState);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPost("logout/response")]
        [ProducesResponseType(typeof(Response<bool>), 200)]
        [ProducesResponseType(typeof(Response<bool>), 400)]
        public async Task<IActionResult> CompleteLogout([FromBody] SAMLWrapper logoutResponse)
        {
            var result = await _Module.CompleteLogout(logoutResponse);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

    }
}
