import { expect, type Locator, type Page } from '@playwright/test';

/**
 * Helpers for the app's Chakra/ark comboboxes and react-datepicker inputs.
 * They drive the controls the way a user does and leave assertions to specs.
 */
export async function chooseOption(page: Page, combobox: Locator, option: string): Promise<void> {
  await combobox.click();
  await combobox.press('ArrowDown');
  await optionLocator(page, option).click();
}

function optionLocator(page: Page, option: string): Locator {
  return page.getByRole('option', { name: option, exact: true });
}

/**
 * Pick from a server-searched combobox (customer, product, vendor...): typing
 * three or more characters triggers the Find call, then the option is clicked.
 */
export async function searchAndChoose(page: Page, combobox: Locator, search: string, option: string): Promise<void> {
  await combobox.click();
  // Typing fires a Find request whose results replace the option list. Clicking
  // an option from the stale list races that re-render and loses the selection,
  // so wait for the search to come back first.
  const searched = page.waitForResponse((r) => /\/api\/v1\/[^/]+\/Find[^/?]*/i.test(r.url()));
  await combobox.fill(search);
  await searched;
  await optionLocator(page, option).click();
  // Synchronization, not a test assertion: wait until the pick has landed.
  await expect(combobox).toHaveValue(option);
}

/**
 * Edit an ag-grid cell: double-click it, type into the editor, press Enter.
 * `colId` is the column's `field`. Grid cells have no accessible name of their
 * own, so ag-grid's `col-id` attribute is the stable handle.
 */
export async function editGridCell(row: Locator, colId: string, value: string | number): Promise<void> {
  const cell = row.locator(`[col-id="${colId}"]`);
  await cell.dblclick();
  const editor = cell.locator('input');
  await editor.fill(String(value));
  await editor.press('Enter');
}

/**
 * The value cell of a Chakra DataList item (<dt>label</dt><dd>value</dd>).
 * Some forms put their inputs in DataLists, which don't label them (BUG-014),
 * so controls are found inside the <dd> that follows the label.
 */
export function dataListValue(page: Page, label: string): Locator {
  return page.getByRole('term').filter({ hasText: new RegExp(`^\\s*${escapeRegExp(label)}\\s*$`) }).locator('xpath=following-sibling::*[1]');
}

/** An ag-grid data row by position (for rows with no text to find them by yet). */
export function gridRow(page: Page, index: number): Locator {
  return page.locator(`[role="row"][row-index="${index}"]`);
}

function escapeRegExp(text: string): string {
  return text.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
}

/** Type a date (MM/dd/yyyy) into a react-datepicker input and close the popup. */
export async function setDate(input: Locator, date: string): Promise<void> {
  await input.fill(date);
  await input.press('Enter');
}
