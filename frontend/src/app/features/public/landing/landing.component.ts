import { AsyncPipe, CurrencyPipe } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

import { HotelFilter } from '../../../core/models/hotel.model';
import { HotelService } from '../../../core/services/hotel.service';
import { toHotelsView } from '../../../core/state/hotels-view';
import { HotelSearchComponent } from '../../../shared/components/hotel-search.component';
import { StarRatingComponent } from '../../../shared/components/star-rating.component';
import { ImageSrcPipe } from '../../../shared/pipes/image-src.pipe';

@Component({
  selector: 'app-landing',
  imports: [AsyncPipe, CurrencyPipe, HotelSearchComponent, StarRatingComponent, ImageSrcPipe],
  templateUrl: './landing.component.html',
  styleUrl: './landing.component.scss',
})
export class LandingComponent {
  private readonly hotelService = inject(HotelService);
  private readonly filter$ = new BehaviorSubject<HotelFilter>({});

  /** Every filter change triggers a new API request; the template subscribes with the async pipe. */
  protected readonly view$ = this.filter$.pipe(
    toHotelsView((filter) => this.hotelService.search({ ...filter, pageSize: 100 }), 'No pudimos cargar los hoteles en este momento.'),
  );

  /** Hotels whose image failed to load; a placeholder is shown instead. */
  protected readonly brokenImages = signal(new Set<number>());

  protected search(filter: HotelFilter): void {
    this.filter$.next(filter);
  }

  protected markBroken(id: number): void {
    this.brokenImages.update((set) => new Set(set).add(id));
  }
}
