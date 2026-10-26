import { Component, inject, output } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { debounceTime, distinctUntilChanged } from 'rxjs';

import { HotelFilter } from '../../core/models/hotel.model';

export type RatingMode = 'exact' | 'min';

/** Reusable search bar: emits name and/or rating filters that are sent to the API. */
@Component({
  selector: 'app-hotel-search',
  imports: [ReactiveFormsModule],
  template: `
    <form class="search" [formGroup]="form" (ngSubmit)="emit()" role="search">
      <label class="field grow">
        <span>Nombre del hotel</span>
        <input type="search" formControlName="name" placeholder="Ej. Cartagena" autocomplete="off" />
      </label>
      <label class="field">
        <span>Calificación</span>
        <select formControlName="rating">
          <option [ngValue]="null">Todas</option>
          @for (r of ratings; track r) {
            <option [ngValue]="r">{{ r }} {{ r === 1 ? 'estrella' : 'estrellas' }}</option>
          }
        </select>
      </label>
      <label class="field">
        <span>Criterio</span>
        <select formControlName="mode">
          <option value="exact">Exacta</option>
          <option value="min">Mínima</option>
        </select>
      </label>
      <button type="button" class="btn btn-ghost" (click)="clear()">Limpiar</button>
    </form>
  `,
  styles: `
    .search { display: flex; flex-wrap: wrap; gap: 12px; align-items: flex-end; }
    .grow { flex: 1 1 220px; }
    .field { flex: 0 1 160px; }
    @media (max-width: 560px) { .field, .grow, .btn { flex: 1 1 100%; } }
  `,
})
export class HotelSearchComponent {
  readonly filterChange = output<HotelFilter>();

  protected readonly ratings = [5, 4, 3, 2, 1];
  protected readonly form = inject(FormBuilder).nonNullable.group({
    name: '',
    rating: null as number | null,
    mode: 'exact' as RatingMode,
  });

  constructor() {
    // Live search: debounce typing and skip identical filters; unsubscribes automatically on destroy.
    this.form.valueChanges
      .pipe(
        debounceTime(300),
        distinctUntilChanged((a, b) => JSON.stringify(a) === JSON.stringify(b)),
        takeUntilDestroyed(),
      )
      .subscribe(() => this.emit());
  }

  protected emit(): void {
    const { name, rating, mode } = this.form.getRawValue();
    this.filterChange.emit({
      name: name.trim() || undefined,
      rating: mode === 'exact' ? rating : null,
      minRating: mode === 'min' ? rating : null,
    });
  }

  protected clear(): void {
    this.form.reset();
  }
}
