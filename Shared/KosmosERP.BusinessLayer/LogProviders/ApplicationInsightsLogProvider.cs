using KosmosERP.BusinessLayer.Interfaces;
using KosmosERP.Models.Interfaces;
using Microsoft.Extensions.Logging;

namespace KosmosERP.BusinessLayer.LogProviders;

public class ApplicationInsightsLogProvider: ILogProvider
{
    private ILogProviderSettings _Settings;
    private ILogger<LogProviderFactory> _Logger;

    public ApplicationInsightsLogProvider(ILogProviderSettings settings, ILogger<LogProviderFactory> logger)
    {
        _Settings = settings;
        _Logger = logger;
    }

    public async Task<bool> LogError(int severity, string source, string method, Exception e)
    {
        _Logger.LogError(e.Message, new
        {
            Severity = severity,
            Source = source,
            Method = method,
            Exception = e
        });

        return true;
    }

    public async Task<bool> LogTrace(string message)
    {
        _Logger.LogInformation(message);

        return true;
    }
}