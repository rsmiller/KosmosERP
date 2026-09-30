import type { Page } from '@playwright/test';
import { test, expect } from '../../fixtures/test';
import type { MockApi } from '../../mocks/mock-api';
import { fail, ok } from '../../mocks/envelope';
import { buildWorld, type World } from '../../factories/world';
import { PaymentTermKeys } from '../../factories/lookups';
import { mockWorld } from '../../mocks/kits/world';
import { ARInvoiceFromOrderPage, ARListPage } from '../../pages/ar-pages';

/** The invoice and due dates come from the browser clock. */
const TODAY = new Date('2026-03-10T10:00:00');

/** Saving opens the printable invoice (server-rendered PDF) in a popup; stub it. */
async function stubPrintPopup(page: Page) {
  await page.context().route('**/docs/api/**', (route) =>
    route.fulfill({ status: 200, contentType: 'text/html', body: '<p>invoice pdf</p>' }),
  );
}

test('Create Invoice on the AR list opens the invoice form for that order', async ({ page, api }) => {
  const world = mockWorld(api, buildWorld());
  const order = world.orders[0];

  await page.goto('/erp/ar');
  await new ARListPage(page).createInvoiceButton(order.order_number!).click();

  await expect(page).toHaveURL(`/erp/ar/new/${order.guid}`);
});

test.describe('invoice an order', () => {
  let world: World;
  let form: ARInvoiceFromOrderPage;

  test.beforeEach(async ({ page, api }) => {
    await page.clock.setFixedTime(TODAY);
    world = mockWorld(api, buildWorld());
    form = new ARInvoiceFromOrderPage(page);
    await page.goto(`/erp/ar/new/${world.orders[0].guid}`);
    await expect(form.line(world.orders[0].order_lines![0].line_description)).toBeVisible();
  });

  test('creates the invoice, opens it for printing and shows it', async ({ page, api }) => {
    const order = world.orders[0];
    const line = order.order_lines![0];
    const customer = world.customers[0];
    await stubPrintPopup(page);
    // The page opens the created invoice, so answer with one that exists.
    api.on('POST', '/ARInvoice/CreateARInvoice', (req) => ok({ ...world.arInvoices[0], ...(req.body as object) }));

    await form.setUnitsToInvoice(line.line_description, 2);
    const popup = page.waitForEvent('popup');
    await form.save.click();

    const request = await api.waitForRequest('POST', '/ARInvoice/CreateARInvoice');
    expect(request.body).toMatchObject({
      customer_id: customer.id,
      order_header_id: order.id,
      payment_terms: customer.payment_terms,
      invoice_date: '2026-03-10',
      invoice_due_date: '2026-04-09',
    });
    const lines = (request.body as { ar_invoice_lines: Record<string, unknown>[] }).ar_invoice_lines;
    expect(lines).toHaveLength(1);
    expect(lines[0]).toMatchObject({ order_line_id: line.id, product_id: line.product_id, unit_price: line.unit_price });
    expect(Number(lines[0].invoice_qty)).toBe(2);
    expect(Number(lines[0].total_price)).toBe(2 * line.unit_price);

    expect((await popup).url()).toContain(encodeURIComponent(`/docs/ar/${world.arInvoices[0].guid}`));
    await expect(page).toHaveURL(`/erp/ar/view/${world.arInvoices[0].guid}`);
  });

  test('cannot be saved until a line has units to invoice', async () => {
    await expect(form.save).toBeDisabled();

    await form.setUnitsToInvoice(world.orders[0].order_lines![0].line_description, 1);
    await expect(form.save).toBeEnabled();
  });

  test('stays on the form when the API rejects the invoice', async ({ page, api }) => {
    api.on('POST', '/ARInvoice/CreateARInvoice', fail(-5, 'Order already invoiced'));

    await form.setUnitsToInvoice(world.orders[0].order_lines![0].line_description, 1);
    await form.save.click();

    await api.waitForRequest('POST', '/ARInvoice/CreateARInvoice');
    await expect(page).toHaveURL(`/erp/ar/new/${world.orders[0].guid}`);
  });
});

test.describe('invoice due date', () => {
  /** Invoice the first order line and return the create request's body. */
  async function invoiceFirstLine(page: Page, api: MockApi, world: World) {
    await page.clock.setFixedTime(TODAY);
    mockWorld(api, world);
    await stubPrintPopup(page);
    api.on('POST', '/ARInvoice/CreateARInvoice', (req) => ok({ ...world.arInvoices[0], ...(req.body as object) }));
    const form = new ARInvoiceFromOrderPage(page);
    const line = world.orders[0].order_lines![0];

    await page.goto(`/erp/ar/new/${world.orders[0].guid}`);
    await form.setUnitsToInvoice(line.line_description, 1);
    await form.save.click();
    return (await api.waitForRequest('POST', '/ARInvoice/CreateARInvoice')).body as { invoice_date: string; invoice_due_date: string };
  }

  test('uses the payment term\'s days, whatever the term is called', async ({ page, api }) => {
    // Regression for BUG-019: the page used to parse days from the name ("NET30").
    const world = buildWorld();
    world.customers[0].payment_terms = PaymentTermKeys.Net60;
    world.customers[0].payment_terms_name = 'Sixty days net';

    const body = await invoiceFirstLine(page, api, world);

    expect(body).toMatchObject({ invoice_date: '2026-03-10', invoice_due_date: '2026-05-09' });
  });

  test('a term without days is due on the invoice date', async ({ page, api }) => {
    const world = buildWorld();
    world.customers[0].payment_terms = PaymentTermKeys.DueOnReceipt;
    world.customers[0].payment_terms_name = 'Due on Receipt';

    const body = await invoiceFirstLine(page, api, world);

    expect(body).toMatchObject({ invoice_date: '2026-03-10', invoice_due_date: '2026-03-10' });
  });
});
