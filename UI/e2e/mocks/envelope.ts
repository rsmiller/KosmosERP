import type { ApiResponse, PagedApiResponse } from '@/models/base-models';

/** Successful ApiResponse<T> envelope, as returned by every KosmosERP API endpoint. */
export function ok<T>(data: T): ApiResponse<T> {
  return { success: true, resultCode: 0, exception: null, data };
}

/** Successful PagedApiResponse<T[]> envelope for the Find* endpoints. */
export function paged<T>(
  items: T[],
  { page = 1, pageSize = 50, total = items.length }: { page?: number; pageSize?: number; total?: number } = {},
): PagedApiResponse<T[]> {
  return {
    success: true,
    resultCode: 0,
    exception: null,
    data: items,
    currentPage: page,
    totalResultCount: total,
    totalPages: Math.max(1, Math.ceil(total / pageSize)),
  };
}

/** Business-level failure (HTTP 200, success: false) — how the API reports validation and auth errors. */
export function fail(resultCode = -1, message = 'Request failed'): ApiResponse<never> {
  return { success: false, resultCode, exception: { message }, data: undefined };
}
