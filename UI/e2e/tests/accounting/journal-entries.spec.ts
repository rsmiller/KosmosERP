import { test, expect } from '../../fixtures/test';
import { fail, ok } from '../../mocks/envelope';
import { buildJournalEntry } from '../../factories/accounting';
import { buildWorld, type World } from '../../factories/world';
import { mockWorld } from '../../mocks/kits/world';
import { JournalEntryFormPage } from '../../pages/journal-entry-pages';

const TODAY = new Date('2026-03-10T10:00:00');

test.describe('new journal entry', () => {
  let world: World;
  let form: JournalEntryFormPage;

  test.beforeEach(async ({ page, api }) => {
    await page.clock.setFixedTime(TODAY);
    world = mockWorld(api, buildWorld());
    form = new JournalEntryFormPage(page);
    await page.goto('/erp/journalentries/new');
    await expect(page.getByRole('heading', { name: 'New Journal Entry' })).toBeVisible();
  });

  test('creates a balanced two-line entry and opens it for editing', async ({ page, api }) => {
    const [revenue, cash] = world.chartOfAccounts;
    // The page opens the created entry, so answer with one that exists.
    api.on('POST', '/JournalEntry/CreateJournalEntry', (req) => ok({ ...world.journalEntries[0], ...(req.body as object) }));

    await form.description.fill('Cash sale correction');
    await form.addLine.click();
    await form.addLine.click();
    await form.fillLine(0, { accountId: cash.id, debit: 250, description: 'Cash in' });
    await form.fillLine(1, { accountId: revenue.id, credit: 250, description: 'Revenue' });

    await expect(form.balance).toHaveText(/✓ Balanced/);
    await expect(form.save).toBeEnabled();
    await form.save.click();

    const request = await api.waitForRequest('POST', '/JournalEntry/CreateJournalEntry');
    expect(request.body).toMatchObject({ description: 'Cash sale correction', reference_type: 1, fiscal_period: '2026-03' });
    const lines = (request.body as { lines: Record<string, unknown>[] }).lines;
    expect(lines.map((l) => [Number(l.chart_of_account_id), Number(l.debit_amount), Number(l.credit_amount), l.description])).toEqual([
      [cash.id, 250, 0, 'Cash in'],
      [revenue.id, 0, 250, 'Revenue'],
    ]);

    await expect(page).toHaveURL(`/erp/journalentries/edit/${world.journalEntries[0].guid}`);
  });

  test('an unbalanced entry cannot be saved', async () => {
    const [revenue, cash] = world.chartOfAccounts;

    await form.addLine.click();
    await form.addLine.click();
    await form.fillLine(0, { accountId: cash.id, debit: 300, description: 'Cash in' });
    await form.fillLine(1, { accountId: revenue.id, credit: 250, description: 'Revenue' });

    await expect(form.balance).toHaveText(/✗ Out of Balance/);
    await expect(form.save).toBeDisabled();
  });

  test('a single line cannot be saved', async () => {
    await form.addLine.click();
    await form.fillLine(0, { accountId: world.chartOfAccounts[1].id, debit: 100, description: 'Lonely line' });

    await expect(form.save).toBeDisabled();
  });
});

test.describe('existing journal entry', () => {
  test('posting a draft entry marks it posted and offers reversal', async ({ page, api }) => {
    const world = mockWorld(api, buildWorld());
    const entry = world.journalEntries[0]; // a balanced draft
    // Like the API, posting flips the stored entry; the page reloads it afterwards.
    api.on('POST', '/JournalEntry/PostJournalEntry', () => {
      entry.is_posted = true;
      return ok(true);
    });
    const form = new JournalEntryFormPage(page);

    await page.goto(`/erp/journalentries/edit/${entry.guid}`);
    await form.post.click();

    const request = await api.waitForRequest('POST', '/JournalEntry/PostJournalEntry');
    expect(request.body).toMatchObject({ id: entry.id });
    await expect(form.reverse).toBeVisible();
    await expect(form.post).toBeHidden();
  });

  test('reversing a posted entry opens the reversing entry', async ({ page, api }) => {
    await page.clock.setFixedTime(TODAY);
    const world = buildWorld();
    const entry = world.journalEntries[0];
    entry.is_posted = true;
    const reversal = buildJournalEntry({ description: `Reversal of ${entry.entry_number}`, is_posted: true });
    world.journalEntries.push(reversal);
    mockWorld(api, world);
    api.on('POST', '/JournalEntry/ReverseJournalEntry', () => ok(reversal));
    const form = new JournalEntryFormPage(page);

    await page.goto(`/erp/journalentries/edit/${entry.guid}`);
    await expect(form.post).toBeHidden();
    await form.reverse.click();

    const request = await api.waitForRequest('POST', '/JournalEntry/ReverseJournalEntry');
    expect(request.body).toMatchObject({ id: entry.id, reversal_description: `Reversal of ${entry.entry_number}` });
    await expect(page).toHaveURL(`/erp/journalentries/view/${reversal.guid}`);
  });

  test('a failed post leaves the entry as a draft', async ({ page, api }) => {
    const world = mockWorld(api, buildWorld());
    api.on('POST', '/JournalEntry/PostJournalEntry', fail(-6, 'Fiscal period closed'));
    const form = new JournalEntryFormPage(page);

    await page.goto(`/erp/journalentries/edit/${world.journalEntries[0].guid}`);
    await form.post.click();

    await api.waitForRequest('POST', '/JournalEntry/PostJournalEntry');
    await expect(form.post).toBeVisible();
    await expect(form.reverse).toBeHidden();
    // The page never shows the failure message (BUG-020; pinned in known-bugs.spec.ts).
  });
});
