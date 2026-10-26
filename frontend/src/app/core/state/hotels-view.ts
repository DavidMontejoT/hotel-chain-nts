import { Observable, OperatorFunction, catchError, map, of, startWith, switchMap } from 'rxjs';

import { Hotel, HotelFilter, PagedResult } from '../models/hotel.model';
import { errorMessage, isHandledGlobally } from '../services/error-message';

/** Everything a hotel list template needs, emitted as one immutable snapshot. */
export interface HotelsView {
  loading: boolean;
  items: Hotel[];
  total: number;
  totalPages: number;
  error: string | null;
}

const LOADING: HotelsView = { loading: true, items: [], total: 0, totalPages: 0, error: null };

/**
 * Turns a stream of filters into a stream of list snapshots. switchMap cancels the previous
 * request, so a slow response can never overwrite the results of a newer search.
 */
export function toHotelsView(
  search: (filter: HotelFilter) => Observable<PagedResult<Hotel>>,
  fallbackError: string,
): OperatorFunction<HotelFilter, HotelsView> {
  return switchMap((filter) =>
    search(filter).pipe(
      map((res): HotelsView => ({ loading: false, items: res.items, total: res.total, totalPages: res.totalPages, error: null })),
      catchError((err: unknown) =>
        of<HotelsView>({ ...LOADING, loading: false, error: isHandledGlobally(err) ? fallbackError : errorMessage(err) }),
      ),
      startWith(LOADING),
    ),
  );
}
