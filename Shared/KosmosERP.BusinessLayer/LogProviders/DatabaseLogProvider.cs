using KosmosERP.BusinessLayer.Interfaces;
using KosmosERP.Database;
using KosmosERP.Models.Interfaces;

namespace KosmosERP.BusinessLayer.LogProviders;

public class DatabaseLogProvider: ILogProvider
{
    private ILogProviderSettings _Settings;
    private IBaseERPContext _Context;

    public DatabaseLogProvider(ILogProviderSettings settings, IBaseERPContext context)
    {
        _Settings = settings;
        _Context = context;
    }

    public async Task<bool> LogError(int severity, string source, string method, Exception e)
    {
        try
        {
            await _Context.ErrorLogs.AddAsync(new Database.Models.ErrorLog()
            {
                source = source,
                method = method,
                error_severity = severity,
                error_message = e.Message,
                inner_message = e.InnerException != null ? e.InnerException.Message : "",
                created_on = DateTime.UtcNow,
            });

            await _Context.SaveChangesAsync();

            return true;
        }
        catch(Exception)
        {
            return false;
        }
    }

    public async Task<bool> LogTrace(string message)
    {
        try
        {
            var now = DateTime.UtcNow;

            await _Context.GeneralLogs.AddAsync(new Database.Models.GeneralLog()
            {
                category = "Trace",
                message = message,
                created_on = now,
            });

            await _Context.SaveChangesAsync();

            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}