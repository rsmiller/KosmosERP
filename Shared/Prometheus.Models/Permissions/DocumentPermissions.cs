using Prometheus.Models.Interfaces;

namespace Prometheus.Models.Permissions;

public class DocumentPermissions : IModulePermissions
{
    public static string Read { get { return "read_document"; } }
    public static string Create { get { return "create_document"; } }
    public static string Edit { get { return "edit_document"; } }
    public static string Delete { get { return "delete_document"; } }
}