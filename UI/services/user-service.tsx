import { AssignUserRoleCommand, AuthenticateCommand, AuthenticatedUserDto, ModulePermissionDeleteCommand, RoleCreateCommand, RoleDeleteCommand, RoleDto, RoleModulePermissionCreateCommand, RoleModulePermissionEditCommand, RolePermissionsDto, UserAdminListDto, UserCreateCommand, UserDeleteCommand, UserDto, UserEditCommand, UserFindCommand, UserListDto, UserPermissionsSet } from "@/models/user-models";
import axiosInstance from "./axios-instance";
import { ApiResponse, PagedApiResponse } from "@/models/base-models";

export const userService = {

  async authenticateUser(authenticateCommand: AuthenticateCommand, token: string = ""): Promise<ApiResponse<AuthenticatedUserDto>> {
    try
    {
      const response = await axiosInstance(token).post<ApiResponse<AuthenticatedUserDto>>(`/api/v1/User/AuthenticateUser`, authenticateCommand);

      return response.data;
    }
    catch(ex: any)
    {
      if(ex.response != undefined && ex.response.data != undefined)
      {
        return ex.response.data as ApiResponse<AuthenticatedUserDto>;
      }
      else
      {
        return { success: false, exception: ex, resultCode: -5, data: undefined};
      }
    }
  },

  async get(userId: number, token: string): Promise<ApiResponse<UserDto>> {
    const response = await axiosInstance(token).get<ApiResponse<UserDto>>(`/api/v1/User/GetUser/?id=${userId}`);
    return response.data;
  },

  async getByGuid(guid: string, token: string): Promise<ApiResponse<UserDto>> {
    const response = await axiosInstance(token).get<ApiResponse<UserDto>>(`/api/v1/User/GetUserByGuid/?guid=${guid}`);
    return response.data;
  },

  async getUserByDepartmentId(department_id: number, token: string): Promise<ApiResponse<UserDto>> {
    const response = await axiosInstance(token).get<ApiResponse<UserDto>>(`/api/v1/User/GetUsersByDepartment/?department_id=${department_id}`);
    return response.data;
  },

  async getUserBySessionId(sessionId: string, token: string): Promise<ApiResponse<UserDto>> {
    const response = await axiosInstance(token).get<ApiResponse<UserDto>>(`/api/v1/User/GetUserBySessionId/?session_id=${sessionId}`);
    return response.data;
  },

  async getUsers(token: string): Promise<ApiResponse<UserAdminListDto[]>> {
    const response = await axiosInstance(token).get<ApiResponse<UserAdminListDto[]>>(`/api/v1/User/GetUsers`);
    return response.data;
  },

  async getRoles(token: string): Promise<ApiResponse<RoleDto[]>> {
    const response = await axiosInstance(token).get<ApiResponse<RoleDto[]>>(`/api/v1/User/GetRoles`);
    return response.data;
  },

  async getRolePermissions(token: string): Promise<ApiResponse<RolePermissionsDto[]>> {
    const response = await axiosInstance(token).get<ApiResponse<RolePermissionsDto[]>>(`/api/v1/User/GetRolePermissions`);
    return response.data;
  },

  async updateRoleModulePermission(editCommand: RoleModulePermissionEditCommand, token: string): Promise<ApiResponse<Boolean>> {
    const response = await axiosInstance(token).put<ApiResponse<Boolean>>(`/api/v1/User/EditRoleModulePermission`, editCommand);
    return response.data;
  },
  
  async createNewRoleModulePermission(command: RoleModulePermissionCreateCommand, token: string): Promise<ApiResponse<Boolean>> {
    const response = await axiosInstance(token).post<ApiResponse<Boolean>>(`/api/v1/User/CreateNewRoleModulePermission`, command);
    return response.data;
  },

  async find(userFindCommand: UserFindCommand, token: string, start: number = 1, resultCount: number = 20, sortOrder: string = "created_on-asc"): Promise<PagedApiResponse<UserListDto[]>> {
    const response = await axiosInstance(token).post<PagedApiResponse<UserListDto[]>>(`/api/v1/User/FindUser?Start=${start}&ResultCount=${resultCount}&SortOrder=${sortOrder}`, userFindCommand);
    return response.data;
  },

  async create(userCreateCommand: UserCreateCommand, token: string): Promise<ApiResponse<UserDto>> {
    const response = await axiosInstance(token).post<ApiResponse<UserDto>>(`/api/v1/User/CreateUser`, userCreateCommand);
    return response.data;
  },
  
  async createRole(createCommand: RoleCreateCommand, token: string): Promise<ApiResponse<RoleDto>> {
    const response = await axiosInstance(token).post<ApiResponse<RoleDto>>(`/api/v1/User/CreateRole`, createCommand);
    return response.data;
  },
  
  async assignUserRole(assignCommand: AssignUserRoleCommand, token: string): Promise<ApiResponse<boolean>> {
    const response = await axiosInstance(token).post<ApiResponse<boolean>>(`/api/v1/User/AssignUserRole`, assignCommand);
    return response.data;
  },

  async update(userEditCommand: UserEditCommand, token: string): Promise<ApiResponse<UserDto>> {
    const response = await axiosInstance(token).put<ApiResponse<UserDto>>(`/api/v1/User/UpdateUser`, userEditCommand);
    return response.data;
  },

  async delete(userDeleteCommand: UserDeleteCommand, token: string): Promise<ApiResponse<UserDto>> {
    const response = await axiosInstance(token).post<ApiResponse<UserDto>>(`/api/v1/User/DeleteUser`, userDeleteCommand);
    return response.data;
  },

  async deleteRoleModulePermission(deleteCommand: ModulePermissionDeleteCommand, token: string): Promise<ApiResponse<Boolean>> {
    const response = await axiosInstance(token).post<ApiResponse<Boolean>>(`/api/v1/User/DeleteRoleModulePermission`, deleteCommand);
    return response.data;
  },
  
  async deleteRole(deleteCommand: RoleDeleteCommand, token: string): Promise<ApiResponse<Boolean>> {
    const response = await axiosInstance(token).post<ApiResponse<Boolean>>(`/api/v1/User/DeleteRole`, deleteCommand);
    return response.data;
  },

  async getPermissionSet(sessionId: string, token: string): Promise<ApiResponse<UserPermissionsSet>> {
    const response = await axiosInstance(token).get<ApiResponse<UserPermissionsSet>>(`/api/v1/User/GetPermissionSet/?session_id=${sessionId}`);
    return response.data;
  },
};
