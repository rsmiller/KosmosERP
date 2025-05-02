using KosmosERP.Models.Interfaces;

namespace KosmosERP.Models.Permissions;

public class PurchaseOrderReceivePermissions : IModulePermissions
{
    public static string Read { get { return "read_purchaseorderreceive"; } }
    public static string Create { get { return "create_purchaseorderreceive"; } }
    public static string Edit { get { return "edit_purchaseorderreceive"; } }
    public static string Delete { get { return "delete_purchaseorderreceive"; } }
}
