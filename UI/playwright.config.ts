import { defineConfig, devices } from '@playwright/test';

const PORT = Number(process.env.E2E_PORT || 3100);
const BASE_URL = `http://localhost:${PORT}`;
const isCI = !!process.env.CI;

/**
 * Build-time env for the app under test. NEXT_PUBLIC_* values are inlined by
 * `next build`, and process env takes precedence over a developer's .env.local.
 *
 * The API base URL is the UI's own origin: every /api/v1/** call is fulfilled
 * by the MockApi fixture (e2e/mocks/mock-api.ts), so no CORS or DNS is involved
 * and nothing ever leaves the browser.
 */
const appEnv = {
  NEXT_PUBLIC_AUTH_METHOD: 'database',
  NEXT_PUBLIC_API_BASE_URL: BASE_URL,
  NEXT_DIST_DIR: '.next-e2e',
  NEXT_TELEMETRY_DISABLED: '1',
};

// E2E_DEV=1 runs against `next dev` for a faster edit/re-run loop locally.
const serverCommand = process.env.E2E_DEV
  ? `npx next dev -p ${PORT}`
  : `npx next build && npx next start -p ${PORT}`;

export default defineConfig({
  testDir: './e2e/tests',
  fullyParallel: true,
  forbidOnly: isCI,
  retries: isCI ? 2 : 0,
  workers: isCI ? '50%' : undefined,
  reporter: isCI
    ? [['github'], ['html', { open: 'never' }]]
    : [['list'], ['html', { open: 'never' }]],

  expect: { timeout: 10_000 },

  use: {
    baseURL: BASE_URL,
    // Pin the time zone so dates render identically on dev machines, in Docker Don't add
    // `locale` here: on Windows it made the first page in each worker take
    // 20s+ and time out; the default (en-US) is what we want anyway.
    timezoneId: 'America/Chicago',
    trace: 'on-first-retry',
    screenshot: 'only-on-failure',
    video: 'retain-on-failure',
  },

  projects: [
    {
      name: 'setup',
      testMatch: /.*\.setup\.ts/,
    },
    {
      name: 'chromium',
      use: {
        ...devices['Desktop Chrome'],
        storageState: 'e2e/.auth/admin.json',
      },
      dependencies: ['setup'],
    },
  ],

  webServer: {
    command: serverCommand,
    url: BASE_URL,
    env: appEnv,
    reuseExistingServer: !isCI,
    timeout: 300_000,
    stdout: 'ignore',
    stderr: 'pipe',
  },
});
