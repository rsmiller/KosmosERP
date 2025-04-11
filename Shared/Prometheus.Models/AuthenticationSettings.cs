using Prometheus.Models.Interfaces;

namespace Prometheus.Models;

public class AuthenticationSettings : IAuthenticationSettings
{
    public string APIPrivateKey { get; set; }
}
