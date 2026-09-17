using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using KosmosERP.BusinessLayer.AuthenticationProviders.Models;
using KosmosERP.BusinessLayer.Interfaces;
using KosmosERP.BusinessLayer.Models.Module.User.Command.Create;
using KosmosERP.BusinessLayer.Models.Module.User.Dto;
using KosmosERP.Database;
using KosmosERP.Database.Models;
using KosmosERP.Models;
using KosmosERP.Models.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace KosmosERP.BusinessLayer.AuthenticationProviders;

public class KeycloakAuthenticationProvider : IAuthenticationProvider
{
    private IAuthenticationSettings _Settings;
    private IBaseERPContext _Context;
    
    public KeycloakAuthenticationProvider(IAuthenticationSettings settings, IBaseERPContext context)
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
        var result = await _Context.Users.SingleOrDefaultAsync(m => m.username.ToLower() == username.ToLower());
        if (result == null || result.is_deleted)
            return new Response<AuthenticatedUserDto>("Could not find user", ResultCode.InvalidPermission);

        try
        {
            HttpClient httpClient = new HttpClient();

            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "password",
                ["client_id"] = _Settings.Audience,
                ["client_secret"] = _Settings.ClientSecret,
                ["username"] = username,
                ["password"] = password
            });

            var response = await httpClient.PostAsync(_Settings.TokenURL, content);

            if (!response.IsSuccessStatusCode)
                throw new UnauthorizedAccessException("Invalid username or password");

            var json = await response.Content.ReadAsStringAsync();

            var keycloak_token = Newtonsoft.Json.JsonConvert.DeserializeObject<KeycloakToken>(json);

            if(keycloak_token == null || string.IsNullOrEmpty(keycloak_token.access_token))
                throw new UnauthorizedAccessException("Invalid username or password");

            var sessionState = await this.FindCreateOrUpdateUserSession(result.id);

            var dto = new AuthenticatedUserDto()
            {
                id = result.id,
                authenticated = true,
                session = sessionState,
                token = keycloak_token
            };

            return new Response<AuthenticatedUserDto>(dto);
        }
        catch (UnauthorizedAccessException ex)
        {
            result.login_attempt += 1;

            if(result.login_attempt >= 5)
                result.is_disabled = true;

            _Context.Users.Update(result);
            await _Context.SaveChangesAsync();

            return new Response<AuthenticatedUserDto>($"Authentication failed: {ex.Message}", ResultCode.InvalidPermission);
        }
    }

    public async Task<UserSessionState?> FindCreateOrUpdateUserSession(string external_user_id)
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

    public async Task<Response<AuthProviderUserDto>> CreateUser(UserCreateCommand commandModel, string auth_token)
    {
        Response<AuthProviderUserDto> response = new Response<AuthProviderUserDto>();

        try
        {
            HttpClient httpClient = new HttpClient();

            var create_user_url = $"{_Settings.BaseURL}/admin/realms/{_Settings.Realm}/users";
        

            var payload = new
            {
                username = commandModel.username,
                enabled = true,
                email = commandModel.email,
                firstName = commandModel.first_name,
                lastName = commandModel.last_name,
                emailVerified = true
            };

            var user_request = new HttpRequestMessage(HttpMethod.Post, create_user_url)
            {
                Content = new StringContent(
                    JsonSerializer.Serialize(payload),
                    Encoding.UTF8,
                    "application/json")
            };

            user_request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", auth_token);

            
            var http_response = await httpClient.SendAsync(user_request);

            http_response.EnsureSuccessStatusCode();

            var userId = http_response.Headers?.Location?.Segments.LastOrDefault();
            response.Data = new AuthProviderUserDto()
            {
                id = userId
            };

            var set_password_url = $"{_Settings.BaseURL}/admin/realms/{_Settings.Realm}/users/{userId}/reset-password";

            var password_payload = new
            {
                type = "password",
                value = "TempPassword123!",
                temporary = false
            };

            var password_request = new HttpRequestMessage(HttpMethod.Put, set_password_url)
            {
                Content = new StringContent(
                    JsonSerializer.Serialize(password_payload),
                    Encoding.UTF8,
                    "application/json")
            };

            password_request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", auth_token);

            var password_response = await httpClient.SendAsync(password_request);
            password_response.EnsureSuccessStatusCode();

        }
        catch (Exception ex)
        {
            response.SetException($"Error creating user in Keycloak: {ex.Message}", ResultCode.Error);
        }

        return response;
    }
}