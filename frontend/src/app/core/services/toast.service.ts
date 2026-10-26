import { Injectable, signal } from '@angular/core';

export type ToastType = 'success' | 'error' | 'info';

export interface Toast {
  id: number;
  type: ToastType;
  message: string;
}

const AUTO_DISMISS_MS = 4500;

/** App-wide notifications rendered by ToastContainerComponent. */
@Injectable({ providedIn: 'root' })
export class ToastService {
  private nextId = 1;
  readonly toasts = signal<Toast[]>([]);

  success(message: string): void {
    this.show('success', message);
  }

  error(message: string): void {
    this.show('error', message);
  }

  info(message: string): void {
    this.show('info', message);
  }

  dismiss(id: number): void {
    this.toasts.update((list) => list.filter((t) => t.id !== id));
  }

  private show(type: ToastType, message: string): void {
    // Avoid stacking the same message when several requests fail at once.
    if (this.toasts().some((t) => t.message === message)) return;
    const toast: Toast = { id: this.nextId++, type, message };
    this.toasts.update((list) => [...list, toast]);
    setTimeout(() => this.dismiss(toast.id), AUTO_DISMISS_MS);
  }
}
