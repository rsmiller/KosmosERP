using KosmosERP.Database.Models;
using KosmosERP.Models.Interfaces;

namespace KosmosERP.BusinessLayer.Models.Module.User.Dto;

public class AuthenticatedUserDto
{
    public int id { get; set; }
    public bool authenticated { get; set; } = false;
    public UserDto user { get; set; }
    public required UserSessionState session { get; set; }
    public required IAuthenticationToken token { get; set; }
}
