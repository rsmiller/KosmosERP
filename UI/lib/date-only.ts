import { format, isValid, parse } from "date-fns";

/**
 * Dates for the API's DateOnly fields (order_date, required_date, start_date,
 * end_date, next_date, planned_start_date, planned_complete_date,
 * actual_completed_on, invoice_date, invoice_due_date, expected_close, paid_on).
 *
 * They arrive as "yyyy-MM-dd". Both `new Date(value)` and date-fns
 * `format(value, ...)` read that as UTC midnight, which is still the previous
 * day anywhere west of UTC, so US users saw every date a day early (BUG-011).
 * These read it as a calendar day in the browser's time zone instead.
 */

/** "2026-04-01" (or "2026-04-01T..."), or a Date, as a local Date; undefined if empty or invalid. */
export function parseDateOnly(value: string | Date | null | undefined): Date | undefined {
  if (!value) return undefined;
  if (value instanceof Date) return isValid(value) ? value : undefined;

  const date = parse(value.slice(0, 10), "yyyy-MM-dd", new Date());
  return isValid(date) ? date : undefined;
}

/** Format a DateOnly value; "" when it's empty or invalid. */
export function formatDateOnly(value: string | Date | null | undefined, pattern = "MM/dd/yyyy"): string {
  const date = parseDateOnly(value);
  return date ? format(date, pattern) : "";
}
