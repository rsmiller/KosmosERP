import { test, expect } from '../fixtures/test';
import { fail } from '../mocks/envelope';
import { buildWorld } from '../factories/world';
import { mockWorld } from '../mocks/kits/world';
import { chooseOption, searchAndChoose } from '../pages/controls';

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

test('BUG-011: date-only values display on the right day', async ({ page, api }) => {
  knownBug('BUG-011: DateOnly strings are parsed as UTC midnight, so US time zones show the previous day');

  const world = mockWorld(api, buildWorld());
  const opportunity = world.opportunities[0]; // expected_close: '2026-04-01'

  await page.goto(`/erp/opportunities/view/${opportunity.guid}`);

  await expect(page.getByRole('textbox', { name: 'Select date' })).toHaveValue('04/01/2026');
});

test('BUG-006: new-record forms have no Delete Record button', async ({ page, api }) => {
  knownBug('BUG-006: PageActionsComponent shows Delete on create pages that pass no onDelete');

  mockWorld(api, buildWorld());
  await page.goto('/erp/customers/new');
  await expect(page.getByRole('heading', { name: 'New Customer' })).toBeVisible();

  await expect(page.getByRole('button', { name: 'Delete Record' })).toBeHidden();
});

test('BUG-020: a failed journal entry post shows an error', async ({ page, api }) => {
  knownBug('BUG-020: journal entry and chart of accounts pages pass showSaveFailed/showDelete/... props PageActionsComponent ignores');

  const world = mockWorld(api, buildWorld());
  api.on('POST', '/JournalEntry/PostJournalEntry', fail(-6, 'Fiscal period closed'));
  await page.goto(`/erp/journalentries/edit/${world.journalEntries[0].guid}`);

  await page.getByRole('button', { name: 'Post Entry' }).click();

  await expect(page.getByText('Record could not be saved!')).toBeVisible();
});

test('BUG-023: a product can be created after loading /erp/products/new directly', async ({ page, api }) => {
  knownBug('BUG-023: the permission effect has deps [setValue], runs before auth is ready and never re-runs, so hasWritePermission stays false');

  const world = mockWorld(api, buildWorld());
  await page.goto('/erp/products/new');
  await page.getByRole('textbox', { name: 'Product Name' }).fill('DDR5 64GB Kit');
  await searchAndChoose(page, page.getByRole('combobox', { name: 'Vendor' }), 'Cont', world.vendors[1].vendor_name!);
  await page.getByRole('textbox', { name: 'Product Class' }).fill('Component');
  await chooseOption(page, page.getByRole('combobox', { name: 'Category' }), 'Memory');
  await page.getByRole('textbox', { name: 'Identifier 1' }).fill('MEM-64G');
  await page.getByRole('textbox', { name: 'Internal Description' }).fill('64GB kit');
  for (const [name, value] of [['Required Stock Level', '20'], ['Our Cost', '150'], ['Unit Cost', '160'], ['Sales Price', '210']] as const) {
    await page.getByRole('spinbutton', { name }).fill(value);
  }

  await expect(page.getByRole('button', { name: 'Save Record' })).toBeEnabled();
});

test('BUG-024: a production order status change is saved', async ({ page, api }) => {
  knownBug('BUG-024: the production order edit page never calls the API; handleSaveClick and CheckFormValidity are empty stubs');

  const world = mockWorld(api, buildWorld());
  const row = page.getByRole('row').filter({ hasText: world.products[0].product_name! });
  await page.goto(`/erp/productionorders/edit/${world.productionOrders[0].guid}`);

  await row.locator('[col-id="status"]').dblclick();
  await chooseOption(page, row.getByRole('combobox'), 'Work In Progress');
  await row.getByRole('combobox').press('Enter');
  const save = page.getByRole('button', { name: 'Save Record' });
  if (await save.isEnabled()) await save.click();

  await expect.poll(() => api.requests.filter((r) => r.method === 'PUT' && /ProductionOrder/.test(r.path)).length).toBeGreaterThan(0);
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
