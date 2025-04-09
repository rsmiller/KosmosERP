using Prometheus.Models.Interfaces;

namespace Prometheus.Models.Permissions;

public class OpportunityPermissions : IModulePermissions
{
    public static string Read { get { return "read_opportunity"; } }
    public static string Create { get { return "create_opportunity"; } }
    public static string Edit { get { return "edit_opportunity"; } }
    public static string Delete { get { return "delete_opportunity"; } }
}