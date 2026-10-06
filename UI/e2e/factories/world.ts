import type { KeyValueDto } from '@/models/key-value-models';
import {
  buildAPInvoice,
  buildAPInvoiceLine,
  buildARInvoice,
  buildARInvoiceLine,
  buildChartOfAccount,
  buildCreditMemo,
  buildCreditMemoLine,
  buildFinancialTransaction,
  buildJournalEntry,
  buildJournalEntryLine,
  buildOrderReadyForInvoicing,
  buildPartialInvoice,
} from './accounting';
import {
  buildAdminUser,
  buildCountry,
  buildDocumentUploadObject,
  buildDocumentUploadObjectTag,
  buildModuleInfo,
  buildReportCatalog,
  buildRole,
  buildSettings,
  buildState,
} from './admin';
import { buildActivity, buildAddress, buildContact, buildLead, buildOpportunity, buildOpportunityLine } from './crm';
import { buildCustomer } from './customer';
import { allLookups } from './lookups';
import { buildBom, buildInventoryCount, buildInventoryTransaction, buildProductionOrder, buildProductionOrderLine } from './production';
import { buildProduct } from './product';
import { buildPOReceive, buildPOReceiveLine, buildPurchaseOrder, buildPurchaseOrderLine } from './purchasing';
import { buildOrder, buildOrderLine, buildReadyToShip, buildShipment, buildShipmentLine, buildSubscription } from './sales';
import { buildVendor } from './vendor';

/**
 * A small, internally consistent dataset covering every module: one primary
 * record per entity (index 0) wired to the others by id, plus a second record
 * where lists benefit from more than one row. `mockWorld` serves it as the API.
 *
 * Build a fresh world per test (`buildWorld()`), then tweak records before
 * calling `mockWorld` to shape a scenario.
 */
export function buildWorld() {
  const address = buildAddress({ street_address1: '100 Fabrication Way' });
  const addresses = [address, buildAddress({ street_address1: '200 Warehouse Blvd', city: 'Round Rock' })];

  const customer = buildCustomer({ customer_name: 'Acme Components' });
  const customers = [customer, buildCustomer({ customer_name: 'Globex Systems' })];

  const contact = buildContact({ customer_id: customer.id, customer_name: customer.customer_name, first_name: 'Casey', last_name: 'Buyer' });
  const vendor = buildVendor({ vendor_name: 'Northwind Silicon' });
  const vendors = [vendor, buildVendor({ vendor_name: 'Contoso Memory' })];

  const product = buildProduct({ product_name: 'Ryzen 9 7950X', identifier1: 'CPU-7950X', vendor_id: vendor.id });
  const component = buildProduct({ product_name: 'DDR5 32GB Kit', identifier1: 'MEM-32G', vendor_id: vendor.id });
  const products = [product, component];
  const boms = [buildBom({ parent_product_id: product.id, product_id: component.id, product_name: component.product_name })];

  const orderLine = buildOrderLine({
    product_id: product.id,
    product_name: product.product_name,
    identifier1: product.identifier1 ?? undefined,
    line_description: 'Ryzen workstation build',
  });
  const order = buildOrder({
    customer_id: customer.id,
    customer_name: customer.customer_name,
    ship_to_address_id: address.id,
    billing_address_id: address.id,
    ship_to_address: address,
    order_lines: [orderLine],
  });
  orderLine.order_header_id = order.id;
  const orders = [order, buildOrder({ customer_id: customers[1].id, customer_name: customers[1].customer_name })];

  const shipmentLine = buildShipmentLine({ order_line_id: orderLine.id, line_description: orderLine.line_description });
  const shipment = buildShipment({
    order_header_id: order.id,
    order_number: order.order_number,
    customer_name: customer.customer_name,
    address_id: address.id,
    address,
    shipment_lines: [shipmentLine],
  });
  shipmentLine.shipment_header_id = shipment.id;
  const readyToShip = [
    buildReadyToShip({ order_number: order.order_number, order_guid: order.guid ?? '', customer_name: customer.customer_name, product_name: product.product_name ?? '' }),
  ];

  const subscription = buildSubscription({ customer_id: customer.id, order_header_id: order.id, customer: customer, order: order });

  const arLine = buildARInvoiceLine({ order_line_id: orderLine.id, product_id: product.id, line_description: orderLine.line_description });
  const arInvoice = buildARInvoice({
    customer_id: customer.id,
    customer_name: customer.customer_name,
    order_header_id: order.id,
    order_number: order.order_number,
    ar_invoice_lines: [arLine],
  });
  arLine.ar_invoice_header_id = arInvoice.id;
  const readyForInvoicing = [buildOrderReadyForInvoicing({ order_number: String(order.order_number), order_guid: order.guid ?? '', customer_name: customer.customer_name, customer_guid: customer.guid ?? '' })];
  const partialInvoices = [buildPartialInvoice({ order_number: String(order.order_number), order_guid: order.guid ?? '', customer_name: customer.customer_name, customer_guid: customer.guid ?? '' })];

  const poLine = buildPurchaseOrderLine({ product_id: component.id, product_name: component.product_name, identifier1: component.identifier1 ?? undefined });
  const purchaseOrder = buildPurchaseOrder({ vendor_id: vendor.id, vendor_name: vendor.vendor_name, purchase_order_lines: [poLine] });
  poLine.purchase_order_header_id = purchaseOrder.id;
  const purchaseOrders = [purchaseOrder, buildPurchaseOrder({ vendor_id: vendors[1].id, vendor_name: vendors[1].vendor_name })];

  const receiveLine = buildPOReceiveLine({ purchase_order_line_id: poLine.id, product_name: component.product_name });
  const poReceive = buildPOReceive({ purchase_order_id: purchaseOrder.id, po_number: purchaseOrder.po_number, received_lines: [receiveLine] });
  receiveLine.purchase_order_receive_header_id = poReceive.id;

  const apLine = buildAPInvoiceLine({ association_object_id: purchaseOrder.id, association_object_line_id: poLine.id });
  const apInvoice = buildAPInvoice({
    vendor_id: vendor.id,
    vendor_name: vendor.vendor_name,
    association_object_id: purchaseOrder.id,
    association_number: purchaseOrder.po_number,
    ap_invoice_lines: [apLine],
    po_lines: [poLine],
  });
  apLine.ap_invoice_header_id = apInvoice.id;

  const creditLine = buildCreditMemoLine({ ar_invoice_line_id: arLine.id, order_line_id: orderLine.id, product_id: product.id, product_name: product.product_name });
  const creditMemo = buildCreditMemo({
    customer_id: customer.id,
    customer_name: customer.customer_name,
    ar_invoice_header_id: arInvoice.id,
    ar_invoice_number: String(arInvoice.invoice_number),
    order_header_id: order.id,
    order_number: String(order.order_number),
    credit_memo_lines: [creditLine],
  });
  creditLine.credit_memo_header_id = creditMemo.id;

  const revenueAccount = buildChartOfAccount({ account_number: '4010', account_name: 'Sales Revenue' });
  const cashAccount = buildChartOfAccount({ account_number: '1010', account_name: 'Operating Cash', account_type: 1, account_type_name: 'Asset', normal_balance: 1, normal_balance_name: 'Debit' });
  const chartOfAccounts = [revenueAccount, cashAccount];

  const journalEntry = buildJournalEntry({
    lines: [
      buildJournalEntryLine({ line_number: 1, chart_of_account_id: cashAccount.id, account_number: '1010', account_name: 'Operating Cash', debit_amount: 100, credit_amount: 0 }),
      buildJournalEntryLine({ line_number: 2, chart_of_account_id: revenueAccount.id, account_number: '4010', account_name: 'Sales Revenue', debit_amount: 0, credit_amount: 100 }),
    ],
  });
  const financialTransactions = [
    buildFinancialTransaction({ chart_of_account_id: revenueAccount.id, account_number: '4010', account_name: 'Sales Revenue' }),
  ];

  const productionLine = buildProductionOrderLine({ order_line_id: orderLine.id, order_number: order.order_number, order_line: orderLine });
  const productionOrder = buildProductionOrder({ order_header_id: order.id, order_number: order.order_number, order_header: order, production_order_lines: [productionLine] });
  productionLine.production_order_header_id = productionOrder.id;

  const inventory = [
    buildInventoryCount({ product_id: product.id, product_name: product.product_name }),
    buildInventoryCount({ product_id: component.id, product_name: component.product_name }),
  ];
  const transactions = [buildInventoryTransaction({ product_id: component.id, product_name: component.product_name })];

  const opportunity = buildOpportunity({
    customer_id: customer.id,
    customer_name: customer.customer_name,
    contact_id: contact.id,
    contact_name: `${contact.first_name} ${contact.last_name}`,
    opportunity_lines: [buildOpportunityLine({ product_id: product.id, product_name: product.product_name })],
  });
  const lead = buildLead({ company_name: 'Initech Hardware' });
  const activity = buildActivity({ customer_id: customer.id, customer_name: customer.customer_name });

  const role = buildRole({ name: 'Warehouse' });
  const users = [buildAdminUser({ first_name: 'Uma', last_name: 'Operator', user_roles: [{ role_id: role.role_id, role_name: role.name, permissions: role.role_permissions }] })];

  return {
    addresses,
    customers,
    contacts: [contact],
    vendors,
    products,
    boms,
    orders,
    shipments: [shipment],
    readyToShip,
    subscriptions: [subscription],
    arInvoices: [arInvoice],
    readyForInvoicing,
    partialInvoices,
    purchaseOrders,
    poReceipts: [poReceive],
    apInvoices: [apInvoice],
    creditMemos: [creditMemo],
    chartOfAccounts,
    journalEntries: [journalEntry],
    financialTransactions,
    productionOrders: [productionOrder],
    inventory,
    transactions,
    opportunities: [opportunity],
    leads: [lead],
    activities: [activity],
    comments: [],
    users,
    roles: [role],
    moduleInfo: buildModuleInfo(),
    settings: buildSettings(),
    countries: [buildCountry()],
    states: [buildState()],
    uploadObjects: [buildDocumentUploadObject()],
    uploadObjectTags: [buildDocumentUploadObjectTag()],
    reportCatalog: buildReportCatalog(),
    lookups: allLookups() as Record<string, KeyValueDto[]>,
  };
}

export type World = ReturnType<typeof buildWorld>;
