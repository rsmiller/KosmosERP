using Prometheus.Models;

namespace Prometheus.BusinessLayer.Models.Module.User.Command.Find;

public class UserFindCommand : DataCommand
{
    public string? wildcard { get; set; }
}
