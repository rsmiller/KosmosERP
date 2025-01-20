using Microsoft.AspNetCore.Mvc;
using Prometheus.Module;

namespace Prometheus.Module
{
    public class ERPApiController : ControllerBase 
    {
        private IBaseERPModule _Module;

        public ERPApiController(IBaseERPModule module)
        {
            _Module = module;
        }
    }
}
