
using KosmosERP.Database;

namespace KosmosERP.BusinessLayer.Models.Module.User.Dto;

public class RolePermissionsDto : BaseDatabaseModel
{
    public int role_id { get; set; }
    public string module_id { get; set; }
    public string module_name { get; set; }
    public bool read { get; set; } = false;
    public bool write { get; set; } = false;
    public bool edit { get; set; } = false;
    public bool delete { get; set; } = false;
    public bool requires_admin { get; set; } = false;
    public bool requires_management { get; set; } = false;
    public bool requires_guest { get; set; } = false;
}