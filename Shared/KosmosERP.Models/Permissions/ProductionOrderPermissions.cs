using KosmosERP.Models.Interfaces;

namespace KosmosERP.Models.Permissions;

public class ProductionOrderPermissions : IModulePermissions
{
    public static string Read { get { return "read_productionorder"; } }
    public static string Create { get { return "create_productionorder"; } }
    public static string Edit { get { return "edit_productionorder"; } }
    public static string Delete { get { return "delete_productionorder"; } }
}

