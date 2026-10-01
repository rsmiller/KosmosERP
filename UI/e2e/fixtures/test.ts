import * as fs from 'fs';
import { test as base, expect } from '@playwright/test';
import { MockApi } from '../mocks/mock-api';

/**
 * Pre-existing app bugs that surface as page errors. They are reported as a
 * `known-issue` annotation instead of failing every test. Keep this list short
 * and remove an entry as soon as the underlying bug is fixed.
 */
const KNOWN_PAGE_ERRORS: { pattern: RegExp; reason: string }[] = [];

type Fixtures = {
  /** Mocked KosmosERP API. Installed for every test; strict unless `allowEmptyFallback()` is called. */
  api: MockApi;
  /** Uncaught exceptions thrown in the page. Any entry fails the test at teardown. */
  pageErrors: Error[];
};

type WorkerFixtures = {
  /** Loads the app once per worker so the first test doesn't pay the cold-browser cost. */
  warmBrowser: void;
};

/**
 * Specs import `test`/`expect` from here, never from @playwright/test directly,
 * so every test gets API mocking and the page-error guard.
 */
export const test = base.extend<Fixtures, WorkerFixtures>({
  // A fresh browser has to download and compile the app's large bundles (Chakra,
  // ag-grid) on its first page. With many workers starting at once that first
  // navigation can take 20-30s and time out whichever test gets it, so each
  // worker loads a grid page once, outside any test's time budget.
  warmBrowser: [
    async ({ browser }, use, workerInfo) => {
      const storageState = fs.existsSync('e2e/.auth/admin.json') ? 'e2e/.auth/admin.json' : undefined;
      const context = await browser.newContext({ baseURL: workerInfo.project.use.baseURL, storageState });
      const page = await context.newPage();
      await page.route('**/api/v1/**', (route) => route.fulfill({ json: { success: true, resultCode: 0, data: [] } }));
      await page.goto(storageState ? '/erp/customers' : '/login/database');
      await page.waitForLoadState('networkidle');
      await context.close();
      await use();
    },
    { scope: 'worker', auto: true, timeout: 120_000 },
  ],

  api: [
    async ({ page }, use) => {
      const api = new MockApi(page);
      await api.install();
      await use(api);
      expect(
        api.describeUnhandled(),
        'The page called API endpoints that have no e2e mock. Register them with api.on(...)',
      ).toEqual([]);
      expect(
        api.describeUnauthenticated(),
        'The page called protected API endpoints without a token (the real API answers 401). Does an effect run before auth is ready?',
      ).toEqual([]);
    },
    { auto: true },
  ],

  pageErrors: [
    async ({ page }, use, testInfo) => {
      const errors: Error[] = [];
      const seenKnown = new Set<string>();

      page.on('pageerror', (error) => {
        const known = KNOWN_PAGE_ERRORS.find((k) => k.pattern.test(error.message));
        if (!known) {
          errors.push(error);
        } else if (!seenKnown.has(known.reason)) {
          seenKnown.add(known.reason);
          testInfo.annotations.push({ type: 'known-issue', description: known.reason });
        }
      });

      await use(errors);
      expect(errors.map((e) => e.message), 'Uncaught exceptions in the page').toEqual([]);
    },
    { auto: true },
  ],
});

export { expect };
