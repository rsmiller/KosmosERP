using Microsoft.AspNetCore.Mvc;
using KosmosERP.Module;

namespace KosmosERP.Module
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
