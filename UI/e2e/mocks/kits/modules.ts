import { KeyValueModules } from '../../factories/lookups';
import type { World } from '../../factories/world';
import { fail, ok, paged } from '../envelope';
import type { MockApi } from '../mock-api';
import { mockCrud, mockEcho, standardEndpoints } from './crud';

/*
 * One kit per KosmosERP module. Each serves that module's slice of a World
 * (factories/world.ts) behind the endpoints its services call
 * (services/*-service.tsx). `mockWorld` installs them all; tests can install a
 * single kit, or override any endpoint afterwards with api.on(...).
 */

// ---- Shared reference data -------------------------------------------------

/** GET /KeyValue/GetKeyValuesByModule answers by ?module_id=; unknown ids fail loudly. */
export function mockLookups(api: MockApi, world: World) {
  api.on('GET', '/KeyValue/GetKeyValuesByModule', (req) => {
    const moduleId = req.url.searchParams.get('module_id') ?? '';
    const values = world.lookups[moduleId];
    return values ? ok(values) : fail(-404, `No e2e lookup data for module_id ${moduleId}`);
  });
  api.on('POST', '/KeyValue/FindKeyValue', () => paged(Object.values(world.lookups).flat()));
  api.on('GET', '/KeyValue/GetModuleInfo', () => ok(world.moduleInfo));
  mockEcho(api, 'POST', '/KeyValue/CreateKeyValue');
  mockEcho(api, 'PUT', '/KeyValue/UpdateKeyValue');
  mockEcho(api, 'POST', '/KeyValue/DeleteKeyValue');
}

export function mockReferenceData(api: MockApi, world: World) {
  mockCrud(api, standardEndpoints('Address'), world.addresses);
  mockCrud(api, standardEndpoints('Country'), world.countries);
  api.on('GET', '/Country/GetCountryByISOAsync', (req) => {
    const iso3 = req.url.searchParams.get('iso3');
    return ok(world.countries.find((c) => c.iso3 === iso3) ?? world.countries[0]);
  });
  mockCrud(api, standardEndpoints('State'), world.states);
  api.on('GET', '/State/GetStateByISOAsync', (req) => {
    const iso2 = req.url.searchParams.get('iso2');
    return ok(world.states.find((s) => s.iso2 === iso2) ?? world.states[0]);
  });
}

/** The tabs on view/edit pages: activities, comments, documents. */
export function mockRecordPanels(api: MockApi, world: World) {
  mockCrud(api, standardEndpoints('Activity'), world.activities);
  mockCrud(api, standardEndpoints('Comment'), world.comments);
  api.on('POST', '/Document/FindDocument', () => paged([]));
}

// ---- Sales & CRM -----------------------------------------------------------

export function mockCustomers(api: MockApi, world: World) {
  mockCrud(api, standardEndpoints('Customer'), world.customers);
  // These three return bare arrays, not the usual envelope.
  api.on('GET', '/Customer/GetPaymentTerms', () => world.lookups[KeyValueModules.PaymentTerm] ?? []);
  api.on('GET', '/Customer/GetShippingMethods', () => world.lookups[KeyValueModules.ShipmentMethod] ?? []);
  api.on('GET', '/Customer/GetPayMethods', () => world.lookups[KeyValueModules.PaymentMethod] ?? []);
}

export function mockContacts(api: MockApi, world: World) {
  mockCrud(api, standardEndpoints('Contact'), world.contacts);
}

export function mockLeads(api: MockApi, world: World) {
  mockCrud(api, standardEndpoints('Lead'), world.leads);
}

export function mockOpportunities(api: MockApi, world: World) {
  mockCrud(api, standardEndpoints('Opportunity'), world.opportunities);
  mockEcho(api, 'POST', '/Opportunity/CreateOpportunityLine');
  mockEcho(api, 'PUT', '/Opportunity/UpdateOpportunityLine');
  mockEcho(api, 'POST', '/Opportunity/DeleteOpportunityLine');
}

export function mockSalesOrders(api: MockApi, world: World) {
  mockCrud(api, standardEndpoints('Order'), world.orders);
  mockEcho(api, 'POST', '/Order/CreateOrderLine');
  mockEcho(api, 'PUT', '/OrderLine/UpdateOrderLine');
  mockEcho(api, 'POST', '/Order/DeleteOrderLine');
}

export function mockShipments(api: MockApi, world: World) {
  mockCrud(api, standardEndpoints('Shipment', 'ShipmentHeader'), world.shipments);
  api.on('GET', '/Shipment/GetReadyToShip', () => ok(world.readyToShip));
  mockEcho(api, 'POST', '/ShipmentLine/CreateShipmentLine');
  mockEcho(api, 'PUT', '/ShipmentLine/UpdateShipmentLine');
  mockEcho(api, 'POST', '/ShipmentLine/DeleteShipmentLine');
}

export function mockSubscriptions(api: MockApi, world: World) {
  mockCrud(api, standardEndpoints('Subscription'), world.subscriptions);
}

// ---- Purchasing, products & production ------------------------------------

export function mockVendors(api: MockApi, world: World) {
  mockCrud(api, standardEndpoints('Vendor'), world.vendors);
}

export function mockProducts(api: MockApi, world: World) {
  mockCrud(api, standardEndpoints('Product'), world.products);
  mockCrud(api, standardEndpoints('BOM'), world.boms);
}

export function mockPurchaseOrders(api: MockApi, world: World) {
  mockCrud(api, standardEndpoints('PurchaseOrder', 'PurchaseOrderHeader'), world.purchaseOrders);
  api.on('GET', '/PurchaseOrder/GetByPONumber', (req) => {
    const poNumber = Number(req.url.searchParams.get('po_number'));
    const po = world.purchaseOrders.find((p) => p.po_number === poNumber);
    return po ? ok(po) : fail(-404, `No purchase order ${poNumber}`);
  });
  mockEcho(api, 'POST', '/PurchaseOrder/CreatePurchaseOrderLine');
  mockEcho(api, 'PUT', '/PurchaseOrder/UpdatePurchaseOrderLine');
  mockEcho(api, 'POST', '/PurchaseOrder/DeletePurchaseOrderLine');
}

export function mockPOReceipts(api: MockApi, world: World) {
  mockCrud(
    api,
    {
      find: '/PurchaseOrderReceive/FindPurchaseOrderReceiveHeader',
      get: '/PurchaseOrderReceive/GetPurchaseOrderReceive',
      getByGuid: '/PurchaseOrderReceive/GetPurchaseOrderReceiveHeaderByGuid',
      create: '/PurchaseOrderReceive/CreatePurchaseOrderReceiveHeader',
      update: '/PurchaseOrderReceive/UpdatePurchaseOrderReceiveHeader',
      delete: '/PurchaseOrderReceive/DeletePurchaseOrderReceive',
    },
    world.poReceipts,
  );
  api.on('GET', '/PurchaseOrderReceive/GetDtoByPOId', (req) => {
    const poId = Number(req.url.searchParams.get('purchase_order_id'));
    return ok(world.poReceipts.find((r) => r.purchase_order_id === poId) ?? world.poReceipts[0]);
  });
}

export function mockProductionOrders(api: MockApi, world: World) {
  mockCrud(api, standardEndpoints('ProductionOrder'), world.productionOrders);
  mockEcho(api, 'POST', '/ProductionOrder/CreateProductionOrderLine');
  mockEcho(api, 'PUT', '/ProductionOrder/UpdateProductionOrderLine');
  mockEcho(api, 'POST', '/ProductionOrder/DeleteProductionOrderLine');
}

export function mockInventory(api: MockApi, world: World) {
  api.on('GET', '/Inventory/GetCounts', () => ok(world.inventory));
  mockCrud(api, standardEndpoints('Transaction'), world.transactions);
}

// ---- Accounting ------------------------------------------------------------

export function mockARInvoices(api: MockApi, world: World) {
  mockCrud(api, standardEndpoints('ARInvoice'), world.arInvoices);
  api.on('GET', '/ARInvoice/GetOrdersReadyForInvoicing', () => ok(world.readyForInvoicing));
  api.on('GET', '/ARInvoice/GetPartialInvoices', () => ok(world.partialInvoices));
  mockEcho(api, 'POST', '/ARInvoice/CreateARInvoiceLine');
  mockEcho(api, 'PUT', '/ARInvoice/UpdateARInvoiceLine');
  mockEcho(api, 'POST', '/ARInvoice/DeleteARInvoiceLine');
}

export function mockAPInvoices(api: MockApi, world: World) {
  mockCrud(api, standardEndpoints('APInvoice'), world.apInvoices);
  mockEcho(api, 'POST', '/APInvoice/CreateAPInvoiceLine');
  mockEcho(api, 'PUT', '/APInvoice/UpdateAPInvoiceLine');
  mockEcho(api, 'POST', '/APInvoice/DeleteAPInvoiceLine');
}

export function mockCreditMemos(api: MockApi, world: World) {
  mockCrud(api, standardEndpoints('CreditMemo'), world.creditMemos);
  mockEcho(api, 'POST', '/CreditMemo/CreateCreditMemoLine');
  mockEcho(api, 'PUT', '/CreditMemo/UpdateCreditMemoLine');
  mockEcho(api, 'POST', '/CreditMemo/DeleteCreditMemoLine');
}

export function mockChartOfAccounts(api: MockApi, world: World) {
  mockCrud(api, standardEndpoints('ChartOfAccount'), world.chartOfAccounts);
  api.on('GET', '/ChartOfAccount/GetChildAccounts', () => ok([]));
  api.on('GET', '/ChartOfAccount/GetAccountsByType', (req) => {
    const type = Number(req.url.searchParams.get('accountType'));
    return ok(world.chartOfAccounts.filter((a) => Number(a.account_type) === type));
  });
}

export function mockJournalEntries(api: MockApi, world: World) {
  mockCrud(api, standardEndpoints('JournalEntry'), world.journalEntries);
  api.on('GET', '/JournalEntry/GetUnpostedEntries', () => ok(world.journalEntries.filter((j) => !j.is_posted)));
  mockEcho(api, 'POST', '/JournalEntry/CreateJournalEntryLine');
  mockEcho(api, 'PUT', '/JournalEntry/UpdateJournalEntryLine');
  mockEcho(api, 'POST', '/JournalEntry/DeleteJournalEntryLine');
  mockEcho(api, 'POST', '/JournalEntry/PostJournalEntry');
  mockEcho(api, 'POST', '/JournalEntry/ReverseJournalEntry');
}

export function mockFinancialTransactions(api: MockApi, world: World) {
  mockCrud(api, standardEndpoints('FinancialTransaction'), world.financialTransactions);
}

// ---- Documents, admin, reports, search ------------------------------------

export function mockDocuments(api: MockApi, world: World) {
  api.on('POST', '/Document/FindDocument', () => paged([]));
  api.on('POST', '/Document/SearchDocuments', () => ok([]));
  api.on('GET', '/Document/GetObjectCategories', () => ok([]));
  api.on('GET', '/Document/GetUploadObjects', () => ok(world.uploadObjects));
  api.on('GET', '/Document/GetDocumentObjectTags', () => ok(world.uploadObjectTags));
}

export function mockAdmin(api: MockApi, world: World) {
  api.on('GET', '/User/GetUsers', () => ok(world.users));
  api.on('GET', '/User/GetRoles', () => ok(world.roles));
  api.on('GET', '/User/GetRolePermissions', () => ok(world.roles.flatMap((r) => r.role_permissions ?? [])));
  mockEcho(api, 'POST', '/User/CreateUser');
  mockEcho(api, 'PUT', '/User/UpdateUser');
  mockEcho(api, 'POST', '/User/CreateRole');
  mockEcho(api, 'POST', '/User/AssignUserRole');
  api.on('GET', '/Settings/GetBaseSettings', () => ok(world.settings));
  api.on('PUT', '/Settings/UpdateSettings', (req) => ok({ ...world.settings, ...(req.body as object) }));
}

export function mockReports(api: MockApi, world: World) {
  api.on('GET', '/Reports/Catalog', () => ok(world.reportCatalog));
}

export function mockGlobalSearch(api: MockApi, world: World) {
  api.on('POST', '/GlobalSearch/Search', () =>
    ok({
      sales_order: world.orders,
      purchase_orders: world.purchaseOrders,
      customers: world.customers,
      vendors: world.vendors,
      contacts: world.contacts,
      opportunities: world.opportunities,
      leads: world.leads,
      documents: [],
    }),
  );
}
