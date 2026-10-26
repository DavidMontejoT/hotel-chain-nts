import { AsyncPipe, CurrencyPipe } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { BehaviorSubject, map, tap } from 'rxjs';

import { Hotel, HotelFilter } from '../../../core/models/hotel.model';
import { ConfirmService } from '../../../core/services/confirm.service';
import { errorMessage, isHandledGlobally } from '../../../core/services/error-message';
import { HotelService } from '../../../core/services/hotel.service';
import { ToastService } from '../../../core/services/toast.service';
import { HotelsView, toHotelsView } from '../../../core/state/hotels-view';
import { HotelSearchComponent } from '../../../shared/components/hotel-search.component';
import { StarRatingComponent } from '../../../shared/components/star-rating.component';

const PAGE_SIZE = 10;

@Component({
  selector: 'app-hotel-list',
  imports: [AsyncPipe, CurrencyPipe, RouterLink, HotelSearchComponent, StarRatingComponent],
  templateUrl: './hotel-list.component.html',
  styleUrl: './hotel-list.component.scss',
})
export class HotelListComponent {
  private readonly hotelService = inject(HotelService);
  private readonly confirm = inject(ConfirmService);
  private readonly toast = inject(ToastService);

  /** Current filter + page. Emitting the same value again re-fetches the list (used after a delete). */
  private readonly query$ = new BehaviorSubject<HotelFilter>({ page: 1, pageSize: PAGE_SIZE });
  private lastView: HotelsView | null = null;

  protected readonly page$ = this.query$.pipe(map((q) => q.page ?? 1));
  protected readonly view$ = this.query$.pipe(
    toHotelsView((query) => this.hotelService.search(query), 'No se pudo cargar el listado.'),
    tap((view) => (this.lastView = view)),
  );
  protected readonly deleteError = signal<string | null>(null);

  protected onFilter(filter: HotelFilter): void {
    this.query$.next({ ...filter, page: 1, pageSize: PAGE_SIZE });
  }

  protected goTo(page: number): void {
    this.query$.next({ ...this.query$.value, page });
  }

  protected async remove(hotel: Hotel): Promise<void> {
    const confirmed = await this.confirm.ask({
      title: 'Eliminar hotel',
      message: `¿Seguro que quieres eliminar "${hotel.name}"? Esta acción no se puede deshacer.`,
      confirmText: 'Sí, eliminar',
      danger: true,
    });
    if (!confirmed) return;

    this.deleteError.set(null);
    this.hotelService.delete(hotel.id).subscribe({
      next: () => {
        this.toast.success(`Se eliminó "${hotel.name}".`);
        this.refreshAfterDelete();
      },
      error: (err) => {
        if (!isHandledGlobally(err)) this.deleteError.set(errorMessage(err));
        this.refreshAfterDelete();
      },
    });
  }

  /** Re-fetches the list; if the last row of a page was removed, steps back one page. */
  private refreshAfterDelete(): void {
    const query = this.query$.value;
    const page = query.page ?? 1;
    const wasLastRow = this.lastView?.items.length === 1 && page > 1;
    this.query$.next({ ...query, page: wasLastRow ? page - 1 : page });
  }
}
