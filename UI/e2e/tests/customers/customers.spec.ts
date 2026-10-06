import { test, expect } from '../../fixtures/test';
import { fail, ok, paged } from '../../mocks/envelope';
import type { MockApi } from '../../mocks/mock-api';
import type { CustomerDto } from '@/models/customer-models';
import { buildCustomer, buildCustomerListItem } from '../../factories/customer';
import { KeyValueModules, PaymentTermKeys, paymentTerms } from '../../factories/lookups';
import { CustomerFormPage, CustomersListPage } from '../../pages/customers-pages';

/** Lookups every customer form loads. */
function mockCustomerLookups(api: MockApi) {
  api.on('GET', '/KeyValue/GetKeyValuesByModule', (req) => {
    expect(req.url.searchParams.get('module_id')).toBe(KeyValueModules.PaymentTerm);
    return ok(paymentTerms());
  });
}

/** Everything the edit and view pages load for one customer. */
function mockCustomerRecord(api: MockApi, customer: CustomerDto) {
  mockCustomerLookups(api);
  api.on('GET', '/Customer/GetCustomerByGuid', (req) => {
    expect(req.url.searchParams.get('guid')).toBe(customer.guid);
    return ok(customer);
  });
  // The view page opens on its Documents tab.
  api.on('POST', '/Document/FindDocument', paged([]));
}

test.describe('customers list', () => {
  test('shows customers returned by the API', async ({ page, api }) => {
    const customers = [
      buildCustomerListItem({ customer_name: 'Acme Components' }),
      buildCustomerListItem({ customer_name: 'Globex Systems' }),
      buildCustomerListItem({ customer_name: 'Initech Hardware' }),
    ];
    api.on('POST', '/Customer/FindCustomer', paged(customers));
    const list = new CustomersListPage(page);

    await list.goto();

    await expect(list.heading).toBeVisible();
    for (const customer of customers) {
      const row = list.row(customer.customer_name!);
      await expect(row).toContainText(String(customer.customer_number));
      await expect(row).toContainText(customer.payment_terms_name!);
    }
  });

  test('view and edit buttons open the matching customer', async ({ page, api }) => {
    const customer = buildCustomer({ customer_name: 'Acme Components' });
    api.on('POST', '/Customer/FindCustomer', paged([customer]));
    mockCustomerRecord(api, customer);
    const list = new CustomersListPage(page);

    await list.goto();
    await list.editButton('Acme Components').click();
    await expect(page).toHaveURL(`/erp/customers/edit/${customer.guid}`);

    await page.goBack();
    await list.viewButton('Acme Components').click();
    await expect(page).toHaveURL(`/erp/customers/view/${customer.guid}`);
  });
});

test.describe('create customer', () => {
  test.beforeEach(async ({ page, api }) => {
    mockCustomerLookups(api);
    await page.goto('/erp/customers/new');
  });

  test('saves a new customer and returns to the list', async ({ page, api }) => {
    api.on('POST', '/Customer/CreateCustomer', (req) => ok(buildCustomer(req.body as object)));
    api.on('POST', '/Customer/FindCustomer', paged([]));
    const form = new CustomerFormPage(page);

    await form.customerName.fill('Contoso Circuits');
    await form.description.fill('Board-level assembly partner');
    await form.accountingEmail.fill('ap@contoso.example.test');
    await form.generalEmail.fill('hello@contoso.example.test');
    await form.phone.fill('555-0199');
    await form.website.fill('https://contoso.example.test');
    await form.choose(form.category, 'Manufacturing');
    await form.choose(form.paymentTerms, 'Net 60');
    await form.save.click();

    const request = await api.waitForRequest('POST', '/Customer/CreateCustomer');
    expect(request.body).toMatchObject({
      customer_name: 'Contoso Circuits',
      customer_description: 'Board-level assembly partner',
      general_email: 'hello@contoso.example.test',
      phone: '555-0199',
      website: 'https://contoso.example.test',
      category: 'Manufacturing',
      payment_terms: PaymentTermKeys.Net60,
      is_taxable: false,
      tax_rate: 0,
    });
    await expect(page).toHaveURL(/\/erp\/customers\/?$/);
  });

  test('cannot be saved until the required fields are filled', async ({ page }) => {
    const form = new CustomerFormPage(page);

    await form.customerName.fill('Contoso Circuits');
    await expect(form.save).toBeDisabled();

    await form.generalEmail.fill('hello@contoso.example.test');
    await form.phone.fill('555-0199');
    await form.choose(form.category, 'Manufacturing');
    await expect(form.save).toBeDisabled();

    await form.choose(form.paymentTerms, 'Net 30');
    await expect(form.save).toBeEnabled();
  });

  test('shows an error and stays on the form when the API rejects it', async ({ page, api }) => {
    api.on('POST', '/Customer/CreateCustomer', fail(-3, 'Duplicate customer'));
    const form = new CustomerFormPage(page);

    await form.customerName.fill('Contoso Circuits');
    await form.generalEmail.fill('hello@contoso.example.test');
    await form.phone.fill('555-0199');
    await form.choose(form.category, 'Manufacturing');
    await form.choose(form.paymentTerms, 'Net 30');
    await form.save.click();

    await expect(form.failedAlert).toBeVisible();
    await expect(page).toHaveURL(/\/erp\/customers\/new$/);
  });
});

test.describe('edit customer', () => {
  test('loads the customer and saves changes', async ({ page, api }) => {
    const customer = buildCustomer({ customer_name: 'Acme Components', payment_terms: PaymentTermKeys.Net30 });
    mockCustomerRecord(api, customer);
    api.on('PUT', '/Customer/UpdateCustomer', (req) => ok({ ...customer, ...(req.body as object) }));
    const form = new CustomerFormPage(page);

    await page.goto(`/erp/customers/edit/${customer.guid}`);
    await expect(form.customerName).toHaveValue('Acme Components');
    await expect(form.phone).toHaveValue(customer.phone!);

    await form.customerName.fill('Acme Components Ltd');
    await expect(form.save).toBeEnabled();
    await form.save.click();

    const request = await api.waitForRequest('PUT', '/Customer/UpdateCustomer');
    expect(request.body).toMatchObject({
      id: customer.id,
      customer_name: 'Acme Components Ltd',
      general_email: customer.general_email,
      payment_terms: PaymentTermKeys.Net30,
    });
    await expect(form.savedAlert).toBeVisible();
  });

  test('cannot be saved while a required field is empty', async ({ page, api }) => {
    const customer = buildCustomer({ customer_name: 'Acme Components', payment_terms: PaymentTermKeys.Net30 });
    mockCustomerRecord(api, customer);
    const form = new CustomerFormPage(page);

    await page.goto(`/erp/customers/edit/${customer.guid}`);
    await expect(form.phone).toHaveValue(customer.phone!);

    await form.phone.fill('');
    await expect(form.save).toBeDisabled();

    await form.phone.fill('555-0142');
    await expect(form.save).toBeEnabled();
  });

  test('a refused delete closes the confirmation and shows the error', async ({ page, api }) => {
    // The confirmation used to stay open with its buttons disabled behind a
    // spinner whenever the delete didn't navigate away (seen with BUG-006).
    const customer = buildCustomer({ customer_name: 'Acme Components' });
    mockCustomerRecord(api, customer);
    api.on('POST', '/Customer/DeleteCustomer', fail(-6, 'Customer has open orders'));
    const form = new CustomerFormPage(page);
    const confirm = page.getByRole('alertdialog', { name: 'Are you sure?' });

    await page.goto(`/erp/customers/edit/${customer.guid}`);
    await page.getByRole('button', { name: 'Delete Record' }).click();
    await confirm.getByRole('button', { name: 'Delete', exact: true }).click();

    await api.waitForRequest('POST', '/Customer/DeleteCustomer');
    await expect(confirm).toBeHidden();
    await expect(form.failedAlert).toBeVisible();
    await expect(page).toHaveURL(`/erp/customers/edit/${customer.guid}`);

    // The dialog works again: nothing is stuck disabled.
    await page.getByRole('button', { name: 'Delete Record' }).click();
    await expect(confirm.getByRole('button', { name: 'Cancel' })).toBeEnabled();
    await expect(confirm.getByRole('button', { name: 'Delete', exact: true })).toBeEnabled();
  });
});

test.describe('view customer', () => {
  test('shows the customer details read-only', async ({ page, api }) => {
    const customer = buildCustomer({ customer_name: 'Acme Components', payment_terms_name: 'Net 30' });
    mockCustomerRecord(api, customer);

    await page.goto(`/erp/customers/view/${customer.guid}`);

    await expect(page.getByRole('heading', { name: 'View Customer' })).toBeVisible();
    await expect(page.getByRole('textbox', { name: 'Customer Name' })).toHaveValue('Acme Components');
    await expect(page.getByRole('textbox', { name: 'Email', exact: true })).toHaveValue(customer.general_email!);
    await expect(page.getByRole('textbox', { name: 'Payment Terms' })).toHaveValue('Net 30');
  });
});
