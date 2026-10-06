import type { Page, Request, Route } from '@playwright/test';
import { ok, paged } from './envelope';

export type HttpMethod = 'GET' | 'POST' | 'PUT' | 'PATCH' | 'DELETE';

export interface RecordedRequest {
  method: string;
  /** Path relative to /api/v1, e.g. "/Customer/CreateCustomer". */
  path: string;
  url: URL;
  /** Parsed JSON body (or raw text when it isn't JSON). */
  body: unknown;
  /** Request headers, lower-cased names (e.g. authorization). */
  headers: Record<string, string>;
}

type ReplyFn = (req: RecordedRequest) => unknown;
type Reply = ReplyFn | object | string | number | boolean | null;

interface Handler {
  method: string;
  path: string;
  reply?: Reply;
  status: number;
  abort: boolean;
}

interface Waiter {
  method: string;
  path: string;
  resolve: (req: RecordedRequest) => void;
}

const API_PREFIX = '/api/v1';

/** "/api/v1/Customer/GetCustomer/?id=1" -> "/customer/getcustomer" */
function normalizePath(path: string): string {
  const withoutPrefix = path.toLowerCase().startsWith(API_PREFIX) ? path.slice(API_PREFIX.length) : path;
  const clean = withoutPrefix.split('?')[0].replace(/\/+$/, '');
  return (clean.startsWith('/') ? clean : `/${clean}`).toLowerCase();
}

function label(req: { method: string; path: string }): string {
  return `${req.method} ${req.path}`;
}

/**
 * Endpoints the real API serves without a token. Mirrors PublicEndpoints in
 * Tests/KosmosERP.Tests.Shared/AuthorizationTests.cs; paths are normalized.
 */
const ANONYMOUS_PATHS = [/^\/diagnostics\/health$/, /^\/docs\//, /^\/saml\//, /^\/settings\/getbasesettings$/, /^\/user\/authenticateuser$/];

/** "Bearer <token>", not "Bearer", "Bearer undefined" or "Bearer null". */
function hasBearerToken(req: RecordedRequest): boolean {
  return /^Bearer (?!undefined$|null$)\S+/.test(req.headers.authorization ?? '');
}

/**
 * Intercepts every browser call to /api/v1/** and answers it from handlers the
 * test registered. Strict by default: an unmocked call gets a 501 and is recorded
 * in `unhandled`, which the `api` fixture turns into a test failure naming the
 * missing endpoint.
 */
export class MockApi {
  readonly requests: RecordedRequest[] = [];
  readonly unhandled: RecordedRequest[] = [];

  private handlers: Handler[] = [];
  private waiters: Waiter[] = [];
  private emptyFallback = false;

  constructor(private readonly page: Page) {}

  async install(): Promise<void> {
    await this.page.route(`**${API_PREFIX}/**`, (route, request) => this.handle(route, request));
  }

  /**
   * Answer `method path` with `reply` — a response body, or a function of the
   * recorded request returning one. Later registrations win, so a test can
   * override a default registered by a helper.
   */
  on(method: HttpMethod, path: string, reply: Reply, { status = 200 }: { status?: number } = {}): this {
    this.handlers.unshift({ method, path: normalizePath(path), reply, status, abort: false });
    return this;
  }

  /** Simulate a network failure (connection refused, CORS, offline...). */
  abort(method: HttpMethod, path: string): this {
    this.handlers.unshift({ method, path: normalizePath(path), status: 0, abort: true });
    return this;
  }

  /**
   * Opt out of strict mode: unmocked calls get an empty successful response
   * instead of failing the test. Meant for broad smoke tests only.
   */
  allowEmptyFallback(): this {
    this.emptyFallback = true;
    return this;
  }

  /** All recorded calls to `method path`, oldest first. */
  requestsTo(method: HttpMethod, path: string): RecordedRequest[] {
    const target = normalizePath(path);
    return this.requests.filter((r) => r.method === method && normalizePath(r.path) === target);
  }

  /**
   * Resolves with the first call to `method path` — including one that already
   * happened, so it is safe to call after the triggering action.
   */
  waitForRequest(method: HttpMethod, path: string, { timeout = 10_000 } = {}): Promise<RecordedRequest> {
    const existing = this.requestsTo(method, path)[0];
    if (existing) return Promise.resolve(existing);

    return new Promise((resolve, reject) => {
      const waiter: Waiter = { method, path: normalizePath(path), resolve };
      const timer = setTimeout(() => {
        this.waiters = this.waiters.filter((w) => w !== waiter);
        const seen = this.requests.map(label).join('\n  ') || '(none)';
        reject(new Error(`Timed out after ${timeout}ms waiting for ${method} ${path}.\nRequests seen:\n  ${seen}`));
      }, timeout);
      waiter.resolve = (req) => {
        clearTimeout(timer);
        resolve(req);
      };
      this.waiters.push(waiter);
    });
  }

  describeUnhandled(): string[] {
    return this.unhandled.map(label);
  }

  /**
   * Calls to protected endpoints sent without a token. The mocks answer them,
   * but the real API returns 401, so the page would show nothing (BUG-023).
   * Usually an effect that ran before auth was ready and never re-ran.
   */
  describeUnauthenticated(): string[] {
    const found = this.requests.filter((r) => !ANONYMOUS_PATHS.some((p) => p.test(normalizePath(r.path))) && !hasBearerToken(r));
    return [...new Set(found.map(label))];
  }

  private async handle(route: Route, request: Request): Promise<void> {
    const url = new URL(request.url());
    const recorded: RecordedRequest = {
      method: request.method(),
      path: url.pathname.slice(API_PREFIX.length) || '/',
      url,
      body: parseBody(request),
      headers: request.headers(),
    };
    this.requests.push(recorded);
    this.notifyWaiters(recorded);

    const key = normalizePath(url.pathname);
    const handler = this.handlers.find((h) => h.method === recorded.method && h.path === key);

    if (handler) {
      if (handler.abort) return route.abort('connectionrefused');
      const body = typeof handler.reply === 'function' ? await (handler.reply as ReplyFn)(recorded) : handler.reply;
      return route.fulfill({ status: handler.status, json: body });
    }

    if (this.emptyFallback) {
      const isFind = /\/find[^/]*$/.test(key);
      return route.fulfill({ status: 200, json: isFind ? paged([]) : ok([]) });
    }

    this.unhandled.push(recorded);
    return route.fulfill({
      status: 501,
      json: { success: false, resultCode: -501, exception: { message: `No e2e mock for ${label(recorded)}` } },
    });
  }

  private notifyWaiters(req: RecordedRequest): void {
    const key = normalizePath(req.path);
    for (const waiter of [...this.waiters]) {
      if (waiter.method === req.method && waiter.path === key) {
        this.waiters = this.waiters.filter((w) => w !== waiter);
        waiter.resolve(req);
      }
    }
  }
}

function parseBody(request: Request): unknown {
  const raw = request.postData();
  if (raw == null) return undefined;
  try {
    return JSON.parse(raw);
  } catch {
    return raw;
  }
}
