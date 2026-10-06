import type { AuthenticatedUserDto, RolePermissionsDto, UserDto } from '@/models/user-models';
import { ERPModulesId } from '@/services/permissions-service';
import { nextId } from './sequence';

type Access = Pick<RolePermissionsDto, 'read' | 'write' | 'edit' | 'delete'>;

const FULL: Access = { read: true, write: true, edit: true, delete: true };
const READ_ONLY: Access = { read: true, write: false, edit: false, delete: false };

/** One permission row per module the UI knows a GUID for (ERPModulesId). */
function permissionsFor(access: Access): RolePermissionsDto[] {
  return Object.entries(ERPModulesId).map(([moduleName, moduleId], index) => ({
    id: index + 1,
    role_id: 1,
    module_id: moduleId,
    module_name: moduleName,
    is_active: true,
    is_deleted: false,
    ...access,
  }));
}

export const adminPermissions = () => permissionsFor(FULL);
export const readOnlyPermissions = () => permissionsFor(READ_ONLY);

export function buildUser(overrides: Partial<UserDto> = {}): UserDto {
  const id = nextId();
  return {
    id,
    first_name: 'Test',
    last_name: `User${id}`,
    email: `test.user${id}@example.test`,
    username: `test.user${id}`,
    password: '',
    password_salt: '',
    employee_number: `E${1000 + id}`,
    is_external_user: false,
    is_deleted: false,
    is_admin: false,
    is_management: false,
    is_guest: false,
    created_on: '2026-01-01T00:00:00',
    created_on_timezone: 'UTC',
    created_on_string: '2026-01-01',
    ...overrides,
  };
}

/** Response payload for POST /User/AuthenticateUser. */
export function buildAuthenticatedUser({
  user = buildUser(),
  permissions = adminPermissions(),
  roleName = 'E2E Role',
}: { user?: UserDto; permissions?: RolePermissionsDto[]; roleName?: string } = {}): AuthenticatedUserDto {
  return {
    id: user.id,
    authenticated: true,
    user,
    session: {
      id: user.id,
      user_id: user.id,
      session_id: `e2e-session-${user.id}`,
      session_expires: '2099-01-01T00:00:00Z',
      created_on: '2026-01-01T00:00:00',
    },
    token: {
      access_token: `e2e-access-token-${user.id}`,
      expires_in: 21600,
      refresh_token: `e2e-refresh-token-${user.id}`,
      token_type: 'Bearer',
    },
    roles: [{ role_id: 1, role_name: roleName, permissions }],
  };
}
