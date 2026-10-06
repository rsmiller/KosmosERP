import { test, expect } from '../../fixtures/test';
import { fail } from '../../mocks/envelope';
import { buildWorld, type World } from '../../factories/world';
import { PaymentMethodKeys, ShipmentMethodKeys } from '../../factories/lookups';
import { mockWorld } from '../../mocks/kits/world';
import { SalesOrderFormPage } from '../../pages/salesorders-pages';

/** How the address selector labels an address. */
const formatAddress = (a: World['addresses'][number]) => `${a.street_address1}, ${a.city}, ${a.state}, ${a.postal_code}`;

test.describe('new sales order', () => {
  let world: World;
  let form: SalesOrderFormPage;

  test.beforeEach(async ({ page, api }) => {
    world = mockWorld(api, buildWorld());
    form = new SalesOrderFormPage(page);
    await page.goto('/erp/salesorders/new');
    await expect(page.getByRole('heading', { name: 'New Sales Order' })).toBeVisible();
  });

  /** Fill every header field the form requires. */
  async function fillHeader() {
    await form.chooseOrderType('Release');
    await form.chooseCustomer(world.customers[0].customer_name!);
    await form.setRequiredDate('04/15/2026');
    await form.poNumber.fill('PO-7788');
    await form.choosePaymentMethod('Net Terms');
    await form.chooseShippingMethod('Common Carrier');
    await form.chooseShippingAddress(formatAddress(world.addresses[0]));
  }

  test('creates an order with a line and returns to the list', async ({ page, api }) => {
    const product = world.products[0];

    await fillHeader();
    await form.addOrderLine({ product: product.product_name!, description: 'Two workstation builds', quantity: 2 });
    await expect(form.line('Two workstation builds')).toBeVisible();
    await form.save.click();

    const request = await api.waitForRequest('POST', '/Order/CreateOrder');
    expect(request.body).toMatchObject({
      customer_id: world.customers[0].id,
      order_type: 'R',
      required_date: '2026-04-15',
      po_number: 'PO-7788',
      pay_method: PaymentMethodKeys.NetTerms,
      shipping_method: ShipmentMethodKeys.Carrier,
      ship_to_address_id: world.addresses[0].id,
    });
    const lines = (request.body as { order_lines: Record<string, unknown>[] }).order_lines;
    expect(lines).toHaveLength(1);
    expect(lines[0]).toMatchObject({ product_id: product.id, line_number: 1, line_description: 'Two workstation builds' });
    expect(Number(lines[0].quantity)).toBe(2);
    expect(Number(lines[0].unit_price)).toBe(product.list_price);

    await expect(page).toHaveURL(/\/erp\/salesorders\/?$/);
  });

  test('cannot be saved until type, customer, required date and ship-to are set', async () => {
    await expect(form.save).toBeDisabled();

    await form.chooseOrderType('Release');
    await form.chooseCustomer(world.customers[0].customer_name!);
    await form.setRequiredDate('04/15/2026');
    await expect(form.save).toBeDisabled();

    await form.chooseShippingAddress(formatAddress(world.addresses[0]));
    await expect(form.save).toBeEnabled();
  });

  test('a line can be removed before saving', async ({ api }) => {
    await fillHeader();
    await form.addOrderLine({ product: world.products[0].product_name!, description: 'Temporary line', quantity: 1 });

    await form.deleteLineButton('Temporary line').click();
    await expect(form.line('Temporary line')).toBeHidden();

    await form.save.click();
    const request = await api.waitForRequest('POST', '/Order/CreateOrder');
    expect((request.body as { order_lines: unknown[] }).order_lines).toEqual([]);
  });

  test('shows an error and stays on the form when the API rejects it', async ({ page, api }) => {
    api.on('POST', '/Order/CreateOrder', fail(-2, 'Customer on credit hold'));

    await fillHeader();
    await form.save.click();

    await expect(form.failedAlert).toBeVisible();
    await expect(page).toHaveURL(/\/erp\/salesorders\/new$/);
  });
});

test.describe('edit sales order', () => {
  let world: World;
  let form: SalesOrderFormPage;

  test.beforeEach(async ({ page, api }) => {
    world = mockWorld(api, buildWorld());
    form = new SalesOrderFormPage(page);
    await page.goto(`/erp/salesorders/edit/${world.orders[0].guid}`);
    await expect(form.poNumber).toHaveValue(world.orders[0].po_number!);
  });

  test('saves header changes with the existing lines', async ({ api }) => {
    const order = world.orders[0];
    const line = order.order_lines![0];

    await form.poNumber.fill('PO-CHANGED');
    await form.chooseShippingMethod('Customer Pickup');
    await expect(form.save).toBeEnabled();
    await form.save.click();

    const request = await api.waitForRequest('PUT', '/Order/UpdateOrder');
    expect(request.body).toMatchObject({
      id: order.id,
      customer_id: order.customer_id,
      po_number: 'PO-CHANGED',
      shipping_method: ShipmentMethodKeys.Pickup,
      order_lines: [expect.objectContaining({ id: line.id, product_id: line.product_id, quantity: line.quantity })],
    });
    await expect(form.savedAlert).toBeVisible();
  });

  test('adds a line through the dialog', async ({ api }) => {
    const component = world.products[1];

    await form.addOrderLine({ product: component.product_name!, description: 'Spare memory kit', quantity: 3 });

    const request = await api.waitForRequest('POST', '/Order/CreateOrderLine');
    expect(request.body).toMatchObject({ order_header_id: world.orders[0].id, product_id: component.id, line_description: 'Spare memory kit' });
    expect(Number((request.body as { quantity: unknown }).quantity)).toBe(3);
    await expect(form.line('Spare memory kit')).toBeVisible();
  });

  test('deletes a line', async ({ api }) => {
    const line = world.orders[0].order_lines![0];

    await form.deleteLineButton(line.line_description).click();

    const request = await api.waitForRequest('POST', '/Order/DeleteOrderLine');
    expect(request.body).toMatchObject({ id: line.id });
    await expect(form.line(line.line_description)).toBeHidden();
  });

  test('keeps the line when the API refuses to delete it', async ({ api }) => {
    const line = world.orders[0].order_lines![0];
    api.on('POST', '/Order/DeleteOrderLine', fail(-3, 'Line already shipped'));

    await form.deleteLineButton(line.line_description).click();

    await api.waitForRequest('POST', '/Order/DeleteOrderLine');
    await expect(form.line(line.line_description)).toBeVisible();
  });
});

test('sales order view page is read-only', async ({ page, api }) => {
  const world = mockWorld(api, buildWorld());
  const form = new SalesOrderFormPage(page);

  await page.goto(`/erp/salesorders/view/${world.orders[0].guid}`);

  await expect(form.customer).toBeDisabled();
  await expect(form.poNumber).toBeDisabled();
  await expect(form.addLine).toBeHidden();
  await expect(form.save).toBeHidden();
  // Nothing writes from a view page.
  expect(api.requests.filter((r) => r.method !== 'GET' && !/\/Find/i.test(r.path))).toEqual([]);
});

