using Prometheus.Models;

namespace Prometheus.BusinessLayer.Models.Module.Contact.Command.Find;

public class ContactFindCommand : DataCommand
{
    public string? wildcard { get; set; }
}
