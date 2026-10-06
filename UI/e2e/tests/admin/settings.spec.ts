import { test, expect } from '../../fixtures/test';
import { fail } from '../../mocks/envelope';
import { buildWorld } from '../../factories/world';
import { mockWorld } from '../../mocks/kits/world';

test.describe('company settings', () => {
  test('an admin can change and save the company settings', async ({ page, api }) => {
    const world = mockWorld(api, buildWorld());
    const settings = world.settings;

    await page.goto('/erp/admin/settings');
    const companyName = page.getByRole('textbox', { name: 'Company Name' });
    await expect(companyName).toHaveValue(settings.company_name!);

    await companyName.fill('Kosmos Manufacturing LLC');
    await page.getByRole('textbox', { name: 'AP Email' }).fill('payables@kosmos.example.test');
    const save = page.getByRole('button', { name: 'Save Record' });
    await expect(save).toBeEnabled();
    await save.click();

    const request = await api.waitForRequest('PUT', '/Settings/UpdateSettings');
    expect(request.body).toMatchObject({
      id: settings.id,
      company_name: 'Kosmos Manufacturing LLC',
      company_ap_email: 'payables@kosmos.example.test',
      company_ar_email: settings.company_ar_email,
      tax_id: settings.tax_id,
    });
    await expect(page.getByText('Record saved!')).toBeVisible();
  });

  test('shows an error when the settings cannot be saved', async ({ page, api }) => {
    mockWorld(api, buildWorld());
    api.on('PUT', '/Settings/UpdateSettings', fail(-8, 'Invalid tax id'));

    await page.goto('/erp/admin/settings');
    await page.getByRole('textbox', { name: 'Tax ID' }).fill('not-a-tax-id');
    await page.getByRole('button', { name: 'Save Record' }).click();

    await expect(page.getByText('Record could not be saved!')).toBeVisible();
  });
});
