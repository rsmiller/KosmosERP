namespace Prometheus.Models.Interfaces;

public interface IAuthenticationSettings
{
    string APIPrivateKey { get; set; }
    string APIUsername { get; set; }
    string APIPassword { get; set; }
}
