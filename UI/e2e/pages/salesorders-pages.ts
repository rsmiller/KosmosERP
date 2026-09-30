import type { Locator, Page } from '@playwright/test';
import { chooseOption, searchAndChoose, setDate } from './controls';

/** Shared by /erp/salesorders/new and /erp/salesorders/edit/[id]. */
export class SalesOrderFormPage {
  readonly orderType: Locator;
  readonly customer: Locator;
  readonly requiredDate: Locator;
  readonly poNumber: Locator;
  readonly paymentMethod: Locator;
  readonly shippingMethod: Locator;
  readonly shippingAddress: Locator;
  readonly addLine: Locator;
  readonly lines: Locator;
  readonly save: Locator;
  readonly savedAlert: Locator;
  readonly failedAlert: Locator;

  constructor(private readonly page: Page) {
    this.orderType = page.getByRole('combobox', { name: 'Order Type' });
    this.customer = page.getByRole('combobox', { name: 'Customer' });
    // react-datepicker's input is only named "Select date" (BUG-014); it's the only one on this form.
    this.requiredDate = page.getByRole('textbox', { name: 'Select date' });
    this.poNumber = page.getByRole('textbox', { name: 'PO Number' });
    this.paymentMethod = page.getByRole('combobox', { name: 'Payment Method' });
    this.shippingMethod = page.getByRole('combobox', { name: 'Shipping Method' });
    this.shippingAddress = page.getByRole('combobox', { name: 'Select Shipping Address' });
    this.addLine = page.getByRole('button', { name: 'Add New Line' });
    this.lines = page.getByRole('row');
    this.save = page.getByRole('button', { name: 'Save Record' });
    this.savedAlert = page.getByText('Record saved!');
    this.failedAlert = page.getByText('Record could not be saved!');
  }

  line(text: string): Locator {
    return this.lines.filter({ hasText: text });
  }

  async chooseOrderType(label: string) {
    await chooseOption(this.page, this.orderType, label);
  }

  async chooseCustomer(name: string) {
    await searchAndChoose(this.page, this.customer, name.slice(0, 4), name);
  }

  async chooseShippingAddress(formatted: string) {
    await chooseOption(this.page, this.shippingAddress, formatted);
  }

  async setRequiredDate(mmddyyyy: string) {
    await setDate(this.requiredDate, mmddyyyy);
  }

  async choosePaymentMethod(label: string) {
    await chooseOption(this.page, this.paymentMethod, label);
  }

  async chooseShippingMethod(label: string) {
    await chooseOption(this.page, this.shippingMethod, label);
  }

  /** Add a line through the "Add Order Line" dialog. */
  async addOrderLine({ product, description, quantity }: { product: string; description: string; quantity: number }) {
    await this.addLine.click();
    const dialog = this.page.getByRole('dialog', { name: 'Add Order Line' });
    await searchAndChoose(this.page, dialog.getByRole('combobox', { name: 'Product' }), product.slice(0, 4), product, { inModal: true });
    await dialog.getByRole('textbox', { name: 'Description' }).fill(description);
    await dialog.getByRole('spinbutton', { name: 'Quantity' }).fill(String(quantity));
    await dialog.getByRole('button', { name: 'Add Line' }).click();
    await dialog.waitFor({ state: 'hidden' });
  }

  /** The Delete button in a line's grid row. */
  deleteLineButton(text: string): Locator {
    return this.line(text).getByRole('button', { name: 'Delete' });
  }
}
