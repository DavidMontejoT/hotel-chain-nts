import { Component, inject } from '@angular/core';

import { ToastService } from '../../core/services/toast.service';

@Component({
  selector: 'app-toast-container',
  template: `
    <div class="toasts" aria-live="polite">
      @for (toast of toastService.toasts(); track toast.id) {
        <div class="toast" [class]="'toast toast-' + toast.type" role="status">
          <span>{{ toast.message }}</span>
          <button type="button" aria-label="Cerrar" (click)="toastService.dismiss(toast.id)">×</button>
        </div>
      }
    </div>
  `,
  styles: `
    .toasts { position: fixed; right: 16px; bottom: 16px; z-index: 100; display: flex; flex-direction: column; gap: 8px; max-width: min(380px, calc(100vw - 32px)); }
    .toast { display: flex; align-items: flex-start; gap: 12px; padding: 12px 14px; border-radius: 10px; box-shadow: var(--shadow); background: var(--c-surface); border-left: 4px solid var(--c-primary); }
    .toast-success { border-left-color: var(--c-success); }
    .toast-error { border-left-color: var(--c-danger); }
    .toast span { flex: 1; }
    .toast button { background: none; border: 0; font-size: 1.2rem; line-height: 1; cursor: pointer; color: var(--c-muted); }
  `,
})
export class ToastContainerComponent {
  protected readonly toastService = inject(ToastService);
}
