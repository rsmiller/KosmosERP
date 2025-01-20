using Prometheus.Models.Interfaces;

namespace Prometheus.Models;

public class AuthenticationSettings : IAuthenticationSettings
{
    public string APIPrivateKey { get; set; }
    public string APIUsername { get; set; }
    public string APIPassword { get; set; }
}
