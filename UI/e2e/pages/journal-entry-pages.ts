import { expect, type Locator, type Page } from '@playwright/test';
import { editGridCell, gridRow } from './controls';

export interface JournalLine {
  /** The chart of accounts entry to pick; its number is typed, then "number - name" is chosen. */
  account: { account_number?: string; account_name?: string };
  debit?: number;
  credit?: number;
  description: string;
}

/** Shared by /erp/journalentries/new and /erp/journalentries/edit/[id]. */
export class JournalEntryFormPage {
  readonly description: Locator;
  readonly addLine: Locator;
  readonly save: Locator;
  readonly deleteRecord: Locator;
  readonly post: Locator;
  readonly reverse: Locator;
  readonly balance: Locator;
  readonly savedAlert: Locator;
  readonly failedAlert: Locator;

  constructor(private readonly page: Page) {
    this.description = page.getByRole('textbox', { name: 'Description' });
    this.addLine = page.getByRole('button', { name: 'Add Line' });
    this.save = page.getByRole('button', { name: 'Save Record' });
    this.deleteRecord = page.getByRole('button', { name: 'Delete Record' });
    this.post = page.getByRole('button', { name: 'Post Entry' });
    this.reverse = page.getByRole('button', { name: 'Reverse Entry' });
    this.balance = page.getByText(/Balanced|Out of Balance/);
    this.savedAlert = page.getByText('Record saved!');
    this.failedAlert = page.getByText('Record could not be saved!');
  }

  /** "1010 - Operating Cash", as the Account cell and its options show it. */
  static accountLabel(account: JournalLine['account']): string {
    return `${account.account_number} - ${account.account_name}`;
  }

  /**
   * Pick a line's account with the Account cell's picker. `search` is what's
   * typed (the account number by default; a name works too).
   */
  async chooseAccount(index: number, account: JournalLine['account'], search = account.account_number!) {
    const cell = gridRow(this.page, index).locator('[col-id="chart_of_account_id"]');
    const label = JournalEntryFormPage.accountLabel(account);
    await cell.dblclick();
    await this.page.getByRole('combobox', { name: 'Account' }).fill(search);
    await this.page.getByRole('option', { name: label, exact: true }).click();
    // Synchronization, not a test assertion: wait until the pick has landed.
    await expect(cell).toHaveText(label);
  }

  /** Fill a line's cells in the order a user would: account, amounts, then description. */
  async fillLine(index: number, line: JournalLine) {
    const row = gridRow(this.page, index);
    await this.chooseAccount(index, line.account);
    if (line.debit) await editGridCell(row, 'debit_amount', line.debit);
    if (line.credit) await editGridCell(row, 'credit_amount', line.credit);
    await editGridCell(row, 'description', line.description);
  }
}
