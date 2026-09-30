import { test, expect } from '../../fixtures/test';
import { ok, paged } from '../../mocks/envelope';
import { users } from '../../fixtures/users';
import { buildCustomerListItem } from '../../factories/customer';
import { paymentTerms } from '../../factories/lookups';
import { AppShell } from '../../pages/app-shell';
import { CustomersListPage } from '../../pages/customers-pages';

test.describe('signed out', () => {
  test.use({ storageState: { cookies: [], origins: [] } });

  test('ERP pages redirect to the landing page', async ({ page }) => {
    await page.goto('/erp/customers');

    await expect(page).toHaveURL(/\/$/);
    await expect(page.getByRole('button', { name: 'Username & Password' })).toBeVisible();
  });
});

test.describe('signed in', () => {
  test('logout clears the session and returns to the landing page', async ({ page }) => {
    await page.goto('/erp');
    await new AppShell(page).logout();

    await expect(page).toHaveURL(/\/$/);
    expect(await page.evaluate(() => localStorage.getItem('t'))).toBeNull();

    await page.goto('/erp/customers');
    await expect(page).toHaveURL(/\/$/);
  });
});

test.describe('read-only user', () => {
  test.use({ storageState: users.readOnly.storageState });

  test('sees customers but not the Edit action', async ({ page, api }) => {
    const customer = buildCustomerListItem({ customer_name: 'Read Only Industries' });
    api.on('POST', '/Customer/FindCustomer', paged([customer]));
    const list = new CustomersListPage(page);

    await list.goto();

    await expect(list.row('Read Only Industries')).toBeVisible();
    await expect(list.viewButton('Read Only Industries')).toBeVisible();
    await expect(list.editButton('Read Only Industries')).toBeHidden();
  });

  test('is redirected away from the new customer form', async ({ page, api }) => {
    // The payment terms combobox starts loading before the permission redirect.
    api.on('GET', '/KeyValue/GetKeyValuesByModule', ok(paymentTerms()));

    await page.goto('/erp/customers/new');

    await expect(page).toHaveURL(/\/erp\/?$/);
  });
});
