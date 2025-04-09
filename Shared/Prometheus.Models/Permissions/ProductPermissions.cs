using Prometheus.Models.Interfaces;

namespace Prometheus.Models.Permissions;

public class ProductPermissions : IModulePermissions
{
    public static string Read { get { return "read_product"; } }
    public static string Create { get { return "create_product"; } }
    public static string Edit { get { return "edit_product"; } }
    public static string Delete { get { return "delete_product"; } }
}