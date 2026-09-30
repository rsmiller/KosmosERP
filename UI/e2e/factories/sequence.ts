let counter = 0;

/** Deterministic, per-worker unique number for ids and names in factory data. */
export function nextId(): number {
  counter += 1;
  return counter;
}

/** Stable, readable GUID-shaped string derived from an id. */
export function guidFor(prefix: string, id: number): string {
  const hex = id.toString(16).padStart(12, '0');
  const tag = Buffer.from(prefix).toString('hex').padEnd(8, '0').slice(0, 8);
  return `${tag}-0000-4000-8000-${hex}`;
}

/** .NET serializes DateTime without a zone suffix. */
export const FIXED_DATE = '2026-03-02T15:00:00';

/** The BaseDto fields every entity carries: id, guid and audit columns. */
export function baseDto(prefix: string, id: number = nextId()) {
  return {
    id,
    guid: guidFor(prefix, id),
    is_deleted: false,
    created_on: FIXED_DATE,
    created_on_string: '2026-03-02',
    created_on_timezone: 'UTC',
    created_by: 'Ada Admin',
  };
}

/** C# DateOnly fields serialize as a bare date; DateTime fields carry a time. */
export const DATE_ONLY = '2026-03-02';
export const DUE_DATE_ONLY = '2026-04-01';
