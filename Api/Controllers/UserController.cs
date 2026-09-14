using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using KosmosERP.BusinessLayer.Models.Module.User.Command.Create;
using KosmosERP.BusinessLayer.Models.Module.User.Command.Delete;
using KosmosERP.BusinessLayer.Models.Module.User.Command.Edit;
using KosmosERP.BusinessLayer.Models.Module.User.Command.Find;
using KosmosERP.BusinessLayer.Models.Module.User.Dto;
using KosmosERP.BusinessLayer.Models.Module.User.ListProfiles;
using KosmosERP.BusinessLayer.Modules;
using KosmosERP.Models;
using KosmosERP.Module;

namespace KosmosERP.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class UserController : ERPApiController
{
    private IUserModule _Module;

    public UserController(IUserModule userModule) : base(userModule)
    {
        _Module = userModule;
    }

    [HttpGet("GetUser", Name = "GetUser")]
    [ProducesResponseType(typeof(Response<UserDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> Get([FromQuery] int id)
    {
        var result = await _Module.GetDto(id);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }



    [HttpGet("GetUserByGuid", Name = "GetUserByGuid")]
    [ProducesResponseType(typeof(Response<UserDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> GetByGuid([FromQuery] string guid)
    {
        var result = await _Module.GetDtoByGuid(guid);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }


    [HttpGet("GetUserBySessionId", Name = "GetUserBySessionId")]
    [ProducesResponseType(typeof(Response<UserDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> GetUserBySessionId([FromQuery] string session_id)
    {
        var result = await _Module.GetBySession(session_id);

        return Ok(result);
    }

    [Authorize(Roles = "admin")]
    [HttpGet("GetUsers", Name = "GetUsers")]
    [ProducesResponseType(typeof(Response<List<UserAdminListDto>>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> GetUsers()
    {
        var result = await _Module.GetUsers();

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [Authorize(Roles = "admin")]
    [HttpGet("GetRoles", Name = "GetRoles")]
    [ProducesResponseType(typeof(Response<List<RoleDto>>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> GetRoles()
    {
        var result = await _Module.GetRoles();

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }



    [HttpGet("GetRolePermissions", Name = "GetRolePermissions")]
    [ProducesResponseType(typeof(Response<List<RolePermissionsDto>>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> GetModulePermissions([FromQuery] int role_id)
    {
        var result = await _Module.GetRolePermissions(role_id);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }


    [HttpGet("GetPermissionSet", Name = "GetPermissionSet")]
    [ProducesResponseType(typeof(Response<List<UserPermissionsSet>>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> GetPermissionSet([FromQuery] string session_id)
    {
        var result = await _Module.GetPermissionSet(session_id);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPost("AuthenticateUser", Name = "AuthenticateUser")]
    [ProducesResponseType(typeof(Response<AuthenticatedUserDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> Authenticate([FromBody] AuthenticateCommand command)
    {
        var result = await _Module.Authenticate(command);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }


    [HttpPost("FindUser", Name = "FindUser")]
    [ProducesResponseType(typeof(PagingResult<UserListDto>), 200)]
    [ProducesResponseType(500)]
    public async Task<ActionResult> Find([FromQuery] GeneralListProfile listProfile, [FromBody] UserFindCommand command)
    {
        try
        {
            if (command != null)
            {
                var sortingParams = new PagingSortingParameters(listProfile.Start, listProfile.ResultCount, listProfile.SortOrder);

                var result = await _Module.Find(sortingParams, command);

                return Ok(result);
            }
            else
            {
                return StatusCode(500, "Api body is null");
            }
        }
        catch (Exception e)
        {
            return StatusCode(500, e.Message);
        }
    }

    [Authorize(Roles = "admin")]
    [HttpPost("CreateUser", Name = "CreateUser")]
    [ProducesResponseType(typeof(Response<UserDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> Create([FromBody] UserCreateCommand createCommand)
    {
        string bearerToken = null;

        if (Request?.Headers != null && Request.Headers.ContainsKey("Authorization"))
        {
            var authHeader = Request.Headers["Authorization"].ToString();
            if (!string.IsNullOrWhiteSpace(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                bearerToken = authHeader.Substring("Bearer ".Length).Trim();
            }
        }

        var result = await _Module.CreateExtended(createCommand, bearerToken);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [Authorize(Roles = "admin")]
    [HttpPost("CreateRole", Name = "CreateRole")]
    [ProducesResponseType(typeof(Response<RoleDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> CreateRole([FromBody] RoleCreateCommand createCommand)
    {
        var result = await _Module.CreateRole(createCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
    
    [Authorize(Roles = "admin")]
    [HttpPost("AssignUserRole", Name = "AssignUserRole")]
    [ProducesResponseType(typeof(Response<bool>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> AssignUserRole([FromBody] AssignUserRoleCommand createCommand)
    {
        var result = await _Module.AssignUserRole(createCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }


    [Authorize(Roles = "admin")]
    [HttpPut("UpdateUser", Name = "UpdateUser")]
    [ProducesResponseType(typeof(Response<UserDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> Edit([FromBody] UserEditCommand editCommand)
    {
        var result = await _Module.Edit(editCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [Authorize(Roles = "admin")]
    [HttpPut("EditRoleModulePermission", Name = "EditRoleModulePermission")]
    [ProducesResponseType(typeof(Response<UserDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> EditRoleModulePermission([FromBody] RoleModulePermissionEditCommand editCommand)
    {
        var result = await _Module.EditRoleModulePermission(editCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }



    [HttpGet("GetUsersByDepartment", Name = "GetUsersByDepartment")]
    [ProducesResponseType(typeof(Response<UserDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> GetUsersByDepartmentName([FromQuery] string department_id)
    {
        var result = await _Module.GetUsersByDepartment(department_id);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [Authorize(Roles = "admin")]
    [HttpPost("DeleteUser", Name = "DeleteUser")]
    [ProducesResponseType(typeof(Response<UserDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> Delete([FromBody] UserDeleteCommand deleteCommand)
    {
        var result = await _Module.Delete(deleteCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [Authorize(Roles = "admin")]
    [HttpPost("DeleteRoleModulePermission", Name = "DeleteRoleModulePermission")]
    [ProducesResponseType(typeof(Response<bool>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> DeleteRoleModulePermission([FromBody] ModulePermissionDeleteCommand deleteCommand)
    {
        var result = await _Module.DeleteRoleModulePermission(deleteCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [Authorize(Roles = "admin")]
    [HttpPost("DeleteRole", Name = "DeleteRole")]
    [ProducesResponseType(typeof(Response<bool>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> DeleteRole([FromBody] RoleDeleteCommand deleteCommand)
    {
        var result = await _Module.DeleteRole(deleteCommand);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
    
    [Authorize(Roles = "admin")]
    [HttpPost("CreateNewRoleModulePermission", Name = "CreateNewRoleModulePermission")]
    [ProducesResponseType(typeof(Response<bool>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> CreateNewRoleModulePermission([FromBody] RoleModulePermissionCreateCommand command)
    {
        var result = await _Module.CreateNewRoleModulePermission(command);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
}
