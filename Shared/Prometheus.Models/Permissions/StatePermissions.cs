using Prometheus.Models.Interfaces;

namespace Prometheus.Models.Permissions;

public class StatePermissions : IModulePermissions
{
    public static string Read { get { return "read_state"; } }
    public static string Create { get { return "create_state"; } }
    public static string Edit { get { return "edit_state"; } }
    public static string Delete { get { return "delete_state"; } }
}
