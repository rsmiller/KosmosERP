using KosmosERP.BusinessLayer.AuthenticiationProviders.Models;
using KosmosERP.BusinessLayer.Interfaces;
using KosmosERP.BusinessLayer.Models.Module.User.Dto;
using KosmosERP.Database;
using KosmosERP.Models;
using KosmosERP.Models.Interfaces;
using KosmosERP.Module;
using Microsoft.Azure.Amqp.Sasl;

namespace KosmosERP.BusinessLayer.Modules
{
    public interface ISAMLModule : IBaseERPModule
    {
        Task<Response<SAMLRequest>> BeginSAML(SAMLRequest command);
        Task<Response<AuthenticatedUserDto>> LoginSAML(SAMLWrapper samlResponse);

    }

    public class SAMLModule : BaseERPModule, ISAMLModule
    {
        public override Guid ModuleIdentifier => Guid.Parse("33bc1eff-1dc8-4bef-9ef6-f00fdbf85d34");
        public override string ModuleName => "SAML";

        private readonly IBaseERPContext _Context;
        private IAuthenticationProvider _AuthenticationProvider;
        private readonly ILogProvider _LogProvider;

        public SAMLModule(IBaseERPContext context, IAuthenticationFactory authenticationFactory, ILogProviderFactory logProviderFactory) : base(logProviderFactory)
        {
            _Context = context;

            _AuthenticationProvider = authenticationFactory.GetProvider();
            _LogProvider = logProviderFactory.GetProvider();
        }

        public async Task<Response<SAMLRequest>> BeginSAML(SAMLRequest command)
        {
            // Todo: Add database logging here
            await _LogProvider.LogTrace($"Starting SAML authentication process: {command.Id} - {command.RedirectUrl}");
            
            return new Response<SAMLRequest>(command);
        }

        public async Task<Response<AuthenticatedUserDto>> LoginSAML(SAMLWrapper samlResponse)
        {
            return await _AuthenticationProvider.Authenticate(samlResponse); ;
        }
    }
}
