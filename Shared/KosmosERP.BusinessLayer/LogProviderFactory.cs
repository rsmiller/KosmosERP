using KosmosERP.BusinessLayer.LogProviders;
using KosmosERP.Database;
using KosmosERP.Models;
using KosmosERP.Models.Interfaces;
using Microsoft.Extensions.Logging;

namespace KosmosERP.BusinessLayer;

public class LogProviderFactory : ILogProviderFactory
{
    private IBaseERPContext _Context;
    private ILogProviderSettings _Settings;
    private ILogProvider _LogProvider;

    public LogProviderFactory(ILogProviderSettings settings, IBaseERPContext context, ILogger<LogProviderFactory> logger)
    {
        _Context = context;
        _Settings = settings;

        ValidateSettings(settings);

        if (settings.log_provider.Equals(LogProviderType.Azure, StringComparison.OrdinalIgnoreCase))
        {
            _LogProvider = new ApplicationInsightsLogProvider(settings, logger);
        }
        else if (settings.log_provider.Equals(LogProviderType.DataDog, StringComparison.OrdinalIgnoreCase))
        {
            _LogProvider = new DataDogLogProvider(settings, logger);
        }
        else if (settings.log_provider.Equals(LogProviderType.Database, StringComparison.OrdinalIgnoreCase))
        {
            _LogProvider = new DatabaseLogProvider(settings, context);
        }
        else if (settings.log_provider.Equals(LogProviderType.MOCK, StringComparison.OrdinalIgnoreCase))
        {
            _LogProvider = new ConsoleLogProvider();
        }
        else
        {
            throw new ArgumentNullException("Log provider not supported.");
        }
    }

    public ILogProvider GetProvider()
    {
        return _LogProvider;
    }

    private void ValidateSettings(ILogProviderSettings settings)
    {
        if (string.IsNullOrEmpty(settings.log_provider))
            throw new ArgumentNullException("Log account provider cannot be null or empty.");
    }
}
