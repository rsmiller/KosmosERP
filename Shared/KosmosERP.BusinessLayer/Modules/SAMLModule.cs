using KosmosERP.BusinessLayer.AuthenticiationProviders.Models;
using KosmosERP.BusinessLayer.Interfaces;
using KosmosERP.BusinessLayer.Models.Module.User.Dto;
using KosmosERP.Database;
using KosmosERP.Models;
using KosmosERP.Models.Interfaces;
using KosmosERP.Module;
using Microsoft.EntityFrameworkCore;

namespace KosmosERP.BusinessLayer.Modules
{
    public interface ISAMLModule : IBaseERPModule
    {
        string GetMetadata();
        Task<Response<SAMLRequest>> BeginSAML(SAMLRequest command);
        Task<Response<AuthenticatedUserDto>> LoginSAML(SAMLWrapper samlResponse);
        Task<Response<bool>> Logout(string session_id);
        Task<Response<SAMLRequest>> BeginLogout(string session_id, string? relayState);
        Task<Response<bool>> CompleteLogout(SAMLWrapper logoutResponse);
    }

    public class SAMLModule : BaseERPModule, ISAMLModule
    {
        public override Guid ModuleIdentifier => Guid.Parse("33bc1eff-1dc8-4bef-9ef6-f00fdbf85d34");
        public override string ModuleName => "SAML";

        private readonly IBaseERPContext _Context;
        private IAuthenticationProvider _AuthenticationProvider;
        private readonly ILogProvider _LogProvider;
        private readonly IAuthenticationSettings _AuthenticationSettings;

        public SAMLModule(IBaseERPContext context, IAuthenticationFactory authenticationFactory, IAuthenticationSettings authenticationSettings, ILogProviderFactory logProviderFactory) : base(logProviderFactory)
        {
            _Context = context;

            _AuthenticationProvider = authenticationFactory.GetProvider();
            _AuthenticationSettings = authenticationSettings;
            _LogProvider = logProviderFactory.GetProvider();
        }

        public string GetMetadata()
        {
            return new SAMLMetadata(_AuthenticationSettings).Build();
        }

        public async Task<Response<SAMLRequest>> BeginSAML(SAMLRequest command)
        {
            if (string.IsNullOrEmpty(_AuthenticationSettings.Authority))
                return new Response<SAMLRequest>("SAML IdP Authority is not configured", ResultCode.Invalid);

            if (string.IsNullOrEmpty(_AuthenticationSettings.BaseURL))
                return new Response<SAMLRequest>("SAML BaseURL is not configured", ResultCode.Invalid);

            var authnRequest = new SAMLAuthnRequest(_AuthenticationSettings).Build(command?.RelayState);

            // Todo: Persist authnRequest.Id so the consume endpoint can validate InResponseTo.
            await _LogProvider.LogTrace($"Starting SAML authentication process: {authnRequest.Id} - {authnRequest.RedirectUrl}");

            return new Response<SAMLRequest>(authnRequest);
        }

        public async Task<Response<AuthenticatedUserDto>> LoginSAML(SAMLWrapper samlResponse)
        {
            return await _AuthenticationProvider.Authenticate(samlResponse);
        }

        public async Task<Response<bool>> Logout(string session_id)
        {
            return await _AuthenticationProvider.Logout(session_id);
        }

        public async Task<Response<SAMLRequest>> BeginLogout(string session_id, string? relayState)
        {
            if (string.IsNullOrEmpty(session_id))
                return new Response<SAMLRequest>("A session id is required to begin logout", ResultCode.NullItemInput);

            if (string.IsNullOrEmpty(SAMLServiceProvider.IdentityProviderLogoutUrl(_AuthenticationSettings)))
                return new Response<SAMLRequest>("SAML IdP logout URL is not configured", ResultCode.Invalid);

            if (string.IsNullOrEmpty(_AuthenticationSettings.BaseURL))
                return new Response<SAMLRequest>("SAML BaseURL is not configured", ResultCode.Invalid);

            // Resolve the IdP NameID (external id) for this session before it is torn down.
            var user = await (from u in _Context.Users
                              join s in _Context.UserSessionStates on u.id equals s.user_id
                              where s.session_id == session_id
                              select u).FirstOrDefaultAsync();

            if (user == null)
                return new Response<SAMLRequest>("Session not found", ResultCode.NotFound);

            if (string.IsNullOrEmpty(user.external_id))
                return new Response<SAMLRequest>("User does not have an IdP identifier", ResultCode.Invalid);

            // Terminate the local session first so the app session cannot outlive the redirect.
            await this.Logout(session_id);

            var logoutRequest = new SAMLLogoutRequest(_AuthenticationSettings).Build(user.external_id, relayState);

            await _LogProvider.LogTrace($"Starting SAML logout process: {logoutRequest.Id} - {logoutRequest.RedirectUrl}");

            return new Response<SAMLRequest>(logoutRequest);
        }

        public async Task<Response<bool>> CompleteLogout(SAMLWrapper logoutResponse)
        {
            if (logoutResponse == null || string.IsNullOrEmpty(logoutResponse.Response))
                return new Response<bool>("SAML logout response is null or empty", ResultCode.NullItemInput);

            var response = new SAMLLogoutResponse(_AuthenticationSettings);
            response.LoadXmlFromBase64(logoutResponse.Response);

            if (response.IsValid() == false)
                return new Response<bool>("SAML logout response is not valid", ResultCode.Invalid);

            if (response.IsSuccess() == false)
                return new Response<bool>("SAML logout was not successful", ResultCode.Invalid);

            await _LogProvider.LogTrace($"Completed SAML logout process: {response.GetInResponseTo()}");

            return new Response<bool>(true);
        }
    }
}
