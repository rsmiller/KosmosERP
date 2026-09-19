import { UserRoleDto } from '@/models/user-models'
import { ERPModules, ERPModulesId, ERPModulePermission } from '@/services/permissions-service'

// Build a reverse lookup: module GUID -> permission key (e.g. "customers").
// ERPModulesId maps ModuleName -> GUID; ERPModules maps ModuleName -> permission key.
const guidToModuleKey: Record<string, string> = Object.entries(ERPModulesId).reduce(
  (acc, [moduleName, guid]) => {
    const key = (ERPModules as Record<string, string>)[moduleName]
    if (key) acc[guid as string] = key
    return acc
  },
  {} as Record<string, string>
)

/**
 * Converts the API permission shape (UserRoleDto[] with module_id GUID + boolean
 * read/write/edit/delete flags) into the flat role-string scheme that
 * permissionsService.HasPermission expects (e.g. "customers_read", "admin").
 *
 * This lets database/SAML users share the exact permission checks Keycloak
 * realm roles already use. Unknown module GUIDs are skipped.
 */
export function rolesToPermissionStrings(roles: UserRoleDto[] | undefined | null): string[] {
  if (!roles || roles.length === 0) return []

  const result = new Set<string>()

  for (const role of roles) {
    for (const permission of role.permissions ?? []) {
      const moduleKey = permission.module_id ? guidToModuleKey[permission.module_id] : undefined
      if (!moduleKey) continue

      if (permission.read) result.add(`${moduleKey}_${ERPModulePermission.Read}`)
      if (permission.write) result.add(`${moduleKey}_${ERPModulePermission.Write}`)
      if (permission.edit) result.add(`${moduleKey}_${ERPModulePermission.Edit}`)
      if (permission.delete) result.add(`${moduleKey}_${ERPModulePermission.Delete}`)
    }
  }

  return Array.from(result)
}
