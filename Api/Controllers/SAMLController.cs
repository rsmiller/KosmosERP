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

        [HttpPost("init")]
        [ProducesResponseType(typeof(Response<SAMLRequest>), 200)]
        public async Task<IActionResult> BeginSAML([FromBody] SAMLRequest command)
        {
            var result = await _Module.BeginSAML(command);
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

    }
}
