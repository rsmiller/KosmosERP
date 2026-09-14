using KosmosERP.Models.Interfaces;

namespace KosmosERP.Models.Permissions;

public class CreditMemoPermissions : IModulePermissions
{
    public static string Read { get { return "read_creditmemo"; } }
    public static string Create { get { return "create_creditmemo"; } }
    public static string Edit { get { return "edit_creditmemo"; } }
    public static string Delete { get { return "delete_creditmemo"; } }
} 