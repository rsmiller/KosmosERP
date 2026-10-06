import { test, expect } from '../../fixtures/test';
import { fail, ok } from '../../mocks/envelope';
import { buildWorld } from '../../factories/world';
import { mockWorld } from '../../mocks/kits/world';

// Native <select> values are the model enums (models/chart-of-account-models.tsx).
const AccountType = { Asset: '1', Revenue: '4', Expense: '5' };
const NormalBalance = { Debit: '1', Credit: '2' };

test.describe('new account', () => {
  test('creates an account; the type sets the normal balance', async ({ page, api }) => {
    const world = mockWorld(api, buildWorld());
    api.on('POST', '/ChartOfAccount/CreateChartOfAccount', (req) => ok({ ...world.chartOfAccounts[0], ...(req.body as object) }));

    await page.goto('/erp/chartofaccounts/new');
    await page.getByRole('textbox', { name: 'Account Number' }).fill('5100');
    await page.getByRole('textbox', { name: 'Account Name' }).fill('Freight Expense');
    await page.getByRole('combobox', { name: 'Account Type' }).selectOption(AccountType.Expense);
    await expect(page.getByRole('combobox', { name: 'Normal Balance' })).toHaveValue(NormalBalance.Debit);

    await page.getByRole('combobox', { name: 'Account Type' }).selectOption(AccountType.Revenue);
    await expect(page.getByRole('combobox', { name: 'Normal Balance' })).toHaveValue(NormalBalance.Credit);

    await page.getByRole('textbox', { name: 'Description' }).fill('Inbound freight');
    await page.getByRole('button', { name: 'Save Record' }).click();

    const request = await api.waitForRequest('POST', '/ChartOfAccount/CreateChartOfAccount');
    expect(request.body).toMatchObject({
      account_number: '5100',
      account_name: 'Freight Expense',
      account_type: Number(AccountType.Revenue),
      normal_balance: Number(NormalBalance.Credit),
      description: 'Inbound freight',
      is_active: true,
    });
    await expect(page).toHaveURL(`/erp/chartofaccounts/edit/${world.chartOfAccounts[0].guid}`);
  });

  test('cannot be saved without a number and a name', async ({ page, api }) => {
    mockWorld(api, buildWorld());
    const save = page.getByRole('button', { name: 'Save Record' });

    await page.goto('/erp/chartofaccounts/new');
    await expect(save).toBeDisabled();

    await page.getByRole('textbox', { name: 'Account Number' }).fill('5100');
    await expect(save).toBeDisabled();

    await page.getByRole('textbox', { name: 'Account Name' }).fill('Freight Expense');
    await expect(save).toBeEnabled();
  });
});

test.describe('existing account', () => {
  test('saves a renamed account', async ({ page, api }) => {
    const world = mockWorld(api, buildWorld());
    const account = world.chartOfAccounts[0];

    await page.goto(`/erp/chartofaccounts/edit/${account.guid}`);
    await expect(page.getByRole('textbox', { name: 'Account Name' })).toHaveValue(account.account_name!);

    await page.getByRole('textbox', { name: 'Account Name' }).fill('Product Sales Revenue');
    await page.getByRole('button', { name: 'Save Record' }).click();

    const request = await api.waitForRequest('PUT', '/ChartOfAccount/UpdateChartOfAccount');
    expect(request.body).toMatchObject({
      id: account.id,
      account_number: account.account_number,
      account_name: 'Product Sales Revenue',
      account_type: Number(AccountType.Revenue),
    });
    await expect(page.getByText('Record saved!')).toBeVisible();
  });

  test('a refused save shows the error', async ({ page, api }) => {
    // Regression for BUG-020: the page passed props the button bar ignored, so no message showed.
    const world = mockWorld(api, buildWorld());
    const account = world.chartOfAccounts[0];
    api.on('PUT', '/ChartOfAccount/UpdateChartOfAccount', fail(-2, 'Account number already exists'));

    await page.goto(`/erp/chartofaccounts/edit/${account.guid}`);
    await page.getByRole('textbox', { name: 'Account Name' }).fill('Product Sales Revenue');
    await page.getByRole('button', { name: 'Save Record' }).click();

    await api.waitForRequest('PUT', '/ChartOfAccount/UpdateChartOfAccount');
    await expect(page.getByText('Record could not be saved!')).toBeVisible();
    await expect(page.getByText('Record saved!')).toBeHidden();
  });

  test('deletes an account after confirmation', async ({ page, api }) => {
    const world = mockWorld(api, buildWorld());
    const account = world.chartOfAccounts[0];

    await page.goto(`/erp/chartofaccounts/edit/${account.guid}`);
    await page.getByRole('button', { name: 'Delete Record' }).click();
    const confirm = page.getByRole('alertdialog', { name: 'Are you sure?' });
    await confirm.getByRole('button', { name: 'Delete', exact: true }).click();

    const request = await api.waitForRequest('POST', '/ChartOfAccount/DeleteChartOfAccount');
    expect(request.body).toMatchObject({ id: account.id });
    await expect(page).toHaveURL(/\/erp\/chartofaccounts\/?$/);
  });

  test('cancelling the delete keeps the account', async ({ page, api }) => {
    const world = mockWorld(api, buildWorld());

    await page.goto(`/erp/chartofaccounts/edit/${world.chartOfAccounts[0].guid}`);
    await page.getByRole('button', { name: 'Delete Record' }).click();
    await page.getByRole('alertdialog', { name: 'Are you sure?' }).getByRole('button', { name: 'Cancel' }).click();

    await expect(page.getByRole('alertdialog', { name: 'Are you sure?' })).toBeHidden();
    expect(api.requestsTo('POST', '/ChartOfAccount/DeleteChartOfAccount')).toHaveLength(0);
  });
});
