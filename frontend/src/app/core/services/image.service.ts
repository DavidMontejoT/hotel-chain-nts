import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { environment } from '../../../environments/environment';

export const MAX_IMAGE_BYTES = 5 * 1024 * 1024;
export const ACCEPTED_IMAGE_TYPES = ['image/jpeg', 'image/png', 'image/webp', 'image/gif'];

@Injectable({ providedIn: 'root' })
export class ImageService {
  private readonly http = inject(HttpClient);

  /** Uploads a photo and returns the path to store in the hotel (e.g. /uploads/abc.jpg). */
  upload(file: File): Observable<{ url: string }> {
    const form = new FormData();
    form.append('file', file);
    return this.http.post<{ url: string }>(`${environment.apiUrl}/images`, form);
  }
}
