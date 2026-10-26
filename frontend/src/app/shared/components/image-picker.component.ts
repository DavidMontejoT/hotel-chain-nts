import { DecimalPipe } from '@angular/common';
import { Component, forwardRef, inject, signal } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';

import { errorMessage, isHandledGlobally } from '../../core/services/error-message';
import { ACCEPTED_IMAGE_TYPES, ImageService, MAX_IMAGE_BYTES, UploadedImage } from '../../core/services/image.service';
import { ImageSrcPipe } from '../pipes/image-src.pipe';

type Mode = 'upload' | 'url';

/** Form control that lets the admin either upload a photo or paste an image URL. */
@Component({
  selector: 'app-image-picker',
  imports: [DecimalPipe, ImageSrcPipe],
  providers: [{ provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => ImagePickerComponent), multi: true }],
  template: `
    <div class="tabs" role="tablist">
      <button type="button" role="tab" [class.active]="mode() === 'upload'" [attr.aria-selected]="mode() === 'upload'" (click)="mode.set('upload')">
        Subir foto
      </button>
      <button type="button" role="tab" [class.active]="mode() === 'url'" [attr.aria-selected]="mode() === 'url'" (click)="mode.set('url')">
        Usar URL
      </button>
    </div>

    @if (mode() === 'upload') {
      <label class="drop" [class.busy]="uploading()">
        <input type="file" [accept]="accept" (change)="onFile($event)" [disabled]="disabled() || uploading()" />
        <strong>{{ uploading() ? 'Subiendo y optimizando…' : 'Haz clic para elegir una imagen' }}</strong>
        <span>JPG, PNG, WEBP o GIF · hasta 10 MB · se optimiza a 400 KB o menos</span>
      </label>
    } @else {
      <input type="url" placeholder="https://…" [value]="isExternal() ? value() : ''" [disabled]="disabled()"
             (input)="onUrlInput($event)" (blur)="onTouched()" />
    }

    @if (error()) {
      <span class="error-text">{{ error() }}</span>
    }

    @if (value(); as current) {
      <div class="preview">
        <img [src]="current | imageSrc" alt="Vista previa de la imagen" (error)="error.set('No se pudo cargar la vista previa de esta imagen.')" />
        @if (optimized(); as info) {
          <span class="info">Optimizada: {{ info.width }} × {{ info.height }} px · {{ info.sizeBytes / 1024 | number: '1.0-0' }} KB</span>
        }
        <button type="button" class="btn btn-ghost btn-sm" (click)="setValue('')" [disabled]="disabled()">Quitar imagen</button>
      </div>
    }
  `,
  styles: `
    :host { display: flex; flex-direction: column; gap: 10px; }
    .tabs { display: inline-flex; border: 1px solid var(--c-border-strong); border-radius: 8px; overflow: hidden; align-self: flex-start; }
    .tabs button { font: inherit; padding: 6px 14px; border: 0; background: var(--c-surface); cursor: pointer; color: var(--c-muted); }
    .tabs button.active { background: var(--c-primary); color: var(--c-on-primary); }
    .drop { display: flex; flex-direction: column; gap: 2px; border: 2px dashed var(--c-border-strong); border-radius: 10px; padding: 16px; text-align: center; cursor: pointer; color: var(--c-muted); font-weight: 400; }
    .drop:hover, .drop:focus-within { border-color: var(--c-primary); }
    .drop strong { color: var(--c-primary); }
    .drop span { font-size: .85rem; }
    .drop.busy { opacity: .6; cursor: progress; }
    /* The native input stays focusable for keyboard users but is visually hidden. */
    .drop input { position: absolute; width: 1px; height: 1px; opacity: 0; overflow: hidden; }
    .preview { display: flex; align-items: center; gap: 12px; flex-wrap: wrap; }
    .info { font-size: .85rem; color: var(--c-muted); }
    .preview img { width: 160px; aspect-ratio: 16 / 10; object-fit: cover; border-radius: 8px; background: var(--c-border); }
  `,
})
export class ImagePickerComponent implements ControlValueAccessor {
  private readonly images = inject(ImageService);

  protected readonly accept = ACCEPTED_IMAGE_TYPES.join(',');
  protected readonly mode = signal<Mode>('upload');
  protected readonly value = signal('');
  protected readonly uploading = signal(false);
  protected readonly disabled = signal(false);
  protected readonly error = signal<string | null>(null);
  /** Final dimensions and weight of the last uploaded photo. */
  protected readonly optimized = signal<UploadedImage | null>(null);

  private onChange: (value: string) => void = () => undefined;
  protected onTouched: () => void = () => undefined;

  protected isExternal(): boolean {
    return !this.value().startsWith('/uploads/');
  }

  writeValue(value: string | null): void {
    this.value.set(value ?? '');
    // Existing hotels with an external picture open on the URL tab.
    if (value && !value.startsWith('/uploads/')) this.mode.set('url');
  }

  registerOnChange(fn: (value: string) => void): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: () => void): void {
    this.onTouched = fn;
  }

  setDisabledState(isDisabled: boolean): void {
    this.disabled.set(isDisabled);
  }

  protected setValue(value: string): void {
    this.error.set(null);
    this.optimized.set(null);
    this.value.set(value);
    this.onChange(value);
  }

  protected onUrlInput(event: Event): void {
    this.setValue((event.target as HTMLInputElement).value);
  }

  protected onFile(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    input.value = '';
    if (!file) return;

    this.onTouched();
    if (!ACCEPTED_IMAGE_TYPES.includes(file.type)) {
      this.error.set('Formato no soportado. Usa una imagen JPG, PNG, WEBP o GIF.');
      return;
    }
    if (file.size > MAX_IMAGE_BYTES) {
      this.error.set('La imagen no puede superar 10 MB.');
      return;
    }

    this.uploading.set(true);
    this.error.set(null);
    this.images.upload(file).subscribe({
      next: (image) => {
        this.setValue(image.url);
        this.optimized.set(image);
        this.uploading.set(false);
      },
      error: (err) => {
        if (!isHandledGlobally(err)) this.error.set(errorMessage(err));
        this.uploading.set(false);
      },
    });
  }
}
