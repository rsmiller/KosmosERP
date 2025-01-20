using Prometheus.Database.Models;

namespace Prometheus.Api.Models.Module.User.Dto;

public class UserDto
{
    public int id { get; set; }
    public string first_name { get; set; } = "";
    public string last_name { get; set; } = "";
    public string email { get; set; } = "";
    public string username { get; set; } = "";
    public string password { get; set; } = "";
    public string password_salt { get; set; } = "";
    public string employee_number { get; set; } = "";
    public int? department { get; set; }
    public DateTime created_on { get; set; }
    public int? created_by { get; set; }
    public DateTime? updated_on { get; set; }
    public int? updated_by { get; set; }
    public bool is_external_user { get; set; } = false;
    public bool is_deleted { get; set; } = false;
    public bool is_admin { get; set; } = false;
    public bool is_management { get; set; } = false;
    public bool is_guest { get; set; } = false;
}
