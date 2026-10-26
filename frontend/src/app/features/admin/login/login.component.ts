import { Component, inject, input, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';

import { AuthService } from '../../../core/services/auth.service';
import { errorMessage } from '../../../core/services/error-message';

@Component({
  selector: 'app-login',
  imports: [ReactiveFormsModule],
  template: `
    <section class="container page">
      <form class="card login" [formGroup]="form" (ngSubmit)="submit()">
        <h1>Ingreso administrador</h1>
        <p class="muted">Accede para crear, editar y eliminar hoteles.</p>

        @if (error()) {
          <div class="alert alert-error" role="alert">{{ error() }}</div>
        }

        <label class="field">
          <span>Usuario</span>
          <input formControlName="username" autocomplete="username" />
        </label>
        <label class="field">
          <span>Contraseña</span>
          <input type="password" formControlName="password" autocomplete="current-password" />
        </label>

        <button class="btn" type="submit" [disabled]="form.invalid || loading()">
          {{ loading() ? 'Ingresando…' : 'Ingresar' }}
        </button>
      </form>
    </section>
  `,
  styles: `
    .login { max-width: 420px; margin: 0 auto; padding: 28px; display: flex; flex-direction: column; gap: 16px; }
    .login h1 { font-size: 1.5rem; margin: 0; }
    .login p { margin: 0; }
  `,
})
export class LoginComponent {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  /** URL to return to after signing in (query param). */
  readonly returnUrl = input<string>();

  protected readonly loading = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly form = inject(FormBuilder).nonNullable.group({
    username: ['', Validators.required],
    password: ['', Validators.required],
  });

  constructor() {
    if (this.auth.isAdmin()) this.router.navigate(['/admin/hotels']);
  }

  protected submit(): void {
    if (this.form.invalid) return;
    const { username, password } = this.form.getRawValue();
    this.loading.set(true);
    this.error.set(null);
    this.auth.login(username, password).subscribe({
      next: () => this.router.navigateByUrl(this.returnUrl() || '/admin/hotels'),
      error: (err) => {
        this.error.set(errorMessage(err));
        this.loading.set(false);
      },
    });
  }
}
