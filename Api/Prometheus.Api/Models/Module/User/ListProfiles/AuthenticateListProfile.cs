namespace Prometheus.Api.Models.Module.User.ListProfiles;

public class AuthenticateListProfile
{
    public required string username { get; set; }
    public required string password { get; set; }
}
