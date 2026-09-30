import type { CountryDto, StateDto } from '@/models/country-models';
import type { DocumentUploadObjectDto, DocumentUploadObjectTagDto } from '@/models/document-models';
import type { ModuleObjectDto } from '@/models/key-value-models';
import type { ReportCategory } from '@/models/report-models';
import type { SettingsDto } from '@/models/settings-models';
import type { RoleDto, UserAdminListDto } from '@/models/user-models';
import { ERPModulesId } from '@/services/permissions-service';
import { baseDto, FIXED_DATE, nextId } from './sequence';

export function buildAdminUser(overrides: Partial<UserAdminListDto> = {}): UserAdminListDto {
  const id = overrides.id ?? nextId();
  return {
    id,
    first_name: 'Uma',
    last_name: `User${id}`,
    username: `uma.user${id}`,
    email: `uma${id}@kosmos.example.test`,
    employee_number: `E${2000 + id}`,
    department: 'Operations',
    created_on: FIXED_DATE,
    created_on_timezone: 'UTC',
    created_on_string: '2026-03-02',
    is_external_user: false,
    is_deleted: false,
    is_admin: false,
    is_management: false,
    is_guest: false,
    user_roles: [],
    ...overrides,
  };
}

export function buildRole(overrides: Partial<RoleDto> = {}): RoleDto {
  const roleId = overrides.role_id ?? nextId();
  return {
    role_id: roleId,
    name: `Role ${roleId}`,
    is_deleted: false,
    role_permissions: Object.entries(ERPModulesId).slice(0, 3).map(([moduleName, moduleId], index) => ({
      id: index + 1,
      role_id: roleId,
      module_id: moduleId,
      module_name: moduleName,
      read: true,
      write: false,
      edit: false,
      delete: false,
    })),
    ...overrides,
  };
}

/** GET /KeyValue/GetModuleInfo rows (the admin Lists and Roles pages). */
export function buildModuleInfo(): ModuleObjectDto[] {
  return Object.entries(ERPModulesId).map(([moduleName, moduleId]) => ({
    module_id: moduleId,
    module_name: moduleName,
    name: moduleName,
  }));
}

export function buildSettings(overrides: Partial<SettingsDto> = {}): SettingsDto {
  return {
    ...baseDto('sett', 1),
    company_name: 'Kosmos Test Manufacturing',
    company_address1: '1 Assembly Row',
    company_address2: '',
    company_city: 'Austin',
    company_state: 'TX',
    company_zip: '78701',
    company_country: 'USA',
    company_phone: '555-0100',
    company_ar_email: 'ar@kosmos.example.test',
    company_ap_email: 'ap@kosmos.example.test',
    company_general_email: 'hello@kosmos.example.test',
    company_website: 'https://kosmos.example.test',
    tax_id: '12-3456789',
    fiscal_year_start: 1,
    ...overrides,
  } as unknown as SettingsDto;
}

export function buildCountry(overrides: Partial<CountryDto> = {}): CountryDto {
  return {
    ...baseDto('ctry', 1),
    country_name: 'United States',
    iso3: 'USA',
    phonecode: '1',
    currency: 'USD',
    currency_symbol: '$',
    region: 'Americas',
    states: [],
    ...overrides,
  } as unknown as CountryDto;
}

export function buildState(overrides: Partial<StateDto> = {}): StateDto {
  return {
    ...baseDto('stat', 1),
    country_id: 1,
    state_name: 'Texas',
    iso2: 'TX',
    ...overrides,
  } as unknown as StateDto;
}

export function buildDocumentUploadObject(overrides: Partial<DocumentUploadObjectDto> = {}): DocumentUploadObjectDto {
  const id = overrides.id ?? nextId();
  return {
    ...baseDto('duob', id),
    friendly_name: 'Packing List',
    internal_name: 'packing_list',
    tag_templates: [],
    ...overrides,
  } as unknown as DocumentUploadObjectDto;
}

export function buildDocumentUploadObjectTag(overrides: Partial<DocumentUploadObjectTagDto> = {}): DocumentUploadObjectTagDto {
  return { id: 1, document_object_id: 1, name: 'PO Number', is_required: true, ...overrides } as DocumentUploadObjectTagDto;
}

export function buildReportCatalog(): ReportCategory[] {
  return [
    {
      name: 'Sales',
      reports: [
        {
          key: 'sales_by_customer',
          name: 'Sales by Customer',
          description: 'Invoiced sales grouped by customer',
          category: 'Sales',
          endpoint: 'api/v1/Reports/SalesByCustomer',
          parameters: [
            { name: 'start', label: 'Start date', type: 'date', required: true },
            { name: 'end', label: 'End date', type: 'date', required: true },
          ],
        },
      ],
    },
  ];
}
