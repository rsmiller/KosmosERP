import { test, expect } from '../../fixtures/test';
import { fail, ok } from '../../mocks/envelope';
import { buildWorld, type World } from '../../factories/world';
import { FreightCarrierKeys, ShipmentMethodKeys } from '../../factories/lookups';
import { buildShipmentLine } from '../../factories/sales';
import { users } from '../../fixtures/users';
import { mockWorld } from '../../mocks/kits/world';
import { ShipmentFormPage, ShipmentsListPage } from '../../pages/shipments-pages';

test('Ready To Ship opens a new shipment for that order', async ({ page, api }) => {
  const world = mockWorld(api, buildWorld());
  const order = world.orders[0];
  const list = new ShipmentsListPage(page);

  await page.goto('/erp/shipments');
  await list.newShipmentButton(order.order_number!).click();

  await expect(page).toHaveURL(`/erp/shipments/new/${order.guid}`);
});

test.describe('new shipment from an order', () => {
  let world: World;
  let form: ShipmentFormPage;

  test.beforeEach(async ({ page, api }) => {
    world = mockWorld(api, buildWorld());
    form = new ShipmentFormPage(page);
    await page.goto(`/erp/shipments/new/${world.orders[0].guid}`);
    await expect(form.line(world.orders[0].order_lines![0].line_description)).toBeVisible();
  });

  test('creates a shipment for the order lines and opens it', async ({ page, api }) => {
    const order = world.orders[0];
    const orderLine = order.order_lines![0];
    // The page redirects to the created shipment, so answer with one that exists.
    api.on('POST', '/Shipment/CreateShipmentHeader', (req) => ok({ ...world.shipments[0], ...(req.body as object) }));

    await expect(form.shippingMethod).toHaveValue(order.shipping_method_name!);
    await form.chooseFreightCarrier('UPS');
    await form.freightCharge.fill('45');
    await form.shipAttention.fill('Dock 4');
    await form.setUnitsToShip(orderLine.line_description, 3);
    await form.save.click();

    const request = await api.waitForRequest('POST', '/Shipment/CreateShipmentHeader');
    expect(request.body).toMatchObject({
      order_header_id: order.id,
      address_id: order.ship_to_address!.id,
      ship_via: ShipmentMethodKeys.Carrier,
      freight_carrier: FreightCarrierKeys.Ups,
      ship_attn: 'Dock 4',
      is_complete: false,
      is_canceled: false,
    });
    const body = request.body as { freight_charge_amount: unknown; shipment_lines: Record<string, unknown>[] };
    expect(Number(body.freight_charge_amount)).toBe(45);
    expect(body.shipment_lines).toHaveLength(1);
    expect(body.shipment_lines[0]).toMatchObject({ order_line_id: orderLine.id, units_ordered: orderLine.quantity });
    expect(Number(body.shipment_lines[0].units_shipped)).toBe(3);

    await expect(page).toHaveURL(`/erp/shipments/view/${world.shipments[0].guid}`);
  });

  test('cannot be saved until a freight carrier is chosen', async () => {
    await expect(form.save).toBeDisabled();

    // Freight Charge is marked required but defaults to "0", which passes the
    // page's truthiness check, so choosing a carrier is enough (see
    // "Needs verification" in bugs.md).
    await form.chooseFreightCarrier('FedEx');
    await expect(form.save).toBeEnabled();
  });

  test('shows an error when the API rejects the shipment', async ({ page, api }) => {
    api.on('POST', '/Shipment/CreateShipmentHeader', fail(-4, 'Nothing to ship'));

    await form.chooseFreightCarrier('UPS');
    await form.freightCharge.fill('45');
    await form.save.click();

    await expect(form.failedAlert).toBeVisible();
    await expect(page).toHaveURL(/\/erp\/shipments\/new\//);
  });
});

test.describe('edit shipment', () => {
  test('saves changes, then releases the shipment', async ({ page, api }) => {
    const world = mockWorld(api, buildWorld());
    const shipment = world.shipments[0];
    const form = new ShipmentFormPage(page);

    await page.goto(`/erp/shipments/edit/${shipment.guid}`);
    await expect(form.shipAttention).toHaveValue(shipment.ship_attn!);

    await form.shipAttention.fill('Receiving - Bay 2');
    await form.chooseFreightCarrier('FedEx');
    await form.save.click();

    const update = await api.waitForRequest('PUT', '/Shipment/UpdateShipmentHeader');
    expect(update.body).toMatchObject({
      id: shipment.id,
      ship_attn: 'Receiving - Bay 2',
      freight_carrier: FreightCarrierKeys.FedEx,
      ship_via: shipment.ship_via,
    });

    // A successful save offers to release the shipment.
    await expect(form.releaseDialog).toBeVisible();
    await form.releaseDialog.getByRole('button', { name: 'Release' }).click();

    await expect.poll(() => api.requestsTo('PUT', '/Shipment/UpdateShipmentHeader').length).toBe(2);
    expect(api.requestsTo('PUT', '/Shipment/UpdateShipmentHeader')[1].body).toMatchObject({ id: shipment.id, is_released: true });
    await expect(form.releaseDialog).toBeHidden();
  });

  test('declining release sends no second update', async ({ page, api }) => {
    const world = mockWorld(api, buildWorld());
    const form = new ShipmentFormPage(page);

    await page.goto(`/erp/shipments/edit/${world.shipments[0].guid}`);
    await form.shipAttention.fill('Front desk');
    await form.save.click();

    await expect(form.releaseDialog).toBeVisible();
    await form.releaseDialog.getByRole('button', { name: 'No' }).click();

    await expect(form.releaseDialog).toBeHidden();
    expect(api.requestsTo('PUT', '/Shipment/UpdateShipmentHeader')).toHaveLength(1);
  });

  test('a released shipment is read-only', async ({ page, api }) => {
    const world = buildWorld();
    world.shipments[0].is_released = true;
    mockWorld(api, world);
    const form = new ShipmentFormPage(page);

    await page.goto(`/erp/shipments/edit/${world.shipments[0].guid}`);
    await expect(form.shipAttention).toHaveValue(world.shipments[0].ship_attn!);

    await expect(form.shipAttention).toBeDisabled();
    await expect(form.freightCarrier).toBeDisabled();
    await expect(form.save).toBeDisabled();
  });
});

test.describe('shipment line Delete', () => {
  /** A world whose first shipment has a second line, so removing one leaves one. */
  function worldWithTwoLines(): { world: World; kept: string; removed: string } {
    const world = buildWorld();
    const shipment = world.shipments[0];
    const extra = buildShipmentLine({ shipment_header_id: shipment.id, line_description: 'Spare power supply', line_number: 2 });
    shipment.shipment_lines = [...shipment.shipment_lines!, extra];
    return { world, kept: shipment.shipment_lines[0].line_description!, removed: extra.line_description! };
  }

  test('removes the line after confirmation', async ({ page, api }) => {
    // Regression for BUG-017: the "Delete" button used to mark the line shipped.
    const { world, kept, removed } = worldWithTwoLines();
    mockWorld(api, world);
    const shipment = world.shipments[0];
    const removedLine = shipment.shipment_lines![1];
    const form = new ShipmentFormPage(page);
    await page.goto(`/erp/shipments/edit/${shipment.guid}`);

    await form.deleteLineButton(removed).click();
    await expect(form.removeLineDialog).toContainText(removed);
    await form.removeLineDialog.getByRole('button', { name: 'Remove' }).click();

    const request = await api.waitForRequest('POST', '/ShipmentLine/DeleteShipmentLine');
    expect(request.body).toEqual({ id: removedLine.id });
    await expect(form.line(removed)).toHaveCount(0);
    await expect(form.line(kept)).toBeVisible();
    expect(api.requestsTo('PUT', '/ShipmentLine/UpdateShipmentLine')).toHaveLength(0);

    // Saving afterwards no longer sends the removed line.
    await form.save.click();
    const update = await api.waitForRequest('PUT', '/Shipment/UpdateShipmentHeader');
    const lineIds = (update.body as { shipment_lines: { id: number }[] }).shipment_lines.map((l) => l.id);
    expect(lineIds).toEqual([shipment.shipment_lines![0].id]);
  });

  test('Cancel keeps the line', async ({ page, api }) => {
    const { world, removed } = worldWithTwoLines();
    mockWorld(api, world);
    const form = new ShipmentFormPage(page);
    await page.goto(`/erp/shipments/edit/${world.shipments[0].guid}`);

    await form.deleteLineButton(removed).click();
    await form.removeLineDialog.getByRole('button', { name: 'Cancel' }).click();

    await expect(form.removeLineDialog).toBeHidden();
    await expect(form.line(removed)).toBeVisible();
    expect(api.requestsTo('POST', '/ShipmentLine/DeleteShipmentLine')).toHaveLength(0);
  });

  test('keeps the line and shows an error when the API refuses', async ({ page, api }) => {
    const { world, removed } = worldWithTwoLines();
    mockWorld(api, world);
    api.on('POST', '/ShipmentLine/DeleteShipmentLine', fail(-4, 'Shipment header has been released and lines can not be deleted'));
    const form = new ShipmentFormPage(page);
    await page.goto(`/erp/shipments/edit/${world.shipments[0].guid}`);

    await form.deleteLineButton(removed).click();
    await form.removeLineDialog.getByRole('button', { name: 'Remove' }).click();

    await expect(form.failedAlert).toBeVisible();
    await expect(form.removeLineDialog).toBeHidden();
    await expect(form.line(removed)).toBeVisible();
  });

  test('is disabled on a released shipment', async ({ page, api }) => {
    const world = buildWorld();
    world.shipments[0].is_released = true;
    mockWorld(api, world);
    const form = new ShipmentFormPage(page);

    await page.goto(`/erp/shipments/edit/${world.shipments[0].guid}`);

    await expect(form.deleteLineButton(world.shipments[0].shipment_lines![0].line_description!)).toBeDisabled();
  });

  test.describe('without delete permission', () => {
    test.use({ storageState: users.readOnly.storageState });

    test('is disabled', async ({ page, api }) => {
      const world = mockWorld(api, buildWorld());
      const form = new ShipmentFormPage(page);

      await page.goto(`/erp/shipments/edit/${world.shipments[0].guid}`);

      await expect(form.deleteLineButton(world.shipments[0].shipment_lines![0].line_description!)).toBeDisabled();
    });
  });
});
