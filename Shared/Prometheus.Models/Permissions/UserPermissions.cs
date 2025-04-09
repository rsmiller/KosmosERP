using Prometheus.Models.Interfaces;

namespace Prometheus.Models.Permissions;

public class UserPermissions : IModulePermissions
{
    public static string Read { get { return "read_user"; } }
    public static string Create { get { return "create_user"; } }
    public static string Edit { get { return "edit_user"; } }
    public static string Delete { get { return "delete_user"; } }
}
