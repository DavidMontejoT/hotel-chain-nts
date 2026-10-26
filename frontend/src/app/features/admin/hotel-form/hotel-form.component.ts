import { Component, OnInit, inject, input, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';

import { HotelRequest } from '../../../core/models/hotel.model';
import { errorMessage } from '../../../core/services/error-message';
import { HotelService } from '../../../core/services/hotel.service';

const URL_PATTERN = /^https?:\/\/.+/i;

/** Formulario compartido para crear y editar hoteles. */
@Component({
  selector: 'app-hotel-form',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './hotel-form.component.html',
  styleUrl: './hotel-form.component.scss',
})
export class HotelFormComponent implements OnInit {
  private readonly hotelService = inject(HotelService);
  private readonly router = inject(Router);

  /** Id del hotel en edición (parámetro de ruta); vacío al crear. */
  readonly id = input<string>();

  protected readonly ratings = [1, 2, 3, 4, 5];
  protected readonly loading = signal(false);
  protected readonly saving = signal(false);
  protected readonly error = signal<string | null>(null);

  protected readonly form = inject(FormBuilder).nonNullable.group({
    name: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(150)]],
    city: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(100)]],
    address: ['', [Validators.required, Validators.minLength(3), Validators.maxLength(250)]],
    description: ['', Validators.maxLength(2000)],
    rating: [3, [Validators.required, Validators.min(1), Validators.max(5)]],
    pricePerNight: [0, [Validators.required, Validators.min(0)]],
    imageUrl: ['', [Validators.maxLength(500), Validators.pattern(URL_PATTERN)]],
  });

  protected get isEdit(): boolean {
    return !!this.id();
  }

  ngOnInit(): void {
    if (!this.isEdit) return;
    this.loading.set(true);
    this.hotelService.getById(Number(this.id())).subscribe({
      next: (hotel) => {
        this.form.patchValue({
          ...hotel,
          description: hotel.description ?? '',
          imageUrl: hotel.imageUrl ?? '',
        });
        this.loading.set(false);
      },
      error: (err) => {
        this.error.set(errorMessage(err));
        this.loading.set(false);
      },
    });
  }

  protected invalid(name: keyof typeof this.form.controls): boolean {
    const control = this.form.controls[name];
    return control.invalid && (control.touched || control.dirty);
  }

  protected submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();
    const request: HotelRequest = {
      ...value,
      description: value.description.trim() || null,
      imageUrl: value.imageUrl.trim() || null,
    };

    this.saving.set(true);
    this.error.set(null);
    const save$ = this.isEdit
      ? this.hotelService.update(Number(this.id()), request)
      : this.hotelService.create(request);

    save$.subscribe({
      next: () => this.router.navigate(['/admin/hotels']),
      error: (err) => {
        this.error.set(errorMessage(err));
        this.saving.set(false);
      },
    });
  }
}
