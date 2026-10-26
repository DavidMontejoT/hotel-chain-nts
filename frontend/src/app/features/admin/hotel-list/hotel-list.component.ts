import { CurrencyPipe } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';

import { Hotel, HotelFilter } from '../../../core/models/hotel.model';
import { errorMessage } from '../../../core/services/error-message';
import { HotelService } from '../../../core/services/hotel.service';
import { HotelSearchComponent } from '../../../shared/components/hotel-search.component';
import { StarRatingComponent } from '../../../shared/components/star-rating.component';

const PAGE_SIZE = 10;

@Component({
  selector: 'app-hotel-list',
  imports: [CurrencyPipe, RouterLink, HotelSearchComponent, StarRatingComponent],
  templateUrl: './hotel-list.component.html',
  styleUrl: './hotel-list.component.scss',
})
export class HotelListComponent {
  private readonly hotelService = inject(HotelService);

  protected readonly hotels = signal<Hotel[]>([]);
  protected readonly total = signal(0);
  protected readonly page = signal(1);
  protected readonly totalPages = signal(1);
  protected readonly loading = signal(true);
  protected readonly error = signal<string | null>(null);
  protected readonly notice = signal<string | null>(null);
  /** Id del hotel cuya eliminación se está confirmando. */
  protected readonly confirmingId = signal<number | null>(null);

  private filter: HotelFilter = {};

  constructor() {
    this.load();
  }

  protected onFilter(filter: HotelFilter): void {
    this.filter = filter;
    this.page.set(1);
    this.load();
  }

  protected goTo(page: number): void {
    this.page.set(page);
    this.load();
  }

  protected remove(hotel: Hotel): void {
    this.hotelService.delete(hotel.id).subscribe({
      next: () => {
        this.confirmingId.set(null);
        this.notice.set(`Se eliminó "${hotel.name}".`);
        if (this.hotels().length === 1 && this.page() > 1) this.page.update((p) => p - 1);
        this.load();
      },
      error: (err) => {
        this.confirmingId.set(null);
        this.error.set(errorMessage(err));
      },
    });
  }

  private load(): void {
    this.loading.set(true);
    this.error.set(null);
    this.hotelService.search({ ...this.filter, page: this.page(), pageSize: PAGE_SIZE }).subscribe({
      next: (res) => {
        this.hotels.set(res.items);
        this.total.set(res.total);
        this.totalPages.set(Math.max(res.totalPages, 1));
        this.loading.set(false);
      },
      error: (err) => {
        this.error.set(errorMessage(err));
        this.loading.set(false);
      },
    });
  }
}
