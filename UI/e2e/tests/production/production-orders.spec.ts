import type { Page } from '@playwright/test';
import { test, expect } from '../../fixtures/test';
import { buildWorld } from '../../factories/world';
import { KeyValueModules, ProductionStatusKeys } from '../../factories/lookups';
import { mockWorld } from '../../mocks/kits/world';
import { fail, ok } from '../../mocks/envelope';
import { chooseOption } from '../../pages/controls';

/** Change a production line's status with the grid's status editor. */
async function changeStatus(page: Page, productName: string, status: string) {
  const row = page.getByRole('row').filter({ hasText: productName });
  await row.locator('[col-id="status"]').dblclick();
  await chooseOption(page, row.getByRole('combobox'), status);
  await row.getByRole('combobox').press('Enter');
  await expect(row).toContainText(status);
}

test.describe('production order', () => {
  test('the list shows each order with its status', async ({ page, api }) => {
    const world = mockWorld(api, buildWorld());
    const order = world.productionOrders[0];

    await page.goto('/erp/productionorders');

    await expect(page.getByRole('row').filter({ hasText: String(order.order_number) })).toContainText('Submitted');
  });

  test.describe('edit', () => {
    const save = (page: Page) => page.getByRole('button', { name: 'Save Record' });

    test('a line status change is saved', async ({ page, api }) => {
      // Regression for BUG-024: the page never called the API.
      const world = mockWorld(api, buildWorld());
      const order = world.productionOrders[0];
      await page.goto(`/erp/productionorders/edit/${order.guid}`);
      await expect(save(page)).toBeDisabled();

      await changeStatus(page, world.products[0].product_name!, 'Work In Progress');
      await save(page).click();

      const request = await api.waitForRequest('PUT', '/ProductionOrder/UpdateProductionOrderLine');
      expect(request.body).toEqual({ id: order.production_order_lines![0].id, status: ProductionStatusKeys.Wip });
      await expect(page.getByText('Record saved!')).toBeVisible();
      // Nothing left to save.
      await expect(save(page)).toBeDisabled();
    });

    test('a line can be marked Ready To Ship, which the shipments list needs', async ({ page, api }) => {
      // Regression for BUG-003: the status editor read a retired lookup list, so
      // "Ready To Ship" (the status GetReadyToShip filters on) was never offered.
      const world = mockWorld(api, buildWorld());
      const order = world.productionOrders[0];
      await page.goto(`/erp/productionorders/edit/${order.guid}`);

      await changeStatus(page, world.products[0].product_name!, 'Ready To Ship');
      await save(page).click();

      const request = await api.waitForRequest('PUT', '/ProductionOrder/UpdateProductionOrderLine');
      expect(request.body).toEqual({ id: order.production_order_lines![0].id, status: ProductionStatusKeys.ReadyToShip });
      const lookup = api.requestsTo('GET', '/KeyValue/GetKeyValuesByModule').map((r) => r.url.searchParams.get('module_id'));
      expect(new Set(lookup)).toEqual(new Set([KeyValueModules.ProductionStatus]));
    });

    test('a failed save says so and can be retried', async ({ page, api }) => {
      const world = mockWorld(api, buildWorld());
      const order = world.productionOrders[0];
      let attempts = 0;
      api.on('PUT', '/ProductionOrder/UpdateProductionOrderLine', (req) =>
        ++attempts === 1 ? fail(-5, 'Database unavailable') : ok(req.body),
      );
      await page.goto(`/erp/productionorders/edit/${order.guid}`);

      await changeStatus(page, world.products[0].product_name!, 'Work In Progress');
      await save(page).click();

      await expect(page.getByText('Record could not be saved!')).toBeVisible();
      await expect(save(page)).toBeEnabled();

      await save(page).click();

      await expect(page.getByText('Record saved!')).toBeVisible();
      await expect(page.getByText('Record could not be saved!')).toBeHidden();
      expect(api.requestsTo('PUT', '/ProductionOrder/UpdateProductionOrderLine')).toHaveLength(2);
    });

    test('a completed order cannot be saved', async ({ page, api }) => {
      const world = buildWorld();
      world.productionOrders[0].is_complete = true;
      mockWorld(api, world);

      await page.goto(`/erp/productionorders/edit/${world.productionOrders[0].guid}`);

      await expect(page.getByRole('row').filter({ hasText: world.products[0].product_name! })).toBeVisible();
      await expect(save(page)).toBeDisabled();
    });
  });
});
