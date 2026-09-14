namespace KosmosERP.Models.Interfaces;

public interface ILogProvider
{
    Task<bool> LogError(int severity, string source, string method, Exception e);
    Task<bool> LogTrace(string message);
}
