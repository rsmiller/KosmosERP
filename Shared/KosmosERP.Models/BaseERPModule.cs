using KosmosERP.Models.Interfaces;

namespace KosmosERP.Module;

public interface IBaseERPModule
{
    Task<bool> LogError(int severity, string source, string method, Exception e);
    void SeedPermissions();
}

public class BaseERPModule : IBaseERPModule, IDisposable
{
    private ILogProvider _LogProvider;

    public BaseERPModule(ILogProviderFactory logProviderFactory)
    {
        _LogProvider = logProviderFactory.GetProvider();
    }

    public virtual Guid ModuleIdentifier { get; set; }
    public virtual string ModuleName { get; set; }

    public virtual void SeedPermissions() { }

    public async Task<bool> LogError(int severity, string source, string method, Exception e)
    {
        await _LogProvider.LogError(severity, source, method, e);

        return true;
    }

    public async Task<bool> LogTrace(string category, string message)
    {
        await _LogProvider.LogTrace($"[{category}] {message}");

        return true;
    }
    
    public void Dispose()
    {
        //this._ERPDbContext
    }
}
