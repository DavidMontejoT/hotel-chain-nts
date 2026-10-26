import { HttpClient } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { Observable, tap } from 'rxjs';

import { environment } from '../../../environments/environment';
import { LoginResponse } from '../models/hotel.model';

const STORAGE_KEY = 'hotelchain.session';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);
  private readonly session = signal<LoginResponse | null>(this.restore());

  readonly username = computed(() => this.session()?.username ?? null);
  readonly isAdmin = computed(() => {
    const s = this.session();
    return !!s && s.role === 'Admin' && new Date(s.expiresAt) > new Date();
  });

  get token(): string | null {
    return this.isAdmin() ? this.session()!.token : null;
  }

  login(username: string, password: string): Observable<LoginResponse> {
    return this.http
      .post<LoginResponse>(`${environment.apiUrl}/auth/login`, { username, password })
      .pipe(
        tap((res) => {
          this.session.set(res);
          sessionStorage.setItem(STORAGE_KEY, JSON.stringify(res));
        }),
      );
  }

  logout(): void {
    this.session.set(null);
    sessionStorage.removeItem(STORAGE_KEY);
    this.router.navigate(['/admin/login']);
  }

  private restore(): LoginResponse | null {
    try {
      const raw = sessionStorage.getItem(STORAGE_KEY);
      return raw ? (JSON.parse(raw) as LoginResponse) : null;
    } catch {
      return null;
    }
  }
}
