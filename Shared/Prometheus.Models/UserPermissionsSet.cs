using Prometheus.Database.Models;

namespace Prometheus.Models;

public class UserPermissionsSet
{
    public int user_id { get; set; }
    public List<ModulePermission> permissions { get; set; } = new List<ModulePermission>();
}
