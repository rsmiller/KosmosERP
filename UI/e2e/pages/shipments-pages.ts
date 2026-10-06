import type { Locator, Page } from '@playwright/test';
import { chooseOption, editGridCell } from './controls';

/** Shared by /erp/shipments/new/[orderGuid] and /erp/shipments/edit/[id]. */
export class ShipmentFormPage {
  readonly shippingMethod: Locator;
  readonly freightCarrier: Locator;
  readonly freightCharge: Locator;
  readonly shipAttention: Locator;
  readonly save: Locator;
  readonly savedAlert: Locator;
  readonly failedAlert: Locator;
  readonly releaseDialog: Locator;
  readonly removeLineDialog: Locator;

  constructor(private readonly page: Page) {
    this.shippingMethod = page.getByRole('combobox', { name: 'Shipping Method' });
    this.freightCarrier = page.getByRole('combobox', { name: 'Freight Carrier' });
    this.freightCharge = page.getByRole('spinbutton', { name: 'Freight Charge' });
    this.shipAttention = page.getByRole('textbox', { name: 'Ship Attention' });
    this.save = page.getByRole('button', { name: 'Save Record' });
    this.savedAlert = page.getByText('Record saved!');
    this.failedAlert = page.getByText('Record could not be saved!');
    this.releaseDialog = page.getByRole('alertdialog', { name: 'Release Shipment?' }).or(page.getByRole('dialog', { name: 'Release Shipment?' }));
    this.removeLineDialog = page.getByRole('alertdialog', { name: 'Remove line?' }).or(page.getByRole('dialog', { name: 'Remove line?' }));
  }

  line(text: string): Locator {
    return this.page.getByRole('row').filter({ hasText: text });
  }

  /** The edit page's per-line Delete button (opens the Remove line? dialog). */
  deleteLineButton(lineText: string): Locator {
    return this.line(lineText).getByRole('button', { name: `Delete line ${lineText}` });
  }

  async chooseFreightCarrier(label: string) {
    await chooseOption(this.page, this.freightCarrier, label);
  }

  /** Units To Ship on the create page (column `units_shipped`). */
  async setUnitsToShip(lineText: string, units: number) {
    await editGridCell(this.line(lineText), 'units_shipped', units);
  }
}

export class ShipmentsListPage {
  readonly readyToShip: Locator;

  constructor(private readonly page: Page) {
    this.readyToShip = page.getByRole('heading', { name: 'Ready To Ship' });
  }

  newShipmentButton(orderNumber: string | number): Locator {
    return this.page.getByRole('row').filter({ hasText: String(orderNumber) }).getByRole('button', { name: 'New Shipment' });
  }
}
