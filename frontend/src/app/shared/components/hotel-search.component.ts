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
        <span class="label">
          <svg viewBox="0 0 24 24" aria-hidden="true"><path d="M12 21s-7-6.1-7-11.5a7 7 0 0 1 14 0C19 14.9 12 21 12 21Z" /><circle cx="12" cy="9.5" r="2.5" /></svg>
          Nombre del hotel
        </span>
        <input type="search" formControlName="name" placeholder="¿A dónde quieres ir?" autocomplete="off" />
      </label>
      <label class="field">
        <span class="label">
          <svg viewBox="0 0 24 24" aria-hidden="true"><path d="m12 3 2.7 5.6 6.1.9-4.4 4.3 1 6.1L12 17l-5.4 2.9 1-6.1-4.4-4.3 6.1-.9Z" /></svg>
          Calificación
        </span>
        <select formControlName="rating">
          <option [ngValue]="null">Todas</option>
          @for (r of ratings; track r) {
            <option [ngValue]="r">{{ r }} {{ r === 1 ? 'estrella' : 'estrellas' }}</option>
          }
        </select>
      </label>
      <label class="field">
        <span class="label">Criterio</span>
        <select formControlName="mode">
          <option value="exact">Exacta</option>
          <option value="min">Mínima</option>
        </select>
      </label>
      <div class="actions">
        <button type="button" class="btn btn-ghost" (click)="clear()">Limpiar</button>
        <button type="submit" class="btn search-btn">
          <svg viewBox="0 0 24 24" aria-hidden="true"><circle cx="11" cy="11" r="7" /><path d="m20 20-3.5-3.5" /></svg>
          Buscar hoteles
        </button>
      </div>
    </form>
  `,
  styles: `
    .search { display: flex; flex-wrap: wrap; gap: 16px; align-items: flex-end; }
    .grow { flex: 1 1 240px; }
    .field { flex: 0 1 170px; }
    .label {
      display: inline-flex; align-items: center; gap: 6px;
      text-transform: uppercase; letter-spacing: .12em; font-size: .72rem; font-weight: 600; color: var(--c-muted);
    }
    .label svg { width: 14px; height: 14px; fill: none; stroke: var(--c-accent); stroke-width: 2; stroke-linecap: round; stroke-linejoin: round; }
    input, select { border-color: transparent; border-bottom: 1px solid var(--c-border-strong); border-radius: 10px; background: rgb(255 255 255 / .7); font-size: 1rem; }
    .actions { display: flex; gap: 10px; margin-left: auto; }
    .search-btn svg { width: 18px; height: 18px; fill: none; stroke: currentColor; stroke-width: 2.2; stroke-linecap: round; }
    @media (max-width: 560px) {
      .field, .grow, .actions { flex: 1 1 100%; }
      .actions .btn { flex: 1; }
    }
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
