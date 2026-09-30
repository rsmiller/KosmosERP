import type { Page } from '@playwright/test';
import { test, expect } from '../../fixtures/test';
import { ok } from '../../mocks/envelope';
import { buildWorld, type World } from '../../factories/world';
import { mockWorld } from '../../mocks/kits/world';
import { chooseOption, dataListValue, searchAndChoose, setDate } from '../../pages/controls';

/**
 * The AP invoice form lays its inputs out in a DataList with no form labels
 * (BUG-014), so each control is found in the value cell next to its label.
 */
async function fillNewInvoice(page: Page, world: World) {
  const vendor = world.vendors[0];
  const po = world.purchaseOrders[0];

  await dataListValue(page, 'Invoice Number').getByRole('textbox').fill('NW-88121');
  await searchAndChoose(page, dataListValue(page, 'Vendor Name').getByRole('combobox'), 'North', vendor.vendor_name!);
  await setDate(dataListValue(page, 'Invoice Date').getByRole('textbox'), '03/05/2026');
  await setDate(dataListValue(page, 'Received Date').getByRole('textbox'), '03/06/2026');
  await setDate(dataListValue(page, 'Due Date').getByRole('textbox'), '04/04/2026');
  await chooseOption(page, dataListValue(page, 'Object Type').getByRole('combobox'), 'Purchase Order');
  await searchAndChoose(
    page,
    dataListValue(page, 'Object Number').getByRole('combobox'),
    'North',
    `${po.po_number} - ${vendor.vendor_name}`,
  );
}

test.describe('new AP invoice', () => {
  test('creates an invoice against a purchase order and opens it', async ({ page, api }) => {
    const world = mockWorld(api, buildWorld());
    api.on('POST', '/APInvoice/CreateAPInvoice', (req) => ok({ ...world.apInvoices[0], ...(req.body as object) }));

    await page.goto('/erp/ap/new');
    await fillNewInvoice(page, world);
    const save = page.getByRole('button', { name: 'Save Record' });
    await expect(save).toBeEnabled();
    await save.click();

    const request = await api.waitForRequest('POST', '/APInvoice/CreateAPInvoice');
    expect(request.body).toMatchObject({
      invoice_number: 'NW-88121',
      vendor_id: world.vendors[0].id,
      association_object_id: world.purchaseOrders[0].id,
      association_is_purchase_order: true,
    });
    await expect(page).toHaveURL(`/erp/ap/edit/${world.apInvoices[0].guid}`);
  });

  test('cannot be saved without an associated object', async ({ page, api }) => {
    const world = mockWorld(api, buildWorld());
    const save = page.getByRole('button', { name: 'Save Record' });

    await page.goto('/erp/ap/new');
    await dataListValue(page, 'Invoice Number').getByRole('textbox').fill('NW-88121');
    await searchAndChoose(page, dataListValue(page, 'Vendor Name').getByRole('combobox'), 'North', world.vendors[0].vendor_name!);
    await setDate(dataListValue(page, 'Invoice Date').getByRole('textbox'), '03/05/2026');
    await setDate(dataListValue(page, 'Received Date').getByRole('textbox'), '03/06/2026');
    await setDate(dataListValue(page, 'Due Date').getByRole('textbox'), '04/04/2026');

    await expect(save).toBeDisabled();
  });
});
