namespace Prometheus.Models.Interfaces;
public interface IModulePermissions
{
    static string Read { get;}
    static string Create { get;}
    static string Edit { get; }
    static string Delete { get; }
}
