using KosmosERP.Models.Interfaces;

namespace KosmosERP.Models.Permissions;

public class BOMPermissions : IModulePermissions
{
    public static string Read { get { return "read_bom"; } }
    public static string Create { get { return "create_bom"; } }
    public static string Edit { get { return "edit_bom"; } }
    public static string Delete { get { return "delete_bom"; } }
}
