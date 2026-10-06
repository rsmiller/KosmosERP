import type { Locator, Page } from '@playwright/test';
import { editGridCell } from './controls';

export class ARListPage {
  constructor(private readonly page: Page) {}

  /** "Create Invoice" in the Ready to Invoice grid row for an order. */
  createInvoiceButton(orderNumber: string | number): Locator {
    return this.page
      .getByRole('row')
      .filter({ hasText: String(orderNumber) })
      .getByRole('button', { name: 'Create Invoice' })
      .first();
  }
}

/** /erp/ar/new/[orderGuid]: invoice an order's lines. */
export class ARInvoiceFromOrderPage {
  readonly save: Locator;

  constructor(private readonly page: Page) {
    this.save = page.getByRole('button', { name: 'Save and Print Invoice' });
  }

  line(text: string): Locator {
    return this.page.getByRole('row').filter({ hasText: text });
  }

  async setUnitsToInvoice(lineText: string, units: number) {
    await editGridCell(this.line(lineText), 'invoice_qty', units);
  }
}
