using KosmosERP.Models.Interfaces;

namespace KosmosERP.Models.Permissions;

public class CustomerPermissions : IModulePermissions
{
    public static string Read { get { return "read_customer"; } }
    public static string Create { get { return "create_customer"; } }
    public static string Edit { get { return "edit_customer"; } }
    public static string Delete { get { return "delete_customer"; } }
}