using Prometheus.Models;

namespace Prometheus.Api.Models.Module.User.Command.Find;

public class UserFindCommand : DataCommand
{
    public string? wildcard { get; set; }
}
