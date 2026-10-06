using KosmosERP.BusinessLayer.AuthenticationProviders.Models;
using KosmosERP.BusinessLayer.Interfaces;
using KosmosERP.BusinessLayer.Models.Module.User.Command.Create;
using KosmosERP.BusinessLayer.Models.Module.User.Dto;
using KosmosERP.BusinessLayer.Modules;
using KosmosERP.Database;
using KosmosERP.Database.Models;
using KosmosERP.Models;
using KosmosERP.Models.Interfaces;
using KosmosERP.BusinessLayer.Helpers;
using Microsoft.EntityFrameworkCore;

namespace KosmosERP.BusinessLayer.AuthenticationProviders;

public class DatabaseAuthenticationProvider : IAuthenticationProvider
{
    private IAuthenticationSettings _Settings;
    private IBaseERPContext _Context;
 
    public DatabaseAuthenticationProvider(IAuthenticationSettings settings, IBaseERPContext context)
    {
        _Settings = settings;
        _Context = context;
    }

    public Task<Response<AuthenticatedUserDto>> Authenticate<T>(T responseObj)
    {
        throw new NotImplementedException();
    }

    public async Task<Response<AuthenticatedUserDto>> Authenticate(string username, string password)
    {
        // Usernames are case-insensitive: lowercase both sides (only the stored one was).
        var lowered = (username ?? "").ToLower();
        var result = await _Context.Users.SingleOrDefaultAsync(m => m.username.ToLower() == lowered);

        if (result != null && result.is_deleted)
            return new Response<AuthenticatedUserDto>("Could not find user", ResultCode.InvalidPermission);

        if (result != null && (result.login_attempt >= 5 || result.is_disabled))
            return new Response<AuthenticatedUserDto>("User is deactivated", ResultCode.InvalidPermission);


        if (result != null)
        {
            if (PasswordHasher.Verify(password, result.password, result.password_salt))
            {
                var sessionState = await this.FindCreateOrUpdateUserSession(result.id);

                var token = TokenModule.CreateSecurityToken(username, _Settings.APIPrivateKey);

                var dto = new AuthenticatedUserDto()
                {
                    id = result.id,
                    authenticated = true,
                    session = sessionState,
                    token = token
                };

                return new Response<AuthenticatedUserDto>(dto);
            }
            else
            {
                result.login_attempt += 1;

                if(result.login_attempt >= 5)
                    result.is_disabled = true;

                _Context.Users.Update(result);
                await _Context.SaveChangesAsync();
            }
        }

        return new Response<AuthenticatedUserDto>("Credentials could not be authenticated", ResultCode.Invalid);
    }

    public async Task<UserSessionState> FindCreateOrUpdateUserSession(string external_user_id)
    {
        throw new NotImplementedException();
    }

    public async Task<UserSessionState> FindCreateOrUpdateUserSession(int user_id)
    {
        var session = await _Context.UserSessionStates.FirstOrDefaultAsync(m => m.user_id == user_id);

        if (session != null)
        {
            session.session_expires = DateTime.UtcNow.AddHours(6);
            _Context.UserSessionStates.Update(session);
            await _Context.SaveChangesAsync();
        }
        else
        {
            session = new UserSessionState()
            {
                user_id = user_id,
                created_on = DateTime.UtcNow,
                session_id = Guid.NewGuid().ToString(),
                session_expires = DateTime.UtcNow.AddHours(6),
            };

            await _Context.UserSessionStates.AddAsync(session);
            await _Context.SaveChangesAsync();
        }

        return session;
    }

    public async Task<Response<bool>> Logout(string session_id)
    {
        throw new NotImplementedException();
    }

    public async Task<Response<AuthProviderUserDto>> CreateUser(UserCreateCommand commandModel, string auth_token)
    {
        return new Response<AuthProviderUserDto>();
    }


}