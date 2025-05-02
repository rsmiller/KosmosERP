using KosmosERP.Models.Interfaces;

namespace KosmosERP.Models.Permissions;

public class TransactionPermissions : IModulePermissions
{
    public static string Read { get { return "read_transaction"; } }
    public static string Create { get { return "create_transaction"; } }
    public static string Edit { get { return "edit_transaction"; } }
    public static string Delete { get { return "delete_transaction"; } }
}