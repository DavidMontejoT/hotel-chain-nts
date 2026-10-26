import { CurrencyPipe } from '@angular/common';
import { Component, inject, signal } from '@angular/core';

import { Hotel, HotelFilter } from '../../../core/models/hotel.model';
import { errorMessage, isHandledGlobally } from '../../../core/services/error-message';
import { HotelService } from '../../../core/services/hotel.service';
import { HotelSearchComponent } from '../../../shared/components/hotel-search.component';
import { StarRatingComponent } from '../../../shared/components/star-rating.component';

@Component({
  selector: 'app-landing',
  imports: [CurrencyPipe, HotelSearchComponent, StarRatingComponent],
  templateUrl: './landing.component.html',
  styleUrl: './landing.component.scss',
})
export class LandingComponent {
  private readonly hotelService = inject(HotelService);

  protected readonly hotels = signal<Hotel[]>([]);
  protected readonly total = signal(0);
  protected readonly loading = signal(true);
  protected readonly error = signal<string | null>(null);
  /** Hoteles cuya imagen no cargó; se muestra el marcador de posición. */
  protected readonly brokenImages = signal(new Set<number>());

  constructor() {
    this.load({});
  }

  protected markBroken(id: number): void {
    this.brokenImages.update((set) => new Set(set).add(id));
  }

  protected load(filter: HotelFilter): void {
    this.loading.set(true);
    this.error.set(null);
    this.hotelService.search({ ...filter, pageSize: 100 }).subscribe({
      next: (res) => {
        this.hotels.set(res.items);
        this.total.set(res.total);
        this.loading.set(false);
      },
      error: (err) => {
        this.error.set(isHandledGlobally(err) ? 'No pudimos cargar los hoteles en este momento.' : errorMessage(err));
        this.loading.set(false);
      },
    });
  }
}
