import { test, expect } from '../../fixtures/test';
import { buildWorld } from '../../factories/world';
import { mockWorld } from '../../mocks/kits/world';
import { chooseOption } from '../../pages/controls';

test.describe('production order', () => {
  test('the list shows each order with its status', async ({ page, api }) => {
    const world = mockWorld(api, buildWorld());
    const order = world.productionOrders[0];

    await page.goto('/erp/productionorders');

    await expect(page.getByRole('row').filter({ hasText: String(order.order_number) })).toContainText('Submitted');
  });

  test('a line status can be changed in the grid', async ({ page, api }) => {
    const world = mockWorld(api, buildWorld());
    const order = world.productionOrders[0];
    const row = page.getByRole('row').filter({ hasText: world.products[0].product_name! });

    await page.goto(`/erp/productionorders/edit/${order.guid}`);
    await expect(row).toContainText('Submitted');

    await row.locator('[col-id="status"]').dblclick();
    await chooseOption(page, row.getByRole('combobox'), 'Work In Progress');
    await row.getByRole('combobox').press('Enter');

    await expect(row).toContainText('Work In Progress');
    // Nothing is sent to the API: the change is never saved (BUG-024; pinned in known-bugs.spec.ts).
  });
});
