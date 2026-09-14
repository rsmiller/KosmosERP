using KosmosERP.Models.Interfaces;

namespace KosmosERP.Models.Permissions;

public class LeadPermissions : IModulePermissions
{
    public static string Read { get { return "read_lead"; } }
    public static string Create { get { return "create_lead"; } }
    public static string Edit { get { return "edit_lead"; } }
    public static string Delete { get { return "delete_lead"; } }
}
