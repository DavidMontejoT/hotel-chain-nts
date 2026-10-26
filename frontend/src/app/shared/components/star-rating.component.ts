import { Component, computed, input } from '@angular/core';

@Component({
  selector: 'app-star-rating',
  template: `
    <span class="stars" [attr.aria-label]="value() + ' de 5 estrellas'" role="img">
      @for (filled of stars(); track $index) {
        <span [class.on]="filled" aria-hidden="true">★</span>
      }
    </span>
  `,
  styles: `
    .stars { color: var(--c-border-strong); letter-spacing: 1px; white-space: nowrap; }
    .on { color: var(--c-star); }
  `,
})
export class StarRatingComponent {
  readonly value = input.required<number>();
  protected readonly stars = computed(() => [1, 2, 3, 4, 5].map((n) => n <= this.value()));
}
