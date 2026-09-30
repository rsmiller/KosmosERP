import type { KeyValueDto } from '@/models/key-value-models';

/**
 * Module ids the UI passes to GET /KeyValue/GetKeyValuesByModule, as used by the
 * comboboxes in components/ and a few pages. Keys and labels follow the seeder
 * (Tools/KosmosERP.Seeder/DatabaseSeeder.Sales.cs).
 *
 * Freight carriers come from ShipmentModule (KeyValueIds.FreightCarriers), which
 * creates them when the API starts.
 *
 * Note: the UI asks for production statuses under an id the backend doesn't
 * populate (it uses 97dd4b13…), so that dropdown is empty against real data
 * (BUG-003 in bugs.md). The mocks follow the UI.
 */
export const KeyValueModules = {
  PaymentTerm: '93bf02ec-5578-4aa4-a45b-f82962adf4bd',
  PaymentMethod: '83156a35-d140-4442-8fbf-699658bf65e9',
  ShipmentMethod: '9da95117-2792-44e5-996a-e91a244b0384',
  FreightCarrier: '2a2d1004-5283-40ef-96fd-8cc30c65cefa',
  ProductCategory: 'f6e28b05-265d-4416-b5fd-48399036493a',
  ProductionStatus: 'f157469e-5e5c-4a5b-b071-89a28b2a0310',
  OpportunityStage: '0c3959c3-15dc-44ab-8e2c-9b9e2773e65f',
  LeadStage: '9d624ee2-6433-49f0-bc6c-3e6978e2ac9c',
  TransactionType: '416786e0-47b3-440a-90da-b7036d72b1f7',
  GLAccount: 'eea9df53-1b36-41ea-94fa-31420315ff60',
} as const;

export type KeyValueModule = keyof typeof KeyValueModules;

/** Keys follow the seeder's format (Tools/KosmosERP.Seeder/DatabaseSeeder.Sales.cs). */
export const PaymentTermKeys = {
  Net30: 'payment_terms_net_30',
  Net60: 'payment_terms_net_60',
  DueOnReceipt: 'payment_terms_due_on_receipt',
} as const;

export function buildKeyValue(key: string, value: string, moduleId: string, id = 1): KeyValueDto {
  return { id, key, value, module_id: moduleId };
}

function list(module: KeyValueModule, entries: [key: string, value: string][]): KeyValueDto[] {
  return entries.map(([key, value], index) => buildKeyValue(key, value, KeyValueModules[module], index + 1));
}

export function paymentTerms(): KeyValueDto[] {
  return list('PaymentTerm', [
    [PaymentTermKeys.Net30, 'Net 30'],
    [PaymentTermKeys.Net60, 'Net 60'],
    [PaymentTermKeys.DueOnReceipt, 'Due on Receipt'],
  ]);
}

/** Module id the ProductCategoryCombobox loads (components/product-category-combobox.tsx). */
export const ProductCategoryModule = KeyValueModules.ProductCategory;

export const ProductCategoryKeys = {
  Processors: 'product_category_processors',
  Memory: 'product_category_memory',
} as const;

export function productCategories(): KeyValueDto[] {
  return list('ProductCategory', [
    [ProductCategoryKeys.Processors, 'Processors'],
    [ProductCategoryKeys.Memory, 'Memory'],
  ]);
}

export const PaymentMethodKeys = { NetTerms: 'pay_method_net_terms', CreditCard: 'pay_method_credit_card' } as const;
export const ShipmentMethodKeys = { Carrier: 'shipping_method_carrier', Pickup: 'shipping_method_pickup' } as const;
/** Keys created by ShipmentModule.SeedPermissions (Shared/KosmosERP.BusinessLayer/Modules/ShipmentModule.cs). */
export const FreightCarrierKeys = { Ups: 'freight_carrier_ups', FedEx: 'freight_carrier_fedex', Dhl: 'freight_carrier_dhl' } as const;
export const ProductionStatusKeys = {
  Submitted: 'production_order_status_submitted',
  Wip: 'production_order_status_wip',
  Complete: 'production_order_status_complete',
} as const;
export const OpportunityStageKeys = { Proposal: 'opporunity_stage_proposal', ClosedWon: 'opporunity_stage_closed_won' } as const;
export const LeadStageKeys = { New: 'lead_stage_new', Qualified: 'lead_stage_qualified' } as const;
export const GLAccountKeys = { Revenue: 'gl_account_sales_revenue', Purchases: 'gl_account_purchases' } as const;

/** Default options for every lookup module, keyed by module id. */
export function allLookups(): Record<string, KeyValueDto[]> {
  return {
    [KeyValueModules.PaymentTerm]: paymentTerms(),
    [KeyValueModules.ProductCategory]: productCategories(),
    [KeyValueModules.PaymentMethod]: list('PaymentMethod', [
      [PaymentMethodKeys.NetTerms, 'Net Terms'],
      [PaymentMethodKeys.CreditCard, 'Credit Card'],
    ]),
    [KeyValueModules.ShipmentMethod]: list('ShipmentMethod', [
      [ShipmentMethodKeys.Carrier, 'Common Carrier'],
      [ShipmentMethodKeys.Pickup, 'Customer Pickup'],
    ]),
    [KeyValueModules.FreightCarrier]: list('FreightCarrier', [
      [FreightCarrierKeys.Ups, 'UPS'],
      [FreightCarrierKeys.FedEx, 'FedEx'],
      [FreightCarrierKeys.Dhl, 'DHL'],
    ]),
    [KeyValueModules.ProductionStatus]: list('ProductionStatus', [
      [ProductionStatusKeys.Submitted, 'Submitted'],
      [ProductionStatusKeys.Wip, 'Work In Progress'],
      [ProductionStatusKeys.Complete, 'Complete'],
    ]),
    [KeyValueModules.OpportunityStage]: list('OpportunityStage', [
      [OpportunityStageKeys.Proposal, 'Proposal'],
      [OpportunityStageKeys.ClosedWon, 'Closed Won'],
    ]),
    [KeyValueModules.LeadStage]: list('LeadStage', [
      [LeadStageKeys.New, 'New'],
      [LeadStageKeys.Qualified, 'Qualified'],
    ]),
    [KeyValueModules.TransactionType]: list('TransactionType', [
      ['transaction_type_adjustment', 'Adjustment'],
      ['transaction_type_count', 'Cycle Count'],
    ]),
    [KeyValueModules.GLAccount]: list('GLAccount', [
      [GLAccountKeys.Revenue, '4010'],
      [GLAccountKeys.Purchases, '5010'],
    ]),
  };
}
