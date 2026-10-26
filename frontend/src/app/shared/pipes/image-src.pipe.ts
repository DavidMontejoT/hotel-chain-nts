import { Pipe, PipeTransform } from '@angular/core';

import { environment } from '../../../environments/environment';

/** Origin that serves uploaded images: the API host in development, the same origin in production. */
const API_ORIGIN = environment.apiUrl.replace(/\/api\/?$/, '');

/** Resolves image URLs: uploaded images (/uploads/...) are served by the API, external URLs pass through. */
@Pipe({ name: 'imageSrc' })
export class ImageSrcPipe implements PipeTransform {
  transform(url: string | null | undefined): string | null {
    if (!url) return null;
    return url.startsWith('/uploads/') ? API_ORIGIN + url : url;
  }
}
