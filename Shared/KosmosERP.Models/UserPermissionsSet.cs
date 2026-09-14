using KosmosERP.Database.Models;

namespace KosmosERP.Models;

public class UserPermissionsSet
{
    public int user_id { get; set; }
    public bool is_admin { get; set; } = false;
    public List<RolePermission> permissions { get; set; } = new List<RolePermission>();
}
