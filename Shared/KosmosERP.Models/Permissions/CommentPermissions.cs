using KosmosERP.Models.Interfaces;

namespace KosmosERP.Models.Permissions;

public class CommentPermissions : IModulePermissions
{
    public static string Read { get { return "read_comments"; } }
    public static string Create { get { return "create_comments"; } }
    public static string Edit { get { return "edit_comments"; } }
    public static string Delete { get { return "delete_comments"; } }
}
