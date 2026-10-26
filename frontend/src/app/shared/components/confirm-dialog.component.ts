import { Component, ElementRef, effect, inject, viewChild } from '@angular/core';

import { ConfirmService } from '../../core/services/confirm.service';

/** Modal confirmation built on the native <dialog> element (focus trap and Esc support for free). */
@Component({
  selector: 'app-confirm-dialog',
  template: `
    <dialog #dialog class="dialog" (cancel)="$event.preventDefault(); confirm.close(false)" aria-labelledby="confirm-title">
      @if (confirm.pending(); as p) {
        <h2 id="confirm-title">{{ p.title }}</h2>
        <p>{{ p.message }}</p>
        <div class="buttons">
          <button type="button" class="btn btn-ghost" (click)="confirm.close(false)">{{ p.cancelText ?? 'Cancelar' }}</button>
          <button type="button" class="btn" [class.btn-solid-danger]="p.danger" (click)="confirm.close(true)">
            {{ p.confirmText ?? 'Confirmar' }}
          </button>
        </div>
      }
    </dialog>
  `,
  styles: `
    .dialog { border: 0; border-radius: var(--radius); padding: 24px; width: min(440px, calc(100vw - 32px)); box-shadow: var(--shadow); }
    .dialog::backdrop { background: rgb(0 0 0 / .45); }
    h2 { font-size: 1.2rem; }
    p { margin: 0 0 20px; color: var(--c-muted); }
    .buttons { display: flex; justify-content: flex-end; gap: 12px; flex-wrap: wrap; }
    .btn-solid-danger { background: var(--c-danger); }
    .btn-solid-danger:hover { background: #912018; }
  `,
})
export class ConfirmDialogComponent {
  protected readonly confirm = inject(ConfirmService);
  private readonly dialog = viewChild.required<ElementRef<HTMLDialogElement>>('dialog');

  constructor() {
    effect(() => {
      const el = this.dialog().nativeElement;
      if (this.confirm.pending() && !el.open) el.showModal();
      else if (!this.confirm.pending() && el.open) el.close();
    });
  }
}
