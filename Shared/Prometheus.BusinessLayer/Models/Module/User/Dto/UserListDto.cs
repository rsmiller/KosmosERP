namespace Prometheus.BusinessLayer.Models.Module.User.Dto;

public class UserListDto
{
    public int id { get; set; }
    public required string first_name { get; set; }
    public required string last_name { get; set; }
    public required string username { get; set; }
    public string? email { get; set; }
    public required string employee_number { get; set; }
    public int? department { get; set; }
    public DateTime created_on { get; set; }
    public string? created_by_name { get; set; }
    public string created_on_timezone { get; set; }
    public string created_on_string { get; set; }
    public DateTime? updated_on { get; set; }
    public int? updated_by { get; set; }
    public string? updated_by_name { get; set; }
    public string? updated_on_timezone { get; set; }
    public string? updated_on_string { get; set; }
    public bool is_external_user { get; set; }
    public bool is_deleted { get; set; }
    public bool is_admin { get; set; }
    public bool is_management { get; set; }
    public bool is_guest { get; set; }
}
