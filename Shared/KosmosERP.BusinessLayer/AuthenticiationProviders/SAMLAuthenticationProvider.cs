using KosmosERP.BusinessLayer.AuthenticationProviders.Models;
using KosmosERP.BusinessLayer.AuthenticiationProviders.Models;
using KosmosERP.BusinessLayer.Interfaces;
using KosmosERP.BusinessLayer.Models.Module.User.Command.Create;
using KosmosERP.BusinessLayer.Models.Module.User.Dto;
using KosmosERP.BusinessLayer.Modules;
using KosmosERP.Database;
using KosmosERP.Database.Models;
using KosmosERP.Models;
using KosmosERP.Models.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace KosmosERP.BusinessLayer.AuthenticiationProviders
{
    public class SAMLAuthenticationProvider : IAuthenticationProvider
    {
        private IBaseERPContext _Context;
        private IAuthenticationSettings _AuthenticationSettings;

        public SAMLAuthenticationProvider(IAuthenticationSettings authenticationSettings, IBaseERPContext context) 
        {
            _AuthenticationSettings = authenticationSettings;
            _Context = context;
        }

        public Task<Response<AuthenticatedUserDto>> Authenticate(string username, string password)
        {
            throw new NotImplementedException();
        }

        public async Task<Response<AuthenticatedUserDto>> Authenticate<T>(T responseObj)
        {
            var wrapper = responseObj as SAMLWrapper;

            if (wrapper == null)
                return new Response<AuthenticatedUserDto>("SAML response is null or empty", ResultCode.NullItemInput);            

            var newResponse = new SAMLResponse(_AuthenticationSettings);

            if(string.IsNullOrEmpty(wrapper.Response))
                return new Response<AuthenticatedUserDto>("SAML response is null or empty", ResultCode.NullItemInput);

            newResponse.LoadXmlFromBase64(wrapper.Response);

            if(newResponse.IsValid() == false)
                return new Response<AuthenticatedUserDto>("SAML response is not valid", ResultCode.Invalid);

            var userId = newResponse.GetNameID();
            var startTime = newResponse.GetStartDateTime();
            var endTime = newResponse.GetEndDateTime();

            if (string.IsNullOrEmpty(userId))
                return new Response<AuthenticatedUserDto>("SAML response does not contain a valid NameID", ResultCode.NullItemInput);

            if (startTime > DateTime.Now || endTime < DateTime.Now)
                return new Response<AuthenticatedUserDto>("SAML response is not within the valid time range", ResultCode.Invalid);


            var user = await _Context.Users.FirstOrDefaultAsync(m => m.external_id == userId);

            if (user == null)
                return new Response<AuthenticatedUserDto>("User not found", ResultCode.NotFound);

            var sessionState = await this.FindCreateOrUpdateUserSession(userId);

            // Todo: Add auto-provisioning logic here if the user does not exist in the database

            var token = TokenModule.CreateSecurityToken(userId, _AuthenticationSettings.APIPrivateKey);

            var dto = new AuthenticatedUserDto()
            {
                id = user.id,
                authenticated = true,
                session = sessionState,
                token = token
            };

            return new Response<AuthenticatedUserDto>(dto);
        }

        public async Task<UserSessionState> FindCreateOrUpdateUserSession(int user_id)
        {
            throw new NotImplementedException();
        }

        public async Task<UserSessionState> FindCreateOrUpdateUserSession(string external_user_id)
        {
            var user = await _Context.Users.FirstOrDefaultAsync(m => m.external_id == external_user_id);

            if (user == null)
                return null;

            var session = await _Context.UserSessionStates.FirstOrDefaultAsync(m => m.user_id == user.id);

            if (session != null)
            {
                session.session_expires = DateTime.UtcNow.AddHours(1);
                _Context.UserSessionStates.Update(session);
                await _Context.SaveChangesAsync();
            }
            else
            {
                session = new UserSessionState()
                {
                    user_id = user.id,
                    created_on = DateTime.UtcNow,
                    session_id = Guid.NewGuid().ToString(),
                    session_expires = DateTime.UtcNow.AddHours(1),
                };

                await _Context.UserSessionStates.AddAsync(session);
                await _Context.SaveChangesAsync();
            }

            return session;
        }

        public async Task<Response<bool>> Logout(string session_id)
        {

            var session = await _Context.UserSessionStates.FirstOrDefaultAsync(m => m.session_id == session_id);

            if (session != null)
            {
                session.session_expires = DateTime.UtcNow;

                _Context.UserSessionStates.Update(session);
                await _Context.SaveChangesAsync();
            }

            return new Response<bool>(true);
        }

        public Task<Response<AuthProviderUserDto>> CreateUser(UserCreateCommand commandModel, string auth_token)
        {
            throw new NotImplementedException();
        }
    }
}
