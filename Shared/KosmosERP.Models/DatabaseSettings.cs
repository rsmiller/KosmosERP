using KosmosERP.Models.Interfaces;

namespace KosmosERP.Models;

public class DatabaseSettings : IDatabaseSettings
{
    public string DatabaseConnectionString { get; set; }
}
