using Prometheus.Models.Interfaces;

namespace Prometheus.Models.Permissions;

public class ShipmentPermissions : IModulePermissions
{
    public static string Read { get { return "read_shipment"; } }
    public static string Create { get { return "create_shipment"; } }
    public static string Edit { get { return "edit_shipment"; } }
    public static string Delete { get { return "delete_shipment"; } }
}