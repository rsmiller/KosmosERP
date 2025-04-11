using Microsoft.EntityFrameworkCore;
using Prometheus.Database;
using Prometheus.Database.Models;
using Prometheus.Models;

namespace Prometheus.Module;

public interface IBaseERPModule
{
    Task<bool> LogError(int severity, string source, string method, Exception e);
    Task<bool> LogGeneral(string category, string message);
    Task<UserPermissionsSet> GetUserPermissions(int calling_user_id);
    Task<bool> HasPermission(int calling_user_id, string token, string permission_name, bool read = false, bool write = false, bool edit = false, bool delete = false);
    void SeedPermissions();
}

public class BaseERPModule : IBaseERPModule
{
    private IBaseERPContext _ERPDbContext;

    public BaseERPModule(IBaseERPContext context)
    {
        _ERPDbContext = context;
    }

    public virtual Guid ModuleIdentifier { get; set; }
    public virtual string ModuleName { get; set; }

    public virtual void SeedPermissions() { }


    public virtual Task<Response<UserPermissionsSet>> GetPermissionSet(int? user_id)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> LogError(int severity, string source, string method, Exception e)
    {
        try
        {
            await _ERPDbContext.ErrorLogs.AddAsync(new Database.Models.ErrorLog()
            {
                source = source,
                method = method,
                error_severity = severity,
                error_message = e.Message,
                inner_message = e.InnerException != null ? e.InnerException.Message : "",
                created_on = DateTime.Now,
            });

            await _ERPDbContext.SaveChangesAsync();

            return true;
        }
        catch(Exception)
        {
            return false;
        }
    }

    public async Task<bool> LogGeneral(string category, string message)
    {
        try
        {
            var now = DateTime.UtcNow;

            await _ERPDbContext.GeneralLogs.AddAsync(new Database.Models.GeneralLog()
            {
                category = category,
                message = message,
                created_on = now,
            });

            await _ERPDbContext.SaveChangesAsync();

            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<UserPermissionsSet> GetUserPermissions(int calling_user_id)
    {
        UserPermissionsSet response = new UserPermissionsSet();

        try
        {
            var assigned_module_permissions = await (from ur in _ERPDbContext.UserRoles
                                                        join rp in _ERPDbContext.RolePermissions on ur.role_id equals rp.role_id
                                                        join mp in _ERPDbContext.ModulePermissions on rp.module_permission_id equals mp.id
                                                        where ur.user_id == calling_user_id
                                                        select mp).ToListAsync();

            var results = await _ERPDbContext.UserRoles.Where(m => m.user_id == calling_user_id).ToListAsync();

            response = new UserPermissionsSet()
            {
                user_id = calling_user_id,
                permissions = assigned_module_permissions,
            };
        }
        catch (Exception ex)
        {
            await LogError(90, this.GetType().Name, "BaseERPModule:GetUserPermissions", ex);
        }

        return response;
    }

    public async Task<bool> HasPermission(int calling_user_id, string token, string permission_name, bool read = false, bool write = false, bool edit = false, bool delete = false)
    {
        bool hasPermission = false;

        try
        {
            if (this.ModuleIdentifier == Guid.Empty)
                throw new Exception("A Modules ModuleIdentifier must have a value. Do not change a value once it has been set.");

            // Checking general login and if an admin
            var user = await (from u in _ERPDbContext.Users 
                             join us in _ERPDbContext.UserSessionStates on u.id equals us.user_id
                             where us.session_id == token && us.user_id == calling_user_id
                             select u).SingleOrDefaultAsync();

            if (user == null)
                return false;

            if (user.is_admin == true)
                return true;

            var assigned_module_permissions = await (from ur in _ERPDbContext.UserRoles
                                                     join rp in _ERPDbContext.RolePermissions on ur.role_id equals rp.role_id
                                                     join mp in _ERPDbContext.ModulePermissions on rp.module_permission_id equals mp.id
                                                     where ur.user_id == calling_user_id
                                                     && mp.internal_permission_name == permission_name
                                                     select mp).ToListAsync();

            if (read)
                hasPermission = assigned_module_permissions.Where(m => m.read == true).Any();
            if (write)
                hasPermission = assigned_module_permissions.Where(m => m.write == true).Any();
            if (edit)
                hasPermission = assigned_module_permissions.Where(m => m.edit == true).Any();
            if (delete)
                hasPermission = assigned_module_permissions.Where(m => m.delete == true).Any();
        }
        catch (Exception ex)
        {
            await LogError(90, this.GetType().Name, "BaseERPModule:HasPermission", ex);
        }

        return hasPermission;
    }
}
