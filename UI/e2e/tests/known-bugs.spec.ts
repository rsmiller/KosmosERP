import { test } from '../fixtures/test';

/**
 * Open app bugs pinned as tests (details in bugs.md at the repo root). Each
 * test asserts the CORRECT behavior and is marked with knownBug(), so it passes
 * while the bug exists. When a fix lands the test starts "unexpectedly
 * passing" and fails the run: move it to the right spec and drop knownBug().
 *
 * E2E_SHOW_KNOWN_BUGS=1 turns the markers off so you can see each real failure
 * (and check it fails for the reason in its BUG entry, not something else).
 *
 * None open right now; every pinned bug has been fixed and its test moved.
 */
export const knownBug = (reason: string) => test.fail(!process.env.E2E_SHOW_KNOWN_BUGS, reason);
