using Prometheus.Models.Interfaces;

namespace Prometheus.Models.Permissions;

public class PurchaseOrderPermissions : IModulePermissions
{
    public static string Read { get { return "read_purchaseorder"; } }
    public static string Create { get { return "create_purchaseorder"; } }
    public static string Edit { get { return "edit_purchaseorder"; } }
    public static string Delete { get { return "delete_purchaseorder"; } }
}
