import { UserRoleDto } from '@/models/user-models'
import { ERPModules, ERPModulesId, ERPModulePermission } from '@/services/permissions-service'

// Build a reverse lookup: module GUID -> permission keys (e.g. ["customers"]).
// ERPModulesId maps ModuleName -> GUID; ERPModules maps ModuleName -> permission key.
// A GUID can map to several keys because the API reuses some module GUIDs.
const guidToModuleKeys: Record<string, string[]> = Object.entries(ERPModulesId).reduce(
  (acc, [moduleName, guid]) => {
    const key = (ERPModules as Record<string, string>)[moduleName]
    if (key) (acc[guid] ??= []).push(key)
    return acc
  },
  {} as Record<string, string[]>
)

const allModuleKeys = Array.from(new Set(Object.values(ERPModules)))
const allPermissions = Object.values(ERPModulePermission)

/**
 * Converts the API permission shape (UserRoleDto[] with module_id GUID + boolean
 * read/write/edit/delete flags) into the flat role-string scheme that
 * permissionsService.HasPermission expects (e.g. "customers_read", "admin_read").
 *
 * This lets database/SAML users share the exact permission checks Keycloak
 * realm roles already use. Unknown module GUIDs are skipped.
 *
 * `isAdmin` mirrors the API, where UserDto.is_admin bypasses every module
 * permission check: it grants every permission on every module, including
 * modules with no API GUID (admin, general_ledger).
 */
export function rolesToPermissionStrings(
  roles: UserRoleDto[] | undefined | null,
  isAdmin = false
): string[] {
  const result = new Set<string>()

  if (isAdmin) {
    for (const moduleKey of allModuleKeys)
      for (const permission of allPermissions) result.add(`${moduleKey}_${permission}`)
  }

  for (const role of roles ?? []) {
    for (const permission of role.permissions ?? []) {
      const moduleKeys = permission.module_id ? guidToModuleKeys[permission.module_id] : undefined
      if (!moduleKeys) continue

      for (const moduleKey of moduleKeys) {
        if (permission.read) result.add(`${moduleKey}_${ERPModulePermission.Read}`)
        if (permission.write) result.add(`${moduleKey}_${ERPModulePermission.Write}`)
        if (permission.edit) result.add(`${moduleKey}_${ERPModulePermission.Edit}`)
        if (permission.delete) result.add(`${moduleKey}_${ERPModulePermission.Delete}`)
      }
    }
  }

  return Array.from(result)
}
