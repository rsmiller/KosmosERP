import type { Page } from '@playwright/test';
import { test, expect } from '../../fixtures/test';
import { ok } from '../../mocks/envelope';
import { buildWorld } from '../../factories/world';
import { KeyValueModules, PaymentTermKeys } from '../../factories/lookups';
import { mockWorld } from '../../mocks/kits/world';

/**
 * Buttons inside ag-grid cells, after loading the page directly (URL, bookmark,
 * refresh). Grid column definitions are often built once, on the first render,
 * before auth or permissions are ready; their buttons then act on that first
 * render's state (BUG-027). The api fixture fails any test whose page calls a
 * protected endpoint without a token, so each click here is checked for that too.
 */

const row = (page: Page, text: string) => page.getByRole('row').filter({ hasText: text });
const confirmDialog = (page: Page) => page.getByRole('alertdialog', { name: 'Are you sure?' });

test.describe('line Delete', () => {
  test('purchase order line', async ({ page, api }) => {
    const world = mockWorld(api, buildWorld());
    const order = world.purchaseOrders[0];
    const line = order.purchase_order_lines![0];
    await page.goto(`/erp/purchaseorders/edit/${order.guid}`);

    await row(page, world.products[1].product_name!).getByRole('button', { name: 'Delete' }).click();

    const request = await api.waitForRequest('POST', '/PurchaseOrder/DeletePurchaseOrderLine');
    expect(request.body).toMatchObject({ id: line.id });
  });

  test('credit memo line', async ({ page, api }) => {
    const world = mockWorld(api, buildWorld());
    const memo = world.creditMemos[0];
    const line = memo.credit_memo_lines![0];
    await page.goto(`/erp/creditmemos/edit/${memo.guid}`);

    await row(page, line.description!).getByRole('button', { name: 'Delete' }).click();

    const request = await api.waitForRequest('POST', '/CreditMemo/DeleteCreditMemoLine');
    expect(request.body).toMatchObject({ id: line.id });
  });

  test('opportunity line, after confirmation', async ({ page, api }) => {
    const world = mockWorld(api, buildWorld());
    const opportunity = world.opportunities[0];
    const line = opportunity.opportunity_lines![0];
    await page.goto(`/erp/opportunities/edit/${opportunity.guid}`);

    await row(page, line.product_name!).getByRole('button', { name: 'Delete' }).click();
    await confirmDialog(page).getByRole('button', { name: 'Delete' }).click();

    const request = await api.waitForRequest('POST', '/Opportunity/DeleteOpportunityLine');
    expect(request.body).toMatchObject({ id: line.id });
  });
});

test.describe('admin grids', () => {
  test('Lists: Delete an entry, after confirmation', async ({ page, api }) => {
    const world = mockWorld(api, buildWorld());
    const net30 = world.lookups[KeyValueModules.PaymentTerm].find((t) => t.key === PaymentTermKeys.Net30)!;
    await page.goto('/erp/admin/lists');

    await row(page, net30.key).getByRole('button', { name: 'Delete' }).click();
    await confirmDialog(page).getByRole('button', { name: 'Delete' }).click();

    const request = await api.waitForRequest('POST', '/KeyValue/DeleteKeyValue');
    expect(request.body).toMatchObject({ id: net30.id });
  });

  test('Document Types: Edit Tags loads the tags, and a tag can be deleted', async ({ page, api }) => {
    const world = mockWorld(api, buildWorld());
    const uploadObject = world.uploadObjects[0];
    const tag = { id: 501, document_object_id: uploadObject.id, name: 'Invoice', is_required: false };
    api.on('GET', '/Document/GetDocumentObjectTags', ok([tag]));
    api.on('DELETE', '/Document/DeleteDocumentObjectTags', (req) => ok(req.body));
    await page.goto('/erp/admin/document-types');

    await row(page, uploadObject.friendly_name!).getByRole('button', { name: 'Edit Tags' }).click();

    const tagsRequest = await api.waitForRequest('GET', '/Document/GetDocumentObjectTags');
    expect(tagsRequest.url.searchParams.get('document_object_id')).toBe(String(uploadObject.id));
    const tags = page.getByRole('dialog', { name: 'Tags' });
    await row(page, 'Invoice').getByRole('button', { name: 'Delete' }).click();
    await confirmDialog(page).getByRole('button', { name: 'Delete' }).click();

    const deleteRequest = await api.waitForRequest('DELETE', '/Document/DeleteDocumentObjectTags');
    expect(deleteRequest.body).toMatchObject({ id: tag.id });
    await expect(tags).toBeVisible();
  });

  test('Document Types: Delete a document type, after confirmation', async ({ page, api }) => {
    const world = mockWorld(api, buildWorld());
    const uploadObject = world.uploadObjects[0];
    api.on('DELETE', '/Document/DeleteUploadObject', (req) => ok(req.body));
    await page.goto('/erp/admin/document-types');

    await row(page, uploadObject.friendly_name!).getByRole('button', { name: 'Delete' }).click();
    await confirmDialog(page).getByRole('button', { name: 'Delete' }).click();

    const request = await api.waitForRequest('DELETE', '/Document/DeleteUploadObject');
    expect(request.body).toMatchObject({ id: uploadObject.id });
  });
});

test.describe('list Edit buttons for a user who may edit', () => {
  test('vendors', async ({ page, api }) => {
    const world = mockWorld(api, buildWorld());
    const vendor = world.vendors[0];
    await page.goto('/erp/vendors');

    await row(page, vendor.vendor_name!).getByRole('button', { name: 'Edit' }).click();

    await expect(page).toHaveURL(`/erp/vendors/edit/${vendor.guid}`);
  });

  test('production orders', async ({ page, api }) => {
    const world = mockWorld(api, buildWorld());
    const order = world.productionOrders[0];
    await page.goto('/erp/productionorders');

    await row(page, String(order.order_number)).getByRole('button', { name: 'Edit' }).click();

    await expect(page).toHaveURL(`/erp/productionorders/edit/${order.guid}`);
  });
});
