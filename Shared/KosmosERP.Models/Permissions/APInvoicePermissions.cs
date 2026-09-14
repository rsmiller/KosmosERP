using KosmosERP.Models.Interfaces;

namespace KosmosERP.Models.Permissions;

public class APInvoicePermissions : IModulePermissions
{
    public static string Read { get { return "read_apinvoice"; } }
    public static string Create { get { return "create_apinvoice"; } }
    public static string Edit { get { return "edit_apinvoice"; } }
    public static string Delete { get { return "delete_apinvoice"; } }
}
