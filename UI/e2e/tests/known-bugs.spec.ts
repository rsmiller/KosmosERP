import { test, expect } from '../fixtures/test';
import { fail } from '../mocks/envelope';
import { buildWorld } from '../factories/world';
import { mockWorld } from '../mocks/kits/world';

/**
 * Open app bugs pinned as tests (details in bugs.md at the repo root). Each
 * test asserts the CORRECT behavior and is marked test.fail(), so it passes
 * while the bug exists. When a fix lands the test starts "unexpectedly
 * passing" and fails the run: remove its knownBug() line.
 *
 * E2E_SHOW_KNOWN_BUGS=1 turns the markers off so you can see each real failure
 * (and check it fails for the reason in its BUG entry, not something else).
 */
const knownBug = (reason: string) => test.fail(!process.env.E2E_SHOW_KNOWN_BUGS, reason);

test('BUG-020: a failed journal entry post shows an error', async ({ page, api }) => {
  knownBug('BUG-020: journal entry and chart of accounts pages pass showSaveFailed/showDelete/... props PageActionsComponent ignores');

  const world = mockWorld(api, buildWorld());
  api.on('POST', '/JournalEntry/PostJournalEntry', fail(-6, 'Fiscal period closed'));
  await page.goto(`/erp/journalentries/edit/${world.journalEntries[0].guid}`);

  await page.getByRole('button', { name: 'Post Entry' }).click();

  await expect(page.getByText('Record could not be saved!')).toBeVisible();
});

test('BUG-025: a completed production order\'s lines are read-only', async ({ page, api }) => {
  knownBug('BUG-025: colDefs live in useState, so editable: !completedOrDisabled is fixed at mount (before the order loads)');

  const world = buildWorld();
  world.productionOrders[0].is_complete = true;
  mockWorld(api, world);
  const row = page.getByRole('row').filter({ hasText: world.products[0].product_name! });
  await page.goto(`/erp/productionorders/edit/${world.productionOrders[0].guid}`);
  await expect(row).toContainText('Submitted');

  const statusCell = row.locator('[col-id="status"]');
  await statusCell.dblclick();

  // ag-grid marks focus and (if editable) inline editing in the same event, so
  // once the cell is focused its editing state is settled. Asserting the editor
  // is hidden right after the double-click would pass before it renders.
  await expect(statusCell).toHaveClass(/ag-cell-focus/);
  await expect(statusCell).not.toHaveClass(/ag-cell-inline-editing/);
});
