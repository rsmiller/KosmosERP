using Prometheus.Models;

namespace Prometheus.Api.Models.Module.Contact.Command.Find;

public class ContactFindCommand : DataCommand
{
    public string? wildcard { get; set; }
}
