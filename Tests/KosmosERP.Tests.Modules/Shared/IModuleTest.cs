namespace KosmosERP.Tests.Modules.Shared;

public interface IModuleTest
{
    Task SetupModule();
    Task Get();
    Task Create();
    Task Edit();
    Task Delete();
    Task Find();
}
