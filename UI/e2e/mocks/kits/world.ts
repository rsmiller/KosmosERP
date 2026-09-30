import { buildWorld, type World } from '../../factories/world';
import type { MockApi } from '../mock-api';
import * as kits from './modules';

/**
 * Serve an entire World as the KosmosERP API: every module kit at once.
 * Mocking stays strict — an endpoint no kit knows about still fails the test.
 *
 *   const world = mockWorld(api);                  // default dataset
 *   const world = mockWorld(api, myTweakedWorld);  // a scenario
 *   api.on('POST', '/Order/CreateOrder', ...);     // then override specifics
 */
export function mockWorld(api: MockApi, world: World = buildWorld()): World {
  kits.mockLookups(api, world);
  kits.mockReferenceData(api, world);
  kits.mockRecordPanels(api, world);
  kits.mockCustomers(api, world);
  kits.mockContacts(api, world);
  kits.mockLeads(api, world);
  kits.mockOpportunities(api, world);
  kits.mockSalesOrders(api, world);
  kits.mockShipments(api, world);
  kits.mockSubscriptions(api, world);
  kits.mockVendors(api, world);
  kits.mockProducts(api, world);
  kits.mockPurchaseOrders(api, world);
  kits.mockPOReceipts(api, world);
  kits.mockProductionOrders(api, world);
  kits.mockInventory(api, world);
  kits.mockARInvoices(api, world);
  kits.mockAPInvoices(api, world);
  kits.mockCreditMemos(api, world);
  kits.mockChartOfAccounts(api, world);
  kits.mockJournalEntries(api, world);
  kits.mockFinancialTransactions(api, world);
  kits.mockDocuments(api, world);
  kits.mockAdmin(api, world);
  kits.mockReports(api, world);
  kits.mockGlobalSearch(api, world);
  return world;
}
