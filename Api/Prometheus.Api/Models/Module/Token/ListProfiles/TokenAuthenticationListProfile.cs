using System.ComponentModel.DataAnnotations;

namespace Prometheus.Api.Models.Module.Token.ListProfiles;

public class TokenAuthenticationListProfile
{
    [Required]
    public required string username { get; set; }
    [Required]
    public required string password { get; set; }
}
