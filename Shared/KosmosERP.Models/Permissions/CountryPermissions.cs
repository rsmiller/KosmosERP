using KosmosERP.Models.Interfaces;

namespace KosmosERP.Models.Permissions;

public class CountryPermissions : IModulePermissions
{
    public static string Read { get { return "read_country"; } }
    public static string Create { get { return "create_country"; } }
    public static string Edit { get { return "edit_country"; } }
    public static string Delete { get { return "delete_country"; } }
}