import type { BOMDto } from '@/models/bom-models';
import type { InventoryDto } from '@/models/inventory-models';
import type {
  ProductionOrderHeaderDto,
  ProductionOrderHeaderListDto,
  ProductionOrderLineDto,
} from '@/models/production-orders-models';
import type { TransactionListDto } from '@/models/transaction-models';
import { ProductionStatusKeys } from './lookups';
import { baseDto, DATE_ONLY, DUE_DATE_ONLY, FIXED_DATE, nextId } from './sequence';

export function buildProductionOrderLine(overrides: Partial<ProductionOrderLineDto> = {}): ProductionOrderLineDto {
  const id = overrides.id ?? nextId();
  return {
    ...baseDto('prol', id),
    production_order_header_id: 1,
    order_line_id: 1,
    quantity: 4,
    status: ProductionStatusKeys.Submitted,
    status_name: 'Submitted',
    is_complete: false,
    production_lead_minutes: 120,
    order_number: 50001,
    boms: [],
    ...overrides,
  } as ProductionOrderLineDto;
}

/** Serves both the detail DTO and the list row (which adds order_number). */
export function buildProductionOrder(
  overrides: Partial<ProductionOrderHeaderDto & ProductionOrderHeaderListDto> = {},
): ProductionOrderHeaderDto & ProductionOrderHeaderListDto {
  const id = overrides.id ?? nextId();
  return {
    ...baseDto('proh', id),
    order_header_id: 1,
    status: ProductionStatusKeys.Submitted,
    status_name: 'Submitted',
    priority_id: 1,
    planned_start_date: DATE_ONLY,
    planned_complete_date: '2026-03-09',
    is_complete: false,
    production_lead_minutes: 480,
    production_order_lines: [],
    ...overrides,
  } as ProductionOrderHeaderDto & ProductionOrderHeaderListDto;
}

export function buildBom(overrides: Partial<BOMDto> = {}): BOMDto {
  const id = overrides.id ?? nextId();
  return {
    ...baseDto('bom', id),
    parent_product_id: 1,
    product_id: 2,
    quantity: 2,
    instructions: 'Seat firmly',
    product_name: 'Component',
    ...overrides,
  } as BOMDto;
}

export function buildInventoryCount(overrides: Partial<InventoryDto> = {}): InventoryDto {
  const id = overrides.id ?? nextId();
  return {
    ...baseDto('invt', id),
    product_id: 1,
    product_name: 'Product',
    current_stock: 40,
    required_stock: 10,
    reorder_level: 5,
    on_hand: 40,
    reserved: 4,
    on_order: 10,
    to_order: 0,
    total_units_sold: 12,
    total_units_received: 52,
    total_units_shipped: 12,
    total_on_purchased: 10,
    updated_on: FIXED_DATE,
    ...overrides,
  } as unknown as InventoryDto;
}

export function buildInventoryTransaction(overrides: Partial<TransactionListDto> = {}): TransactionListDto {
  const id = overrides.id ?? nextId();
  return {
    ...baseDto('trns', id),
    product_id: 1,
    transaction_type: 1,
    transaction_date: FIXED_DATE,
    units_sold: 0,
    units_shipped: 0,
    units_purchased: 0,
    units_received: 5,
    purchased_unit_cost: 95,
    sold_unit_price: 0,
    ...overrides,
  } as unknown as TransactionListDto;
}
