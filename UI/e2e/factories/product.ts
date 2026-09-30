import type { ProductDto } from '@/models/product-models';
import { ProductCategoryKeys } from './lookups';
import { guidFor, nextId } from './sequence';

export function buildProduct(overrides: Partial<ProductDto> = {}): ProductDto {
  const id = overrides.id ?? nextId();
  return {
    id,
    guid: guidFor('prod', id),
    vendor_id: 1,
    product_class: 'Component',
    category: ProductCategoryKeys.Processors,
    identifier1: `SKU-${id}`,
    identifier2: null,
    identifier3: null,
    product_name: `Product ${id}`,
    internal_description: `Internal description for product ${id}`,
    external_description: null,
    required_stock_level: 10,
    required_reorder_level: 5,
    required_min_order: 1,
    our_cost: 100,
    unit_cost: 120,
    sales_price: 180,
    list_price: 200,
    rfid_id: null,
    is_taxable: true,
    is_stock: true,
    is_material: false,
    is_rental_item: false,
    is_sales_item: true,
    is_labor: false,
    is_shippable: true,
    is_retired: false,
    created_on: '2026-01-01T00:00:00',
    ...overrides,
  } as ProductDto;
}
