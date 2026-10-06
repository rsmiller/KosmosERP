import { adminPermissions, buildAuthenticatedUser, buildUser, readOnlyPermissions } from '../factories/user';

/** Well-known personas. auth.setup.ts logs each one in and saves its storage state. */
export const users = {
  admin: {
    storageState: 'e2e/.auth/admin.json',
    password: 'e2e-password',
    auth: buildAuthenticatedUser({
      user: buildUser({ id: 9001, first_name: 'Ada', last_name: 'Admin', username: 'ada.admin', is_admin: true }),
      permissions: adminPermissions(),
      roleName: 'Administrators',
    }),
  },
  readOnly: {
    storageState: 'e2e/.auth/readonly.json',
    password: 'e2e-password',
    auth: buildAuthenticatedUser({
      user: buildUser({ id: 9002, first_name: 'Rita', last_name: 'Reader', username: 'rita.reader' }),
      permissions: readOnlyPermissions(),
      roleName: 'Read Only',
    }),
  },
} as const;

export type Persona = (typeof users)[keyof typeof users];
