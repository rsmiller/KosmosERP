using Prometheus.Models.Interfaces;

namespace Prometheus.Models.Permissions;

public class ContactPermissions : IModulePermissions
{
    public static string Read { get { return "read_contact"; } }
    public static string Create { get { return "create_contact"; } }
    public static string Edit { get { return "edit_contact"; } }
    public static string Delete { get { return "delete_contact"; } }
}