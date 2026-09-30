import type { VendorDto } from '@/models/vendor-models';
import { guidFor, nextId } from './sequence';

export function buildVendor(overrides: Partial<VendorDto> = {}): VendorDto {
  const id = overrides.id ?? nextId();
  return {
    id,
    guid: guidFor('vend', id),
    vendor_number: 20000 + id,
    vendor_name: `Vendor ${id}`,
    vendor_description: `Description for vendor ${id}`,
    phone: '555-0200',
    general_email: `sales${id}@vendor.example.test`,
    website: `https://vendor${id}.example.test`,
    category: 'Supplies',
    created_on: '2026-01-01T00:00:00',
    ...overrides,
  } as VendorDto;
}
