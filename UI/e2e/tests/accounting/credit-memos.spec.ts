import type { Page } from '@playwright/test';
import { test, expect } from '../../fixtures/test';
import { fail } from '../../mocks/envelope';
import { GLAccountKeys } from '../../factories/lookups';
import { buildWorld, type World } from '../../factories/world';
import { mockWorld } from '../../mocks/kits/world';
import { chooseOption, editGridCell, gridRow, searchAndChoose, setDate } from '../../pages/controls';

/** Fill the credit memo header: customer, AR invoice, dates, reason. */
async function fillHeader(page: Page, world: World) {
  const customer = world.customers[0];
  const invoice = world.arInvoices[0];

  await searchAndChoose(page, page.getByRole('combobox', { name: 'Customer' }), 'Acme', customer.customer_name!);

  await page.getByRole('button', { name: 'Search AR invoices' }).click();
  const picker = page.getByRole('alertdialog', { name: 'Search AR Invoices' });
  await picker.getByRole('row').filter({ hasText: String(invoice.invoice_number) }).getByRole('button', { name: 'Select' }).click();
  await expect(picker).toBeHidden();

  // The date inputs have no accessible name (BUG-014); find them by their field group.
  const dateInput = (label: string) => page.getByRole('group').filter({ hasText: label }).getByRole('textbox');
  await setDate(dateInput('Credit Memo Date'), '03/10/2026');
  await setDate(dateInput('Due Date'), '04/09/2026');
  await page.getByRole('textbox', { name: 'Memo' }).fill('RMA 5521');
  // Validity is re-checked on keydown, which fill() doesn't send: type it.
  await page.getByRole('textbox', { name: 'Credit Reason' }).pressSequentially('Damaged in transit');
}

/** Add a line and fill description, amount and GL account. */
async function addLine(page: Page, index: number, { description, amount, gl }: { description: string; amount: number; gl: string }) {
  await page.getByRole('button', { name: 'Add New Line' }).click();
  const row = gridRow(page, index);
  await editGridCell(row, 'description', description);
  await editGridCell(row, 'line_total', amount);
  await row.locator('[col-id="gl_account_id"]').dblclick();
  await chooseOption(page, row.getByRole('combobox'), gl);
  await row.getByRole('combobox').press('Enter');
}

test.describe('new credit memo', () => {
  let world: World;

  test.beforeEach(async ({ page, api }) => {
    world = mockWorld(api, buildWorld());
    await page.goto('/erp/creditmemos/new');
    await expect(page.getByRole('heading', { name: 'New Credit Memo' })).toBeVisible();
  });

  test('creates a credit memo against an AR invoice', async ({ page, api }) => {
    await fillHeader(page, world);
    await addLine(page, 0, { description: 'Returned unit', amount: 1800, gl: '4010' });
    const save = page.getByRole('button', { name: 'Save Record' });
    await expect(save).toBeEnabled();
    await save.click();

    const request = await api.waitForRequest('POST', '/CreditMemo/CreateCreditMemo');
    expect(request.body).toMatchObject({
      customer_id: world.customers[0].id,
      ar_invoice_header_id: world.arInvoices[0].id,
      credit_memo_date: '2026-03-10',
      credit_memo_due_date: '2026-04-09',
      memo: 'RMA 5521',
      credit_reason: 'Damaged in transit',
    });
    const lines = (request.body as { credit_memo_lines: Record<string, unknown>[] }).credit_memo_lines;
    expect(lines).toHaveLength(1);
    expect(lines[0]).toMatchObject({ description: 'Returned unit', gl_account_id: GLAccountKeys.Revenue });
    expect(Number(lines[0].line_total)).toBe(1800);

    await expect(page).toHaveURL(/\/erp\/creditmemos\/?$/);
  });

  test('cannot be saved without a complete line', async ({ page }) => {
    const save = page.getByRole('button', { name: 'Save Record' });
    await fillHeader(page, world);
    await expect(save).toBeDisabled();

    await page.getByRole('button', { name: 'Add New Line' }).click();
    await editGridCell(gridRow(page, 0), 'description', 'No amount or account yet');
    await expect(save).toBeDisabled();
  });

  test('stays on the form when the API rejects it', async ({ page, api }) => {
    api.on('POST', '/CreditMemo/CreateCreditMemo', fail(-7, 'Credit exceeds invoice'));

    await fillHeader(page, world);
    await addLine(page, 0, { description: 'Returned unit', amount: 1800, gl: '4010' });
    await page.getByRole('button', { name: 'Save Record' }).click();

    await expect(page.getByText('Record could not be saved!')).toBeVisible();
    await expect(page).toHaveURL(/\/erp\/creditmemos\/new$/);
  });
});
