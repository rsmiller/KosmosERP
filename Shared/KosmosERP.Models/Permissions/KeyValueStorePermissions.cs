using KosmosERP.Models.Interfaces;

namespace KosmosERP.Models.Permissions;

public class KeyValueStorePermissions : IModulePermissions
{
    public static string Read { get { return "read_keyvaluestore"; } }
    public static string Create { get { return "create_keyvaluestore"; } }
    public static string Edit { get { return "edit_keyvaluestore"; } }
    public static string Delete { get { return "delete_keyvaluestore"; } }
} 