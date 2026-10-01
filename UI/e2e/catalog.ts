import type { Locator, Page } from '@playwright/test';
import { expect } from './fixtures/test';
import type { World } from './factories/world';

export type PageKind = 'home' | 'list' | 'view' | 'edit' | 'new' | 'admin';

/** An assertion about what a loaded page shows, given the World it was served. */
export type Check = (page: Page, world: World) => Promise<void>;

export interface CatalogPage {
  /** Stable id used in test titles, e.g. "salesorders/view". */
  id: string;
  kind: PageKind;
  /** URL for this page against a World (detail routes use the primary record's guid). */
  path: (world: World) => string;
  /** Locator that shows the page has loaded (usually its main heading). */
  ready: (page: Page, world: World) => Locator;
  /** What the page must show once loaded: rows, field values, button states. */
  checks?: Check[];
  /**
   * The record can be deleted from this page: an admin sees Delete Record (or
   * this label, when the page renames it). Every other page must not show
   * Delete Record (BUG-006: create pages showed one that crashed).
   */
  deletable?: true | string;
  /** Pages known to be broken: skipped with this reason until fixed (see bugs.md). */
  fixme?: string;
}

// ---- Check helpers ---------------------------------------------------------

type Value = string | number | ((world: World) => string | number | null | undefined);
const resolve = (value: Value, world: World) => String(typeof value === 'function' ? value(world) ?? '' : value);

const heading = (name: string | RegExp) => (page: Page) => page.getByRole('heading', { name }).first();

/** A grid row (ag-grid rows have role=row) containing the text. */
const row = (value: Value): Check => async (page, world) => {
  await expect(page.getByRole('row').filter({ hasText: resolve(value, world) }).first()).toBeVisible();
};

/** A form control, found by its accessible name, holding the value. */
const field = (name: string | RegExp, value: Value, role: 'textbox' | 'combobox' | 'spinbutton' = 'textbox'): Check =>
  async (page, world) => {
    await expect(page.getByRole(role, { name, exact: true }).first()).toHaveValue(resolve(value, world));
  };
const combo = (name: string | RegExp, value: Value) => field(name, value, 'combobox');

const text = (value: Value): Check => async (page, world) => {
  await expect(page.getByText(resolve(value, world), { exact: false }).first()).toBeVisible();
};

/**
 * A DateOnly value ("yyyy-MM-dd") as a page shows it: 'MM/dd/yyyy', 'MM-dd-yyyy',
 * or 'locale' (toLocaleDateString in en-US, e.g. 4/1/2026). Built from the string
 * itself, not through Date, so it can't share the off-by-one-day bug it checks for
 * (BUG-011; the browser runs in America/Chicago, see playwright.config.ts).
 */
const dateOnly = (
  // Some UI models type these fields as Date, but the API (and the factories) send strings.
  get: (world: World) => string | Date | null | undefined,
  style: 'MM/dd/yyyy' | 'MM-dd-yyyy' | 'locale' = 'MM/dd/yyyy',
) =>
  (world: World) => {
    const value = get(world);
    if (value instanceof Date) throw new Error('dateOnly expects the API\'s "yyyy-MM-dd" string, got a Date');
    const [y, m, d] = (value ?? '').slice(0, 10).split('-');
    if (style === 'locale') return `${Number(m)}/${Number(d)}/${y}`;
    const sep = style === 'MM-dd-yyyy' ? '-' : '/';
    return [m, d, y].join(sep);
  };

const buttonDisabled = (name: string): Check => async (page) => {
  await expect(page.getByRole('button', { name, exact: true })).toBeDisabled();
};
/** New-record forms start invalid, so Save starts disabled. */
const saveDisabled = buttonDisabled('Save Record');

const guid = (record: { guid?: string | null }) => record.guid ?? '';

// ---- The catalog -------------------------------------------------------------

/**
 * Every page under app/erp, served by mockWorld (mocks/kits/world.ts).
 * Deliberately excluded for now: payments/stripe/* (needs js.stripe.com
 * stubbed) and documents/download/[id] (triggers a file download).
 *
 * Checks read expected values from the World, so they follow the factories.
 * Assert DateOnly values with `dateOnly(...)`, never by parsing them with Date.
 */
export const catalog: CatalogPage[] = [
  {
    id: 'home', kind: 'home', path: () => '/erp', ready: (page) => page.getByRole('textbox', { name: 'Search...' }),
    checks: [
      async (page, w) => {
        await page.getByRole('textbox', { name: 'Search...' }).fill('Acme');
        await page.getByRole('textbox', { name: 'Search...' }).press('Enter');
        await expect(page.getByText(w.customers[0].customer_name!).first()).toBeVisible();
      },
    ],
  },

  // ---- Sales & CRM
  {
    id: 'customers', kind: 'list', path: () => '/erp/customers', ready: heading('Customers'),
    checks: [row((w) => w.customers[0].customer_name), row((w) => w.customers[1].customer_name)],
  },
  { id: 'customers/new', kind: 'new', path: () => '/erp/customers/new', ready: heading('New Customer'), checks: [saveDisabled, field('Customer Name', '')] },
  {
    id: 'customers/view', kind: 'view', path: (w) => `/erp/customers/view/${guid(w.customers[0])}`, ready: heading('View Customer'),
    checks: [
      field('Customer Name', (w) => w.customers[0].customer_name),
      field('Email', (w) => w.customers[0].general_email),
      field('Payment Terms', (w) => w.customers[0].payment_terms_name),
    ],
  },
  {
    id: 'customers/edit', kind: 'edit', deletable: true, path: (w) => `/erp/customers/edit/${guid(w.customers[0])}`, ready: heading('Edit Customer'),
    checks: [
      field('Customer Name', (w) => w.customers[0].customer_name),
      combo('Category', (w) => w.customers[0].category),
      combo('Payment Terms', (w) => w.customers[0].payment_terms_name),
    ],
  },

  {
    id: 'contacts', kind: 'list', path: () => '/erp/contacts', ready: heading('Contacts'),
    checks: [row((w) => `${w.contacts[0].first_name} ${w.contacts[0].last_name}`)],
  },
  { id: 'contacts/new', kind: 'new', path: () => '/erp/contacts/new', ready: heading('New Contact'), checks: [saveDisabled] },
  {
    id: 'contacts/view', kind: 'view', path: (w) => `/erp/contacts/view/${guid(w.contacts[0])}`, ready: heading('View Contact'),
    checks: [
      combo('Customer', (w) => w.contacts[0].customer_name),
      field('First Name', (w) => w.contacts[0].first_name),
      field('Email', (w) => w.contacts[0].email),
      row((w) => w.activities[0].subject),
    ],
  },
  {
    id: 'contacts/edit', kind: 'edit', deletable: true, path: (w) => `/erp/contacts/edit/${guid(w.contacts[0])}`, ready: heading('Edit Contact'),
    checks: [field('First Name', (w) => w.contacts[0].first_name), field('Last Name', (w) => w.contacts[0].last_name), combo('Customer', (w) => w.contacts[0].customer_name)],
  },

  { id: 'leads', kind: 'list', path: () => '/erp/leads', ready: heading('Leads'), checks: [row((w) => w.leads[0].company_name)] },
  { id: 'leads/new', kind: 'new', path: () => '/erp/leads/new', ready: heading('New Lead'), checks: [saveDisabled] },
  {
    id: 'leads/view', kind: 'view', path: (w) => `/erp/leads/view/${guid(w.leads[0])}`, ready: heading('View Lead'),
    checks: [
      field('Company Name', (w) => w.leads[0].company_name),
      field('Email', (w) => w.leads[0].email),
      field('Lead Stage', (w) => w.leads[0].stage_name),
    ],
  },
  {
    id: 'leads/edit', kind: 'edit', deletable: true, path: (w) => `/erp/leads/edit/${guid(w.leads[0])}`, ready: heading('Edit Lead'),
    checks: [
      field('Company Name', (w) => w.leads[0].company_name),
      combo('Lead Stage', (w) => w.leads[0].stage_name),
      combo('Country', (w) => w.countries[0].country_name),
      combo('State', (w) => w.states[0].state_name),
    ],
  },

  {
    id: 'opportunities', kind: 'list', path: () => '/erp/opportunities', ready: heading('Opportunities'),
    checks: [row((w) => w.opportunities[0].opportunity_name)],
  },
  { id: 'opportunities/new', kind: 'new', path: () => '/erp/opportunities/new', ready: heading('New Opportunity'), checks: [saveDisabled] },
  {
    id: 'opportunities/view', kind: 'view', path: (w) => `/erp/opportunities/view/${guid(w.opportunities[0])}`, ready: heading('View Opportunity'),
    checks: [
      field('Opportunity Name', (w) => w.opportunities[0].opportunity_name),
      field(/Expected Close/, dateOnly((w) => w.opportunities[0].expected_close)),
      combo('Customer', (w) => w.opportunities[0].customer_name),
      combo('Contact', (w) => w.opportunities[0].contact_name),
      row((w) => w.products[0].product_name),
    ],
  },
  {
    id: 'opportunities/edit', kind: 'edit', deletable: true, path: (w) => `/erp/opportunities/edit/${guid(w.opportunities[0])}`, ready: heading('Edit Opportunity'),
    checks: [
      field('Opportunity Name', (w) => w.opportunities[0].opportunity_name),
      combo('Opportunity Stage', (w) => w.opportunities[0].stage_name),
      combo('Customer', (w) => w.opportunities[0].customer_name),
      row((w) => w.products[0].product_name),
    ],
  },

  { id: 'activities', kind: 'list', path: () => '/erp/activities', ready: heading('Activities'), checks: [row((w) => w.activities[0].subject)] },

  {
    id: 'salesorders', kind: 'list', path: () => '/erp/salesorders', ready: heading('Sales Orders'),
    checks: [row((w) => w.orders[0].order_number), row((w) => w.orders[1].customer_name)],
  },
  { id: 'salesorders/new', kind: 'new', path: () => '/erp/salesorders/new', ready: heading('New Sales Order'), checks: [saveDisabled] },
  {
    id: 'salesorders/view', kind: 'view', path: (w) => `/erp/salesorders/view/${guid(w.orders[0])}`, ready: heading(/View Sales Order/),
    checks: [
      text((w) => `View Sales Order - ${w.orders[0].order_number}`),
      combo('Customer', (w) => w.orders[0].customer_name),
      field(/Required Date/, dateOnly((w) => w.orders[0].required_date)),
      field('PO Number', (w) => w.orders[0].po_number),
      combo('Payment Method', (w) => w.orders[0].pay_method_name),
      row((w) => w.orders[0].order_lines?.[0].line_description),
    ],
  },
  {
    id: 'salesorders/edit', kind: 'edit', deletable: true, path: (w) => `/erp/salesorders/edit/${guid(w.orders[0])}`, ready: heading(/Edit Sales Order/),
    checks: [
      combo('Customer', (w) => w.orders[0].customer_name),
      field('PO Number', (w) => w.orders[0].po_number),
      combo('Shipping Method', (w) => w.orders[0].shipping_method_name),
      row((w) => w.orders[0].order_lines?.[0].line_description),
    ],
  },

  {
    id: 'shipments', kind: 'list', path: () => '/erp/shipments', ready: heading('Shipments'),
    // Ship Via shows the label, not the stored key (BUG-012).
    checks: [row((w) => w.readyToShip[0].order_number), row((w) => w.shipments[0].shipment_number), row((w) => w.shipments[0].ship_via_name)],
  },
  {
    id: 'shipments/new', kind: 'new', path: (w) => `/erp/shipments/new/${guid(w.orders[0])}`,
    ready: (page, w) => page.getByText(String(w.orders[0].order_number), { exact: true }),
    checks: [combo('Shipping Method', (w) => w.orders[0].shipping_method_name), row((w) => w.orders[0].order_lines?.[0].line_description), saveDisabled],
  },
  {
    id: 'shipments/view', kind: 'view', path: (w) => `/erp/shipments/view/${guid(w.shipments[0])}`, ready: heading(/View Shipment/),
    checks: [text((w) => `View Shipment - ${w.shipments[0].shipment_number}`), row((w) => w.shipments[0].shipment_lines?.[0].line_description)],
  },
  {
    id: 'shipments/edit', kind: 'edit', deletable: true, path: (w) => `/erp/shipments/edit/${guid(w.shipments[0])}`, ready: heading(/Edit Shipment/),
    checks: [
      combo('Freight Carrier', 'UPS'),
      field('Ship Via', 'Common Carrier'),
      field('Ship Attention', (w) => w.shipments[0].ship_attn),
      row((w) => w.shipments[0].shipment_lines?.[0].line_description),
    ],
  },

  {
    id: 'subscriptions', kind: 'list', path: () => '/erp/subscriptions', ready: heading('Subscriptions'),
    checks: [row((w) => w.subscriptions[0].subscription_number), row(dateOnly((w) => w.subscriptions[0].next_date))],
  },
  {
    id: 'subscriptions/view', kind: 'view', path: (w) => `/erp/subscriptions/view/${guid(w.subscriptions[0])}`, ready: heading('Subscription Information'),
    checks: [row((w) => w.products[0].product_name)],
  },
  {
    id: 'subscriptions/edit', kind: 'edit', deletable: 'Cancel Subscription', path: (w) => `/erp/subscriptions/edit/${guid(w.subscriptions[0])}`, ready: heading('Subscription Information'),
    checks: [combo('Choose Type', '30 Days'), row((w) => w.products[0].product_name), text(dateOnly((w) => w.subscriptions[0].next_date, 'MM-dd-yyyy'))],
  },

  // ---- Purchasing, products & production
  {
    id: 'vendors', kind: 'list', path: () => '/erp/vendors', ready: heading('Vendors'),
    checks: [row((w) => w.vendors[0].vendor_name), row((w) => w.vendors[1].vendor_name)],
  },
  { id: 'vendors/new', kind: 'new', path: () => '/erp/vendors/new', ready: heading('New Vendor'), checks: [saveDisabled] },
  {
    id: 'vendors/view', kind: 'view', path: (w) => `/erp/vendors/view/${guid(w.vendors[0])}`, ready: heading('View Vendor'),
    checks: [
      field('Vendor Name', (w) => w.vendors[0].vendor_name),
      field('Category', (w) => w.vendors[0].category),
      field('Email', (w) => w.vendors[0].general_email),
    ],
  },
  {
    id: 'vendors/edit', kind: 'edit', deletable: true, path: (w) => `/erp/vendors/edit/${guid(w.vendors[0])}`, ready: heading('Edit Vendor'),
    checks: [field('Vendor Name', (w) => w.vendors[0].vendor_name), combo('Category', (w) => w.vendors[0].category)],
  },

  {
    id: 'products', kind: 'list', path: () => '/erp/products', ready: heading('Products'),
    checks: [row((w) => w.products[0].product_name), row((w) => w.products[1].identifier1)],
  },
  { id: 'products/new', kind: 'new', path: () => '/erp/products/new', ready: heading('New Product'), checks: [saveDisabled] },
  {
    id: 'products/view', kind: 'view', path: (w) => `/erp/products/view/${guid(w.products[0])}`, ready: heading('View Product'),
    checks: [
      field('Product Name', (w) => w.products[0].product_name),
      combo('Vendor', (w) => w.vendors[0].vendor_name),
      combo('Category', 'Processors'),
      field('Identifier 1', (w) => w.products[0].identifier1),
    ],
  },
  {
    id: 'products/edit', kind: 'edit', deletable: true, path: (w) => `/erp/products/edit/${guid(w.products[0])}`, ready: heading('Edit Product'),
    checks: [
      field('Product Name', (w) => w.products[0].product_name),
      combo('Vendor', (w) => w.vendors[0].vendor_name),
      combo('Category', 'Processors'),
    ],
  },

  {
    id: 'purchaseorders', kind: 'list', path: () => '/erp/purchaseorders', ready: heading('Purchase Orders'),
    checks: [row((w) => w.purchaseOrders[0].po_number), row((w) => w.purchaseOrders[1].vendor_name)],
  },
  { id: 'purchaseorders/new', kind: 'new', path: () => '/erp/purchaseorders/new', ready: heading('New Purchase Order'), checks: [saveDisabled] },
  {
    id: 'purchaseorders/view', kind: 'view', path: (w) => `/erp/purchaseorders/view/${guid(w.purchaseOrders[0])}`, ready: heading('View Purchase Order'),
    checks: [combo('Vendor', (w) => w.purchaseOrders[0].vendor_name), row((w) => w.products[1].product_name)],
  },
  {
    id: 'purchaseorders/edit', kind: 'edit', deletable: true, path: (w) => `/erp/purchaseorders/edit/${guid(w.purchaseOrders[0])}`, ready: heading('Edit Purchase Order'),
    checks: [combo('Vendor', (w) => w.purchaseOrders[0].vendor_name), row((w) => w.products[1].product_name)],
  },

  { id: 'poreceive', kind: 'list', path: () => '/erp/poreceive', ready: heading('Receive Purchase Orders'), checks: [row((w) => w.poReceipts[0].po_number)] },
  { id: 'poreceive/new', kind: 'new', path: () => '/erp/poreceive/new', ready: heading('Receive Purchase Order'), checks: [field('Enter PO Number', '')] },
  {
    id: 'poreceive/view', kind: 'view', path: (w) => `/erp/poreceive/view/${guid(w.poReceipts[0])}`, ready: heading('Receive Purchase Order'),
    checks: [row((w) => w.products[1].product_name)],
  },
  {
    id: 'poreceive/edit', kind: 'edit', path: (w) => `/erp/poreceive/edit/${guid(w.poReceipts[0])}`, ready: heading('Receive Purchase Order'),
    checks: [row((w) => w.products[1].product_name)],
  },

  {
    id: 'productionorders', kind: 'list', path: () => '/erp/productionorders', ready: heading('Production Orders'),
    checks: [row((w) => w.productionOrders[0].order_number), row(dateOnly((w) => w.productionOrders[0].planned_start_date, 'locale'))],
  },
  {
    id: 'productionorders/view', kind: 'view', path: (w) => `/erp/productionorders/view/${guid(w.productionOrders[0])}`, ready: heading('Order'),
    checks: [row((w) => w.products[0].product_name), text(dateOnly((w) => w.productionOrders[0].order_header?.order_date, 'MM-dd-yyyy'))],
  },
  {
    id: 'productionorders/edit', kind: 'edit', path: (w) => `/erp/productionorders/edit/${guid(w.productionOrders[0])}`, ready: heading(/Production Order/),
    checks: [text((w) => `Production Order - ${w.productionOrders[0].order_number}`), row((w) => w.products[0].product_name)],
  },

  {
    id: 'inventory', kind: 'list', path: () => '/erp/inventory', ready: heading('Inventory Counts'),
    checks: [row((w) => w.inventory[0].product_name), row((w) => w.inventory[1].product_name)],
  },
  { id: 'documents', kind: 'list', path: () => '/erp/documents', ready: heading('Documents'), checks: [field('Search Text', '')] },

  // ---- Accounting
  {
    id: 'ar', kind: 'list', path: () => '/erp/ar', ready: heading('Accounts Receivable'),
    checks: [row((w) => w.readyForInvoicing[0].order_number), row((w) => w.arInvoices[0].invoice_number)],
  },
  {
    id: 'ar/new-from-order', kind: 'new', path: (w) => `/erp/ar/new/${guid(w.orders[0])}`, ready: heading('Customer'),
    checks: [row((w) => w.orders[0].order_lines?.[0].line_description), buttonDisabled('Save and Print Invoice')],
  },
  {
    id: 'ar/view', kind: 'view', path: (w) => `/erp/ar/view/${guid(w.arInvoices[0])}`, ready: heading(/Invoice/),
    checks: [
      text((w) => `Invoice - ${w.arInvoices[0].invoice_number}`),
      row((w) => w.arInvoices[0].ar_invoice_lines?.[0].line_description),
      text(dateOnly((w) => w.arInvoices[0].invoice_date, 'MM-dd-yyyy')),
    ],
  },

  { id: 'ap', kind: 'list', path: () => '/erp/ap', ready: heading('Accounts Payable'), checks: [row((w) => w.apInvoices[0].invoice_number)] },
  { id: 'ap/new', kind: 'new', path: () => '/erp/ap/new', ready: heading('Invoice'), checks: [saveDisabled] },
  {
    id: 'ap/view', kind: 'view', path: (w) => `/erp/ap/view/${guid(w.apInvoices[0])}`, ready: heading(/AP Invoice/),
    checks: [text((w) => `AP Invoice - ${w.apInvoices[0].invoice_number}`), row((w) => w.apInvoices[0].ap_invoice_lines?.[0].description)],
  },
  {
    id: 'ap/edit', kind: 'edit', deletable: true, path: (w) => `/erp/ap/edit/${guid(w.apInvoices[0])}`, ready: heading('Invoice'),
    checks: [combo('Vendor Name', (w) => w.apInvoices[0].vendor_name)],
  },

  {
    id: 'creditmemos', kind: 'list', path: () => '/erp/creditmemos', ready: heading('Credit Memos'),
    checks: [row((w) => w.creditMemos[0].credit_memo_number)],
  },
  {
    id: 'creditmemos/new', kind: 'new', path: () => '/erp/creditmemos/new', ready: heading('New Credit Memo'),
    checks: [
      saveDisabled,
      async (page) => {
        await expect(page.getByRole('textbox', { name: /Credit Memo Date/ })).toBeVisible();
        await expect(page.getByRole('textbox', { name: /Due Date/ })).toBeVisible();
      },
    ],
  },
  {
    id: 'creditmemos/view', kind: 'view', path: (w) => `/erp/creditmemos/view/${guid(w.creditMemos[0])}`, ready: heading(/Credit Memo/),
    checks: [
      text((w) => `Credit Memo - ${w.creditMemos[0].credit_memo_number}`),
      field('Customer', (w) => w.creditMemos[0].customer_name),
      field('Memo', (w) => w.creditMemos[0].memo),
      row((w) => w.creditMemos[0].credit_memo_lines?.[0].description),
    ],
  },
  {
    id: 'creditmemos/edit', kind: 'edit', deletable: true, path: (w) => `/erp/creditmemos/edit/${guid(w.creditMemos[0])}`, ready: heading(/^Edit Credit Memo - /),
    checks: [
      field('Memo', (w) => w.creditMemos[0].memo),
      field('Credit Reason', (w) => w.creditMemos[0].credit_reason),
      row((w) => w.creditMemos[0].credit_memo_lines?.[0].description),
    ],
  },

  {
    id: 'chartofaccounts', kind: 'list', path: () => '/erp/chartofaccounts', ready: heading('Chart of Accounts'),
    checks: [row((w) => w.chartOfAccounts[0].account_name), row((w) => w.chartOfAccounts[1].account_name)],
  },
  { id: 'chartofaccounts/new', kind: 'new', path: () => '/erp/chartofaccounts/new', ready: heading('New Account'), checks: [saveDisabled, field('Account Number', '')] },
  {
    id: 'chartofaccounts/view', kind: 'view', path: (w) => `/erp/chartofaccounts/view/${guid(w.chartOfAccounts[0])}`, ready: heading('View Account'),
    checks: [
      field('Account Number', (w) => w.chartOfAccounts[0].account_number),
      field('Account Name', (w) => w.chartOfAccounts[0].account_name),
      field('Account Type', 'Revenue'),
    ],
  },
  {
    id: 'chartofaccounts/edit', kind: 'edit', deletable: true, path: (w) => `/erp/chartofaccounts/edit/${guid(w.chartOfAccounts[0])}`, ready: heading('Edit Account'),
    checks: [
      field('Account Name', (w) => w.chartOfAccounts[0].account_name),
      // Native selects: the value is the enum number (AccountType.Revenue = 4, NormalBalance.Credit = 2).
      combo('Account Type', '4'),
      combo('Normal Balance', '2'),
    ],
  },

  {
    id: 'journalentries', kind: 'list', path: () => '/erp/journalentries', ready: heading('Journal Entries'),
    checks: [row((w) => w.journalEntries[0].entry_number)],
  },
  { id: 'journalentries/new', kind: 'new', path: () => '/erp/journalentries/new', ready: heading('New Journal Entry'), checks: [saveDisabled] },
  {
    id: 'journalentries/view', kind: 'view', path: (w) => `/erp/journalentries/view/${guid(w.journalEntries[0])}`, ready: heading(/View Journal Entry/),
    checks: [
      field('Entry Number', (w) => w.journalEntries[0].entry_number),
      field('Description', (w) => w.journalEntries[0].description),
      row('Operating Cash'),
      row('Sales Revenue'),
    ],
  },
  {
    id: 'journalentries/edit', kind: 'edit', deletable: true, path: (w) => `/erp/journalentries/edit/${guid(w.journalEntries[0])}`, ready: heading(/Edit Journal Entry/),
    checks: [field('Description', (w) => w.journalEntries[0].description), row('Operating Cash')],
  },

  {
    id: 'financialtransactions', kind: 'list', path: () => '/erp/financialtransactions', ready: heading('Financial Transactions'),
    checks: [row((w) => w.financialTransactions[0].account_name)],
  },
  {
    id: 'financialtransactions/view', kind: 'view', path: (w) => `/erp/financialtransactions/view/${guid(w.financialTransactions[0])}`, ready: heading(/Transaction/),
    checks: [field('Account Name', (w) => w.financialTransactions[0].account_name), field('Credit Amount', '$7,200.00')],
  },

  {
    id: 'reports', kind: 'list', path: () => '/erp/reports', ready: heading('Reports'),
    checks: [text((w) => w.reportCatalog[0].reports[0].name)],
  },

  // ---- Administration
  {
    id: 'admin', kind: 'admin', path: () => '/erp/admin', ready: heading('Users'),
    checks: [text('Roles'), text('Object Lists'), text('Document Types')],
  },
  {
    id: 'admin/users', kind: 'admin', path: () => '/erp/admin/users', ready: heading('Users'),
    checks: [
      async (page) => { await expect(page.getByRole('button', { name: 'New User' })).toBeVisible(); },
      async (page, w) => {
        const user = w.users[0];
        await expect(page.getByRole('treeitem', { name: `${user.first_name} ${user.last_name}` })).toBeVisible();
      },
    ],
  },
  {
    id: 'admin/roles', kind: 'admin', path: () => '/erp/admin/roles', ready: (page) => page.getByRole('button', { name: 'Add Role' }),
    checks: [row((w) => w.roles[0].name)],
  },
  {
    id: 'admin/lists', kind: 'admin', path: () => '/erp/admin/lists', ready: (page) => page.getByRole('button', { name: 'Add New Entry' }),
    checks: [row('Net 30'), row('Processors')],
  },
  {
    id: 'admin/settings', kind: 'admin', path: () => '/erp/admin/settings', ready: heading('Settings'),
    checks: [field('Company Name', (w) => w.settings.company_name), field('AR Email', (w) => w.settings.company_ar_email)],
  },
  {
    id: 'admin/document-types', kind: 'admin', path: () => '/erp/admin/document-types', ready: (page) => page.getByRole('button', { name: 'Add New Entry' }),
    checks: [row((w) => w.uploadObjects[0].friendly_name)],
  },
  {
    id: 'admin/adjustments', kind: 'admin', path: () => '/erp/admin/adjustments',
    ready: (page) => page.getByRole('tab', { name: 'Add Adjustment' }),
    checks: [async (page) => { await expect(page.getByRole('tab', { name: 'Sales Orders' })).toBeVisible(); }],
  },
];
