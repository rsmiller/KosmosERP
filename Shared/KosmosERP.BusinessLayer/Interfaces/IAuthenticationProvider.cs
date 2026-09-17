using KosmosERP.BusinessLayer.AuthenticationProviders.Models;
using KosmosERP.BusinessLayer.Models.Module.User.Command.Create;
using KosmosERP.BusinessLayer.Models.Module.User.Dto;
using KosmosERP.Database.Models;
using KosmosERP.Models;

namespace KosmosERP.BusinessLayer.Interfaces;

public interface IAuthenticationProvider
{
    Task<Response<AuthenticatedUserDto>> Authenticate(string username, string password);
    Task<Response<AuthenticatedUserDto>> Authenticate<T>(T responseObj);
    Task<UserSessionState> FindCreateOrUpdateUserSession(int user_id);
    Task<UserSessionState> FindCreateOrUpdateUserSession(string external_user_id);
    Task<Response<AuthProviderUserDto>> CreateUser(UserCreateCommand commandModel, string auth_token);
}
