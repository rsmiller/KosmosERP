import { test, expect } from '../../fixtures/test';
import { catalog } from '../../catalog';
import { buildWorld } from '../../factories/world';
import { mockWorld } from '../../mocks/kits/world';

/**
 * Tier 1: every page in the catalog (e2e/catalog.ts) loads against the mocked
 * World and shows that World's data. Each test also fails on any unmocked API
 * call or uncaught page error (fixtures/test.ts).
 */
test.describe('every page renders with realistic data', () => {
  for (const entry of catalog) {
    const run = entry.fixme ? test.fixme : test;

    run(`${entry.kind}: ${entry.id}`, async ({ page, api }) => {
      const world = mockWorld(api, buildWorld());
      const path = entry.path(world);

      await page.goto(path);
      await expect(entry.ready(page, world)).toBeVisible();

      // Let load-time requests (lazy tabs, comboboxes) finish so the strict
      // mock check at teardown sees every call the page makes on load.
      await page.waitForLoadState('networkidle');
      // A permission redirect can flash the page first; make sure we stayed.
      await expect(page).toHaveURL(path);

      for (const check of entry.checks ?? []) {
        await check(page, world);
      }
    });
  }
});
