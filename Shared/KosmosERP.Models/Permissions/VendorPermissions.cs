using KosmosERP.Models.Interfaces;

namespace KosmosERP.Models.Permissions;

public class VendorPermissions : IModulePermissions
{
    public static string Read { get { return "read_vendor"; } }
    public static string Create { get { return "create_vendor"; } }
    public static string Edit { get { return "edit_vendor"; } }
    public static string Delete { get { return "delete_vendor"; } }
}
