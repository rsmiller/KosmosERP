using Microsoft.AspNetCore.Mvc;
using KosmosERP.BusinessLayer.Modules;

namespace KosmosERP.Api.Controllers;

public class TokenController : Controller
{
    private ITokenModule _Module { get; set; }

    public TokenController(ITokenModule module)
    {
        _Module = module;
    }
}
