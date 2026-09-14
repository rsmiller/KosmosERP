using KosmosERP.Models.Interfaces;

namespace KosmosERP.Models.Permissions;

public class ARInvoicePermissions : IModulePermissions
{
    public static string Read { get { return "read_arinvoice"; } }
    public static string Create { get { return "create_arinvoice"; } }
    public static string Edit { get { return "edit_arinvoice"; } }
    public static string Delete { get { return "delete_arinvoice"; } }
}
