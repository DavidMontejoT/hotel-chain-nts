import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { environment } from '../../../environments/environment';

/** Largest file accepted; the API optimizes it down to 400 KB. */
export const MAX_IMAGE_BYTES = 10 * 1024 * 1024;
export const ACCEPTED_IMAGE_TYPES = ['image/jpeg', 'image/png', 'image/webp', 'image/gif'];

/** Stored image after the API optimizes it (400 KB max, as large as possible). */
export interface UploadedImage {
  url: string;
  width: number;
  height: number;
  sizeBytes: number;
}

@Injectable({ providedIn: 'root' })
export class ImageService {
  private readonly http = inject(HttpClient);

  /** Uploads a photo and returns the path to store in the hotel (e.g. /uploads/abc.webp). */
  upload(file: File): Observable<UploadedImage> {
    const form = new FormData();
    form.append('file', file);
    return this.http.post<UploadedImage>(`${environment.apiUrl}/images`, form);
  }
}
