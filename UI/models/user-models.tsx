import { DataCommand } from "./base-models";

export interface AuthenticatedUserDto {
  id: number;
  authenticated: boolean;
  user: UserDto;
  session: UserSessionState;
  token: JwtToken;
  roles?: UserRoleDto[];
}


export interface UserDto {
  id: number;
  first_name: string;
  last_name: string;
  email: string;
  username: string;
  password: string;
  password_salt: string;
  employee_number: string;
  department?: string;
  is_external_user: boolean;
  is_deleted: boolean;
  is_admin: boolean;
  is_management: boolean;
  is_guest: boolean;
  created_on: string;
  created_by?: number;
  created_on_timezone: string;
  created_on_string: string;
  updated_on?: string;
  updated_by?: number;
  updated_on_timezone?: string;
  updated_on_string?: string;
}


export interface UserListDto {
  id: number;
  first_name: string;
  last_name: string;
  username: string;
  email?: string;
  employee_number: string;
  department?: string;
  created_on: string;
  created_by_name?: string;
  created_on_timezone: string;
  created_on_string: string;
  updated_on?: string;
  updated_by?: number;
  updated_by_name?: string;
  updated_on_timezone?: string; 
  updated_on_string?: string;
  is_external_user: boolean;
  is_deleted: boolean;
  is_admin: boolean;
  is_management: boolean;
  is_guest: boolean;
}

export interface UserAdminListDto {
  id: number;
  first_name: string;
  last_name: string;
  username: string;
  email?: string;
  employee_number: string;
  department?: string;
  created_on: string;
  created_by_name?: string;
  created_on_timezone: string;
  created_on_string: string;
  updated_on?: string;
  updated_by?: number;
  updated_by_name?: string;
  updated_on_timezone?: string; 
  updated_on_string?: string;
  is_external_user: boolean;
  is_deleted: boolean;
  is_admin: boolean;
  is_management: boolean;
  is_guest: boolean;

  user_roles?: UserRoleDto[];
}


export interface UserSessionState {
  id: number;
  user_id: number;
  session_id: string;
  session_expires: string;
  created_on: string;
}


export interface JwtToken
{
  access_token: string;
  expires_in: number;
  refresh_token?: string;
  refresh_expires_in?: number;
  token_type?: string;
}

export class UserFindCommand extends DataCommand
{
  wildcard: string = "";
}

export class UserCreateCommand  extends DataCommand {
  first_name: string = "";
  last_name: string = "";
  password: string = "";
  confirm_password: string = "";
  email?: string;
  username?: string;
  department?: string;
  employee_number?: string;
  is_external_user: boolean = false;
  is_deleted: boolean = false; 
  is_admin: boolean = false; 
  is_management: boolean = false;
  is_guest: boolean = false;
}


export class RoleModulePermissionCreateCommand extends DataCommand  {
  role_id: number = 0;
  module_id: string = "";
  read: boolean = false; 
  write: boolean = false; 
  edit: boolean = false; 
  delete: boolean = false;
}

export class RoleModulePermissionEditCommand extends DataCommand 
{
  id?: number;
  module_id?: string;
  read?: boolean; 
  write?: boolean; 
  edit?: boolean; 
  delete?: boolean;
}

export class ModulePermissionDeleteCommand extends DataCommand 
{
  id?: number;
}

export class RoleDeleteCommand extends DataCommand 
{
  id?: number;
}


export class UserEditCommand extends DataCommand {
  id: number = 0;
  first_name?: string;
  last_name?: string;
  password?: string;
  confirm_password?: string;
  email?: string;
  username?: string;
  department?: string;
  employee_number?: string; 
  is_external_user?: boolean;
  is_deleted?: boolean;
  is_admin?: boolean;
  is_management?: boolean;
  is_guest?: boolean;

}


export class UserPermissionEditCommand extends DataCommand {
  id: number = 0;
  permission_name?: string;
  read?: boolean; 
  write?: boolean;
  edit?: boolean; 
  delete?: boolean;
}

export class RoleCreateCommand extends DataCommand
{
  role_name?: string;
}

export class UserDeleteCommand extends DataCommand
{
    id: number = 0;
}

export class AuthenticateCommand
{
  username: string = "";
  password: string = "";
}


export class RoleDto
{
    role_id?: number;
    name?: string;
    is_deleted?: boolean;

    role_permissions?: RolePermissionsDto[];
}


export class RolePermissionsDto
{
    id?: number;
    role_id?: number;
    module_id?: string;
    module_name?: string;
    read?: boolean;
    write?: boolean;
    edit?: boolean;
    delete?: boolean;
    requires_admin?: boolean;
    requires_management?: boolean;
    requires_guest?: boolean;
    is_active?: boolean;
    is_deleted?: boolean;
}

export class UserRoleDto
{
  role_id?: number;
  role_name?: string;

  permissions?: RolePermissionsDto[];
}

export class AssignUserRoleCommand extends DataCommand
{
  user_id?: number;
  role_id?: number;
}

export class UserPermissionsSet
{
  user_id?: number;
  is_admin: boolean = false;
  permissions?: Array<RolePermissionsDto> = new Array<RolePermissionsDto>();
}