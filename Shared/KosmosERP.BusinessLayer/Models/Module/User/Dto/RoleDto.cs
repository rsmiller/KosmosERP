namespace KosmosERP.BusinessLayer.Models.Module.User.Dto;

public class RoleDto
{
    public int role_id { get; set; }
    public string name { get; set; }
    public bool is_deleted { get; set; }

    public List<RolePermissionsDto> role_permissions { get; set; } = new List<RolePermissionsDto>();
}
