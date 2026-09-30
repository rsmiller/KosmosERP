import type { OrderHeaderDto, OrderLineDto } from '@/models/sales-order-models';
import type { ShipmentHeaderDto, ShipmentLineDto, vw_ReadyToShip } from '@/models/shipments-models';
import type { SubscriptionDto } from '@/models/subscription-models';
import { FreightCarrierKeys, PaymentMethodKeys, ShipmentMethodKeys } from './lookups';
import { baseDto, DATE_ONLY, DUE_DATE_ONLY, FIXED_DATE, nextId } from './sequence';

export function buildOrderLine(overrides: Partial<OrderLineDto> = {}): OrderLineDto {
  const id = overrides.id ?? nextId();
  return {
    ...baseDto('ordl', id),
    order_header_id: 1,
    product_id: 1,
    line_number: 1,
    line_description: 'Workstation build',
    quantity: 4,
    unit_price: 1800,
    shipped_qty: 0,
    product_name: 'Product',
    identifier1: 'SKU-1',
    attributes: [],
    ...overrides,
  } as OrderLineDto;
}

export function buildOrder(overrides: Partial<OrderHeaderDto> = {}): OrderHeaderDto {
  const id = overrides.id ?? nextId();
  return {
    ...baseDto('ordr', id),
    order_number: 50000 + id,
    customer_id: 1,
    customer_name: 'Customer',
    ship_to_address_id: 1,
    billing_address_id: 1,
    shipping_method: ShipmentMethodKeys.Carrier,
    shipping_method_name: 'Common Carrier',
    pay_method: PaymentMethodKeys.NetTerms,
    pay_method_name: 'Net Terms',
    order_type: 'sales',
    revision_number: 1,
    order_date: DATE_ONLY,
    required_date: DUE_DATE_ONLY,
    po_number: `PO-${id}`,
    price: 7200,
    tax: 0,
    shipping_cost: 0,
    is_complete: false,
    is_canceled: false,
    created_by_name: 'Ada Admin',
    order_lines: [],
    ar_lines: [],
    ...overrides,
  } as OrderHeaderDto;
}

export function buildShipmentLine(overrides: Partial<ShipmentLineDto> = {}): ShipmentLineDto {
  const id = overrides.id ?? nextId();
  return {
    ...baseDto('shpl', id),
    shipment_header_id: 1,
    order_line_id: 1,
    units_to_ship: 4,
    units_ordered: 4,
    units_shipped: 0,
    is_complete: false,
    is_canceled: false,
    line_description: 'Workstation build',
    line_number: 1,
    identifier1: 'SKU-1',
    ...overrides,
  } as ShipmentLineDto;
}

export function buildShipment(overrides: Partial<ShipmentHeaderDto> = {}): ShipmentHeaderDto {
  const id = overrides.id ?? nextId();
  return {
    ...baseDto('ship', id),
    order_header_id: 1,
    order_number: 50001,
    shipment_number: 70000 + id,
    address_id: 1,
    units_to_ship: 4,
    units_shipped: 0,
    is_complete: false,
    is_canceled: false,
    is_released: false,
    ship_via: ShipmentMethodKeys.Carrier,
    ship_attn: 'Receiving',
    freight_carrier: FreightCarrierKeys.Ups,
    freight_charge_amount: 45,
    tax: 0,
    customer_name: 'Customer',
    po_number: 'PO-1',
    order_date: DATE_ONLY,
    shipment_lines: [],
    ...overrides,
  } as ShipmentHeaderDto;
}

export function buildReadyToShip(overrides: Partial<vw_ReadyToShip> = {}): vw_ReadyToShip {
  return {
    order_number: 50001,
    order_guid: '',
    customer_name: 'Customer',
    product_name: 'Product',
    sold_quantity: 4,
    produced_quantity: 4,
    shipped_quantity: 0,
    ...overrides,
  };
}

export function buildSubscription(overrides: Partial<SubscriptionDto> = {}): SubscriptionDto {
  const id = overrides.id ?? nextId();
  return {
    ...baseDto('subs', id),
    customer_id: 1,
    order_header_id: 1,
    subscription_number: 80000 + id,
    quantity: 1,
    price: 99,
    tax: 0,
    cycle_days: 30,
    start_date: DATE_ONLY,
    next_date: DUE_DATE_ONLY,
    end_date: '2027-03-02',
    ...overrides,
  } as SubscriptionDto;
}
