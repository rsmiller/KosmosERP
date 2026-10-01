import type { Page } from '@playwright/test';
import { test, expect } from '../../fixtures/test';
import type { MockApi } from '../../mocks/mock-api';
import { ok } from '../../mocks/envelope';
import { ProductCategoryKeys } from '../../factories/lookups';
import { buildWorld, type World } from '../../factories/world';
import { mockWorld } from '../../mocks/kits/world';
import { chooseOption, editGridCell, searchAndChoose } from '../../pages/controls';

test.describe('new product', () => {
  /** Fill in the new product form, save, and check what was sent and where the page went. */
  async function createProduct(page: Page, api: MockApi, world: World) {
    const vendor = world.vendors[1];
    await page.getByRole('textbox', { name: 'Product Name' }).fill('DDR5 64GB Kit');
    await searchAndChoose(page, page.getByRole('combobox', { name: 'Vendor' }), 'Cont', vendor.vendor_name!);
    await page.getByRole('textbox', { name: 'Product Class' }).fill('Component');
    await chooseOption(page, page.getByRole('combobox', { name: 'Category' }), 'Memory');
    await page.getByRole('textbox', { name: 'Identifier 1' }).fill('MEM-64G');
    await page.getByRole('textbox', { name: 'Internal Description' }).fill('64GB DDR5-6000 kit');
    await page.getByRole('spinbutton', { name: 'Required Stock Level' }).fill('20');
    await page.getByRole('spinbutton', { name: 'Our Cost' }).fill('150');
    await page.getByRole('spinbutton', { name: 'Unit Cost' }).fill('160');
    await page.getByRole('spinbutton', { name: 'Sales Price' }).fill('210');
    await page.getByRole('spinbutton', { name: 'List Price' }).fill('229');
    await page.getByText('Sales Item', { exact: true }).click();

    const save = page.getByRole('button', { name: 'Save Record' });
    await expect(save).toBeEnabled();
    await save.click();

    const request = await api.waitForRequest('POST', '/Product/CreateProduct');
    expect(request.body).toMatchObject({
      product_name: 'DDR5 64GB Kit',
      vendor_id: vendor.id,
      product_class: 'Component',
      category: ProductCategoryKeys.Memory,
      identifier1: 'MEM-64G',
      internal_description: '64GB DDR5-6000 kit',
      required_stock_level: 20,
      our_cost: 150,
      unit_cost: 160,
      sales_price: 210,
      list_price: 229,
      is_sales_item: true,
      is_retired: false,
    });
    await expect(page).toHaveURL(`/erp/products/edit/${world.products[0].guid}`);
  }

  test('creates a product from the list\'s New Product button', async ({ page, api }) => {
    const world = mockWorld(api, buildWorld());
    api.on('POST', '/Product/CreateProduct', (req) => ok({ ...world.products[0], ...(req.body as object) }));

    await page.goto('/erp/products');
    await page.getByRole('button', { name: 'New Product' }).click();

    await createProduct(page, api, world);
  });

  test('creates a product after loading the page directly', async ({ page, api }) => {
    // Regression for BUG-023: on a direct load (URL, bookmark, refresh) the
    // permission check ran before auth was ready, so Save never enabled.
    const world = mockWorld(api, buildWorld());
    api.on('POST', '/Product/CreateProduct', (req) => ok({ ...world.products[0], ...(req.body as object) }));

    await page.goto('/erp/products/new');

    await createProduct(page, api, world);
  });
});

test.describe('bill of materials', () => {
  let world: World;

  async function openBomTab(page: Page) {
    await page.goto(`/erp/products/edit/${world.products[0].guid}`);
    await page.getByRole('tab', { name: 'BOM' }).click();
  }

  test.beforeEach(async ({ api }) => {
    world = mockWorld(api, buildWorld());
  });

  test('adds a component through the dialog', async ({ page, api }) => {
    const parent = world.products[0];
    const component = world.products[1];
    api.on('POST', '/BOM/CreateBOM', (req) => ok({ id: 777, guid: 'bom-777', product_name: component.product_name, ...(req.body as object) }));

    await openBomTab(page);
    await page.getByRole('button', { name: 'New BOM Item' }).click();
    // The dialog is titled "Add Address" (BUG-022).
    const dialog = page.getByRole('dialog').filter({ has: page.getByRole('combobox', { name: 'Product' }) });
    await searchAndChoose(page, dialog.getByRole('combobox', { name: 'Product' }), 'DDR5', component.product_name!);
    await dialog.getByRole('textbox', { name: 'Description' }).fill('Two sticks per build');
    await dialog.getByRole('spinbutton', { name: 'Quantity' }).fill('2');
    await dialog.getByRole('button', { name: 'Save' }).click();

    const request = await api.waitForRequest('POST', '/BOM/CreateBOM');
    expect(request.body).toMatchObject({ parent_product_id: parent.id, product_id: component.id, instructions: 'Two sticks per build', order_number: 2 });
    expect(Number((request.body as { quantity: unknown }).quantity)).toBe(2);
    await expect(page.getByRole('row').filter({ hasText: 'Two sticks per build' })).toBeVisible();
  });

  test('saves an inline quantity change', async ({ page, api }) => {
    const bom = world.boms[0];

    await openBomTab(page);
    await editGridCell(page.getByRole('row').filter({ hasText: bom.product_name! }), 'quantity', 4);

    const request = await api.waitForRequest('PUT', '/BOM/UpdateBOM');
    expect(request.body).toMatchObject({ id: bom.id, instructions: bom.instructions });
    expect(Number((request.body as { quantity: unknown }).quantity)).toBe(4);
  });

  test('deletes a component after confirmation', async ({ page, api }) => {
    const bom = world.boms[0];
    const row = page.getByRole('row').filter({ hasText: bom.product_name! });

    await openBomTab(page);
    await row.getByRole('button', { name: 'Delete' }).click();
    await page.getByRole('alertdialog', { name: 'Are you sure?' }).getByRole('button', { name: 'Delete', exact: true }).click();

    const request = await api.waitForRequest('POST', '/BOM/DeleteBOM');
    expect(request.body).toMatchObject({ id: bom.id });
    await expect(row).toBeHidden();
  });
});
