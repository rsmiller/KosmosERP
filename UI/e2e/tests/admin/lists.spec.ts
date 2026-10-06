import { test, expect } from '../../fixtures/test';
import { buildWorld } from '../../factories/world';
import { KeyValueModules, PaymentTermKeys } from '../../factories/lookups';
import { mockWorld } from '../../mocks/kits/world';
import { chooseOption, editGridCell } from '../../pages/controls';

test.describe('admin lists', () => {
  test('adds a payment term with its number of days', async ({ page, api }) => {
    mockWorld(api, buildWorld());

    await page.goto('/erp/admin/lists');
    await page.getByRole('button', { name: 'Add New Entry' }).click();
    const dialog = page.getByRole('dialog', { name: 'Add New Entry' });
    await chooseOption(page, dialog.getByRole('combobox'), 'Payment Term');
    await dialog.getByRole('textbox', { name: 'Key' }).fill('payment_terms_net_90');
    await dialog.getByRole('textbox', { name: 'Value' }).fill('Net 90');
    await dialog.getByRole('spinbutton', { name: 'Number' }).fill('90');
    await dialog.getByRole('button', { name: 'Add Entry' }).click();

    const request = await api.waitForRequest('POST', '/KeyValue/CreateKeyValue');
    expect(request.body).toMatchObject({
      module_id: KeyValueModules.PaymentTerm,
      key: 'payment_terms_net_90',
      value: 'Net 90',
      int_value: 90,
    });
  });

  test('editing a number keeps the label', async ({ page, api }) => {
    const world = mockWorld(api, buildWorld());
    const net30 = world.lookups[KeyValueModules.PaymentTerm].find((t) => t.key === PaymentTermKeys.Net30)!;

    await page.goto('/erp/admin/lists');
    await editGridCell(page.getByRole('row').filter({ hasText: PaymentTermKeys.Net30 }), 'int_value', 31);

    const request = await api.waitForRequest('PUT', '/KeyValue/UpdateKeyValue');
    expect(request.body).toMatchObject({ id: net30.id, value: net30.value, int_value: 31 });
  });
});
