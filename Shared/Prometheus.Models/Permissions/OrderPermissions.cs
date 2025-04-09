using Prometheus.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prometheus.Models.Permissions;

public class OrderPermissions : IModulePermissions
{
    public static string Read { get { return "read_order"; } }
    public static string Create { get { return "create_order"; } }
    public static string Edit { get { return "edit_order"; } }
    public static string Delete { get { return "delete_order"; } }
}
