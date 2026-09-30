import type { PurchaseOrderHeaderDto, PurchaseOrderLineDto } from '@/models/purchase-order-models';
import type { PurchaseOrderReceiveHeaderDto, PurchaseOrderReceiveLineDto } from '@/models/po-receive-models';
import { baseDto, FIXED_DATE, nextId } from './sequence';

export function buildPurchaseOrderLine(overrides: Partial<PurchaseOrderLineDto> = {}): PurchaseOrderLineDto {
  const id = overrides.id ?? nextId();
  return {
    ...baseDto('pol', id),
    purchase_order_header_id: 1,
    product_id: 1,
    revision_number: 1,
    line_number: 1,
    quantity: 10,
    description: 'DDR5 32GB kit',
    unit_price: 95,
    tax: 0,
    is_taxable: false,
    is_complete: false,
    is_canceled: false,
    product_name: 'Product',
    identifier1: 'SKU-1',
    ...overrides,
  } as PurchaseOrderLineDto;
}

export function buildPurchaseOrder(overrides: Partial<PurchaseOrderHeaderDto> = {}): PurchaseOrderHeaderDto {
  const id = overrides.id ?? nextId();
  return {
    ...baseDto('pord', id),
    vendor_id: 1,
    vendor_name: 'Vendor',
    po_type: 'standard',
    revision_number: 1,
    po_number: 30000 + id,
    price: 950,
    tax: 0,
    po_quote_number: `Q-${id}`,
    is_complete: false,
    is_canceled: false,
    po_by: 'Ada Admin',
    purchase_order_lines: [],
    ...overrides,
  } as PurchaseOrderHeaderDto;
}

export function buildPOReceiveLine(overrides: Partial<PurchaseOrderReceiveLineDto> = {}): PurchaseOrderReceiveLineDto {
  const id = overrides.id ?? nextId();
  return {
    ...baseDto('porl', id),
    purchase_order_receive_header_id: 1,
    purchase_order_line_id: 1,
    units_ordered: 10,
    units_received: 10,
    is_complete: true,
    is_canceled: false,
    completed_on: FIXED_DATE,
    product_name: 'Product',
    line_number: 1,
    ...overrides,
  } as PurchaseOrderReceiveLineDto;
}

export function buildPOReceive(overrides: Partial<PurchaseOrderReceiveHeaderDto> = {}): PurchaseOrderReceiveHeaderDto {
  const id = overrides.id ?? nextId();
  return {
    ...baseDto('porh', id),
    purchase_order_id: 1,
    po_number: 30001,
    po_by: 'Ada Admin',
    units_ordered: 10,
    units_received: 10,
    is_complete: true,
    is_canceled: false,
    completed_on: FIXED_DATE,
    received_lines: [],
    received_uploads: [],
    ...overrides,
  } as PurchaseOrderReceiveHeaderDto;
}
