using KosmosERP.Models.Interfaces;

namespace KosmosERP.BusinessLayer.LogProviders;

public class ConsoleLogProvider: ILogProvider
{
    public ConsoleLogProvider()
    {

    }

    public async Task<bool> LogError(int severity, string source, string method, Exception e)
    {
        Console.WriteLine($"Error: {e.Message}");
        if (e.InnerException != null)
        {
            Console.WriteLine($"Inner Exception: {e.InnerException.Message}");
        }
        return await Task.FromResult(true);
    }

    public async Task<bool> LogTrace(string message)
    {
        Console.WriteLine($"Trace: {message}");
        return await Task.FromResult(true);
    }
}