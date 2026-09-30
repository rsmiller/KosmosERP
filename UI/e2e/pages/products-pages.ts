import type { Locator, Page } from '@playwright/test';

/** /erp/products/edit/[id] (Details tab). */
export class ProductFormPage {
  readonly productName: Locator;
  readonly productClass: Locator;
  readonly identifier1: Locator;
  readonly internalDescription: Locator;
  readonly save: Locator;
  readonly savedAlert: Locator;

  constructor(private readonly page: Page) {
    this.productName = page.getByRole('textbox', { name: 'Product Name' });
    this.productClass = page.getByRole('textbox', { name: 'Product Class' });
    this.identifier1 = page.getByRole('textbox', { name: 'Identifier 1' });
    this.internalDescription = page.getByRole('textbox', { name: 'Internal Description' });
    this.save = page.getByRole('button', { name: 'Save Record' });
    this.savedAlert = page.getByText('Record saved!');
  }
}
