import type { CustomerDto, CustomerListDto } from '@/models/customer-models';
import { PaymentTermKeys } from './lookups';
import { guidFor, nextId } from './sequence';

export function buildCustomer(overrides: Partial<CustomerDto> = {}): CustomerDto {
  const id = overrides.id ?? nextId();
  return {
    id,
    guid: guidFor('cust', id),
    customer_number: 10000 + id,
    customer_name: `Customer ${id}`,
    customer_description: `Description for customer ${id}`,
    phone: '555-0100',
    fax: '',
    general_email: `info${id}@customer.example.test`,
    accounting_email: `ap${id}@customer.example.test`,
    website: `https://customer${id}.example.test`,
    category: 'Manufacturing',
    is_taxable: false,
    tax_rate: 0,
    payment_terms: PaymentTermKeys.Net30,
    payment_terms_name: 'Net 30',
    created_on: '2026-01-01T00:00:00',
    ...overrides,
  };
}

/** Find* rows use the same shape as the full DTO. */
export function buildCustomerListItem(overrides: Partial<CustomerListDto> = {}): CustomerListDto {
  return buildCustomer(overrides);
}
