using KosmosERP.Database;
using KosmosERP.Models.Interfaces;
using KosmosERP.Module;

namespace KosmosERP.Reporting;

/// <summary>
/// ERP module for the Reporting feature. It exists to give <c>ReportsController</c> a module
/// identity so <c>[ERPAuthorize]</c> can resolve a module id and enforce RBAC exactly like every
/// other controller, and to seed the module's permission rows. Reports are read-only; the
/// module's Read permission gates report access.
/// </summary>
public interface IReportingModule : IBaseERPModule
{
}

public class ReportingModule : BaseERPModule, IReportingModule
{
    // Stable identity — do not change once deployed (permissions are keyed on it).
    public override Guid ModuleIdentifier => Guid.Parse("1cce546e-91b0-48d6-82a8-722848489e9a");
    public override string ModuleName => "Reports";

    public ReportingModule(IBaseERPContext context, ILogProviderFactory logProviderFactory)
        : base(context, logProviderFactory)
    {
    }

    public override void SeedPermissions()
    {
        // Idempotent (internal guards): seeds this module's read/write/edit/delete
        // ModulePermission rows and the default admin-role grant so RBAC can grant report access.
        CreateFirstRunRolePermissions();
    }
}
