This is a [Next.js](https://nextjs.org) project bootstrapped with [`create-next-app`](https://nextjs.org/docs/app/api-reference/cli/create-next-app).

## Getting Started

### Environment Setup

1. Copy the environment example file:
   ```bash
   cp .env.example .env.local
   ```

2. Update the `NEXT_PUBLIC_API_BASE_URL` in `.env.local` to point to your API backend (default is `http://localhost:5213`)

### Docker Deployment

When deploying with Docker, you can inject the environment variable in several ways:

#### Option 1: Docker Compose
```yaml
services:
  ui:
    build: .
    environment:
      - NEXT_PUBLIC_API_BASE_URL=https://api.yourdomain.com
```

#### Option 2: Docker Run
```bash
docker run -e NEXT_PUBLIC_API_BASE_URL=https://api.yourdomain.com your-image
```

#### Option 3: Environment File
```bash
# Create .env.production
echo "NEXT_PUBLIC_API_BASE_URL=https://api.yourdomain.com" > .env.production

# Use with Docker
docker run --env-file .env.production your-image
```

**Important**: The `NEXT_PUBLIC_` prefix is required for Next.js to expose the variable to the browser/client-side code.

### Running the Development Server

First, run the development server:

```bash
npm run dev
# or
yarn dev
# or
pnpm dev
# or
bun dev
```

Open [http://localhost:3000](http://localhost:3000) with your browser to see the result.

You can start editing the page by modifying `app/page.tsx`. The page auto-updates as you edit the file.

This project uses [`next/font`](https://nextjs.org/docs/app/building-your-application/optimizing/fonts) to automatically optimize and load [Geist](https://vercel.com/font), a new font family for Vercel.

## End-to-end tests

UI tests use [Playwright](https://playwright.dev) and live in `e2e/`. They run against a production build of the app with the **API mocked in the browser**, so no database, API or seed data is needed.

### Running

```bash
npx playwright install chromium   # once
npm run test:e2e                  # build on :3100 and run everything
npm run test:e2e:ui               # interactive UI mode: watch, time-travel, pick locators
npx playwright test customers     # one file or folder
npx playwright show-report        # last HTML report (traces on retry)
npm run test:e2e:docker           # same suite inside the Playwright image, exactly like CI
```

`E2E_DEV=1 npm run test:e2e` uses `next dev` instead of a production build, which is faster while writing tests. The e2e build goes to `.next-e2e/`, so it never clashes with `npm run dev` or `npm run build`.

CI runs the suite on every PR that touches `UI/` (`.github/workflows/ui_e2e_tests.yml`) and uploads the HTML report as an artifact.

### Layout

| Path | Purpose |
| --- | --- |
| `e2e/fixtures/test.ts` | Extended `test`/`expect`. **Always import from here.** Adds the `api` mock and fails a test on uncaught page errors |
| `e2e/fixtures/users.ts` | Personas (`admin`, `readOnly`). `tests/auth.setup.ts` logs each in once and saves `e2e/.auth/*.json` |
| `e2e/mocks/` | `MockApi` (wraps `page.route`) and the `ok()` / `paged()` / `fail()` response envelopes |
| `e2e/mocks/kits/` | Per-module mock kits (`mockSalesOrders`, `mockLookups`, ...) and `mockWorld`, which serves every module at once |
| `e2e/factories/` | Typed builders for API data (`buildCustomer`, `buildOrder`, ...), typed against `models/*`. `world.ts` wires them into one consistent dataset |
| `e2e/catalog.ts` | Every page under `app/erp`, with its URL and "loaded" marker. Drives `tests/pages/render.spec.ts` |
| `e2e/pages/` | Thin page objects: locators and user actions only, **no assertions** |
| `e2e/tests/` | Specs, grouped by feature |

Tests run as the `admin` persona by default. Use `test.use({ storageState: users.readOnly.storageState })` for the read-only user, or `{ cookies: [], origins: [] }` to start signed out.

### Mocking the API

Mocking is **strict**. Any `/api/v1/**` call without a handler gets a 501, and the test fails with a list of the unmocked endpoints. This keeps tests explicit about the data they depend on.

```ts
import { test, expect } from '../../fixtures/test';
import { ok, paged } from '../../mocks/envelope';
import { buildCustomer } from '../../factories/customer';

test('creates a customer', async ({ page, api }) => {
  api.on('POST', '/Customer/FindCustomer', paged([buildCustomer()]));
  api.on('POST', '/Customer/CreateCustomer', (req) => ok(buildCustomer(req.body as object)));

  // ...drive the UI...

  const request = await api.waitForRequest('POST', '/Customer/CreateCustomer');
  expect(request.body).toMatchObject({ customer_name: 'Contoso' });
});
```

- Paths are matched case-insensitively and ignore the query string and any trailing `/`. Handler functions receive the recorded request (`url`, parsed `body`) if they need to check query parameters.
- `api.abort(method, path)` simulates a network failure.
- `api.allowEmptyFallback()` answers unmocked calls with empty results. Use it only for broad smoke tests.
- To add data for a new area, add a factory in `e2e/factories/` typed against the matching `models/*` interface. Then a model change breaks the e2e type-check (`npm run test:e2e:typecheck`) instead of silently drifting.
- Match the API's wire formats in factories. C# `DateOnly` fields arrive as `2026-03-02`, and `DateTime` fields as `2026-03-02T00:00:00`. Several edit pages throw on the wrong one.

#### The mocked world

For pages that pull from several modules (an order page loads customers, addresses, lookups, tabs...), serve a whole dataset instead of mocking endpoint by endpoint:

```ts
import { buildWorld } from '../../factories/world';
import { mockWorld } from '../../mocks/kits/world';

test('ships an order', async ({ page, api }) => {
  const world = buildWorld();
  world.orders[0].is_complete = true;               // shape the scenario
  mockWorld(api, world);                            // every module kit, strict mode still on
  api.on('POST', '/Shipment/CreateShipmentHeader', ...); // override what the test checks

  await page.goto(`/erp/shipments/new/${world.orders[0].guid}`);
});
```

`buildWorld()` returns one primary record per entity (index 0), all cross-referenced by id: the order's customer, addresses, lines, shipment, invoice and so on. Each kit (`mocks/kits/modules.ts`) can also be installed on its own.

#### The page catalog

`e2e/catalog.ts` lists every page under `app/erp`. `tests/pages/render.spec.ts` loads each one against `mockWorld` and runs its `checks`: grid rows, form field values and button states, all read from the World. It also fails on unmocked calls, page errors and permission redirects. **Every new page gets a catalog entry**, e.g.:

```ts
{
  id: 'vendors/view', kind: 'view',
  path: (w) => `/erp/vendors/view/${guid(w.vendors[0])}`,
  ready: heading('View Vendor'),
  checks: [field('Vendor Name', (w) => w.vendors[0].vendor_name)],
}
```

The browser runs in `America/Chicago` (`timezoneId` in `playwright.config.ts`), so dates render the same on every machine, in Docker and in CI.

### Writing tests

- Locate elements the way users do: `getByRole` / `getByLabel` / `getByText`. If an element has no accessible name (for example an icon-only button), give it an `aria-label` in the app rather than reaching for CSS or `data-testid`. Keep `data-testid` for cases with no sensible accessible name.
- Use web-first assertions (`await expect(locator).toBeVisible()`), never `waitForTimeout`.
- Known app bugs are tracked in `bugs.md` at the repo root. Pin one with `knownBug('BUG-0xx: …')` in `tests/known-bugs.spec.ts` (the test asserts the *correct* behavior), or with `fixme` on a catalog entry. The report then shows it, and the test flags itself once the bug is fixed. `E2E_SHOW_KNOWN_BUGS=1` turns the markers off so you can check each one fails for the reason its entry gives.
- Drive app controls with the helpers in `e2e/pages/controls.ts`:
  - `chooseOption` for lookup comboboxes
  - `searchAndChoose` for server-searched pickers; it waits for the search response before clicking
  - `setDate` for react-datepicker
  - `editGridCell` and `gridRow` for ag-grid
  - `dataListValue` for DataList forms

  They encode the app's quirks once, so specs don't have to.
- Freeze the clock (`page.clock.setFixedTime(...)`) when a page defaults dates to today.
- Each worker loads the app once before its tests (`warmBrowser` in `fixtures/test.ts`), so the first test in each worker doesn't pay for downloading and compiling the app's bundles in a fresh browser.

## Learn More

To learn more about Next.js, take a look at the following resources:

- [Next.js Documentation](https://nextjs.org/docs) - learn about Next.js features and API.
- [Learn Next.js](https://nextjs.org/learn) - an interactive Next.js tutorial.

You can check out [the Next.js GitHub repository](https://github.com/vercel/next.js) - your feedback and contributions are welcome!

## Deploy on Vercel

The easiest way to deploy your Next.js app is to use the [Vercel Platform](https://vercel.com/new?utm_medium=default-template&filter=next.js&utm_source=create-next-app&utm_campaign=create-next-app-readme) from the creators of Next.js.

Check out our [Next.js deployment documentation](https://nextjs.org/docs/app/building-your-application/deploying) for more details.
