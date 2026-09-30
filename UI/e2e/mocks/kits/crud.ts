import { guidFor, nextId } from '../../factories/sequence';
import { fail, ok, paged } from '../envelope';
import type { MockApi } from '../mock-api';

export interface CrudEndpoints {
  find?: string;
  get?: string;
  getByGuid?: string;
  create?: string;
  update?: string;
  delete?: string;
}

type Row = { id?: number; guid?: string | null };

/** KosmosERP's usual names: /Customer/FindCustomer, /Customer/GetCustomer, /Customer/GetCustomerByGuid, ... */
export function standardEndpoints(controller: string, entity: string = controller): Required<CrudEndpoints> {
  return {
    find: `/${controller}/Find${entity}`,
    get: `/${controller}/Get${entity}`,
    getByGuid: `/${controller}/Get${entity}ByGuid`,
    create: `/${controller}/Create${entity}`,
    update: `/${controller}/Update${entity}`,
    delete: `/${controller}/Delete${entity}`,
  };
}

/**
 * Serve `records` as an in-memory table behind the given endpoints:
 * Find returns every record (paged); Get/GetByGuid look up ?id= / ?guid= and
 * return a failure envelope for unknown keys; Create/Update/Delete echo the
 * request back (merged over the existing record for Update) so pages proceed.
 */
export function mockCrud<T extends Row>(api: MockApi, endpoints: CrudEndpoints, records: T[]): void {
  const byId = (id: number) => records.find((r) => r.id === id);

  if (endpoints.find) {
    // Like the API, a non-empty `wildcard` narrows results (any text field containing it).
    api.on('POST', endpoints.find, (req) => {
      const wildcard = String((req.body as { wildcard?: string } | undefined)?.wildcard ?? '').toLowerCase();
      if (!wildcard) return paged(records);
      return paged(
        records.filter((r) =>
          Object.values(r).some((v) => (typeof v === 'string' || typeof v === 'number') && String(v).toLowerCase().includes(wildcard)),
        ),
      );
    });
  }

  if (endpoints.get) {
    const path = endpoints.get;
    api.on('GET', path, (req) => {
      const id = Number(req.url.searchParams.get('id'));
      const record = byId(id);
      return record ? ok(record) : fail(-404, `No record with id ${id} for ${path}`);
    });
  }

  if (endpoints.getByGuid) {
    const path = endpoints.getByGuid;
    api.on('GET', path, (req) => {
      const guid = req.url.searchParams.get('guid');
      const record = records.find((r) => r.guid === guid);
      return record ? ok(record) : fail(-404, `No record with guid ${guid} for ${path}`);
    });
  }

  if (endpoints.create) {
    api.on('POST', endpoints.create, (req) => {
      const id = nextId();
      return ok({ ...(req.body as object), id, guid: guidFor('new', id) });
    });
  }

  if (endpoints.update) {
    api.on('PUT', endpoints.update, (req) => {
      const body = req.body as Row;
      return ok({ ...(byId(Number(body?.id)) ?? {}), ...body });
    });
  }

  if (endpoints.delete) {
    api.on('POST', endpoints.delete, (req) => {
      const body = req.body as Row;
      return ok(byId(Number(body?.id)) ?? body);
    });
  }
}

/** Echo a line/child write (CreateXLine, UpdateXLine, ...) back as a success. */
export function mockEcho(api: MockApi, method: 'POST' | 'PUT' | 'DELETE', path: string): void {
  api.on(method, path, (req) => ok({ id: nextId(), ...(req.body as object) }));
}
