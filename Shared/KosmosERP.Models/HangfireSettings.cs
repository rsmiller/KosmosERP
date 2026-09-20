
using KosmosERP.Models.Interfaces;

namespace KosmosERP.Models;

public class HangfireSettings : IHangfireSettings
{
    public string HangfireConnectionString { get; set; }
}
