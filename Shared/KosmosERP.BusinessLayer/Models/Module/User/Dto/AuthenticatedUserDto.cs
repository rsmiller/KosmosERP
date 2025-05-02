using KosmosERP.Database.Models;
using KosmosERP.Models;

namespace KosmosERP.BusinessLayer.Models.Module.User.Dto;

public class AuthenticatedUserDto
{
    public int id { get; set; }
    public bool authenticated { get; set; } = false;
    public required UserDto user { get; set; }
    public required UserSessionState session { get; set; }
    public required JwtToken token { get; set; }
}
