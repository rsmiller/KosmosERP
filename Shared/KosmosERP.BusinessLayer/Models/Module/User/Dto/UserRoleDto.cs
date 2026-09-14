
using KosmosERP.Database;

namespace KosmosERP.BusinessLayer.Models.Module.User.Dto;

public class UserRoleDto : BaseDatabaseModel
{
    public int role_id { get; set; }
    public string role_name { get; set; }

    public List<RolePermissionsDto> permissions { get; set; } = new List<RolePermissionsDto>();
}