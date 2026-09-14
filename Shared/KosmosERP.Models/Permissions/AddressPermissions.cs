using KosmosERP.Models.Interfaces;

namespace KosmosERP.Models.Permissions;

public class AddressPermissions : IModulePermissions
{
    public static string Read { get { return "read_address"; }  }
    public static string Create { get { return "create_address"; } }
    public static string Edit { get { return "edit_address"; } }
    public static string Delete { get { return "delete_address"; } }
}
