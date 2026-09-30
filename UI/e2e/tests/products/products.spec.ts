import { test, expect } from '../../fixtures/test';
import { ok, paged } from '../../mocks/envelope';
import type { MockApi } from '../../mocks/mock-api';
import type { ProductDto } from '@/models/product-models';
import { ProductCategoryKeys, ProductCategoryModule, productCategories } from '../../factories/lookups';
import { buildProduct } from '../../factories/product';
import { buildVendor } from '../../factories/vendor';
import { ProductFormPage } from '../../pages/products-pages';

/** Everything the edit page loads for one product. */
function mockProductRecord(api: MockApi, product: ProductDto) {
  const vendor = buildVendor({ id: product.vendor_id, vendor_name: 'Northwind Silicon' });
  api.on('GET', '/Product/GetProductByGuid', (req) => {
    expect(req.url.searchParams.get('guid')).toBe(product.guid);
    return ok(product);
  });
  api.on('POST', '/BOM/FindBOM', paged([]));
  api.on('GET', '/Vendor/GetVendor', ok(vendor));
  // The vendor combobox also runs an empty search on mount.
  api.on('POST', '/Vendor/FindVendor', paged([vendor]));
  api.on('GET', '/KeyValue/GetKeyValuesByModule', (req) => {
    expect(req.url.searchParams.get('module_id')).toBe(ProductCategoryModule);
    return ok(productCategories());
  });
}

test.describe('edit product', () => {
  test('loads the product and saves changes', async ({ page, api }) => {
    const product = buildProduct({ product_name: 'Ryzen 9 7950X', category: ProductCategoryKeys.Processors });
    mockProductRecord(api, product);
    api.on('PUT', '/Product/UpdateProduct', (req) => ok({ ...product, ...(req.body as object) }));
    const form = new ProductFormPage(page);

    await page.goto(`/erp/products/edit/${product.guid}`);
    await expect(form.productName).toHaveValue('Ryzen 9 7950X');

    await form.productName.fill('Ryzen 9 7950X3D');
    await expect(form.save).toBeEnabled();
    await form.save.click();

    const request = await api.waitForRequest('PUT', '/Product/UpdateProduct');
    expect(request.body).toMatchObject({
      id: product.id,
      product_name: 'Ryzen 9 7950X3D',
      identifier1: product.identifier1,
      category: ProductCategoryKeys.Processors,
    });
    await expect(form.savedAlert).toBeVisible();
  });

  test('cannot be saved while a required field is empty', async ({ page, api }) => {
    const product = buildProduct({ product_name: 'Ryzen 9 7950X' });
    mockProductRecord(api, product);
    const form = new ProductFormPage(page);

    await page.goto(`/erp/products/edit/${product.guid}`);
    await expect(form.identifier1).toHaveValue(product.identifier1!);

    await form.identifier1.fill('');
    await expect(form.save).toBeDisabled();

    await form.identifier1.fill('SKU-RESTORED');
    await expect(form.save).toBeEnabled();
  });
});
