using KosmosERP.Database;
using KosmosERP.Database.Models;
using KosmosERP.Models.Interfaces;

namespace KosmosERP.Module;

public interface IBaseERPModule
{
    Guid ModuleIdentifier { get; set; }
    string ModuleName { get; set; }

    Task<bool> LogError(int severity, string source, string method, Exception e);
    void SeedPermissions();
    void StartUp();
}

public class BaseERPModule : IBaseERPModule, IDisposable
{
    private ILogProvider _LogProvider;
    private readonly IBaseERPContext _Context;

    public BaseERPModule(IBaseERPContext context, ILogProviderFactory logProviderFactory)
    {
        _LogProvider = logProviderFactory.GetProvider();
        _Context = context;
    }

    public virtual Guid ModuleIdentifier { get; set; }
    public virtual string ModuleName { get; set; }

    public virtual void StartUp()
    {
        if (string.IsNullOrEmpty(ModuleName) || _Context == null)
            return;

        var modules = _Context.Modules.Any(m => m.module_id == this.ModuleIdentifier.ToString());

        if (modules == false)
        {
            _Context.Modules.Add(new Database.Models.Module()
            {
                module_id = this.ModuleIdentifier.ToString(),
                module_name = this.ModuleName,
                created_on = DateTime.UtcNow,
                updated_on = DateTime.UtcNow,
                created_on_string = DateTime.UtcNow.ToString("u"),
                updated_on_string = DateTime.UtcNow.ToString("u"),
                created_on_timezone = GetTimezoneAsString(DateTime.UtcNow),
                updated_on_timezone = GetTimezoneAsString(DateTime.UtcNow),
                created_by = "1",
                updated_by = "1",
            });
            _Context.SaveChanges();
        }
    }

    public virtual void SeedPermissions() 
    {
    }

    protected void CreateFirstRunRolePermissions()
    {
        if (string.IsNullOrEmpty(ModuleName) || _Context == null)
            return;

        string clean_name = ModuleName.Replace(" ", "_");

        var rolePermissions = _Context.RolePermissions.Where(rp => rp.module_id == this.ModuleIdentifier.ToString()).Any();

        if(!rolePermissions)
        {
            _Context.RolePermissions.Add(new RolePermission
            {
                role_id = 1,
                module_id = this.ModuleIdentifier.ToString(),
                read = true,
                write = true,
                edit = true,
                delete = true,
                created_on = DateTime.UtcNow,
                updated_on = DateTime.UtcNow,
                created_on_string = DateTime.UtcNow.ToString("u"),
                updated_on_string = DateTime.UtcNow.ToString("u"),
                created_on_timezone = GetTimezoneAsString(DateTime.UtcNow),
                updated_on_timezone = GetTimezoneAsString(DateTime.UtcNow),
                created_by = "1",
                updated_by = "1",
            });
        }

        // Seed ModulePermissions for module
        var existing_permissions = _Context.ModulePermissions.Any(m => m.module_id == this.ModuleIdentifier.ToString());
        if (!existing_permissions)
        {
            var permissions = new List<ModulePermission>
            {
                new ModulePermission()
                {
                    module_id = this.ModuleIdentifier.ToString(),
                    module_name = this.ModuleName,
                    permission_name = $"Read {ModuleName}",
                    internal_permission_name = $"read_{clean_name.ToLower()}",
                    read = true,
                    write = false,
                    edit = false,
                    delete = false,
                    is_active = true,
                    created_on = DateTime.UtcNow,
                    updated_on = DateTime.UtcNow,
                    created_on_string = DateTime.UtcNow.ToString("u"),
                    updated_on_string = DateTime.UtcNow.ToString("u"),
                    created_on_timezone = GetTimezoneAsString(DateTime.UtcNow),
                    updated_on_timezone = GetTimezoneAsString(DateTime.UtcNow),
                    created_by = "1",
                    updated_by = "1",
                },
                new ModulePermission()
                {
                    module_id = this.ModuleIdentifier.ToString(),
                    module_name = this.ModuleName,
                    permission_name = $"Create {ModuleName}",
                    internal_permission_name = $"create_{clean_name.ToLower()}",
                    read = false,
                    write = true,
                    edit = false,
                    delete = false,
                    is_active = true,
                    created_on = DateTime.UtcNow,
                    updated_on = DateTime.UtcNow,
                    created_on_string = DateTime.UtcNow.ToString("u"),
                    updated_on_string = DateTime.UtcNow.ToString("u"),
                    created_on_timezone = GetTimezoneAsString(DateTime.UtcNow),
                    updated_on_timezone = GetTimezoneAsString(DateTime.UtcNow),
                    created_by = "1",
                    updated_by = "1",
                },
                new ModulePermission()
                {
                    module_id = this.ModuleIdentifier.ToString(),
                    module_name = this.ModuleName,
                    permission_name = $"Edit {ModuleName}",
                    internal_permission_name = $"edit_{clean_name.ToLower()}",
                    read = false,
                    write = false,
                    edit = true,
                    delete = false,
                    is_active = true,
                    created_on = DateTime.UtcNow,
                    updated_on = DateTime.UtcNow,
                    created_on_string = DateTime.UtcNow.ToString("u"),
                    updated_on_string = DateTime.UtcNow.ToString("u"),
                    created_on_timezone = GetTimezoneAsString(DateTime.UtcNow),
                    updated_on_timezone = GetTimezoneAsString(DateTime.UtcNow),
                    created_by = "1",
                    updated_by = "1",
                },
                new ModulePermission()
                {
                    module_id = this.ModuleIdentifier.ToString(),
                    module_name = this.ModuleName,
                    permission_name = $"Delete {ModuleName}",
                    internal_permission_name = $"delete_{clean_name.ToLower()}",
                    read = false,
                    write = false,
                    edit = false,
                    delete = true,
                    is_active = true,
                    created_on = DateTime.UtcNow,
                    updated_on = DateTime.UtcNow,
                    created_on_string = DateTime.UtcNow.ToString("u"),
                    updated_on_string = DateTime.UtcNow.ToString("u"),
                    created_on_timezone = GetTimezoneAsString(DateTime.UtcNow),
                    updated_on_timezone = GetTimezoneAsString(DateTime.UtcNow),
                    created_by = "1",
                    updated_by = "1",
                }
            };

            _Context.ModulePermissions.AddRange(permissions);
            _Context.SaveChanges();
        }
    }

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

    private string GetTimezoneAsString(DateTime the_date)
    {
        var offset = TimeZoneInfo.Local.GetUtcOffset(the_date);
        string formattedOffset = offset.ToString(@"hh\:mm");

        if (offset < TimeSpan.Zero)
            formattedOffset = "-" + formattedOffset;
        else
            formattedOffset = "+" + formattedOffset;

        return formattedOffset;
    }
}
