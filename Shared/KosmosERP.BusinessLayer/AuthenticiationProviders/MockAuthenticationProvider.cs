using KosmosERP.BusinessLayer.AuthenticationProviders.Models;
using KosmosERP.BusinessLayer.Interfaces;
using KosmosERP.BusinessLayer.Models.Module.User.Command.Create;
using KosmosERP.BusinessLayer.Models.Module.User.Dto;
using KosmosERP.Database.Models;
using KosmosERP.Models;
using KosmosERP.Models.Interfaces;

namespace KosmosERP.BusinessLayer.AuthenticationProviders;

public class MockAuthenticationProvider : IAuthenticationProvider
{
    private IAuthenticationSettings _Settings;
 
    public MockAuthenticationProvider(IAuthenticationSettings settings)
    {
        _Settings = settings;
    }

    public Task<Response<AuthenticatedUserDto>> Authenticate(string username, string password)
    {
        throw new NotImplementedException();
    }

    public Task<Response<AuthenticatedUserDto>> Authenticate<T>(T responseObj)
    {
        throw new NotImplementedException();
    }

    public async Task<UserSessionState> FindCreateOrUpdateUserSession(string external_user_id)
    {
        throw new NotImplementedException();
    }

    public Task<UserSessionState> FindCreateOrUpdateUserSession(int user_id)
    {
        throw new NotImplementedException();
    }

    public Task<Response<AuthProviderUserDto>> CreateUser(UserCreateCommand commandModel, string auth_token)
    {
        throw new NotImplementedException();
    }
}