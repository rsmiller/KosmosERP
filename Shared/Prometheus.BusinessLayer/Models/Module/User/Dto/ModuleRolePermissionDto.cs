
namespace Prometheus.BusinessLayer.Models.Module.User.Dto;

public class ModuleRolePermissionDto
{
    public required string module_id { get; set; }
    public required string module_name { get; set; }
    public required int module_permission_id { get; set; }
    public required string permission_name { get; set; }
    public bool read { get; set; } = false;
    public bool write { get; set; } = false;
    public bool edit { get; set; } = false;
    public bool delete { get; set; } = false;
    public bool requires_admin { get; set; } = false;
    public bool requires_management { get; set; } = false;
    public bool requires_guest { get; set; } = false;
    public bool is_active { get; set; } = true;
}
