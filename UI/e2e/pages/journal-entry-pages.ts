import type { Locator, Page } from '@playwright/test';
import { editGridCell, gridRow } from './controls';

export interface JournalLine {
  accountId: number;
  debit?: number;
  credit?: number;
  description: string;
}

/** Shared by /erp/journalentries/new and /erp/journalentries/edit/[id]. */
export class JournalEntryFormPage {
  readonly description: Locator;
  readonly addLine: Locator;
  readonly save: Locator;
  readonly post: Locator;
  readonly reverse: Locator;
  readonly balance: Locator;
  readonly failedAlert: Locator;

  constructor(private readonly page: Page) {
    this.description = page.getByRole('textbox', { name: 'Description' });
    this.addLine = page.getByRole('button', { name: 'Add Line' });
    this.save = page.getByRole('button', { name: 'Save Record' });
    this.post = page.getByRole('button', { name: 'Post Entry' });
    this.reverse = page.getByRole('button', { name: 'Reverse Entry' });
    this.balance = page.getByText(/Balanced|Out of Balance/);
    this.failedAlert = page.getByText('Record could not be saved!');
  }

  /**
   * Fill a line's cells in the order a user would: account, amounts, then
   * description. (The Account column is a free-text id cell; see bugs.md.)
   */
  async fillLine(index: number, line: JournalLine) {
    const row = gridRow(this.page, index);
    await editGridCell(row, 'chart_of_account_id', line.accountId);
    if (line.debit) await editGridCell(row, 'debit_amount', line.debit);
    if (line.credit) await editGridCell(row, 'credit_amount', line.credit);
    await editGridCell(row, 'description', line.description);
  }
}
