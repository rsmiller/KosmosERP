using Microsoft.AspNetCore.Mvc;
using Prometheus.BusinessLayer.Modules;

namespace Prometheus.Api.Controllers;

public class TokenController : Controller
{
    private ITokenModule _Module { get; set; }

    public TokenController(ITokenModule module)
    {
        _Module = module;
    }
}
