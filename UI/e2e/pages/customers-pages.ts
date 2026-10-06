import type { Locator, Page } from '@playwright/test';

export class CustomersListPage {
  readonly heading: Locator;
  readonly newCustomer: Locator;
  readonly grid: Locator;

  constructor(private readonly page: Page) {
    this.heading = page.getByRole('heading', { name: 'Customers', level: 1 });
    this.newCustomer = page.getByRole('button', { name: 'New Customer' });
    this.grid = page.getByRole('treegrid').or(page.getByRole('grid'));
  }

  async goto(): Promise<void> {
    await this.page.goto('/erp/customers');
  }

  row(text: string): Locator {
    return this.grid.getByRole('row').filter({ hasText: text });
  }

  viewButton(customerName: string): Locator {
    return this.page.getByRole('button', { name: `View ${customerName}` });
  }

  editButton(customerName: string): Locator {
    return this.page.getByRole('button', { name: `Edit ${customerName}` });
  }
}

/** Shared by /erp/customers/new and /erp/customers/edit/[id]. */
export class CustomerFormPage {
  readonly customerName: Locator;
  readonly description: Locator;
  readonly generalEmail: Locator;
  readonly accountingEmail: Locator;
  readonly phone: Locator;
  readonly fax: Locator;
  readonly website: Locator;
  readonly category: Locator;
  readonly paymentTerms: Locator;
  readonly save: Locator;
  readonly savedAlert: Locator;
  readonly failedAlert: Locator;

  constructor(private readonly page: Page) {
    this.customerName = page.getByRole('textbox', { name: 'Customer Name' });
    this.description = page.getByRole('textbox', { name: 'Description' });
    // "General Email" on the new page, "Email" on the edit page.
    this.generalEmail = page.getByRole('textbox', { name: /^(General )?Email$/ });
    this.accountingEmail = page.getByRole('textbox', { name: 'Accounting Email' });
    this.phone = page.getByRole('textbox', { name: 'Phone' });
    this.fax = page.getByRole('textbox', { name: 'Fax' });
    this.website = page.getByRole('textbox', { name: 'Website' });
    this.category = page.getByRole('combobox', { name: 'Category' });
    this.paymentTerms = page.getByRole('combobox', { name: 'Payment Terms' });
    this.save = page.getByRole('button', { name: 'Save Record' });
    this.savedAlert = page.getByText('Record saved!');
    this.failedAlert = page.getByText('Record could not be saved!');
  }

  /**
   * Type a combobox option's visible label, the way a user searches, then pick it.
   * (Lookup comboboxes used to filter on the stored key, so typing found nothing: BUG-007.)
   */
  async choose(combobox: Locator, option: string): Promise<void> {
    await combobox.click();
    await combobox.fill(option);
    await this.page.getByRole('option', { name: option, exact: true }).click();
  }
}
