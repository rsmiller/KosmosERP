using KosmosERP.Models.Interfaces;

namespace KosmosERP.Models.Permissions;

public class ActivityPermissions : IModulePermissions
{
    public static string Read { get { return "read_activity"; } }
    public static string Create { get { return "create_activity"; } }
    public static string Edit { get { return "edit_activity"; } }
    public static string Delete { get { return "delete_activity"; } }
} 