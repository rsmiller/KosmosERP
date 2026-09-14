namespace KosmosERP.BusinessLayer.Models.Module.User.ListProfiles;

public class AuthenticateCommand
{
    public required string username { get; set; }
    public required string password { get; set; }
}
